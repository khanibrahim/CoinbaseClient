using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CoinbaseClassLibrary
{
    static class CommonFunctions
    {

        private static readonly string baseURL = ConfigurationManager.AppSettings["baseURL"];
        private static readonly string logPath = ConfigurationManager.AppSettings["logPath"];
        private static readonly string apiKey = ConfigurationManager.AppSettings["bitmexKey"];
        private static readonly string apiSecret = ConfigurationManager.AppSettings["bitmexSecret"];


        internal static string Query(string method, string function, Dictionary<string, string> postParameters = null)
        {
            string url = baseURL + function + ((method == "GET" && postParameters != null) ? "?" + BuildQueryData(postParameters) : "");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = method;
            string body = method == "GET" ? null : BuildJSON(postParameters);
            string expires = GetExpires().ToString();
            var message = $"{expires}{method}{function}{body}";
            string signature = GetHMACInHex(apiSecret, message);
            webRequest.Headers.Add("CB-ACCESS-KEY", apiKey);
            webRequest.Headers.Add("CB-ACCESS-TIMESTAMP", expires);
            webRequest.Headers.Add("CB-ACCESS-SIGN", signature);
            webRequest.Headers.Add("CB-VERSION", "2017-08-07");
            webRequest.Headers.Add("CB-ACCESS-PASSPHRASE", "00000000");
            webRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";

            try
            {
                if (method == "POST")
                {

                    string postData = "";

                    foreach (string key in postParameters.Keys)
                    {
                        postData += HttpUtility.UrlEncode(key) + "="
                              + HttpUtility.UrlEncode(postParameters[key]) + "&";
                    }

                    byte[] data = Encoding.ASCII.GetBytes(body);

                    webRequest.ContentType = "application/json";
                    webRequest.ContentLength = data.Length;

                    Stream requestStream = webRequest.GetRequestStream();
                    requestStream.Write(data, 0, data.Length);
                    requestStream.Close();
                }
                using (WebResponse webResponse = webRequest.GetResponse())
                using (Stream str = webResponse.GetResponseStream())
                using (StreamReader sr = new StreamReader(str))
                {
                    var result = sr.ReadToEnd();
                    return result;
                }

            }
            catch (WebException wex)
            {
                using (HttpWebResponse response = (HttpWebResponse)wex.Response)
                {
                    if (response == null)
                        throw;

                    using (Stream str = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(str))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }

        internal static string GetHMACInHex(string key, string data)
        {
            byte[] hmacKey = Convert.FromBase64String(key);

            var dataBytes = Encoding.UTF8.GetBytes(data);

            using (var hmac = new HMACSHA256(hmacKey))
            {
                var sig = hmac.ComputeHash(dataBytes);
                return Convert.ToBase64String(sig);
            }
        }

        internal static long GetExpires()
        {
            return (long)((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);
        }

        internal static string BuildQueryData(Dictionary<string, string> param)
        {
            if (param == null)
            {
                return "";
            }
            StringBuilder b = new StringBuilder();
            foreach (var item in param)
            {
                b.Append(string.Format("&{0}={1}", item.Key, WebUtility.UrlEncode(item.Value)));
            }
            try
            {
                return b.ToString().Substring(1);
            }
            catch (Exception)
            {
                return "";
            }
        }

        internal static string BuildJSON(Dictionary<string, string> param)
        {
            if (param == null)
                return "";

            var entries = new List<string>();
            foreach (var item in param)
                entries.Add(string.Format("\"{0}\":\"{1}\"", item.Key, item.Value));

            return "{" + string.Join(",", entries) + "}";
        }

        internal static void WriteLog(string strMessage)
        {
            FileStream objFilestream = new FileStream(logPath, FileMode.Append, FileAccess.Write);
            StreamWriter objStreamWriter = new StreamWriter((Stream)objFilestream);
            objStreamWriter.WriteLine(strMessage);
            objStreamWriter.Close();
            objFilestream.Close();
        }

    }
}
