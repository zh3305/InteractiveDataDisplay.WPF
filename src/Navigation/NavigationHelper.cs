using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace InteractiveDataDisplay.WPF.Navigation
{
    public static class NavigationHelper
    {
        public static DataRect DoZoom(double factor, Point? mousePos, DataRect rect,
            double navigationLimitMaxX, double navigationLimitMinX,
            double navigationLimitMaxY, double navigationLimitMinY,
            bool isHorizontalNavigationEnabled, bool isVerticalNavigationEnabled,
            double preferredAspectRatio, double aspectRatio)
        {
            bool returnToAvailableArea = NeedReturnToAvailableArea(
                ref rect, navigationLimitMaxX, navigationLimitMinX,
                navigationLimitMaxY, navigationLimitMinY);

            if (!returnToAvailableArea)
            {
                if (isHorizontalNavigationEnabled)
                    rect.X = rect.X.Zoom(factor, mousePos?.X);
                if (isVerticalNavigationEnabled)
                    rect.Y = rect.Y.Zoom(factor, mousePos?.Y);

                if (!NavigationXIsInBounds(rect, navigationLimitMaxX, navigationLimitMinX)
                    || !NavigationYIsInBounds(rect, navigationLimitMaxY, navigationLimitMinY))
                {
                    //adjust rect to navigation limits
                    //simple solution
                    //rect = new DataRect(
                    //MathHelper.Clamp(rect.XMin, NavigationLimitMinX, NavigationLimitMaxX),
                    //MathHelper.Clamp(rect.YMin, NavigationLimitMinY, NavigationLimitMaxY),
                    //MathHelper.Clamp(rect.XMax, NavigationLimitMinX, NavigationLimitMaxX),
                    //MathHelper.Clamp(rect.YMax, NavigationLimitMinY, NavigationLimitMaxY));

                    //more sofisticated solution
                    //feels better when try to zoom near navigation limits
                    double xOversizeMax = rect.XMax - navigationLimitMaxX;
                    double xOversizeMin = navigationLimitMinX - rect.XMin;
                    double yOversizeMax = rect.YMax - navigationLimitMaxY;
                    double yOversizeMin = navigationLimitMinY - rect.YMin;

                    double offsetRectX = 0;
                    bool fitToLimitsX = false;
                    if (xOversizeMax > 0 && xOversizeMin <= 0)
                        offsetRectX = -xOversizeMax;
                    else if (xOversizeMax <= 0 && xOversizeMin > 0)
                        offsetRectX = xOversizeMin;
                    else if (xOversizeMax > 0 && xOversizeMin > 0)
                        fitToLimitsX = true;

                    double offsetRectY = 0;
                    bool fitToLimitsY = false;
                    if (yOversizeMax > 0 && yOversizeMin <= 0)
                        offsetRectY = -yOversizeMax;
                    else if (yOversizeMax <= 0 && yOversizeMin > 0)
                        offsetRectY = yOversizeMin;
                    else if (yOversizeMax > 0 && yOversizeMin > 0)
                        fitToLimitsY = true;

                    rect = new DataRect(
                        fitToLimitsX ? navigationLimitMinX : MathHelper.Clamp(rect.XMin + offsetRectX, navigationLimitMinX, navigationLimitMaxX),
                        fitToLimitsY ? navigationLimitMinY : MathHelper.Clamp(rect.YMin + offsetRectY, navigationLimitMinY, navigationLimitMaxY),
                        fitToLimitsX ? navigationLimitMaxX : MathHelper.Clamp(rect.XMax + offsetRectX, navigationLimitMinX, navigationLimitMaxX),
                        fitToLimitsY ? navigationLimitMaxY : MathHelper.Clamp(rect.YMax + offsetRectY, navigationLimitMinY, navigationLimitMaxY));

                }
                else if (preferredAspectRatio > 0 && aspectRatio <= 0)
                {
                    //feel free to adjust preferred aspect ratio if needed
                    //PreferredAspectRatio not used with AspectRatio constraint
                    double aspect = rect.Width / rect.Height;
                    if (aspect != preferredAspectRatio)
                    {
                        if (aspect < preferredAspectRatio)
                        {
                            //need to increase width
                            double prefWidth = preferredAspectRatio * rect.Height;

                            rect = new DataRect(
                                (rect.XMax + rect.XMin) / 2 - prefWidth / 2,
                                rect.YMin,
                                (rect.XMax + rect.XMin) / 2 + prefWidth / 2,
                                rect.YMax);

                            if (!NavigationXIsInBounds(rect, navigationLimitMaxX, navigationLimitMinX))
                            {
                                //can not increase width so much
                                rect = new DataRect(
                                MathHelper.Clamp(rect.XMin, navigationLimitMinX, navigationLimitMaxX),
                                rect.YMin,
                                MathHelper.Clamp(rect.XMax, navigationLimitMinX, navigationLimitMaxX),
                                rect.YMax);
                            }
                        }
                        else
                        {
                            //need to increase height
                            double prefHeight = rect.Width / preferredAspectRatio;

                            rect = new DataRect(
                                rect.XMin,
                                (rect.YMin + rect.YMax) / 2 - prefHeight / 2,
                                rect.XMax,
                                (rect.YMin + rect.YMax) / 2 + prefHeight / 2);

                            if (!NavigationYIsInBounds(rect, navigationLimitMaxY, navigationLimitMinY))
                            {
                                //can not increase height so much
                                rect = new DataRect(
                                rect.XMin,
                                MathHelper.Clamp(rect.YMin, navigationLimitMinY, navigationLimitMaxY),
                                rect.XMax,
                                MathHelper.Clamp(rect.YMax, navigationLimitMinY, navigationLimitMaxY));
                            }
                        }
                    }
                }
            }

            return rect;
        }

        public static bool NeedReturnToAvailableArea(ref DataRect rect,
            double NavigationLimitMaxX, double NavigationLimitMinX,
            double NavigationLimitMaxY, double NavigationLimitMinY)
        {
            if (!NavigationXIsInBounds(rect, NavigationLimitMaxX, NavigationLimitMinX)
                || !NavigationYIsInBounds(rect, NavigationLimitMaxY, NavigationLimitMinY))
            {
                //initial rect not in limits for some reason
                //navigate to available area
                rect = new DataRect(
                MathHelper.Clamp(rect.XMin, NavigationLimitMinX, NavigationLimitMaxX),
                MathHelper.Clamp(rect.YMin, NavigationLimitMinY, NavigationLimitMaxY),
                MathHelper.Clamp(rect.XMax, NavigationLimitMinX, NavigationLimitMaxX),
                MathHelper.Clamp(rect.YMax, NavigationLimitMinY, NavigationLimitMaxY));

                if (rect.XMin == rect.XMax
                    || rect.YMin == rect.YMax)
                {
                    rect = new DataRect(
                        NavigationLimitMinX,
                        NavigationLimitMinY,
                        NavigationLimitMaxX,
                        NavigationLimitMaxY);
                }

                if (double.IsInfinity(rect.XMax) || double.IsNaN(rect.XMax)
                    || double.IsInfinity(rect.YMax) || double.IsNaN(rect.YMax)
                    || double.IsInfinity(rect.XMin) || double.IsNaN(rect.XMin)
                    || double.IsInfinity(rect.YMin) || double.IsNaN(rect.YMin))
                {
                    rect = new DataRect(0, 0, 1, 1);
                }


                return true;
            }

            return false;
        }

        public static bool NavigationXIsInBounds(DataRect rect,
            double navigationLimitMaxX, double navigationLimitMinX)
        {
            return rect.XMax <= navigationLimitMaxX
                && rect.XMin >= navigationLimitMinX;
        }

        public static bool NavigationYIsInBounds(DataRect rect,
            double navigationLimitMaxY, double navigationLimitMinY)
        {
            return rect.YMax <= navigationLimitMaxY
                && rect.YMin >= navigationLimitMinY;
        }

    }
}
