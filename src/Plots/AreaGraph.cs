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
    /// A plot to draw arectengular area.
    /// </summary>
    [Description("Plots a rectangualr area")]
    public class AreaGraph : Plot
    {

        private Polygon polygon;

        /// <summary>
        /// Gets or sets line graph points.
        /// </summary>
        [Category("InteractiveDataDisplay")]
        [Description("Area graph points")]
        public PointCollection Points
        {
            get { return (PointCollection)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }

        private static void PointsPropertyChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AreaGraph areaPlot = (AreaGraph)d;
            if (areaPlot != null)
            {
                InteractiveDataDisplay.WPF.Plot.SetPoints(areaPlot.polygon, (PointCollection)e.NewValue);
            }
        }

        public AreaGraph()
        {
            polygon = new Polygon
            {
                Stroke = new SolidColorBrush(Colors.Blue),
                StrokeLineJoin = PenLineJoin.Round,
                Fill = new SolidColorBrush(Colors.Green)
            };

            BindingOperations.SetBinding(polygon, Polygon.StrokeThicknessProperty, new Binding("StrokeThickness") { Source = this });

            BindingOperations.SetBinding(this, PlotBase.PaddingProperty, new Binding("StrokeThickness") { Source = this, Converter = new LineGraphThicknessConverter() });

            Children.Add(polygon);
        }

        static AreaGraph()
        {
            PointsProperty.OverrideMetadata(typeof(AreaGraph), new PropertyMetadata(new PointCollection(), PointsPropertyChangedHandler));
        }

        /// <summary>
        /// Updates data in <see cref="Points"/> and causes a redrawing of tolerance graph.
        /// </summary>
        /// <param name="x">A set of x coordinates of new points.</param>
        /// <param name="yUpper">A set of y coordinates of new points for the upper tolerance limit.</param>
        /// <param name="yLower">A set of y coordinates of new points for the lower tolerance limit.</param>
        public void Plot(double x1, double y1, double x2, double y2)
        {
            Points = new PointCollection()
            {
                new Point(x1, y1),
                new Point(x2, y1),
                new Point(x2, y2),
                new Point(x1, y2)
            };
        }

        public void Plot(Rect rect)
        {
            Points = new PointCollection()
            {
                new Point(rect.X, rect.Y),
                new Point(rect.X + rect.Width, rect.Y),
                new Point(rect.X + rect.Width, rect.Y - rect.Height),
                new Point(rect.X, rect.Y - rect.Height)
            };
        }

        #region Description
        /// <summary>
        /// Identifies the <see cref="Description"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DescriptionProperty =
           DependencyProperty.Register("Description",
           typeof(string),
           typeof(AreaGraph),
           new PropertyMetadata(null,
               (s, a) =>
               {
                   var lg = (AreaGraph)s;
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
           typeof(AreaGraph),
           new PropertyMetadata(new SolidColorBrush(Colors.Blue), OnStrokeChanged));

        private static void OnStrokeChanged(object target, DependencyPropertyChangedEventArgs e)
        {
            AreaGraph areaGraph = (AreaGraph)target;
            areaGraph.polygon.Stroke = e.NewValue as Brush;
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
           typeof(AreaGraph),
           new PropertyMetadata(new SolidColorBrush(Colors.Green), OnFillChanged));

        private static void OnFillChanged(object target, DependencyPropertyChangedEventArgs e)
        {
            AreaGraph areaGraph = (AreaGraph)target;
            areaGraph.polygon.Fill = e.NewValue as Brush;
        }

        /// <summary>
        /// Gets or sets the brush to fill the tolerance area.
        /// </summary>
        /// <remarks>
        /// The default color of the fill is green
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
