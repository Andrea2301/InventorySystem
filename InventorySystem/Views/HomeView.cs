using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Definitions.Charts;

namespace InventorySystem.Views
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
            PointLabel = chartPoint => $"{chartPoint.Y}({chartPoint.Participation:P})";
            DataContext = this;





            SaleSeries = new LineSeries
            {
                Title = "Sales", // Título de la serie de ventas
                Values = new ChartValues<double> { 10, 20, 30, 40 },// Datos de ventas (ejemplo)
                LineSmoothness = 0,
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#323d76")),
                PointGeometry = null,
                
                
  
            };

            StockSeries = new LineSeries
            {
                Title = "Stock", // Título de la serie de stock
                Values = new ChartValues<double> { 100, 90, 80, 70 }, // Datos de stock (ejemplo)
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2b96b9")),
                Width = 10,
                LineSmoothness = 0,
                PointGeometry = null

            };


        }


        //cartesin chart

        public LineSeries SaleSeries { get; set; }
        public LineSeries StockSeries { get; set; }















        #region  //---- Pie chart ----\\  

        public Func<ChartPoint, string> PointLabel { get; set; }

        private void PieChart_Click(object sender, RoutedEventArgs e)
        {
            if (sender is LiveCharts.Wpf.PieChart chart)
            {
                foreach (PieSeries series in chart.Series)
                    series.PushOut = 0;

                if (e.OriginalSource is PieSeries selectedSeries)
                {
                    selectedSeries.PushOut = 8;
                }
            }
        }
     
        private void InitializePieChart()
        {
            PieChart pieChart = new PieChart();

            PieSeries mariaSeries = new PieSeries
            {
                Title = "Maria",
                Values = new ChartValues<double> { 3 },
                DataLabels = true,
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222749")),
                Stroke = Brushes.Transparent
            };
            pieChart.Series.Add(mariaSeries);

            PieSeries CharlesSeries = new PieSeries
            {
                Title = "Charles",
                Values = new ChartValues<double> { 3 },
                DataLabels = true,
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3f51b5")),
                Stroke= Brushes.Transparent
            };
            pieChart.Series.Add(CharlesSeries);

            PieSeries FridaSeries = new PieSeries
            {
                Title = "Frida",
                Values = new ChartValues<double> { 3 },
                DataLabels = true,
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7c9dde")),
                Stroke = Brushes.Transparent
            };
            pieChart.Series.Add(FridaSeries);

            PieSeries FredericSeries = new PieSeries
            {
                Title = "Frederic",
                Values = new ChartValues<double> { 3 },
                DataLabels = true,
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5d7dd4")),
                Stroke = Brushes.Transparent
            };
            pieChart.Series.Add(FredericSeries);

            #endregion
        }

       


    }
}
