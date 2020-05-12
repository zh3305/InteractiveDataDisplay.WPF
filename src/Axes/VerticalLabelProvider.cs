using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace InteractiveDataDisplay.WPF.Axes
{
    public class VerticalLabelProvider : ILabelProvider
    {
        /// <summary>
        /// Generates an array of labels from an array of double.
        /// </summary>
        /// <param name="ticks">An array of double ticks.</param>
        /// <returns>An array of <see cref="FrameworkElement"/>.</returns>
        public FrameworkElement[] GetLabels(double[] ticks)
        {
            if (ticks == null)
                throw new ArgumentNullException("ticks");

            List<TextBlock> Labels = new List<TextBlock>();
            foreach (double tick in ticks)
            {
                TextBlock text = new TextBlock();
                text.Text = tick.ToString(CultureInfo.InvariantCulture);
                text.RenderTransform = new RotateTransform(-90);
                Labels.Add(text);
            }
            return Labels.ToArray();
        }
    }
}
