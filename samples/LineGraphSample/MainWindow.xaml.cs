// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using InteractiveDataDisplay.WPF;

namespace LineGraphSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            double[] x = new double[200];
            for (int i = 0; i < x.Length; i++)
                x[i] = 3.1415 * i / (x.Length - 1);

            for (int i = 0; i < 25; i++)
            {
                var lg = new LineGraph();
                lines.Children.Add(lg);
                lg.Stroke = new SolidColorBrush(Color.FromArgb(255, 0, (byte)(i * 10), 0));
                lg.Description = String.Format("Data series {0}", i + 1);
                lg.StrokeThickness = 2;
                lg.Plot(x, x.Select(v => Math.Sin(v + i / 10.0)).ToArray());
            }
        }
    }

    public class CustomLabelProvider : ILabelProvider
    {
        public static DateTime Origin = new DateTime(2000, 1, 1);

        public FrameworkElement[] GetLabels(double[] ticks)
        {
            if (ticks == null)
                throw new ArgumentNullException("ticks");


            List<TextBlock> Labels = new List<TextBlock>();
            foreach (double tick in ticks)
            {
                TextBlock text = new TextBlock();
                var time = Origin + TimeSpan.FromDays(tick);
                if(time.Hour!=0)
                    text.Text = null;
                else
                    text.Text = time.ToString("dd.MM.yyyy");
                Labels.Add(text);
            }
            return Labels.ToArray();
        }
    }

    public class CustomAxis : Axis
    {
        public CustomAxis() : base(new CustomLabelProvider(), new TicksProvider())
        {
        }
    }

    public class VisibilityToCheckedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((Visibility)value) == Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((bool)value) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}