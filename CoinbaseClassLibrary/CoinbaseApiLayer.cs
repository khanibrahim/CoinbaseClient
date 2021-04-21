using CoinbaseClassLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace CoinbaseClientLibrary
{
    public class CoinbaseApiLayer
    {

        public ReturnValue<List<Product>> GetProducts()
        {
            ReturnValue<List<Product>> returnValue = new ReturnValue<List<Product>>();
            List<Product> products = new List<Product>();
            try
            {
                var result = CommonFunctions.Query("GET", "/products");
                products = JsonConvert.DeserializeObject<List<Product>>(result);
                products = products.Where(x => x.Id == "BTC-USD").ToList();
                returnValue.status = "Success";
                returnValue.value = products;
                return returnValue;
            }
            catch (Exception e)
            {
                CommonFunctions.WriteLog(e.Message);
                returnValue.status = "Error";
                returnValue.message = "Some Error Occured";
                return returnValue;

            }
        }

        public ReturnValue<List<Order>> GetOrders()
        {
            ReturnValue<List<Order>> returnValue = new ReturnValue<List<Order>>();
            List<Order> orders = new List<Order>();
            try
            {
                var result = CommonFunctions.Query("GET", "/orders");
                orders = JsonConvert.DeserializeObject<List<Order>>(result);
                orders = orders.Where(x => x.ProductId == "BTC-USD").ToList();
                returnValue.status = "Success";
                returnValue.value = orders;
                return returnValue;
            }
            catch (Exception e)
            {
                CommonFunctions.WriteLog(e.Message);
                returnValue.status = "Error";
                returnValue.message = "Some Error Occured";
                return returnValue;
            }

        }

        public ReturnValue<string> PlaceOrder(string type, string side, string productId, string size, string price)
        {
            ReturnValue<string> returnValue = new ReturnValue<string>();
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
            try
            {
                var result = CommonFunctions.Query("POST", "/orders", param);
                var order = JsonConvert.DeserializeObject<Order>(result);

                returnValue.status = "Success";
                returnValue.value = order.Id;
                return returnValue;
            }
            catch (Exception e)
            {
                CommonFunctions.WriteLog(e.Message);
                returnValue.status = "Error";
                returnValue.message = "Order Could Not Be Placed";
                return returnValue;
            }

        }

        public ReturnValue<string> GetOrderStatus(string orderId)
        {
            ReturnValue<string> returnValue = new ReturnValue<string>();

            try
            {
                var result = CommonFunctions.Query("GET", $"/orders/{orderId}");
                var order = JsonConvert.DeserializeObject<Order>(result);
                returnValue.value = order.Status;
                return returnValue;

            }
            catch (Exception e)
            {

                CommonFunctions.WriteLog(e.Message);
                returnValue.status = "Error";
                returnValue.message = "Some Error Occured, Try again later";
                return returnValue;
            }

        }

        public ReturnValue<string> GetPosition()
        {
            ReturnValue<string> returnValue = new ReturnValue<string>();
            try
            {
                var result = CommonFunctions.Query("GET", $"/accounts");
                var accounts = JsonConvert.DeserializeObject<List<Account>>(result);
                returnValue.value = accounts.Where(x => x.Currency == "BTC").Select(x => (float)x.Balance + ":" + x.Currency + " \n ").Aggregate((x1, x2) => x1 + x2);
                returnValue.status = "Success";
                return returnValue;
            }
            catch (Exception e)
            {

                CommonFunctions.WriteLog(e.Message);
                returnValue.status = "Error";
                returnValue.message = "Some Error Occured, Try again later";
                return returnValue;
            }

        }

        public ReturnValue<string> GetMargin()
        {
            ReturnValue<string> returnValue = new ReturnValue<string>();
            try
            {
                var result = CommonFunctions.Query("GET", $"/accounts");
                var accounts = JsonConvert.DeserializeObject<List<Account>>(result);
                returnValue.status = "Success";
                returnValue.value = accounts.Where(x => x.Currency == "USD").Select(x => (int)x.Balance + ":" + x.Currency + " \n ").FirstOrDefault().ToString();
                return returnValue;
            }
            catch (Exception e)
            {

                CommonFunctions.WriteLog(e.Message);
                returnValue.status = "Error";
                returnValue.message = "Some Error Occured, Try again later";
                return returnValue;
            }

        }

        public ReturnValue<string> CancelOrder(string orderId)
        {
            ReturnValue<string> returnValue = new ReturnValue<string>();
            try
            {
                CommonFunctions.Query("DELETE", $"/orders/{orderId}");
                returnValue.message = "Cancelled Successfully";
                returnValue.status = "Success";
                return returnValue;
            }
            catch (Exception e)
            {
                CommonFunctions.WriteLog(e.Message);
                returnValue.status = "Error";
                returnValue.message = "Some Error Occured, Try again later";
                return returnValue;
            }
        }

        public ReturnValue<string> SquareOff()
        {
            ReturnValue<string> returnValue = new ReturnValue<string>();

            var result = CommonFunctions.Query("GET", $"/accounts");
            var accounts = JsonConvert.DeserializeObject<List<Account>>(result).Where(x => x.Currency == "BTC" && x.Balance != 0);
            try
            {
                foreach (var account in accounts)
                {
                    this.PlaceOrder("market", account.Balance > 0 ? "sell" : "buy", "BTC-USD", account.Balance.ToString(), "");
                }
                returnValue.status = "Success";
                returnValue.message = "Sqare off done successfully";
                return returnValue;
            }
            catch (Exception e)
            {
                CommonFunctions.WriteLog(e.Message);
                returnValue.status = "Error";
                returnValue.message = "Some Error Occured, Try again later";
                return returnValue;
            }

        }

    }

}
