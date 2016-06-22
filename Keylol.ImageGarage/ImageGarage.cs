using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using ChannelAdam.ServiceModel;
using CsQuery;
using CsQuery.Output;
using JetBrains.Annotations;
using Keylol.ImageGarage.ServiceReference;
using Keylol.Models.DTO;
using Keylol.ServiceBase;
using log4net;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Keylol.ImageGarage
{
    internal sealed class ImageGarage : KeylolService
    {
        private readonly IServiceConsumer<IImageGarageCoordinator> _coordinator;
        private readonly Timer _heartbeatTimer = new Timer(10000) {AutoReset = false}; // 10s
        private readonly ILog _logger;
        private readonly IModel _mqChannel;

        public ImageGarage(ILogProvider logProvider, MqClientProvider mqClientProvider,
            IServiceConsumer<IImageGarageCoordinator> coordinator)
        {
            ServiceName = "Keylol.ImageGarage";

            _logger = logProvider.Logger;
            _mqChannel = mqClientProvider.CreateModel();
            _coordinator = coordinator;
            Config.HtmlEncoder = new HtmlEncoderMinimum();

            _heartbeatTimer.Elapsed += (sender, args) =>
            {
                try
                {
                    _coordinator.Operations.Ping();
                }
                catch (Exception e)
                {
                    _logger.Warn("Ping failed.", e);
                    _coordinator.Close();
                }
                _heartbeatTimer.Start();
            };
        }

        protected override void OnStart(string[] args)
        {
            _mqChannel.BasicQos(0, 5, false);
            var consumer = new EventingBasicConsumer(_mqChannel);
            consumer.Received += async (sender, eventArgs) =>
            {
                try
                {
                    using (var streamReader = new StreamReader(new MemoryStream(eventArgs.Body)))
                    {
                        var serializer = new JsonSerializer();
                        var requestDto = serializer.Deserialize<ImageGarageRequestDto>(new JsonTextReader(streamReader));

                        string content, coverImage, title;
                        byte[] rowVersion;
                        switch (requestDto.ContentType)
                        {
                            case ImageGarageRequestContentType.Article:
                                var article = _coordinator.Operations.FindArticle(requestDto.ContentId);
                                if (article == null)
                                {
                                    _mqChannel.BasicNack(eventArgs.DeliveryTag, false, false);
                                    _logger.Warn($"Article {requestDto.ContentId} doesn't exist.");
                                    return;
                                }
                                content = article.Content;
                                title = article.Title;
                                coverImage = article.CoverImage;
                                rowVersion = article.RowVersion;
                                break;

                            case ImageGarageRequestContentType.ArticleComment:
                                var comment = _coordinator.Operations.FindArticleComment(requestDto.ContentId);
                                if (comment == null)
                                {
                                    _mqChannel.BasicNack(eventArgs.DeliveryTag, false, false);
                                    _logger.Warn($"ArticleComment {requestDto.ContentId} doesn't exist.");
                                    return;
                                }
                                content = comment.Content;
                                coverImage = null;
                                title = comment.Id;
                                rowVersion = comment.RowVersion;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(requestDto.ContentType));
                        }

                        var dom = CQ.Create(content);
                        var downloadCount = 0;
                        var uploadCache = new Dictionary<string, string>();
                        foreach (var img in dom["img"])
                        {
                            var imgSrc = img.Attributes["src"];
                            if (string.IsNullOrWhiteSpace(imgSrc)) continue;
                            string url;
                            if (!uploadCache.TryGetValue(imgSrc, out url))
                            {
                                url = await UploadFromUrlAsync(imgSrc);
                                uploadCache[imgSrc] = url;
                                downloadCount++;
                            }
                            if (string.IsNullOrWhiteSpace(url)) continue;
                            img.Attributes["article-image-src"] = url;
                            img.RemoveAttribute("src");
                        }
                        if (!Helpers.IsTrustedUrl(coverImage))
                        {
                            string url;
                            Debug.Assert(coverImage != null, "coverImage != null");
                            if (!uploadCache.TryGetValue(coverImage, out url))
                            {
                                url = await UploadFromUrlAsync(coverImage);
                                uploadCache[coverImage] = url;
                                downloadCount++;
                            }
                            if (!string.IsNullOrWhiteSpace(url))
                                coverImage = url;
                        }
                        switch (requestDto.ContentType)
                        {
                            case ImageGarageRequestContentType.Article:
                                _coordinator.Operations.UpdateArticle(requestDto.ContentId, content, coverImage,
                                    rowVersion);
                                _logger.Info(
                                    $"Article \"{title}\" ({requestDto.ContentId}) finished, {downloadCount} images downloaded.");
                                break;

                            case ImageGarageRequestContentType.ArticleComment:
                                _coordinator.Operations.UpdateArticleComment(requestDto.ContentId, content, rowVersion);
                                _logger.Info(
                                    $"ArticleComment ({title}) finished, {downloadCount} images downloaded.");
                                break;

                            default:
                                throw new ArgumentOutOfRangeException(nameof(requestDto.ContentType));
                        }
                        _mqChannel.BasicAck(eventArgs.DeliveryTag, false);
                    }
                }
                catch (FaultException e)
                {
                    _mqChannel.BasicNack(eventArgs.DeliveryTag, false, false);
                    _logger.Warn("WCF Channel fault.", e);
                }
                catch (Exception e)
                {
                    _mqChannel.BasicNack(eventArgs.DeliveryTag, false, false);
                    _logger.Fatal("RabbitMQ unhandled callback exception.", e);
                }
            };
            _mqChannel.BasicConsume(MqClientProvider.ImageGarageRequestQueue, false, consumer);
        }

        protected override void OnStop()
        {
            _heartbeatTimer.Stop();
            _mqChannel.Close();
            base.OnStop();
        }

        private static string MimeTypeToFileExtension(string mimeType)
        {
            if (string.IsNullOrWhiteSpace(mimeType)) return null;
            var map = new Dictionary<string, string>
            {
                {"image/bmp", "bmp"},
                {"image/gif", "gif"},
                {"image/x-icon", "ico"},
                {"image/jpeg", "jpg"},
                {"image/png", "png"},
                {"image/svg+xml", "svg"},
                {"image/webp", "webp"}
            };
            return map.ContainsKey(mimeType) ? map[mimeType] : null;
        }

        private async Task<string> UploadFromUrlAsync([NotNull] string url)
        {
            try
            {
                url = Regex.Replace(url, @"^\/\/", "http://");
                if (!Regex.IsMatch(url, @"^https?:\/\/")) url = $"http://{url}";
                var request = WebRequest.CreateHttp(url);
                request.Referer = url;
                request.UserAgent =
                    "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.116 Safari/537.36";
                request.Accept = "image/webp,image/*,*/*;q=0.8";
                request.Headers["Accept-Language"] = "en-US,en;q=0.8,zh-CN;q=0.6,zh;q=0.4";
                request.Timeout = 10000;
                request.ReadWriteTimeout = 60000;
                using (var response = await request.GetResponseAsync())
                using (var ms = new MemoryStream(response.ContentLength > 0 ? (int) response.ContentLength : 0))
                {
                    var extension = MimeTypeToFileExtension(response.ContentType?.Split(';')[0]);
                    if (extension == null) // 不支持的类型
                    {
                        _logger.Warn($"Unsupported MIME type: {url}");
                        return null;
                    }
                    if (response.ContentLength > UpyunProvider.MaxImageSize)
                    {
                        _logger.Warn($"Image (Content-Length) is too large: {url}");
                        return null;
                    }
                    var responseStream = response.GetResponseStream();
                    if (responseStream == null)
                    {
                        _logger.Warn($"Null response stream: {url}");
                        return null;
                    }
                    await responseStream.CopyToAsync(ms);
                    var fileData = ms.ToArray();
                    if (fileData.Length <= 0)
                    {
                        _logger.Warn($"Empty response stream: {url}");
                        return null;
                    }
                    if (fileData.Length > UpyunProvider.MaxImageSize)
                    {
                        _logger.Warn($"Image (response stream length) is too large: {url}");
                        return null;
                    }
                    var name = await UpyunProvider.UploadFile(fileData, extension);
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        _logger.Warn($"Upload failed: {url}");
                        return null;
                    }
                    return $"keylol://{name}";
                }
            }
            catch (Exception e)
            {
                _logger.Warn($"Download failed: {url}", e);
                return null;
            }
        }
    }
}