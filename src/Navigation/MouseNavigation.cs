// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.ComponentModel;
using static InteractiveDataDisplay.WPF.Navigation.NavigationHelper;
using InteractiveDataDisplay.WPF.Navigation;

namespace InteractiveDataDisplay.WPF
{
    /// <summary>
    /// Provides mouse navigation around viewport of parent <see cref="PlotBase"/>.
    /// </summary>
    /// <remarks>
    /// Place instance of <see cref="MouseNavigation"/> inside your <see cref="PlotBase"/> element to enable mouse navigation over it.
    /// </remarks>
    [Description("Navigates parent plot by mouse")]
    public class MouseNavigation : Panel
    {
        private Canvas navigationCanvas = new Canvas
        {
            // This background brush allows Canvas to intercept mouse events while remaining transparent
            Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255))
        };

        private PlotBase masterPlot = null;

        /// <summary>First parent Control in visual tree. Used to receive focus on mouse click</summary>
        private Control parentControl = null;

        private bool isPanning = false;
        private bool isSelecting = false;
        private bool wasInside = false;
        private bool isLeftClicked = false;

        private bool selectingStarted = false;
        private Point panningStart;
        private Point panningEnd;
        private Point selectStart;
        private Point selectEnd;
        private DateTime lastClick;

        private bool selectingMode = true;

        private Rectangle selectArea;
        private Plot plot = new NavigationPlot();

        private bool transformChangeRequested = false;

        /// <summary>
        /// Initializes a new instance of <see cref="MouseNavigation"/> class.
        /// </summary>
        public MouseNavigation()
        {
            Children.Add(navigationCanvas);


            Loaded += MouseNavigationLoaded;
            Unloaded += MouseNavigationUnloaded;

            MouseLeave += new MouseEventHandler(MouseNavigationLayer_MouseLeave);
            MouseMove += new MouseEventHandler(MouseNavigationLayer_MouseMove);
            MouseLeftButtonUp += new MouseButtonEventHandler(MouseNavigationLayer_MouseLeftButtonUp);
            MouseLeftButtonDown += new MouseButtonEventHandler(MouseNavigationLayer_MouseLeftButtonDown);
            MouseWheel += new MouseWheelEventHandler(MouseNavigationLayer_MouseWheel);

            LayoutUpdated += (s, a) => transformChangeRequested = false;
        }

        public event MouseEventHandler Click;

        /// <summary>Gets or sets vertical navigation status. True means that user can navigate along Y axis</summary>
        /// <remarks>The default value is true</remarks>
        [Category("InteractiveDataDisplay")]
        public bool IsVerticalNavigationEnabled
        {
            get { return (bool)GetValue(IsVerticalNavigationEnabledProperty); }
            set { SetValue(IsVerticalNavigationEnabledProperty, value); }
        }

        /// <summary>Identifies <see cref="IsVerticalNavigationEnabled"/> property</summary>
        public static readonly DependencyProperty IsVerticalNavigationEnabledProperty =
            DependencyProperty.Register("IsVerticalNavigationEnabled", typeof(bool), typeof(MouseNavigation), new PropertyMetadata(true));

        /// <summary>Gets or sets horizontal navigation status. True means that user can navigate along X axis</summary>
        /// <remarks>The default value is true</remarks>
        [Category("InteractiveDataDisplay")]
        public bool IsHorizontalNavigationEnabled
        {
            get { return (bool)GetValue(IsHorizontalNavigationEnabledProperty); }
            set { SetValue(IsHorizontalNavigationEnabledProperty, value); }
        }

        /// <summary>Identifies <see cref="IsHorizontalNavigationEnabled"/> property</summary>
        public static readonly DependencyProperty IsHorizontalNavigationEnabledProperty =
            DependencyProperty.Register("IsHorizontalNavigationEnabled", typeof(bool), typeof(MouseNavigation), new PropertyMetadata(true));

        [Category("InteractiveDataDisplay")]
        public double NavigationLimitMaxY
        {
            get { return (double)GetValue(NavigationLimitMaxYProperty); }
            set { SetValue(NavigationLimitMaxYProperty, value); }
        }

        public static readonly DependencyProperty NavigationLimitMaxYProperty =
            DependencyProperty.Register("NavigationLimitMaxY", typeof(double),
                typeof(MouseNavigation), new PropertyMetadata(double.PositiveInfinity));

        [Category("InteractiveDataDisplay")]
        public double NavigationLimitMinY
        {
            get { return (double)GetValue(NavigationLimitMinYProperty); }
            set { SetValue(NavigationLimitMinYProperty, value); }
        }

        public static readonly DependencyProperty NavigationLimitMinYProperty =
            DependencyProperty.Register("NavigationLimitMinY", typeof(double),
                typeof(MouseNavigation), new PropertyMetadata(double.NegativeInfinity));

        [Category("InteractiveDataDisplay")]
        public double NavigationLimitMaxX
        {
            get { return (double)GetValue(NavigationLimitMaxXProperty); }
            set { SetValue(NavigationLimitMaxXProperty, value); }
        }

        public static readonly DependencyProperty NavigationLimitMaxXProperty =
            DependencyProperty.Register("NavigationLimitMaxX", typeof(double),
                typeof(MouseNavigation), new PropertyMetadata(double.PositiveInfinity));

        [Category("InteractiveDataDisplay")]
        public double NavigationLimitMinX
        {
            get { return (double)GetValue(NavigationLimitMinXProperty); }
            set { SetValue(NavigationLimitMinXProperty, value); }
        }

        public static readonly DependencyProperty NavigationLimitMinXProperty =
            DependencyProperty.Register("NavigationLimitMinX", typeof(double),
                typeof(MouseNavigation), new PropertyMetadata(double.NegativeInfinity));


        [Category("InteractiveDataDisplay")]
        public double PreferredAspectRatio
        {
            get { return (double)GetValue(PreferredAspectRatioProperty); }
            set { SetValue(PreferredAspectRatioProperty, value); }
        }

        public static readonly DependencyProperty PreferredAspectRatioProperty =
            DependencyProperty.Register("PreferredAspectRatio", typeof(double),
                typeof(MouseNavigation), new PropertyMetadata(0.0));


        [Category("InteractiveDataDisplay")]
        public bool ZoomByShiftAndCtrl
        {
            get { return (bool)GetValue(ZoomByShiftAndCtrlProperty); }
            set { SetValue(ZoomByShiftAndCtrlProperty, value); }
        }

        public static readonly DependencyProperty ZoomByShiftAndCtrlProperty =
            DependencyProperty.Register("ZoomByShiftAndCtrl", typeof(bool),
                typeof(MouseNavigation), new PropertyMetadata(true));


        void MouseNavigationUnloaded(object sender, RoutedEventArgs e)
        {
            if (masterPlot != null)
            {
                masterPlot.MouseLeave -= MouseNavigationLayer_MouseLeave;
                masterPlot.MouseMove -= MouseNavigationLayer_MouseMove;
                masterPlot.MouseLeftButtonUp -= MouseNavigationLayer_MouseLeftButtonUp;
                masterPlot.MouseLeftButtonDown -= MouseNavigationLayer_MouseLeftButtonDown;
                masterPlot.MouseWheel -= MouseNavigationLayer_MouseWheel;
            }

            masterPlot = null;
            parentControl = null;
        }

        void MouseNavigationLoaded(object sender, RoutedEventArgs e)
        {
            masterPlot = PlotBase.FindMaster(this);

            if (masterPlot != null)
            {
                masterPlot.MouseLeave += MouseNavigationLayer_MouseLeave;
                masterPlot.MouseMove += MouseNavigationLayer_MouseMove;
                masterPlot.MouseLeftButtonUp += MouseNavigationLayer_MouseLeftButtonUp;
                masterPlot.MouseLeftButtonDown += MouseNavigationLayer_MouseLeftButtonDown;
                masterPlot.MouseWheel += MouseNavigationLayer_MouseWheel;
            }

            var parent = VisualTreeHelper.GetParent(this);
            var controlParent = parent as Control;
            while (parent != null && controlParent == null)
            {
                parent = VisualTreeHelper.GetParent(parent);
                controlParent = parent as Control;
            }
            parentControl = controlParent;
        }

        /// <summary>
        /// Measures the size in layout required for child elements and determines a size for the Figure. 
        /// </summary>
        /// <param name="availableSize">The available size that this element can give to child elements. Infinity can be specified as a value to indicate that the element will size to whatever content is available.</param>
        /// <returns>The size that this element determines it needs during layout, based on its calculations of child element sizes.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            navigationCanvas.Measure(availableSize);
            return navigationCanvas.DesiredSize;
        }

        /// <summary>
        /// Positions child elements and determines a size for a Figure
        /// </summary>
        /// <param name="finalSize">The final area within the parent that Figure should use to arrange itself and its children</param>
        /// <returns>The actual size used</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            navigationCanvas.Arrange(new Rect(new Point(0, 0), finalSize));
            transformChangeRequested = false;
            return finalSize;
        }

        private void MouseNavigationLayer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point cursorPosition = e.GetPosition(this);
            if (!CheckCursor(cursorPosition))
                return;

            //Zoom relative to mouse position
            double factor = e.Delta < 0 ? 1.2 : 1 / 1.2;
            Point mousePos = new Point(
                masterPlot.XFromLeft(cursorPosition.X),
                masterPlot.YFromTop(cursorPosition.Y));
            DoZoom(factor, mousePos);
            e.Handled = true;
        }

        private void DoZoom(double factor, Point? mousePos = null)
        {
            if (masterPlot != null)
            {
                var rect = masterPlot.PlotRect;

                ValidateNavigationLimits();
                rect = NavigationHelper.DoZoom(factor, mousePos, rect,
                    NavigationLimitMaxX, NavigationLimitMinX,
                    NavigationLimitMaxY, NavigationLimitMinY,
                    IsHorizontalNavigationEnabled, IsVerticalNavigationEnabled,
                    PreferredAspectRatio, masterPlot.AspectRatio);

                if (IsZoomEnable(rect))
                {
                    masterPlot.SetPlotRect(rect);
                    masterPlot.IsAutoFitEnabled = false;
                }

            }
        }

        private bool IsZoomEnable(DataRect rect)
        {
            bool res = true;
            if (IsHorizontalNavigationEnabled)
            {
                double e_max = Math.Log(Math.Max(Math.Abs(rect.XMax), Math.Abs(rect.XMin)), 2);
                double log = Math.Log(rect.Width, 2);
                res = log > e_max - 40;
            }
            if (IsVerticalNavigationEnabled)
            {
                double e_max = Math.Log(Math.Max(Math.Abs(rect.YMax), Math.Abs(rect.YMin)), 2);
                double log = Math.Log(rect.Height, 2);
                res = res && log > e_max - 40;
            }
            return res;
        }

        private void ValidateNavigationLimits()
        {
            //checks navigation limits and reset if not valid
            if (NavigationLimitMaxX <= NavigationLimitMinX)
            {
                NavigationLimitMaxX = double.PositiveInfinity;
                NavigationLimitMinX = double.NegativeInfinity;
            }

            if (NavigationLimitMaxY <= NavigationLimitMinY)
            {
                NavigationLimitMaxY = double.PositiveInfinity;
                NavigationLimitMinY = double.NegativeInfinity;
            }
        }

        private void DoPan(Point screenStart, Point screenEnd)
        {
            if (masterPlot != null)
            {
                double dx = IsHorizontalNavigationEnabled ?
                    masterPlot.XFromLeft(screenEnd.X) - masterPlot.XFromLeft(screenStart.X) : 0;
                double dy = IsVerticalNavigationEnabled ?
                    masterPlot.YFromTop(screenEnd.Y) - masterPlot.YFromTop(screenStart.Y) : 0;
                var rect = masterPlot.PlotRect;

                ValidateNavigationLimits();
                bool returnToAvailableArea = NeedReturnToAvailableArea(
                    ref rect, NavigationLimitMaxX, NavigationLimitMinX,
                    NavigationLimitMaxY, NavigationLimitMinY);

                if (returnToAvailableArea)
                {
                    masterPlot.SetPlotRect(rect);
                }
                else
                {
                    double width = rect.Width;
                    double height = rect.Height;

                    var newRect = new DataRect(
                        rect.XMin - dx,
                        rect.YMin - dy,
                        rect.XMin - dx + width,
                        rect.YMin - dy + height);

                    bool xInBounds = NavigationXIsInBounds(newRect, NavigationLimitMaxX, NavigationLimitMinX);
                    bool yInBounds = NavigationYIsInBounds(newRect, NavigationLimitMaxY, NavigationLimitMinY);


                    if (xInBounds && !yInBounds)
                    {
                        newRect = new DataRect(
                        rect.XMin - dx,
                        rect.YMin,
                        rect.XMin - dx + width,
                        rect.YMin + height);
                        yInBounds = true;
                    }
                    else if (!xInBounds && yInBounds)
                    {
                        newRect = new DataRect(
                        rect.XMin,
                        rect.YMin - dy,
                        rect.XMin + width,
                        rect.YMin - dy + height);
                        xInBounds = true;
                    }


                    if (xInBounds && yInBounds)
                        masterPlot.SetPlotRect(newRect);
                }

                masterPlot.IsAutoFitEnabled = false;
            }
        }

        private void MouseNavigationLayer_MouseLeave(object sender, MouseEventArgs e)
        {
            if (isPanning && (e.OriginalSource == navigationCanvas || e.OriginalSource == masterPlot))
            {
                //panningEnd = e.GetPosition(this);
                DoPan(panningStart, panningEnd);
                isPanning = false;
            }
            if (isSelecting && (e.OriginalSource == navigationCanvas || e.OriginalSource == masterPlot))
            {
                if (selectArea != null)
                {
                    if (navigationCanvas.Children.Contains(selectArea))
                    {
                        navigationCanvas.Children.Remove(selectArea);
                    }

                    if (masterPlot != null && masterPlot.Children.Contains(plot))
                    {
                        masterPlot.Children.Remove(plot);
                    }
                }
                isSelecting = false;
                selectingStarted = false;
            }
        }



        private void MouseNavigationLayer_MouseMove(object sender, MouseEventArgs e)
        {
            Point cursorPosition = e.GetPosition(this);
            if (!CheckCursor(cursorPosition))
            {
                wasInside = true;
                MouseNavigationLayer_MouseLeave(sender, e);
                return;
            }
            else
            {
                if (wasInside && isLeftClicked)
                {
                    HandleMouseUp(e);
                    HandleMouseDown(e);
                }
            }

            if (isPanning)
            {
                if (!transformChangeRequested)
                {
                    transformChangeRequested = true;
                    panningEnd = e.GetPosition(this);
                    DoPan(panningStart, panningEnd);
                    panningStart = panningEnd;
                    InvalidateArrange();
                }
            }

            if (isSelecting)
            {
                selectEnd = e.GetPosition(this);
                if (!selectingStarted)
                {
                    if ((Math.Abs(selectStart.X - selectEnd.X) > 4) || (Math.Abs(selectStart.Y - selectEnd.Y) > 4))
                    {
                        selectingStarted = true;
                        selectArea = new Rectangle();
                        selectArea.Fill = new SolidColorBrush(Colors.LightGray);
                        selectArea.StrokeThickness = 2;
                        selectArea.Stroke = new SolidColorBrush(Color.FromArgb(255, 40, 0, 120));
                        selectArea.Opacity = 0.6;

                        if (masterPlot != null)
                        {
                            plot.IsAutoFitEnabled = false;

                            plot.Children.Clear();
                            plot.Children.Add(selectArea);

                            if (!masterPlot.Children.Contains(plot))
                            {
                                masterPlot.Children.Add(plot);
                                Canvas.SetZIndex(plot, 40000);
                            }

                            double width = Math.Abs(masterPlot.XFromLeft(selectStart.X) - masterPlot.XFromLeft(selectEnd.X));
                            double height = Math.Abs(masterPlot.YFromTop(selectStart.Y) - masterPlot.YFromTop(selectEnd.Y));
                            double xmin = masterPlot.XFromLeft(Math.Min(selectStart.X, selectEnd.X));
                            double ymin = masterPlot.YFromTop(Math.Max(selectStart.Y, selectEnd.Y));

                            Plot.SetX1(selectArea, xmin);
                            Plot.SetY1(selectArea, ymin);
                            Plot.SetX2(selectArea, xmin + width);
                            Plot.SetY2(selectArea, ymin + height);

                        }
                        else
                        {
                            navigationCanvas.Children.Add(selectArea);
                            Canvas.SetLeft(selectArea, selectStart.X);
                            Canvas.SetTop(selectArea, selectStart.Y);
                        }
                    }
                }
                else
                {
                    if (masterPlot == null)
                    {
                        double width = Math.Abs(selectStart.X - selectEnd.X);
                        double height = Math.Abs(selectStart.Y - selectEnd.Y);
                        selectArea.Width = width;
                        selectArea.Height = height;

                        if (selectEnd.X < selectStart.X)
                        {
                            Canvas.SetLeft(selectArea, selectStart.X - width);
                        }

                        if (selectEnd.Y < selectStart.Y)
                        {
                            Canvas.SetTop(selectArea, selectStart.Y - height);
                        }
                    }
                    else
                    {
                        double width = Math.Abs(masterPlot.XFromLeft(selectStart.X) - masterPlot.XFromLeft(selectEnd.X));
                        double height = Math.Abs(masterPlot.YFromTop(selectStart.Y) - masterPlot.YFromTop(selectEnd.Y));
                        double xmin = masterPlot.XFromLeft(Math.Min(selectStart.X, selectEnd.X));
                        double ymin = masterPlot.YFromTop(Math.Max(selectStart.Y, selectEnd.Y));

                        Plot.SetX1(selectArea, xmin);
                        Plot.SetY1(selectArea, ymin);
                        Plot.SetX2(selectArea, xmin + width);
                        Plot.SetY2(selectArea, ymin + height);
                    }
                }
            }

            wasInside = false;
        }

        private void MouseNavigationLayer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = HandleMouseDown(e);
        }

        private void MouseNavigationLayer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = HandleMouseUp(e);
        }

        private bool CheckCursor(Point cursorPosition)
        {
            return !(cursorPosition.X < 0 || cursorPosition.Y < 0 || cursorPosition.X > this.ActualWidth || cursorPosition.Y > this.ActualHeight);
        }

        private bool HandleMouseDown(MouseEventArgs e)
        {
            Point cursorPosition = e.GetPosition(this);
            if (!CheckCursor(cursorPosition))
                return false;
            else
            {
                isLeftClicked = true;

                if (masterPlot != null)
                {
                    if (Keyboard.Modifiers == ModifierKeys.Shift)
                    {
                        if (ZoomByShiftAndCtrl)
                            DoZoom(1.2);
                    }
                    else
                    {
                        if (Keyboard.Modifiers == ModifierKeys.Control)
                        {
                            if (selectingMode && IsVerticalNavigationEnabled && IsHorizontalNavigationEnabled)
                            {
                                isSelecting = true;
                                selectStart = cursorPosition;
                                this.CaptureMouse();
                            }
                            lastClick = DateTime.Now;
                        }
                        else
                        {
                            DateTime d = DateTime.Now;
                            if ((d - lastClick).TotalMilliseconds < 200)
                            {
                                masterPlot.IsAutoFitEnabled = true;
                                return true;
                            }
                            else
                            {

                                isPanning = true;
                                panningStart = cursorPosition;
                                this.CaptureMouse();
                                lastClick = DateTime.Now;

                            }
                        }
                    }
                }
            }

            if (parentControl != null)
                parentControl.Focus();

            return true;
        }

        private bool HandleMouseUp(MouseEventArgs e)
        {
            isLeftClicked = false;
            isPanning = false;
            if ((!isSelecting || !selectingStarted))
            {
                DateTime d = DateTime.Now;
                if ((d - lastClick).TotalMilliseconds < 300)
                {
                    isSelecting = false;
                    if (Keyboard.Modifiers == ModifierKeys.Control
                        && ZoomByShiftAndCtrl)
                        DoZoom(1 / 1.2);
                    else if (Click != null)
                        Click.Invoke(this, e);
                }
            }
            if (isSelecting)
            {
                if (selectingStarted)
                {
                    if (!transformChangeRequested)
                    {
                        transformChangeRequested = true;
                        isSelecting = false;
                        selectingStarted = false;
                        navigationCanvas.Children.Remove(selectArea);

                        if (masterPlot != null)
                        {
                            masterPlot.Children.Remove(plot);

                            double width = Math.Abs(masterPlot.XFromLeft(selectStart.X) - masterPlot.XFromLeft(selectEnd.X));
                            double height = Math.Abs(masterPlot.YFromTop(selectStart.Y) - masterPlot.YFromTop(selectEnd.Y));
                            double xmin = masterPlot.XFromLeft(Math.Min(selectStart.X, selectEnd.X));
                            double ymin = masterPlot.YFromTop(Math.Max(selectStart.Y, selectEnd.Y));

                            masterPlot.SetPlotRect(new DataRect(xmin, ymin, xmin + width, ymin + height));
                            masterPlot.IsAutoFitEnabled = false;
                        }
                    }
                }
            }
            this.ReleaseMouseCapture();

            return true;
        }

    }


    internal class NavigationPlot : Plot
    {
        protected override DataRect ComputeBounds()
        {
            return DataRect.Empty;
        }
    }
}

