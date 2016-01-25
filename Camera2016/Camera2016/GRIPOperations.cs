using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV.Util;
using System.Drawing;

namespace Camera2016
{
    static class GRIPOperations
    {
        private static double kNearlyHorizontalSlope = 0;
        private static double kNearlyVerticalSlope = 0;

        public static Mat HSVThreshold(Mat input)
        {
            MCvScalar low = new MCvScalar(63, 44, 193);
            MCvScalar high = new MCvScalar(97, 255, 255);

            Mat hsv = new Mat();
            Mat output = new Mat();

            CvInvoke.CvtColor(input, hsv, ColorConversion.Bgr2Hsv);
            CvInvoke.InRange(hsv, new ScalarArray(low), new ScalarArray(high), output);
            hsv.Dispose();
            return output;
        }

        public static ContoursReport FindContours(Mat input)
        {
            Image<Gray, byte> tmp = input.ToImage<Gray, byte>();

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(tmp, contours, null, RetrType.List, ChainApproxMethod.ChainApproxTc89Kcos);

            tmp.Dispose();
            return new ContoursReport(contours, input.Rows, input.Cols);
        }

        public static ContoursReport ConvexHull(ContoursReport inReport)
        {
            var inputContours = inReport.Contours;
            VectorOfVectorOfPoint outputContours = new VectorOfVectorOfPoint(inputContours.Size);
            for (int i = 0; i < inputContours.Size; i++)
            {
                CvInvoke.ConvexHull(inputContours[i], outputContours[i]);
            }

            return new ContoursReport(outputContours, inReport.Rows, inReport.Cols);
        }

        public static ContoursReport FilterContours(ContoursReport input)
        {
            double minArea = 0;
            double minPerimeter = 0;
            double minWidth = 0;
            double maxWidth = 0;
            double minHeight = 0;
            double maxHeight = 0;
            double minSolidity = 0;
            double maxSolidity = 0;


            var inputContours = input.Contours;
            var outputContours = new VectorOfVectorOfPoint(inputContours.Size);

            VectorOfPoint hull = new VectorOfPoint();

            for (int i = 0; i < inputContours.Size; i++)
            {
                VectorOfPoint contour = inputContours[i];

                Rectangle bb = CvInvoke.BoundingRectangle(contour);
                if (bb.Width < minWidth || bb.Width > maxWidth) continue;
                if (bb.Height < minHeight || bb.Height > maxHeight) continue;

                double area = CvInvoke.ContourArea(contour);

                if (area < minArea) continue;
                if (CvInvoke.ArcLength(contour, true) < minPerimeter) continue;

                CvInvoke.ConvexHull(contour, hull);
                double solidity = 100 * area / CvInvoke.ContourArea(hull);

                if (solidity < minSolidity || solidity > maxSolidity) continue;

                

                outputContours.Push(contour);
            }

            hull.Dispose();

            return new ContoursReport(outputContours, input.Rows, input.Cols);
        }

        public static ContoursReport CustomFilter(ContoursReport input)
        {
            var inputContours = input.Contours;
            var outputContours = new VectorOfVectorOfPoint(inputContours.Size);

            //VectorOfPoint hull = new VectorOfPoint();

            for (int i = 0; i < inputContours.Size; i++)
            {
                VectorOfPoint contour = inputContours[i];
                if (contour.Size != 4 || !CvInvoke.IsContourConvex(contour)) 
                    //We are not a 4 sided polygon
                    continue;
                int numNearlyHorizontal = 0;
                int numNearlyVertical = 0;

                for (int j = 0; j < 4; j++)
                {
                    double dy = contour[j].Y - contour[(j + 1) % 4].Y;
                    double dx = contour[j].X - contour[(j + 1) % 4].X;

                    double slope = double.MaxValue;
                    if (dx != 0) slope = Math.Abs(dy / dx);
                    if (slope < kNearlyHorizontalSlope)
                        ++numNearlyHorizontal;
                    else if (slope > kNearlyVerticalSlope)
                        ++numNearlyVertical;

                    if (numNearlyHorizontal >= 1 && numNearlyVertical == 2)
                    {
                        //Draw Polygon

                        outputContours.Push(contour);
                    }
                }
            }

            return new ContoursReport(outputContours, input.Rows, input.Cols);
        }


        //More things to do
        //Check for Rectangle
        //Check 
    }
}
