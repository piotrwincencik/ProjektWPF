using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace ProjektWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        record Rate(string Currency, string Code, decimal Ask, decimal Bid);
        Dictionary<string, Rate> Rates = new Dictionary<string, Rate>();

        private void DonwloadData()
        {
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-En");
            WebClient client = new WebClient();
            client.Headers.Add("Accept", "application/xml");
            string xml = client.DownloadString("http://api.nbp.pl/api/exchangerates/tables/C");
            XDocument doc = XDocument.Parse(xml);
            List<Rate> list = doc
                .Elements("ArrayOfExchangeRatesTable")
                .Elements("ExchangeRatesTable")
                .Elements("Rates")
                .Elements("Rate")
                .Select(n =>
                    new Rate(
                        n.Element("Currenct").Value,
                        n.Element("Code").Value,
                        decimal.Parse(n.Element("Ask").Value),
                        decimal.Parse(n.Element("Bid").Value)
                        )
                    ).ToList();
            foreach(Rate rate in list)
            {
                Rates.Add(rate.Code, rate);
            }
        }





        public MainWindow()
        {
            InitializeComponent();
            DonwloadData();
            foreach(string code in Rates.Keys)
            {
                InputCurrencyCode.Items.Add(code);
                OutputCurrencyCode.Items.Add(code);
            }

            InputCurrencyCode.SelectedIndex = 0;
            OutputCurrencyCode.SelectedIndex = 1;


        }

        private void CalcOutput(object sender, RoutedEventArgs e)
        {
            string inputCode = (string)InputCurrencyCode.SelectedItem;
            string outputCode = (string)OutputCurrencyCode.SelectedItem;

            string amountStr = InputValue.Text;
            decimal amount = decimal.Parse(amountStr);

            Rate inputRate = Rates[inputCode];
            Rate outputRate = Rates[outputCode];
            decimal output = amount * inputRate.Ask / outputRate.Ask;

            OutputValue.Text = output.ToString();
        }

        private void NumberValidation(object sender, TextCompositionEventArgs e)
        {
            string input = e.Text;
            if (input.EndsWith(","))
            {
                input += "0";
            }
            e.Handled = !decimal.TryParse(input, out decimal values);
        }
    }
}
