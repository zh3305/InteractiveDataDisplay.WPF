using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Collections;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Generic;

namespace InteractiveDataDisplay.WPF
{
    /// <summary>
    /// A plot to draw an area limited by upper and lower tolerance band.
    /// </summary>
    [Description("Plots tolerance band")]
    public class ToleranceGraph : Plot
    {
        private Polygon polygon;

        /// <summary>
        /// Gets or sets line graph points.
        /// </summary>
        [Category("InteractiveDataDisplay")]
        [Description("Tolerance graph points")]
        public PointCollection Points
        {
            get { return (PointCollection)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }

        private static void PointsPropertyChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToleranceGraph tolerancePlot = (ToleranceGraph)d;
            if (tolerancePlot != null)
            {
                InteractiveDataDisplay.WPF.Plot.SetPoints(tolerancePlot.polygon, (PointCollection)e.NewValue);
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ToleranceGraph"/> class.
        /// </summary>
        public ToleranceGraph()
        {
            polygon = new Polygon
            {
                Stroke = new SolidColorBrush(Colors.Transparent),
                StrokeLineJoin = PenLineJoin.Round,
                Fill = new SolidColorBrush(Colors.Red)
            };


            BindingOperations.SetBinding(polygon, Polygon.StrokeThicknessProperty, new Binding("StrokeThickness") { Source = this });

            // TODO: check
            BindingOperations.SetBinding(this, PlotBase.PaddingProperty, new Binding("StrokeThickness") { Source = this, Converter = new LineGraphThicknessConverter() });

            Children.Add(polygon);
        }

        static ToleranceGraph()
        {
            PointsProperty.OverrideMetadata(typeof(ToleranceGraph), new PropertyMetadata(new PointCollection(), PointsPropertyChangedHandler));
        }

        /// <summary>
        /// Updates data in <see cref="Points"/> and causes a redrawing of tolerance graph.
        /// </summary>
        /// <param name="x">A set of x coordinates of new points.</param>
        /// <param name="yUpper">A set of y coordinates of new points for the upper tolerance limit.</param>
        /// <param name="yLower">A set of y coordinates of new points for the lower tolerance limit.</param>
        public void Plot(IEnumerable x, IEnumerable yUpper, IEnumerable yLower)
        {
            if (x == null)
                throw new ArgumentNullException("x");
            if (yUpper == null)
                throw new ArgumentNullException("yUpper");
            if (yLower == null)
                throw new ArgumentNullException("yLower");

            var points = new PointCollection();
            var enx = x.GetEnumerator();
            var enyl = yLower.GetEnumerator();
            var enyu = yUpper.GetEnumerator();
            List<double> xs = new List<double>();
            List<double> yls = new List<double>();
            List<double> yus = new List<double>();
            while (true)
            {
                var nx = enx.MoveNext();
                var nyl = enyl.MoveNext();
                var nyu = enyu.MoveNext();
                if (nx && nyl && nyu)
                {
                    xs.Add(Convert.ToDouble(enx.Current, CultureInfo.InvariantCulture));
                    yls.Add(Convert.ToDouble(enyl.Current, CultureInfo.InvariantCulture));
                    yus.Add(Convert.ToDouble(enyu.Current, CultureInfo.InvariantCulture));
                }
                else if (!nx && !nyl && !nyu)
                    break;
                else
                    throw new ArgumentException("x, yUpper and yLower have different lengthes");
            }
            for(int i = 0; i < xs.Count; i++)
            {
                points.Add(new Point(xs[i], yls[i]));
            }
            for (int i = (xs.Count - 1); i >= 0; i--)
            {
                points.Add(new Point(xs[i], yus[i]));
            }

            Points = points;
        }

        #region Description
        /// <summary>
        /// Identifies the <see cref="Description"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DescriptionProperty =
           DependencyProperty.Register("Description",
           typeof(string),
           typeof(ToleranceGraph),
           new PropertyMetadata(null,
               (s, a) =>
               {
                   var lg = (ToleranceGraph)s;
                   ToolTipService.SetToolTip(lg, a.NewValue);
               }));

        /// <summary>
        /// Gets or sets description text for line graph. Description text appears in default
        /// legend and tooltip.
        /// </summary>
        [Category("InteractiveDataDisplay")]
        public string Description
        {
            get
            {
                return (string)GetValue(DescriptionProperty);
            }
            set
            {
                SetValue(DescriptionProperty, value);
            }
        }

        #endregion

        #region Stroke

        /// <summary>
        /// Identifies the <see cref="Stroke"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StrokeProperty =
           DependencyProperty.Register("Stroke",
           typeof(Brush),
           typeof(ToleranceGraph),
           new PropertyMetadata(new SolidColorBrush(Colors.Red), OnStrokeChanged));

        private static void OnStrokeChanged(object target, DependencyPropertyChangedEventArgs e)
        {
            ToleranceGraph toleranceGraph = (ToleranceGraph)target;
            toleranceGraph.polygon.Stroke = e.NewValue as Brush;
        }

        /// <summary>
        /// Gets or sets the brush to draw the line.
        /// </summary>
        /// <remarks>
        /// The default color of stroke is black
        /// </remarks>
        [Category("Appearance")]
        public Brush Stroke
        {
            get
            {
                return (Brush)GetValue(StrokeProperty);
            }
            set
            {
                SetValue(StrokeProperty, value);
            }
        }
        #endregion

        #region Fill

        /// <summary>
        /// Identifies the <see cref="Fill"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FillProperty =
           DependencyProperty.Register("Fill",
           typeof(Brush),
           typeof(ToleranceGraph),
           new PropertyMetadata(new SolidColorBrush(Colors.Red), OnFillChanged));

        private static void OnFillChanged(object target, DependencyPropertyChangedEventArgs e)
        {
            ToleranceGraph toleranceGraph = (ToleranceGraph)target;
            toleranceGraph.polygon.Fill = e.NewValue as Brush;
        }

        /// <summary>
        /// Gets or sets the brush to fill the tolerance area.
        /// </summary>
        /// <remarks>
        /// The default color of the fill is red
        /// </remarks>
        [Category("Appearance")]
        public Brush Fill
        {
            get
            {
                return (Brush)GetValue(FillProperty);
            }
            set
            {
                SetValue(FillProperty, value);
            }
        }
        #endregion
    }
}
