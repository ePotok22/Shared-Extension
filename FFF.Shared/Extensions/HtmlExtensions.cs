using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace FFF.Shared
{
    public static class HtmlExtensions
    {
        private readonly static Regex _regDomain = new Regex(@"(((?<scheme>http(s)?):\/\/)?([\w-]+?\.\w+)+([a-zA-Z0-9\~\!\@\#\$\%\^\&amp;\*\(\)_\-\=\+\\\/\?\.\:\;\,]*)?)", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex _regStripHtml = new Regex(@"</?.+?>");
        private static readonly Regex _regUrl = new Regex(@"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?");

        // Used when we want to completely remove HTML code and not encode it with XML entities.
        public static string StripHtml(this string input) =>
            _regStripHtml.Replace(input, " ");

        public static bool IsValidUrl(this string text) =>
            _regUrl.IsMatch(text);

        public static string ToUrlString(this string str)
        {
            if (String.IsNullOrEmpty(str)) return "";
            // Unicode Character Handling: http://blogs.msdn.com/b/michkap/archive/2007/05/14/2629747.aspx
            string stFormD = str.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            foreach (char t in
               from t in stFormD
               let uc = CharUnicodeInfo.GetUnicodeCategory(t)
               where uc != UnicodeCategory.NonSpacingMark
               select t)
            {
                sb.Append(t);
            }
            return Regex.Replace(sb.ToString().Normalize(NormalizationForm.FormC), "[\\W\\s]{1,}", "-").Trim('-');
        }

        public static string ToHtmlEncode(this string data) =>
            HttpUtility.HtmlEncode(data);

        public static string ToHtmlDecode(this string data) =>
            HttpUtility.HtmlDecode(data);

        public static NameValueCollection ParseQueryString(this string query) =>
            HttpUtility.ParseQueryString(query);

        public static string ToUrlEncode(this string url) =>
            HttpUtility.UrlEncode(url);

        public static string ToUrlDecode(this string url) =>
            HttpUtility.UrlDecode(url);

        public static string ToUrlPathEncode(this string url) =>
            HttpUtility.UrlPathEncode(url);

        public static string Linkify(this string text, string target = "_self")
        {
            return _regDomain.Replace(text,
                match =>
                {
                    string link = match.ToString();
                    string scheme = match.Groups["scheme"].Value == "https" ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
                    string url = new UriBuilder(link) { Scheme = scheme }.Uri.ToString();
                    return string.Format(@"<a href=""{0}"" target=""{1}"">{2}</a>", url, target, link);
                }
            );
        }

        public static string Nl2Br(this string s) =>
            s.Replace("\r\n", "<br />").Replace("\n", "<br />");

    }
}
