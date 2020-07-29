using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GymTrainerWPF.ViewModels;
using Syncfusion.UI.Xaml.Charts;

namespace GymTrainerWPF.Views
{
    /// <summary>
    /// Interaction logic for Report.xaml
    /// </summary>
    public partial class Report : Window
    {
        public Report()
        {
            InitializeComponent();
            SfChart chart = new SfChart() { Header = "Chart", Height = 300, Width = 500 };

            //Adding horizontal axis to the chart 
            CategoryAxis primaryAxis = new CategoryAxis();
            primaryAxis.Header = "Name";
            primaryAxis.FontSize = 14;
            chart.PrimaryAxis = primaryAxis;

            //Adding vertical axis to the chart 
            NumericalAxis secondaryAxis = new NumericalAxis();
            secondaryAxis.Header = "Height(in cm)";
            secondaryAxis.FontSize = 14;
            chart.SecondaryAxis = secondaryAxis;

            //Adding Legends for the chart
            ChartLegend legend = new ChartLegend();
            chart.Legend = legend;

            //Initializing column series
            ColumnSeries series = new ColumnSeries();
            series.ItemsSource = (new ReportViewModel()).Data;
            series.XBindingPath = "Name";
            series.YBindingPath = "Height";
            series.ShowTooltip = true;
            series.Label = "Heights";

            //Setting adornment to the chart series
            series.AdornmentsInfo = new ChartAdornmentInfo() { ShowLabel = true };

            //Adding Series to the Chart Series Collection
            chart.Series.Add(series);
            this.Content = chart;
        }
    }
}
