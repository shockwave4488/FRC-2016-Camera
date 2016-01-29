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
        private static double kNearlyHorizontalSlope =  Math.Tan(0.349066);
        private static double kNearlyVerticalSlope = Math.Tan(1.22173);

        public static Mat HSVThreshold(Mat input)
        {
            MCvScalar low = new MCvScalar(47, 37, 37);
            MCvScalar high = new MCvScalar(105, 255, 255);

            Mat hsv = new Mat();
            Mat output = new Mat();

            CvInvoke.CvtColor(input, hsv, ColorConversion.Bgr2Hsv);
            CvInvoke.InRange(hsv, new ScalarArray(low), new ScalarArray(high), output);
            hsv.Dispose();
            return output;
        }

        public static ContoursReport FindContours(Mat input)
        {
            Mat tmp = new Mat();
            input.ConvertTo(tmp, DepthType.Cv8U);

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

                VectorOfPoint outputs = new VectorOfPoint(inputContours.Size);
                CvInvoke.ApproxPolyDP(contour, outputs, 10, true);

                contour = outputs;

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

        static double HorizontalFOVDeg = 47.0;
        static double VerticalFOVDeg = 240.0 / 320.0 * HorizontalFOVDeg;

        static double ShooterOffsetDegrees = 0;

        static double TopTargetHeight = 100; //In Inches


        public static void GetDataToSend(Rectangle valid)
        {
            double x = valid.X + (valid.Width / 2);
            x = (2 * (x / 320.0)) - 1;
            double y = valid.Y + (valid.Height / 2);
            y = (2 * (y / 240.0)) - 1;

            double oldHeading = 0;
            double currentAngle = 0;

            double azimuthX = BoundAngle0to360Degrees(x * HorizontalFOVDeg / 2.0 + oldHeading + ShooterOffsetDegrees);
            double azimuthY = BoundAngle0to360Degrees(y * VerticalFOVDeg / 2.0 + currentAngle);

            double radiusShooter = 1; //Meter
            double currentCameraHeight = 0;//Some sin function I dont wanna do right now.

            double range = (TopTargetHeight - currentCameraHeight) / Math.Tan((y * VerticalFOVDeg / 2.0 + currentAngle) * Math.PI / 180.0);
        }


        private static double BoundAngle0to360Degrees(double angle)
        {
            // Naive algorithm
            while (angle >= 360.0)
            {
                angle -= 360.0;
            }
            while (angle < 0.0)
            {
                angle += 360.0;
            }
            return angle;
        }


        //More things to do
        //Check for Rectangle
        //Check 
    }
}
