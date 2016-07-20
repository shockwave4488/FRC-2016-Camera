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

        static Point TextPoint = new Point(0, 20);
        static Point TextPoint2 = new Point(0, 50);
        static Point TextPoint3 = new Point(0, 80);
        static Point TextPoint4 = new Point(0, 110);
        static Point TextPoint5 = new Point(0, 140);

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

            //ImageGrabber imageGrabber = new ImageGrabber(visionTable);

            Mat HsvIn = new Mat(), HsvOut = new Mat(), output = new Mat(), Temp = new Mat();
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();

            //VectorOfVectorOfPoint filteredContours = new VectorOfVectorOfPoint();

            //MCvScalar low = new MCvScalar(63, 44, 193);
            //MCvScalar high = new MCvScalar(97, 255, 255);

            double[] defaultLow = new double[] { 50, 44, 193 };
            double[] defaultHigh = new double[] { 90, 255, 255 };

            VectorOfDouble arrayLow = new VectorOfDouble(3);
            VectorOfDouble arrayHigh = new VectorOfDouble(3);

            Point TopMidPoint = new Point((int)(ImageWidth / 2), 0);
            Point BottomMidPoint = new Point((int)(ImageWidth / 2), (int)ImageHeight);

            Point LeftMidPoint = new Point(0, (int)(ImageHeight / 2));
            Point RightMidPoint = new Point((int)ImageWidth, (int)(ImageHeight / 2));

            Stopwatch sw = new Stopwatch();

            int count = 0;

            //visionTable.PutNumberArray("HSVLow", defaultLow);
            //visionTable.PutNumberArray("HSVHigh", defaultHigh);

            visionTable.PutNumber("ShooterOffsetDegreesX", ShooterOffsetDegreesX);
            visionTable.PutNumber("ShooterOffsetDegreesY", ShooterOffsetDegreesY);

            Thread timer = new Thread(() =>
                {
                    while (true)
                    {
                        visionTable.PutNumber("KangarooBattery",
                            System.Windows.Forms.SystemInformation.PowerStatus.BatteryLifePercent);
                        Thread.Sleep(5000);
                    }

                });
            timer.Start();
            GC.KeepAlive(timer);
            int imageCount = 0;

            ImageBuffer im = new ImageBuffer();
            Capture cap = new Capture(0); //Change me to 1 to use external camera
            cap.FlipVertical = true;

            cap.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 1280);
            cap.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 720);

            ImageSaver saver = new ImageSaver();
            int saveCount = 0;
            int rdi = 1;
            int kernalSize = 6 * rdi + 1;
            Size ksize = new Size(kernalSize, kernalSize);

            while (true)
            {
                count++;
                sw.Restart();
                //ImageBuffer image = imageGrabber.Image();
                cap.Grab();
                im.GyroAngle = visionTable.GetNumber("Gyro", 0.0);
                cap.Retrieve(im.Image);

                ImageBuffer image = im.Clone();

#if KANGAROO
                visionTable.PutNumber("KangarooHeartBeat", count);
#endif
                if (image == null || image.IsEmpty)
                {
                    image?.Dispose();
                    Thread.Yield();
                    continue;
                }

                if (visionTable.GetBoolean("LightsOn", false))
                {
                    saveCount++;
                    if (saveCount >= 6)
                    {
                        saver.AddToQueue(image.Image);
                        saveCount = 0;
                    }
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

                Mat BlurTemp = new Mat();
                CvInvoke.GaussianBlur(image.Image, BlurTemp, ksize, rdi);
                Mat oldImage = image.Image;
                image.Image = BlurTemp;
                oldImage.Dispose();

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

                    // Filter if shape has more than 4 corners after contour is applied
                    if (polygon.Size != 4)
                    {
                        polygon.Dispose();
                        continue;
                    }

                    // Filter if not convex
                    if (!CvInvoke.IsContourConvex(polygon))
                    {
                        polygon.Dispose();
                        continue;
                    }

                    ///////////////////////////////////////////////////////////////////////
                    // Filter if there isn't a nearly horizontal line
                    ///////////////////////////////////////////////////////////////////////
                    //int numVertical = 0;
                    int numHorizontal = 0;
                    for (int j = 0; j < 4; j++)
                    {
                        double dx = polygon[j].X - polygon[(j + 1) % 4].X;
                        double dy = polygon[j].Y - polygon[(j + 1) % 4].Y;
                        double slope = double.MaxValue;

                        if (dx != 0) slope = Math.Abs(dy / dx);

                        double nearlyHorizontalSlope = Math.Tan(ToRadians(20));
                        //double rad = ToRadians(60);
                        //double nearlyVerticalSlope = Math.Tan(rad);

                        //if (slope > nearlyVerticalSlope) numVertical++;
                        if (slope < nearlyHorizontalSlope) numHorizontal++;
                    }

                    if (numHorizontal < 1)
                    {
                        polygon.Dispose();
                        continue;
                    }
                    ///////////////////////////////////////////////////////////////////////

                    ///////////////////////////////////////////////////////////////////////
                    // Filter if polygon is above a set limit. This should remove overhead lights and windows
                    ///////////////////////////////////////////////////////////////////////
                    Rectangle bounds = CvInvoke.BoundingRectangle(polygon);
                    int topY = 350;
                    if (bounds.Location.Y < topY)
                    {
                        polygon.Dispose();
                        continue;
                    }
                    ///////////////////////////////////////////////////////////////////////

                    //CvInvoke.PutText(image.Image, contours[i].Size.ToString(), TextPoint, FontFace.HersheyPlain, 2, Green);

                    ///////////////////////////////////////////////////////////////////////
                    // Filter by height to width ratio
                    ///////////////////////////////////////////////////////////////////////
                    double ratio = (double)bounds.Height / bounds.Width;
                    if (ratio > 1.0 || ratio < .3)
                    {
                        polygon.Dispose();
                        continue;
                    }
                    ///////////////////////////////////////////////////////////////////////

                    ///////////////////////////////////////////////////////////////////////
                    // Filter by area to vertical position ratio
                    ///////////////////////////////////////////////////////////////////////
                    double area = CvInvoke.ContourArea(contour);
                    double areaVertRatio = area / (1280 - bounds.Location.Y);

                    if (areaVertRatio < 9 || areaVertRatio > 19)
                    {
                        polygon.Dispose();
                        continue;
                    }
                    ///////////////////////////////////////////////////////////////////////

                    CvInvoke.PutText(image.Image, "Vertical: " + (1280 - bounds.Location.Y).ToString(), TextPoint, FontFace.HersheyPlain, 2, Green);
                    CvInvoke.PutText(image.Image, "Area: " + area.ToString(), TextPoint2, FontFace.HersheyPlain, 2, Green);
                    CvInvoke.PutText(image.Image, "Area/Vert: " + areaVertRatio.ToString(), TextPoint3, FontFace.HersheyPlain, 2, Green);

                    CvInvoke.Rectangle(image.Image, bounds, Blue, 2);

                    if (area > currentLargestArea)
                    {
                        largestRectangle = bounds;
                    }

                    //filteredContours.Push(contour);

                    polygon.Dispose();
                }
                visionTable.PutBoolean("TargetFound", largestRectangle != null);
                //CvInvoke.PutText(image.Image, "Target found: " + (largestRectangle != null).ToString(), TextPoint5, FontFace.HersheyPlain, 2, Green);


                if (largestRectangle != null)
                {
                    ProcessData(largestRectangle.Value, image);
                    CvInvoke.Rectangle(image.Image, largestRectangle.Value, Red, 5);
                }

                //ToDo, Draw Crosshairs
                //CvInvoke.Line(image.Image, TopMidPoint, BottomMidPoint, Blue, 3);
                //CvInvoke.Line(image.Image, LeftMidPoint, RightMidPoint, Blue, 3);

                //int fps = (int)(1.0 / sw.Elapsed.TotalSeconds);
                //CvInvoke.PutText(image.Image, fps.ToString(), TextPoint, FontFace.HersheyPlain, 2, Green);

                imageCount++;

                //CvInvoke.Imshow("HSV", HsvOut);
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

        static double TopTargetCenterHeight = 90; //In Inches
        
        const double CameraVerticalAngle = 33; //Relative to ground
        const double currentCameraHeight = 13.5;

        public static void ProcessData(Rectangle valid, ImageBuffer image)
        {
            double x = valid.X + (valid.Width / 2.0);
            x = (2 * (x / ImageWidth)) - 1;
            double y = valid.Y + (valid.Height / 2.0);
            y = -((2 * (y / ImageHeight)) - 1);

            double oldHeading = image.GyroAngle;
            double range = (TopTargetCenterHeight - currentCameraHeight) / Math.Tan((y * VerticalFOVDeg / 2.0 + CameraVerticalAngle) * (Math.PI / 180.0));
            //CvInvoke.PutText(image.Image, "Range: " + range.ToString(), TextPoint4, FontFace.HersheyPlain, 2, Green);

            double azimuthX = BoundAngleNeg180To180Degrees(x * HorizontalFOVDeg / 2.0 + oldHeading + ShooterOffsetDegreesX);
            visionTable.PutNumber("AzimuthX", azimuthX);
            visionTable.PutNumber("Range", range); // in inches
            NetworkTable.Flush();
        }

        /*
        public static void ProcessData(Rectangle target, ImageBuffer image)
        {
            //ShooterOffsetDegreesX = visionTable.GetNumber("ShooterOffsetDegreesX", 0.0);
            //ShooterOffsetDegreesY = visionTable.GetNumber("ShooterOffsetDegreesY", 0.0);

            double targetCenterY = target.Y + (target.Height / 2.0); //Calculate center y coordinate of target
            double relativeTargetY = 1 - (targetCenterY / ImageHeight); //Find relative center position vs image height. Subtract 1 to invert since 1280 is at bottom
            double cameraHeight = 14.75; //Inches from ground to camera lens
            double range = (TopTargetCenterHeight - cameraHeight) / Math.Tan((relativeTargetY * VerticalFOVDeg - (VerticalFOVDeg / 2.0) + CameraVerticalAngle) * (Math.PI / 180.0));
            range = range / 12.0;

            CvInvoke.PutText(image.Image, range.ToString(), TextPoint, FontFace.HersheyPlain, 2, Green);

            double oldHeading = image.GyroAngle;
            double targetCenterX = target.X + (target.Width / 2.0); //Calculate center x coordinate of target
            double relativeTargetX = targetCenterX / ImageWidth; //Find relative center position vs image width
            double targetHorizontalAngle = relativeTargetX * HorizontalFOVDeg - (HorizontalFOVDeg / 2.0) + oldHeading;

            CvInvoke.PutText(image.Image, targetHorizontalAngle.ToString(), TextPoint2, FontFace.HersheyPlain, 2, Green);

            //visionTable.PutNumber("Offset", ShooterOffsetDegreesX);
            visionTable.PutNumber("AzimuthX", targetHorizontalAngle);
            visionTable.PutNumber("Range", range);
            NetworkTable.Flush();
        }*/


        private static double BoundAngleNeg180To180Degrees(double angle)
        {
            while (angle <= -180) angle += 360;
            while (angle > 180) angle -= 360;
            return angle;

            /*
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
        */
    }
    }
}
