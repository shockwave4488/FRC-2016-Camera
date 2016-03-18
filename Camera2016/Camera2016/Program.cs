using System;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using System.Drawing;
using NetworkTables;
using NetworkTables.Tables;
using System.Diagnostics;
using System.Threading;

namespace Camera2016
{
    class Program
    {
        static ITable visionTable;


        static MCvScalar Red = new MCvScalar(0, 0, 255);
        static MCvScalar Green = new MCvScalar(0, 255, 0);
        static MCvScalar Blue = new MCvScalar(255, 0, 0);

        static  Point TextPoint = new Point(0, 20);

        static void Main(string[] args)
        {
            NetworkTable.SetClientMode();
            NetworkTable.SetTeam(4488);
            NetworkTable.SetIPAddress("10.44.88.2");
#if KANGAROO
            NetworkTable.SetNetworkIdentity("Kangaroo");
#else
            NetworkTable.SetNetworkIdentity("CameraTracking");
#endif
            //Switch between Kangaroo and Desktop.
            //On kangaroo, use different table and don't display image
            visionTable = NetworkTable.GetTable("SmartDashboard");

            ImageGrabber imageGrabber = new ImageGrabber(visionTable);

            Mat HsvIn = new Mat(), HsvOut = new Mat(), output = new Mat(), Temp = new Mat();
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();

            //VectorOfVectorOfPoint filteredContours = new VectorOfVectorOfPoint();

            //MCvScalar low = new MCvScalar(63, 44, 193);
            //MCvScalar high = new MCvScalar(97, 255, 255);

            double[] defaultLow = new double[] { 50, 44, 193 };
            double[] defaultHigh = new double[] { 110, 255, 255 };

            VectorOfDouble arrayLow = new VectorOfDouble(3);
            VectorOfDouble arrayHigh = new VectorOfDouble(3);

            Point TopMidPoint = new Point((int)(ImageWidth / 2), 0);
            Point BottomMidPoint = new Point((int)(ImageWidth / 2), (int)ImageHeight);

            Point LeftMidPoint = new Point(0, (int)(ImageHeight / 2));
            Point RightMidPoint = new Point((int)ImageWidth, (int)(ImageHeight / 2));

            

            Stopwatch sw = new Stopwatch();

            int count = 0;

            visionTable.PutNumberArray("HSVLow", defaultLow);
            visionTable.PutNumberArray("HSVHigh", defaultHigh);

            visionTable.PutNumber("ShooterOffsetDegreesX", ShooterOffsetDegreesX);
            visionTable.PutNumber("ShooterOffsetDegreesY", ShooterOffsetDegreesY);

            int imageCount = 0;

            while (true)
            {
                count++;
                sw.Restart();
                ImageBuffer image = imageGrabber.Image();

#if KANGAROO
                visionTable.PutNumber("KangarooHeartBeat", count);
#endif

                if (image == null || image.IsEmpty)
                {
                    image?.Dispose();
                    continue;
                }
                double[] ntLow = visionTable.GetNumberArray("HSVLow", defaultLow);
                double[] ntHigh = visionTable.GetNumberArray("HSVHigh", defaultHigh);

                if (ntLow.Length != 3)
                    ntLow = defaultLow;
                if (ntHigh.Length != 3)
                    ntHigh = defaultHigh;

                arrayLow.Clear();
                arrayLow.Push(ntLow);
                arrayHigh.Clear();
                arrayHigh.Push(ntHigh);

                //HSV Filter
                CvInvoke.CvtColor(image.Image, HsvIn, Emgu.CV.CvEnum.ColorConversion.Bgr2Hsv);
                CvInvoke.InRange(HsvIn, arrayLow, arrayHigh, HsvOut);

                HsvOut.ConvertTo(Temp, DepthType.Cv8U);
                //Contours
                CvInvoke.FindContours(Temp, contours, null, RetrType.List, ChainApproxMethod.ChainApproxTc89Kcos);
                //CvInvoke.DrawContours(output, contours, -1, new MCvScalar(0, 0, 0));

                VectorOfVectorOfPoint convexHulls = new VectorOfVectorOfPoint(contours.Size);

                for (int i = 0; i < contours.Size; i++)
                {
                    CvInvoke.ConvexHull(contours[i], convexHulls[i]);
                }

                Rectangle? largestRectangle = null;
                double currentLargestArea = 0.0;

                //Filter contours
                for (int i = 0; i < convexHulls.Size; i++)
                {
                    VectorOfPoint contour = convexHulls[i];
                    VectorOfPoint polygon = new VectorOfPoint(convexHulls.Size);
                    CvInvoke.ApproxPolyDP(contour, polygon, 10, true);

                    //VectorOfVectorOfPoint cont = new VectorOfVectorOfPoint(1);
                    //cont.Push(polygon);

                    //CvInvoke.DrawContours(image.Image, cont,-1, Green, 2);

                    if (polygon.Size != 4) continue;
                    if (!CvInvoke.IsContourConvex(polygon)) continue;

                    int numVertical = 0;
                    int numHorizontal = 0;
                    for (int j = 0; j < 4; j++)
                    {
                        double dx = polygon[j].X - polygon[(j + 1) % 4].X;
                        double dy = polygon[j].Y - polygon[(j + 1) % 4].Y;
                        double slope = double.MaxValue;

                        if (dx != 0) slope = Math.Abs(dy / dx);

                        double nearlyHorizontalSlope = Math.Tan(ToRadians(20));
                        double rad = ToRadians(60);
                        double nearlyVerticalSlope = Math.Tan(rad);

                        if (slope > nearlyVerticalSlope) numVertical++;
                        if (slope < nearlyHorizontalSlope) numHorizontal++;
                    }

                    if (numHorizontal < 1) continue;

                    Rectangle bounds = CvInvoke.BoundingRectangle(polygon);

                    double ratio = (double)bounds.Height / bounds.Width;
                    if (ratio > 1.0 || ratio < .3) continue;

                    double area = CvInvoke.ContourArea(contour);

                    if (area < 100) continue;

                    CvInvoke.Rectangle(image.Image, bounds, Blue, 2);

                    if (area > currentLargestArea)
                    {
                        largestRectangle = bounds;
                    }

                    //filteredContours.Push(contour);


                    polygon.Dispose();
                }
                visionTable.PutBoolean("TargetFound", largestRectangle != null);

                if (largestRectangle != null)
                {
                    ProcessData(largestRectangle.Value, image);
                    CvInvoke.Rectangle(image.Image, largestRectangle.Value, Red, 5);
                }

                //ToDo, Draw Crosshairs
                CvInvoke.Line(image.Image, TopMidPoint, BottomMidPoint, Blue, 3);
                CvInvoke.Line(image.Image, LeftMidPoint, RightMidPoint, Blue, 3);

                int fps = (int)(1.0 / sw.Elapsed.TotalSeconds);

                //CvInvoke.PutText(image.Image, fps.ToString(), TextPoint, FontFace.HersheyPlain, 2, Green);

                imageCount++;
                CvInvoke.Imshow("HSV", HsvOut);
                CvInvoke.Imshow("MainWindow", image.Image);
                image.Dispose();
                


                //report to NetworkTables

                //Cleanup

                for (int i = 0; i < contours.Size; i++)
                {
                    contours[i].Dispose();
                }
                contours.Clear();

                for (int i = 0; i < convexHulls.Size; i++)
                {
                    convexHulls[i].Dispose();
                }
                convexHulls.Dispose();
                /*
                for (int i = 0; i < filteredContours.Size; i++)
                {
                    filteredContours[i].Dispose();
                }
                filteredContours.Clear();
                */

                CvInvoke.WaitKey(1);
            };
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        private static double ToDegrees(double radians)
        {
            return radians * 180.0 / Math.PI;
        }
#if KANGAROO
        const double ImageWidth = 800.0;
        const double ImageHeight = 600.0;
#else
        const double ImageWidth = 720.0;
        const double ImageHeight = 1280.0;
#endif
        const double HorizontalFOVDeg = 43.3;
        const double VerticalFOVDeg = 70.42;//ImageHeight / ImageWidth * HorizontalFOVDeg;

        static double ShooterOffsetDegreesX = 0;
        private static double ShooterOffsetDegreesY = 0;

        static double TopTargetHeight = 89; //In Inches

        const double CameraShooterOffset = -12.5/12;
        const double CameraAngle = 42.5;



        public static void ProcessData(Rectangle valid, ImageBuffer image)
        {
            //ShooterOffsetDegreesX = visionTable.GetNumber("ShooterOffsetDegreesX", 0.0);
            //ShooterOffsetDegreesY = visionTable.GetNumber("ShooterOffsetDegreesY", 0.0);

            double x = valid.X + (valid.Width / 2.0);
            x = (2 * (x / ImageWidth)) - 1;
            double y = valid.Y + (valid.Height / 2.0);
            y = -((2 * (y / ImageHeight)) - 1);

            double oldHeading = image.GyroAngle;
            //double currentAngle = image.ShooterAngle;

            double radiusShooter = 16.67;
            double currentCameraHeight = 14.75 / 12.0;//Math.Sin(ToRadians(currentAngle + 6.67)) * radiusShooter + 10.0;//Some sin function I dont wanna do right now.
            //Output in Meters
            //currentAngle = currentAngle - 25;//24.4

            double range = (TopTargetHeight - currentCameraHeight) / Math.Tan((y * VerticalFOVDeg / 2.0 + CameraAngle) * (Math.PI / 180.0));
            //range *= 1.696;
            range = range / 12.0;

            ShooterOffsetDegreesX = Math.Asin(CameraShooterOffset/range)*(180.0/Math.PI);

             CvInvoke.PutText(image.Image, range.ToString(), TextPoint, FontFace.HersheyPlain, 2, Green);

            double azimuthX = BoundAngle0to360Degrees(x * HorizontalFOVDeg / 2.0 + oldHeading + ShooterOffsetDegreesX);
            double azimuthY = BoundAngle0to360Degrees(y * VerticalFOVDeg / 2.0 + (CameraAngle) + ShooterOffsetDegreesY);
            visionTable.PutNumber("Offset", ShooterOffsetDegreesX);
            visionTable.PutNumber("AzimuthX", azimuthX);
            visionTable.PutNumber("AzimuthY", azimuthY);
            visionTable.PutNumber("Range", range);
            NetworkTable.Flush();
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

    }
}
