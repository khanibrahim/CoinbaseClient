﻿using CoinbaseClassLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CoinbaseClientLibrary
{
    public class CoinbaseApiLayer
    {

        private const string domain = "https://api-public.sandbox.pro.coinbase.com";
        private string apiKey;
        private string apiSecret;
        private List<Product> products;

        public CoinbaseApiLayer(string bitmexKey = "", string bitmexSecret = "")
        {
            this.apiKey = bitmexKey;
            this.apiSecret = bitmexSecret;
        }


        public List<Product> GetProducts()
        {
            var result = Query("GET", "/products");
            this.products = JsonConvert.DeserializeObject<List<Product>>(result);
            return products;
        }

        public List<Order> GetOrders()
        {
            var result = Query("GET", "/orders?status=all");
            var orders = JsonConvert.DeserializeObject<List<Order>>(result);
            return orders;
        }

        public string PlaceOrder(string type, string side, string productId, string size, string price)
        {

            Dictionary<string, string> param = new Dictionary<string, string>();

            param["type"] = type;
            param["side"] = side;
            param["product_id"] = productId;

            if (!string.IsNullOrEmpty(size))
            {
                param["size"] = size;
            }

            if (!string.IsNullOrEmpty(price))
            {
                if (type == "limit")
                {
                    param["price"] = price;
                }
                else
                {
                    param["funds"] = price;
                }
            }
            var result = Query("POST", "/orders", param);
            var order = JsonConvert.DeserializeObject<Order>(result);
            return order.Id;
        }

        public string GetOrderStatus(string orderId)
        {
            var result = Query("GET", $"/orders/{orderId}");
            var order = JsonConvert.DeserializeObject<Order>(result);
            return order.Status;
        }

        public string GetPosition()
        {
            var result = Query("GET", $"/accounts");
            var accounts = JsonConvert.DeserializeObject<List<Account>>(result);
            return accounts.Where(x => x.Currency != "USD").Select(x => (int)x.Balance + ":" + x.Currency + " \n ").Aggregate((x1, x2) => x1 + x2);
        }

        public string GetMargin()
        {
            var result = Query("GET", $"/accounts");
            var accounts = JsonConvert.DeserializeObject<List<Account>>(result);
            return accounts.Where(x => x.Currency == "USD").Select(x => (int)x.Balance + ":" + x.Currency + " \n ").FirstOrDefault().ToString();
        }

        public string CancelOrder(string orderId)
        {
            try
            {
                Query("DELETE", $"/orders/{orderId}");
            }
            catch
            {
                return "Error Occured";
            }
            return "Cancelled Sucessfully";
        }

        public string RealizedUnrealized()
        {
            return DateTime.Now.ToString();
        }

        public string SquareOff()
        {

            var result = Query("GET", $"/accounts");
            var accounts = JsonConvert.DeserializeObject<List<Account>>(result).Where(x => x.Currency != "USD" && x.Balance != 0);
            try
            {
                foreach (var account in accounts)
                {
                    this.PlaceOrder(account.Balance > 0 ? "Sell" : "Buy", "Market", account.Id, "0", account.Balance.ToString());
                }

                return "Success";
            }
            catch (Exception)
            {

                return "Error";
            }

        }

        #region Basic Functionality


        private string Query(string method, string function, Dictionary<string, string> postParameters = null)
        {
            //string paramData = json ? BuildJSON(postParameters) : BuildQueryData(postParameters);
            string url = domain + function + ((method == "GET" && postParameters != null) ? "?" + BuildQueryData(postParameters) : "");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = method;
            // string body = "{\"price\": \"1.0\", \"size\": \"1.0\", \"side\": \"buy\", \"product_id\": \"BTC-USD\"}";
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
                //if (postData != "")
                //{
                //    webRequest.ContentType = json ? "application/json" : "application/x-www-form-urlencoded";
                //    var data = Encoding.UTF8.GetBytes(postData);
                //    using (var stream = webRequest.GetRequestStream())
                //    {
                //        stream.Write(data, 0, data.Length);
                //    }
                //}

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
            // var hmacKey = Encoding.UTF8.GetBytes(key);
            byte[] hmacKey = Convert.FromBase64String(key);

            var dataBytes = Encoding.UTF8.GetBytes(data);

            using (var hmac = new HMACSHA256(hmacKey))
            {
                var sig = hmac.ComputeHash(dataBytes);
                // return ByteToHexString(sig);
                return Convert.ToBase64String(sig);
            }
        }

        private long GetExpires()
        {
            return (long)((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);
        }

        private string BuildQueryData(Dictionary<string, string> param)
        {
            if (param == null)
                return "";

            StringBuilder b = new StringBuilder();
            foreach (var item in param)
                b.Append(string.Format("&{0}={1}", item.Key, WebUtility.UrlEncode(item.Value)));

            try { return b.ToString().Substring(1); }
            catch (Exception) { return ""; }
        }

        private string BuildJSON(Dictionary<string, string> param)
        {
            if (param == null)
                return "";

            var entries = new List<string>();
            foreach (var item in param)
                entries.Add(string.Format("\"{0}\":\"{1}\"", item.Key, item.Value));

            return "{" + string.Join(",", entries) + "}";
        }


        #endregion

    }

}