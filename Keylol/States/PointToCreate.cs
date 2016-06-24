using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsQuery;
using Keylol.Models.DAL;
using Keylol.StateTreeManager;
using Newtonsoft.Json.Linq;

namespace Keylol.States
{
    /// <summary>
    /// 待开设据点
    /// </summary>
    public class PointToCreate
    {
        /// <summary>
        /// 从 Steam 商店抓取待开设据点信息
        /// </summary>
        /// <param name="steamAppId">Steam App ID</param>
        /// <param name="dbContext"><see cref="KeylolDbContext"/></param>
        /// <returns><see cref="PointToCreate"/></returns>
        public static async Task<PointToCreate> Get(int steamAppId, [Injected] KeylolDbContext dbContext)
        {
            try
            {
                try
                {
                    if (steamAppId <= 0)
                        throw new Exception("App ID 不正确");
                    if (await dbContext.Points.AnyAsync(p => p.SteamAppId == steamAppId))
                        throw new Exception("据点已存在");

                    var cookieContainer = new CookieContainer();
                    cookieContainer.Add(new Uri("http://store.steampowered.com/"), new Cookie("birthtime", "-473410799"));
                    var request =
                        WebRequest.CreateHttp($"http://store.steampowered.com/app/{steamAppId}/?l=english&cc=us");
                    var picsRequest =
                        WebRequest.CreateHttp($"https://steampics-mckay.rhcloud.com/info?apps={steamAppId}");
                    picsRequest.AutomaticDecompression =
                        request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                    picsRequest.CookieContainer = request.CookieContainer = cookieContainer;
                    picsRequest.Timeout = request.Timeout = 30000;
                    var picsAwaiter = picsRequest.GetResponseAsync();
                    using (var response = await request.GetResponseAsync())
                    {
                        var rs = response.GetResponseStream();
                        if (rs == null)
                            throw new Exception("商店页面获取失败");
                        Config.OutputFormatter = OutputFormatters.HtmlEncodingNone;
                        var dom = CQ.Create(rs);
                        var navTexts = dom[".game_title_area .blockbg a"];

                        if (!navTexts.Any() ||
                            (navTexts[0].InnerText != "All Games"))
                            throw new Exception("目前只支持 Steam 游戏");

                        if (dom[".game_area_dlc_bubble"].Any())
                            throw new Exception("不支持 DLC");

                        var result = new PointToCreate();

                        foreach (var child in dom[".game_details .details_block"].First().Find("b"))
                        {
                            var key = child.InnerText.Trim();
                            var values = new List<string>();
                            if (string.IsNullOrWhiteSpace(child.NextSibling.NodeValue))
                            {
                                var current = child;
                                do
                                {
                                    current = current.NextElementSibling;
                                    values.Add(current.InnerText.Trim());
                                } while (current.NextSibling.NodeType == NodeType.TEXT_NODE &&
                                         current.NextSibling.NodeValue.Trim() == ",");
                            }
                            else
                            {
                                values.Add(child.NextSibling.NodeValue.Trim());
                            }
                            if (!values.Any())
                                continue;
                            if (key == "Title:")
                                result.EnglishName = values[0];
                        }


                        foreach (var img in dom[".highlight_strip_screenshot img"])
                        {
                            var match = Regex.Match(img.Attributes["src"], @"ss_([^\/]*)\.\d+x\d+\.jpg");
                            if (!match.Success) continue;
                            result.HeaderImage = $"keylol://steam/app-screenshots/{steamAppId}-{match.Groups[1].Value}";
                            break;
                        }

                        using (var picsResponse = await picsAwaiter)
                        {
                            var picsRs = picsResponse.GetResponseStream();
                            if (picsRs == null)
                                throw new Exception("PICS 获取失败，请重试");
                            var sr = new StreamReader(picsRs);
                            var picsRoot = JToken.Parse(await sr.ReadToEndAsync());
                            if (!(bool) picsRoot["success"])
                                throw new Exception("PICS 获取失败，请重试");
                            result.ThumbnailImage =
                                $"keylol://steam/app-thumbnails/{steamAppId}-{(string) picsRoot["apps"][steamAppId.ToString()]["common"]["logo"]}";
                            result.AvatarImage =
                                $"keylol://steam/app-icons/{steamAppId}-{(string) picsRoot["apps"][steamAppId.ToString()]["common"]["icon"]}";
                        }
                        return result;
                    }
                }
                catch (WebException)
                {
                    throw new Exception("网络错误，请重试");
                }
            }
            catch (Exception e)
            {
                return new PointToCreate
                {
                    Failed = true,
                    FailReason = e.Message
                };
            }
        }

        /// <summary>
        /// 抓取是否失败
        /// </summary>
        public bool? Failed { get; set; }

        /// <summary>
        /// 失败原因
        /// </summary>
        public string FailReason { get; set; }

        /// <summary>
        /// 英文名
        /// </summary>
        public string EnglishName { get; set; }

        /// <summary>
        /// 缩略图
        /// </summary>
        public string ThumbnailImage { get; set; }

        /// <summary>
        /// 页眉图片
        /// </summary>
        public string HeaderImage { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string AvatarImage { get; set; }
    }
}