using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Forms;
using CsQuery;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Keylol.CruatorSpammer
{
    public partial class Main : Form
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public Main()
        {
            InitializeComponent();
        }

        [DllImport("wininet.dll", SetLastError = true)]
        public static extern bool InternetGetCookieEx(
            string url,
            string cookieName,
            StringBuilder cookieData,
            ref int size,
            int dwFlags,
            IntPtr lpReserved);

        private const int InternetCookieHttponly = 0x2000;

        /// <summary>
        /// Gets the URI cookie container.
        /// </summary>
        /// <param name="uri">The URI.</param>
        public static CookieContainer GetUriCookieContainer(Uri uri)
        {
            CookieContainer cookies = null;
            // Determine the size of the cookie
            var datasize = 8192*16;
            var cookieData = new StringBuilder(datasize);
            if (
                !InternetGetCookieEx(uri.ToString(), null, cookieData, ref datasize, InternetCookieHttponly, IntPtr.Zero))
            {
                if (datasize < 0)
                    return null;
                // Allocate stringbuilder large enough to hold the cookie
                cookieData = new StringBuilder(datasize);
                if (!InternetGetCookieEx(
                    uri.ToString(),
                    null, cookieData,
                    ref datasize,
                    InternetCookieHttponly,
                    IntPtr.Zero))
                    return null;
            }
            if (cookieData.Length > 0)
            {
                cookies = new CookieContainer();
                cookies.SetCookies(uri, cookieData.ToString().Replace(';', ','));
            }
            return cookies;
        }

        private void Main_Load(object sender, EventArgs e)
        {
            Font = new Font(Font.FontFamily, Font.SizeInPoints*125/96);

            int browserVer, regVal;

            // get the installed IE version
            using (var wb = new WebBrowser())
                browserVer = wb.Version.Major;

            // set the appropriate IE version
            if (browserVer >= 11)
                regVal = 11001;
            else if (browserVer == 10)
                regVal = 10001;
            else if (browserVer == 9)
                regVal = 9999;
            else if (browserVer == 8)
                regVal = 8888;
            else
                regVal = 7000;

            // set the actual key
            try
            {
                var regKey = Registry.LocalMachine.OpenSubKey(Environment.Is64BitOperatingSystem
                    ? @"SOFTWARE\\Wow6432Node\\Microsoft\\Internet Explorer\\MAIN\\FeatureControl\\FEATURE_BROWSER_EMULATION"
                    : @"SOFTWARE\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION",
                    true);
                if (regKey != null)
                {
                    regKey.SetValue(System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe", regVal,
                        RegistryValueKind.DWord);
                    regKey.Close();
                }
            }
            catch (SecurityException)
            {
            }

            var settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
            if (!File.Exists(settingsPath)) return;
            var settingsText = File.ReadAllText(settingsPath);
            var settings = JObject.Parse(settingsText);
            foreach (var group in settings["groups"])
            {
                groupLinkCheckedListBox.Items.Add((string) group, true);
            }
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            var match = Regex.Match(groupLinkTextBox.Text, @"^(?:(?:https?://)?steamcommunity\.com/groups/)?([^#/]+)");
            if (!match.Success)
            {
                MessageBox.Show(@"输入格式有误");
                return;
            }
            var link = match.Groups[1].Value;
            if (groupLinkCheckedListBox.Items.Contains(link))
            {
                MessageBox.Show(@"已经存在于列表中");
                return;
            }
            groupLinkCheckedListBox.Items.Add(link, true);
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            var settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
            using (var file = File.Open(settingsPath, FileMode.Create))
            using (var writer = new StreamWriter(file))
            {
                var json = new JsonSerializer();
                json.Serialize(writer, new {groups = groupLinkCheckedListBox.Items});
            }
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (groupLinkCheckedListBox.SelectedIndex >= 0)
                groupLinkCheckedListBox.Items.RemoveAt(groupLinkCheckedListBox.SelectedIndex);
        }

        private static void AppendUrlEncoded(StringBuilder sb, string name, string value, bool moreValues = true)
        {
            sb.Append(HttpUtility.UrlEncode(name));
            sb.Append("=");
            sb.Append(HttpUtility.UrlEncode(value));
            if (moreValues)
                sb.Append("&");
        }

        private async void publishButton_Click(object sender, EventArgs e)
        {
            if (contextTextBox.Text.Length > 152)
            {
                MessageBox.Show(@"字数超出 152 字上限");
                return;
            }
            var sessionId = GetSessionId();
            var resultSb = new StringBuilder();
            for (var i = 0; i < groupLinkCheckedListBox.Items.Count; i++)
            {
                var group = (string) groupLinkCheckedListBox.Items[i];
                if (!groupLinkCheckedListBox.GetItemChecked(i))
                    continue;
                try
                {
                    var request = GenerateRequest($"http://steamcommunity.com/groups/{group}/createrecommendation/",
                        "POST", "http://steamcommunity.com/groups/{group}");
                    var postData = new StringBuilder();
                    AppendUrlEncoded(postData, "sessionID", sessionId);
                    AppendUrlEncoded(postData, "appid", appIdTextBox.Text);
                    AppendUrlEncoded(postData, "appname", appNameTextBox.Text);
                    AppendUrlEncoded(postData, "blurb", contextTextBox.Text);
                    AppendUrlEncoded(postData, "link_url", urlTextBox.Text);
                    AppendUrlEncoded(postData, "create_only", overrideExsitedCheckBox.Checked ? "0" : "1", false);
                    using (var rs = await request.GetRequestStreamAsync())
                    {
                        var data = Encoding.UTF8.GetBytes(postData.ToString());
                        await rs.WriteAsync(data, 0, data.Length);
                    }
                    using (var response = await request.GetResponseAsync())
                    {
                        var rs = response.GetResponseStream();
                        if (rs != null)
                        {
                            var sr = new StreamReader(rs);
                            resultSb.Append($"{group}: {await sr.ReadToEndAsync()}\n");
                        }
                    }
                }
                catch (WebException ex)
                {
                    MessageBox.Show($"组 {group} 推荐失败：{ex}");
                }
            }
            MessageBox.Show($"推荐完成，结果如下：\n{resultSb}");
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            webBrowser.Size = new Size(Size.Width - panel1.Size.Width - SystemInformation.VerticalScrollBarWidth,
                webBrowser.Size.Height);
        }

        private void contextTextBox_TextChanged(object sender, EventArgs e)
        {
            publishButton.Text = $"发布（{152 - contextTextBox.Text.Length}）";
        }

        private async void friendStartButton_Click(object sender, EventArgs e)
        {
            friendStartButton.Enabled = false;
            var sessionId = GetSessionId();
            var resultSb = new StringBuilder();
            var friendCount = 0;
            for (var i = 1; i <= 10; i++)
            {
                friendStartButton.Text = i.ToString();
                try
                {
                    var request = GenerateRequest(
                        $"http://steamcommunity.com/app/{friendAppIdTextBox.Text}/homecontent/?userreviewsoffset={(i - 1)*10}&p={i}&browsefilter=toprated&l=english&appHubSubSection=10&filterLanguage=schinese",
                        "GET",
                        $"http://steamcommunity.com/app/{friendAppIdTextBox.Text}/reviews/?browsefilter=toprated&filterLanguage=schinese&p=1");
                    using (var response = await request.GetResponseAsync())
                    {
                        var rs = response.GetResponseStream();
                        if (rs == null) continue;
                        var dom = CQ.Create(rs);
                        var cards = dom[".apphub_Card"];
                        if (cards.Length == 0) break;
                        foreach (var cardDom in cards)
                        {
                            var userUrl = cardDom.Cq().Find(".apphub_CardContentAuthorName a").Attr("href");
                            if (string.IsNullOrWhiteSpace(userUrl)) continue;
                            try
                            {
                                var helpfulMatch = Regex.Match(cardDom.Cq().Find(".found_helpful").Text(),
                                    @"([\d,]+) of [\d,]+ people \((\d+)%\)");

                                if (!helpfulMatch.Success) continue;
                                int helpfulPeople;
                                if (!int.TryParse(helpfulMatch.Groups[1].Value, NumberStyles.AllowThousands,
                                    new NumberFormatInfo(), out helpfulPeople) || helpfulPeople < 5)
                                    continue;

                                int helpfulPercent;
                                if (!int.TryParse(helpfulMatch.Groups[2].Value, out helpfulPercent) ||
                                    helpfulPercent < 80)
                                    continue;

                                if (cardDom.Cq().Find(".apphub_CardTextContent").Text().Length < 300)
                                    continue;

                                var hoursMatch = Regex.Match(cardDom.Cq().Find(".hours").Text(), @"([\d.,]+) hrs");
                                double hours;
                                if (!double.TryParse(hoursMatch.Groups[1].Value, out hours) || hours < 3)
                                    continue;

                                if (cardDom.Cq().Find(".online, .in-game") == null)
                                    continue;

                                var userRequest = GenerateRequest(userUrl, "GET", "http://steamcommunity.com/");
                                using (var userResponse = await userRequest.GetResponseAsync())
                                {
                                    var urs = userResponse.GetResponseStream();
                                    if (urs == null) continue;
                                    using (var sr = new StreamReader(urs))
                                    {
                                        var html = await sr.ReadToEndAsync();
                                        var idMatch = Regex.Match(html, @"""steamid"":""(\d+)""");
                                        if (!idMatch.Success) continue;
                                        var steamId64 = idMatch.Groups[1].Value;
                                        var isKeylolUser = await _httpClient.GetAsync(
                                                $"https://api.keylol.com/user/enhanced-steam?steamId64={steamId64}");
                                        if (isKeylolUser.IsSuccessStatusCode)
                                            continue;
                                        var addFriendRequest =
                                            GenerateRequest("http://steamcommunity.com/actions/AddFriendAjax", "POST",
                                                userUrl, "http://steamcommunity.com");
                                        var postData = new StringBuilder();
                                        AppendUrlEncoded(postData, "sessionID", sessionId);
                                        AppendUrlEncoded(postData, "steamid", steamId64);
                                        AppendUrlEncoded(postData, "accept_invite", "0", false);
                                        using (var requestStream = await addFriendRequest.GetRequestStreamAsync())
                                        {
                                            var data = Encoding.UTF8.GetBytes(postData.ToString());
                                            await requestStream.WriteAsync(data, 0, data.Length);
                                        }
                                        using (var addFriendResponse = await addFriendRequest.GetResponseAsync())
                                        {
                                            var addFriendResponseStream = addFriendResponse.GetResponseStream();
                                            if (addFriendResponseStream != null)
                                            {
                                                var afsr = new StreamReader(addFriendResponseStream);
                                                friendCount++;
                                                resultSb.Append($"{await afsr.ReadToEndAsync()}\n");
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                friendCount++;
                                resultSb.Append($"添加 {userUrl} 失败。\n");
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    resultSb.Append($"翻页到第 {i} 页失败。\n");
                }
            }
            MessageBox.Show($"完成，共尝试添加好友 {friendCount} 人，结果如下：\n{resultSb}");
            friendStartButton.Text = @"启动";
            friendStartButton.Enabled = true;
        }

        private HttpWebRequest GenerateRequest(string url, string method, string referer, string origin = null)
        {
            var request = WebRequest.CreateHttp(url);
            request.Method = method;
            var cookieContainer = GetUriCookieContainer(new Uri("http://steamcommunity.com/"));
            request.CookieContainer = cookieContainer;
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            request.Accept = "*/*";
            request.Headers.Set("Accept-Language", "en-US,en;q=0.8,zh-CN;q=0.6,zh;q=0.4");
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.112 Safari/537.36";
            request.Headers.Set("X-Requested-With", "XMLHttpRequest");
            request.Referer = referer;
            if (origin != null)
                request.Headers.Set("Origin", origin);
            return request;
        }

        private string GetSessionId()
        {
            var cookieContainer = GetUriCookieContainer(new Uri("http://steamcommunity.com/"));
            var cookies = cookieContainer.GetCookies(new Uri("http://steamcommunity.com/"));
            var cookieCount = cookies.Count;
            for (var i = 0; i < cookieCount; i++)
            {
                if (cookies[i].Name == "sessionid")
                    return cookies[i].Value;
            }
            return string.Empty;
        }
    }
}