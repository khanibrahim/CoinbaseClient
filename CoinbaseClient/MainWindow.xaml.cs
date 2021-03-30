using System.Text.RegularExpressions;
using CoinbaseClassLibrary;
using CoinbaseClientLibrary;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Configuration;

namespace CoinbaseClient
{
    public partial class MainWindow : Window
    {


        private static readonly string bitmexKey = ConfigurationManager.AppSettings["bitmexKey"];
        private static readonly string bitmexSecret = ConfigurationManager.AppSettings["bitmexSecret"];

        private static List<string> History = new List<string>();
        readonly CoinbaseApiLayer CAL = new CoinbaseApiLayer(bitmexKey, bitmexSecret);

        public MainWindow()
        {
            InitializeComponent();
            InitiateData();

            var interval = TimeSpan.FromSeconds(3);
            RunPeriodicAsync(OnTick, interval);

        }

        private void InitiateData()
        {
            GetProducts();
            GetOrders();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void GetProducts()
        {
            List<Product> products = CAL.GetProducts();
            productIds.ItemsSource = products.Select(x => x.Id).ToList();
        }

        private void GetOrders()
        {
            List<Order> orders = CAL.GetOrders();
            cancelOrder.SelectedValuePath = "Key";
            cancelOrder.DisplayMemberPath = "Value";
            getOrderStatus.SelectedValuePath = "Key";
            getOrderStatus.DisplayMemberPath = "Value";
            getOrderStatus.ItemsSource = orders.Select(x => new KeyValuePair<string, string>(x.Id, x.ProductId + " :: Price:" + x.Price + " :: Size:" + x.Size + " :: Type:" + x.Type + " :: Side:" + x.Side + " :: Time:" + x.CreatedAt)).ToList(); ;
            cancelOrder.ItemsSource = orders.Where(x => x.Status != "done" && x.Status != "rejected").Select(x => new KeyValuePair<string, string>(x.Id, x.ProductId + " :: Price:" + x.Price + " :: Size:" + x.Size + " :: Type:" + x.Type + " :: Side:" + x.Side + " :: Time:" + x.CreatedAt)).ToList(); ;
        }

        private void Place_Order(object sender, RoutedEventArgs e)
        {
            if (validateInput("Order"))
            {
                try
                {
                    string orderid = CAL.PlaceOrder(limit.IsChecked == true ? "limit" : "market", buy.IsChecked == true ? "buy" : "sell", productIds.Text, size.Text, price.Text);
                    if (!string.IsNullOrEmpty(orderid))
                    {
                        History.Add($"Last Action: Order placed successfully with order ID \n {orderid}");
                    }
                    else
                    {
                        History.Add($"Last Action: Order could not be placed");
                    }
                    notificationPanle.Content = History.Skip(Math.Max(0, History.Count() - 6)).Aggregate((x, y) => x + "\n" + y).ToString(); ;
                    GetOrders();
                }
                catch
                {
                    MessageBox.Show("Request Failed");
                }
            }
            else
            {
                MessageBox.Show("Fill All Fields");
            }
        }

        private bool validateInput(string action)
        {
            if (action == "Order")
            {
                if (string.IsNullOrEmpty(productIds.Text) || (limit.IsChecked == false && market.IsChecked == false) || (buy.IsChecked == false && sell.IsChecked == false))
                    return false;

                if (limit.IsChecked == true && (string.IsNullOrEmpty(price.Text) || string.IsNullOrEmpty(size.Text)))
                    return false;

                if (market.IsChecked == true && (string.IsNullOrEmpty(price.Text) && string.IsNullOrEmpty(size.Text)))
                    return false;
            }
            return true;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[.][0-9]+$|^[0-9]*[.]{0,1}[0-9]*$");
            e.Handled = !regex.IsMatch((sender as TextBox).Text.Insert((sender as TextBox).SelectionStart, e.Text));
        }

        private void getOrderStatusButton_Click(object sender, RoutedEventArgs e)
        {
            KeyValuePair<string, string> selectedEntry = (KeyValuePair<string, string>)getOrderStatus.SelectedItem;
            MessageBox.Show($"Order Status Is : {CAL.GetOrderStatus(selectedEntry.Key)}");
        }

        private void cancelOrderButton_Click(object sender, RoutedEventArgs e)
        {
            KeyValuePair<string, string> selectedEntry = (KeyValuePair<string, string>)cancelOrder.SelectedItem;
            MessageBox.Show(CAL.CancelOrder(selectedEntry.Key));
            GetOrders();
        }

        private void FetNetPosition_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(CAL.GetPosition());
        }

        private void GetAvailableMargin_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(CAL.GetMargin());
        }

        private void SquareOffPosition_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(CAL.SquareOff());
        }

        private static async Task RunPeriodicAsync(Action onTick, TimeSpan interval)
        {
            while (true)
            {
                onTick?.Invoke();

                if (interval > TimeSpan.Zero)
                    await Task.Delay(interval);
            }
        }



        private void OnTick()
        {
            realizedUnrealized.Content = CAL.RealizedUnrealized();
        }

        private void price_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (market.IsChecked == true)
            {
                size.Text = null;
            }
        }

        private void size_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (market.IsChecked == true)
            {
                price.Text = null;
            }
        }
    }


}
