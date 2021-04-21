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
using System.Configuration;

namespace CoinbaseClient
{
    public partial class MainWindow : Window
    {
        private static List<string> History = new List<string>();
        readonly CoinbaseApiLayer CAL = new CoinbaseApiLayer();

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

        private void GetProducts()
        {
            ReturnValue<List<Product>> returnValue = CAL.GetProducts();
            if (returnValue.status == "Success")
            {
                productIds.ItemsSource = returnValue.value.Select(x => x.Id).ToList();
            }
            else
            {
                History.Add(returnValue.message);
            }
        }

        private void GetOrders()
        {
            ReturnValue<List<Order>> returnValue = CAL.GetOrders();

            if (returnValue.status == "Success")
            {
                cancelOrder.SelectedValuePath = "Key";
                cancelOrder.DisplayMemberPath = "Value";
                var cancelOrderList = returnValue.value.Where(x => x.Status != "done" && x.Status != "rejected").Select(x => new KeyValuePair<string, string>(x.Id, x.ProductId + " :: Price: " + x.Price + " :: Size: " + x.Size + " :: Type: " + x.Type + " :: Side: " + x.Side)).ToList();
                cancelOrder.ItemsSource = cancelOrderList;

                getOrderStatus.SelectedValuePath = "Key";
                getOrderStatus.DisplayMemberPath = "Value";
                var orderStatusList = returnValue.value.Select(x => new KeyValuePair<string, string>(x.Id, x.ProductId + " :: Price: " + x.Price + " :: Size: " + x.Size + " :: Type: " + x.Type + " :: Side: " + x.Side)).ToList();
                getOrderStatus.ItemsSource = orderStatusList;
            }
            else
            {
                History.Add(returnValue.message);
            }
        }

        private void Place_Order(object sender, RoutedEventArgs e)
        {
            if (validateInput("Order"))
            {
                ReturnValue<string> returnValue = CAL.PlaceOrder(limit.IsChecked == true ? "limit" : "market", buy.IsChecked == true ? "buy" : "sell", productIds.Text, size.Text, price.Text);
                if (returnValue.status == "Error")
                {
                    History.Add(returnValue.message);
                }
                else
                {
                    History.Add($"Action: Order placed successfully with order ID \n {returnValue.value}");
                }
                notificationPanle.Content = History.Skip(Math.Max(0, History.Count() - 6)).Aggregate((x, y) => x + "\n" + y).ToString(); ;
                GetOrders();
            }
            else
            {
                MessageBox.Show("Fill All Fields");
            }
        }

        private void getOrderStatusButton_Click(object sender, RoutedEventArgs e)
        {
            KeyValuePair<string, string> selectedEntry = (KeyValuePair<string, string>)getOrderStatus.SelectedItem;
            ReturnValue<string> returnValue = CAL.GetOrderStatus(selectedEntry.Key);
            if (returnValue.status == "Error")
            {
                MessageBox.Show(returnValue.message);
            }
            else
            {
                MessageBox.Show($"Order Status Is : {returnValue.value}");
            }
        }

        private void cancelOrderButton_Click(object sender, RoutedEventArgs e)
        {
            KeyValuePair<string, string> selectedEntry = (KeyValuePair<string, string>)cancelOrder.SelectedItem;
            ReturnValue<string> returnValue = CAL.CancelOrder(selectedEntry.Key);
            if (returnValue.status == "Error")
            {
                MessageBox.Show(returnValue.message);
            }
            else
            {
                MessageBox.Show(returnValue.message);
            }
            GetOrders();
        }

        private void FetNetPosition_Click(object sender, RoutedEventArgs e)
        {

            ReturnValue<string> returnValue = CAL.GetPosition();
            if (returnValue.status == "Error")
            {
                MessageBox.Show(returnValue.message);
            }
            else
            {
                MessageBox.Show(returnValue.value);
            }
        }

        private void GetAvailableMargin_Click(object sender, RoutedEventArgs e)
        {

            ReturnValue<string> returnValuee = CAL.GetMargin();
            if (returnValuee.status == "Error")
            {
                MessageBox.Show(returnValuee.message);
            }
            else
            {
                MessageBox.Show(returnValuee.value);
            }
        }

        private void SquareOffPosition_Click(object sender, RoutedEventArgs e)
        {
            ReturnValue<string> returnValue = CAL.SquareOff();
            MessageBox.Show(returnValue.message);
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
            // realizedUnrealized.Content = CAL.GetRealiedUnrealized();
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

    }


}
