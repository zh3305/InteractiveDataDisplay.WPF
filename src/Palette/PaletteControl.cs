// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.IO;

namespace InteractiveDataDisplay.WPF
{
    /// <summary>
    /// A control to show all the information about <see cref="Palette"/>.
    /// </summary>
    [Description("Visually maps value to color")]
    public class PaletteControl : ContentControl
    {
        #region Fields

        private Image image ;
        private Axis axis ;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the palette.
        /// Default value is black palette.
        /// </summary>
        [Category("InteractiveDataDisplay")]
        [TypeConverter(typeof(StringToPaletteTypeConverter))]
        public Palette Palette
        {
            get { return (Palette)GetValue(PaletteProperty); }
            set { SetValue(PaletteProperty, value); }
        }

        /// <summary>
        /// Gets or sets the range of axis values.
        /// Default value is [0, 1].
        /// </summary>
        [Category("InteractiveDataDisplay")]
        [Description("Range of values mapped to color")]
        public Range Range
        {
            get { return (Range)GetValue(RangeProperty); }
            set { SetValue(RangeProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Palette"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PaletteProperty = DependencyProperty.Register(
              "Palette",
              typeof(Palette),
              typeof(PaletteControl),
              new PropertyMetadata(Palette.Parse("Black"), OnPaletteChanged));

        private static void OnPaletteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PaletteControl control = (PaletteControl)d;
            control.OnPaletteChanged();
        }

        /// <summary>
        /// Identifies the <see cref="Range"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RangeProperty = DependencyProperty.Register(
          "Range",
          typeof(Range),
          typeof(PaletteControl),
          new PropertyMetadata(new Range(0, 1), OnRangeChanged));

        /// <summary>
        /// Gets or sets a value indicating whether an axis should be displayed.
        /// Default value is true.
        /// </summary>
        [Category("InteractiveDataDisplay")]
        public bool IsAxisVisible
        {
            get { return (bool)GetValue(IsAxisVisibleProperty); }
            set { SetValue(IsAxisVisibleProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="IsAxisVisible"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsAxisVisibleProperty = DependencyProperty.Register(
          "IsAxisVisible",
          typeof(bool),
          typeof(PaletteControl),
          new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets the height of rendered palette.
        /// Default value is 20.
        /// </summary>
        [Category("InteractiveDataDisplay")]
        public double PaletteHeight
        {
            get { return (double)GetValue(PaletteHeightProperty); }
            set { SetValue(PaletteHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="PaletteHeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PaletteHeightProperty = DependencyProperty.Register(
          "PaletteHeight",
          typeof(double),
          typeof(PaletteControl),
          new PropertyMetadata(20.0, OnPaletteHeightChanged));

        private static void OnPaletteHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PaletteControl control = (PaletteControl)d;
            control.image.Height = (double)e.NewValue;
            control.UpdateBitmap();
        }



        public AxisOrientation PaletteAxisOrientation
        {
            get { return (AxisOrientation)GetValue(PaletteAxisOrientationProperty); }
            set { SetValue(PaletteAxisOrientationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PaletteAxisOrientation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PaletteAxisOrientationProperty =
            DependencyProperty.Register("PaletteAxisOrientation", typeof(AxisOrientation), typeof(PaletteControl), new PropertyMetadata(AxisOrientation.Bottom, OnPaletteAxisOrientationChanged));

        private static void OnPaletteAxisOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PaletteControl control = (PaletteControl)d;
            control.axis.AxisOrientation = (AxisOrientation)e.NewValue;
        }



        public ILabelProvider PaletteLabelProvider
        {
            get { return (ILabelProvider)GetValue(PaletteLabelProviderProperty); }
            set { SetValue(PaletteLabelProviderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyPropertyPaletteLabelProvider.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PaletteLabelProviderProperty =
            DependencyProperty.Register("PaletteLabelProvider", typeof(ILabelProvider), typeof(PaletteControl), new PropertyMetadata(new LabelProvider(), onPaletteLabelProviderChanged));

        private static void onPaletteLabelProviderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PaletteControl control = (PaletteControl)d;
            control.axis.LabelProvider = (ILabelProvider)e.NewValue;
        }



        public Brush PaletteAxisForeground
        {
            get { return (Brush)GetValue(PaletteAxisForegroundProperty); }
            set { SetValue(PaletteAxisForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PaletteAxisForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PaletteAxisForegroundProperty =
            DependencyProperty.Register("PaletteAxisForeground", typeof(Brush), typeof(PaletteControl), new PropertyMetadata(new SolidColorBrush(Colors.Black), 
                (d,e)=>{
                    PaletteControl control = (PaletteControl)d;
                    control.axis.Foreground = (Brush)e.NewValue;
                    control.UpdateBitmap();
                }));



        
        public Orientation PaletteOrientation
        {
            get { return (Orientation)GetValue(PaletteOrientationProperty); }
            set { SetValue(PaletteOrientationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PaletteOrientation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PaletteOrientationProperty =
            DependencyProperty.Register(nameof(PaletteOrientation), typeof(Orientation), typeof(PaletteControl), new PropertyMetadata(Orientation.Horizontal,
                (d,e)=> {
                    PaletteControl control = (PaletteControl)d;
                    if((Orientation)e.NewValue == Orientation.Horizontal) 
                    { 
                        ((StackPanel)control.Content).Orientation = Orientation.Vertical;
                    }
                    else
                    {
                        ((StackPanel)control.Content).Orientation = Orientation.Horizontal;
                    }
                    control.UpdateBitmap();
                }));



        #endregion

        #region ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="PaletteControl"/> class.
        /// </summary>
        public PaletteControl()
        {
            StackPanel stackPanel = new StackPanel();

            image = new Image { Height = PaletteHeight, Stretch = Stretch.None, HorizontalAlignment = HorizontalAlignment.Stretch };
            axis = new Axis { AxisOrientation = PaletteAxisOrientation, HorizontalAlignment = HorizontalAlignment.Stretch , Foreground = PaletteAxisForeground};

            stackPanel.Children.Add(image);
            stackPanel.Children.Add(axis);

            Content = stackPanel;

            SizeChanged += (o, e) =>
            {
                if (e.PreviousSize.Width == 0 || e.PreviousSize.Height == 0 || Double.IsNaN(e.PreviousSize.Width) || Double.IsNaN(e.PreviousSize.Height)) {
                    UpdateBitmap();
                    Thickness marginThickness = this.Margin;
                    Width = (axis.ActualWidth + image.Width+5) - marginThickness.Left - marginThickness.Left;
                    Height = Math.Max(axis.ActualHeight,image.Height) - marginThickness.Top - marginThickness.Bottom;
                    
                }
            };

            IsTabStop = false;
        }

        #endregion

        #region Private methods

        private void OnPaletteChanged()
        {
            UpdateBitmap();
        }

        private void UpdateBitmap()
        {
            if (PaletteOrientation == Orientation.Horizontal && (Width == 0 || Double.IsNaN(Width)))
            {
                image.Source = null;
                return;
            }
            if (PaletteOrientation == Orientation.Vertical && (Height == 0 || Double.IsNaN(Height)))
            {
                image.Source = null;
                return;
            }

            if (Palette == null)
            {
                image.Source = null;
                return;
            }
            int width = 0;
            int height = 0;
            if (PaletteOrientation == Orientation.Horizontal)
            {
                width = (int)Width;
                height = (int)PaletteHeight;
            }
            else
            {
                width = (int)Height;
                height = (int)PaletteHeight;
            }

            WriteableBitmap bmp2 = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            WriteableBitmap bmp = bmp2.Clone();
            bmp.Lock();
            unsafe
            {
                byte* pixels = (byte*)bmp.BackBuffer;
                int stride = bmp.BackBufferStride;
                int pixelWidth = bmp.PixelWidth;
                double min = Palette.Range.Min;
                double coeff = (Palette.Range.Max - min) / bmp.PixelWidth;
                for (int i = 0; i < pixelWidth; i++)
                {
                    double ratio = i * coeff + min;
                    Color color = Palette.GetColor(i * coeff + min);
                    for (int j = 0; j < height; j++)
                    {
                        pixels[(i << 2) + 3 + j * stride] = color.A;
                        pixels[(i << 2) + 2 + j * stride] = color.R;
                        pixels[(i << 2) + 1 + j * stride] = color.G;
                        pixels[(i << 2) + j * stride] = color.B;
                    }
                }
            }
            bmp.Unlock();


            if (PaletteOrientation == Orientation.Vertical)
            {
                //SaveBMP("paleteControl_0.bmp", bmp);
                image.Source = WriteableBitmapreomBitmap(rotateImage90(BitmapFromWriteableBitmap(bmp)));
            }
            else
            {
                image.Source = bmp;
            }
            image.Width = image.Source.Width;
            image.Height = image.Source.Height;
        }

        private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PaletteControl control = (PaletteControl)d;
            control.OnRangeChanged();
        }

        private void OnRangeChanged()
        {
            axis.Range = Range;
        }
        #endregion



        private System.Drawing.Bitmap rotateImage90(System.Drawing.Bitmap b)
        {
            System.Drawing.Bitmap returnBitmap = b;
            returnBitmap.RotateFlip(System.Drawing.RotateFlipType.Rotate270FlipNone);
            return returnBitmap;
        }

        private System.Drawing.Bitmap BitmapFromWriteableBitmap(WriteableBitmap writeBmp)
        {
            System.Drawing.Bitmap bmp;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create((BitmapSource)writeBmp));
                enc.Save(outStream);
                bmp = new System.Drawing.Bitmap(outStream);
            }
            return bmp;
        }
        private WriteableBitmap WriteableBitmapreomBitmap(System.Drawing.Bitmap bmp) { 
        System.Windows.Media.Imaging.BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bmp.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                System.Windows.Media.Imaging.WriteableBitmap writeableBitmap = new System.Windows.Media.Imaging.WriteableBitmap(bitmapSource);
            return writeableBitmap;
        }

        void SaveBMP(string filename, BitmapSource image5)
        {
            if (filename != string.Empty)
            {
                using (FileStream stream5 = new FileStream(filename, FileMode.Create))
                {
                    BmpBitmapEncoder encoder5 = new BmpBitmapEncoder();
                    encoder5.Frames.Add(BitmapFrame.Create(image5));
                    encoder5.Save(stream5);
                }
            }
        }
    }
}

