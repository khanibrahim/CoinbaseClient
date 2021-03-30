using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
namespace CoinbaseClassLibrary
{
    public partial class Product
    {
        [JsonProperty("base_currency")]
        public string BaseCurrency { get; set; }
        [JsonProperty("base_max_size")]
        public decimal BaseMaxSize { get; set; }
        [JsonProperty("base_min_size")]
        public decimal BaseMinSize { get; set; }
        [JsonProperty("cancel_only")]
        public bool CancelOnly { get; set; }
        [JsonProperty("display_name")]
        public string DisplayName { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("limit_only")]
        public bool LimitOnly { get; set; }
        [JsonProperty("margin_enabled")]
        public bool MarginEnabled { get; set; }
        [JsonProperty("max_market_funds")]
        public decimal? MaxMarketFunds { get; set; }
        [JsonProperty("min_market_funds")]
        public decimal? MinMarketFunds { get; set; }
        [JsonProperty("post_only")]
        public bool PostOnly { get; set; }
        [JsonProperty("trading_disabled")]
        public bool TradingDisabled { get; set; }
        [JsonProperty("quote_currency")]
        public string QuoteCurrency { get; set; }
        [JsonProperty("quote_increment")]
        public decimal QuoteIncrement { get; set; }
        [JsonProperty("base_increment")]
        public decimal BaseIncrement { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("status_message")]
        public object StatusMessage { get; set; }
    }
    public partial class PositionRefreshmentAmount
    {
        [JsonProperty("oneDayRenewalAmount")]
        public string OneDayRenewalAmount { get; set; }
        [JsonProperty("twoDayRenewalAmount")]
        public string TwoDayRenewalAmount { get; set; }
    }
    public partial class Order
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("price")]
        public decimal Price { get; set; }
        [JsonProperty("size")]
        public decimal Size { get; set; }
        [JsonProperty("product_id")]
        public string ProductId { get; set; }
        [JsonProperty("side")]
        public OrderSide Side { get; set; }
        [JsonProperty("stp")]
        public SelfTradePrevention Stp { get; set; }
        [JsonProperty("type")]
        public OrderType Type { get; set; }
        [JsonProperty("time_in_force")]
        public TimeInForce TimeInForce { get; set; }
        [JsonProperty("post_only")]
        public bool PostOnly { get; set; }
        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }
        [JsonProperty("fill_fees")]
        public decimal FillFees { get; set; }
        [JsonProperty("filled_size")]
        public decimal FilledSize { get; set; }
        [JsonProperty("executed_value")]
        public decimal ExecutedValue { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("settled")]
        public bool Settled { get; set; }
        [JsonProperty("funds", NullValueHandling = NullValueHandling.Ignore)]
        public decimal Funds { get; set; }
        [JsonProperty("specified_funds", NullValueHandling = NullValueHandling.Ignore)]
        public decimal SpecifiedFunds { get; set; }
        [JsonProperty("done_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? DoneAt { get; set; }
        [JsonProperty("done_reason", NullValueHandling = NullValueHandling.Ignore)]
        public string DoneReason { get; set; }
    }
    public enum OrderSide
    {
        [EnumMember(Value = "buy")]
        Buy,
        [EnumMember(Value = "sell")]
        Sell
    }
    public enum TimeInForce
    {
        [EnumMember(Value = "GTC")]
        GoodTillCanceled,
        [EnumMember(Value = "GTT")]
        GoodTillTime,
        [EnumMember(Value = "IOC")]
        ImmediateOrCancel,
        [EnumMember(Value = "FOK")]
        FillOrKill
    }
    public partial class Account
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("currency")]
        public string Currency { get; set; }
        [JsonProperty("balance")]
        public decimal Balance { get; set; }
        [JsonProperty("available")]
        public decimal Available { get; set; }
        [JsonProperty("hold")]
        public decimal Hold { get; set; }
        [JsonProperty("profile_id")]
        public string ProfileId { get; set; }
    }
    public enum SelfTradePrevention
    {
        [EnumMember(Value = "dc")]
        DecreaseAndCancel,
        [EnumMember(Value = "co")]
        CancelOldest,
        [EnumMember(Value = "cn")]
        CancelNewest,
        [EnumMember(Value = "cb")]
        CancelBoth
    }
    public enum OrderType
    {
        [EnumMember(Value = "limit")]
        Limit,
        [EnumMember(Value = "market")]
        Market
    }
}
