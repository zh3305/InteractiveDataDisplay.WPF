using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveDataDisplay.WPF.Navigation
{
    public static class NavigationBoundsHelper
    {
        public static bool NavigationXIsInBounds(DataRect rect, 
            double navigationLimitMaxX, double navigationLimitMinX)
        {
            return rect.XMax < navigationLimitMaxX
                && rect.XMin > navigationLimitMinX;
        }

        public static bool NavigationYIsInBounds(DataRect rect, 
            double navigationLimitMaxY, double navigationLimitMinY)
        {
            return rect.YMax < navigationLimitMaxY
                && rect.YMin > navigationLimitMinY;
        }

    }
}
