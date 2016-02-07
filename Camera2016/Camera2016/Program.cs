using System;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using System.Drawing;
using System.Net;
using System.Net.NetworkInformation;
using NetworkTables;
using NetworkTables.Tables;

namespace Camera2016
{
    class Program
    {
        static void Main(string[] args)
        {
            NetworkTable.SetClientMode();
            NetworkTable.SetTeam(4488);
            ITable table = NetworkTable.GetTable("SmartDashboard"); 
            table.SetPersistent("HSVHIGH");
            NetworkTable.SetNetworkIdentity("Kangaroo");

            AngledMat.NetworkTable = table;

            var arr = table.GetNumberArray("HSVHIGH", new double[3]);

            //ImageViewer viewer = new ImageViewer();
            ImageGrabber imageGrabber = new ImageGrabber();

            Mat HsvIn = new Mat(), HsvOut = new Mat(), output = new Mat(), Temp = new Mat();
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            VectorOfVectorOfPoint filteredContours = new VectorOfVectorOfPoint();

            MCvScalar low = new MCvScalar(63, 44, 193);
            MCvScalar high = new MCvScalar(97, 255, 255);
            MCvScalar red = new MCvScalar(0, 0, 255);

            while(true)
            {
                AngledMat image = imageGrabber.Image();

                if (image.IsEmpty)
                    continue;

                //HSV Filter
                CvInvoke.CvtColor(image.Image, HsvIn, Emgu.CV.CvEnum.ColorConversion.Bgr2Hsv);
                CvInvoke.InRange(HsvIn, new ScalarArray(low), new ScalarArray(high), HsvOut);
                
                HsvOut.ConvertTo(Temp, DepthType.Cv8U);
                //Contours
                CvInvoke.FindContours(Temp, contours, null, RetrType.List, ChainApproxMethod.ChainApproxTc89Kcos);
                //CvInvoke.DrawContours(output, contours, -1, new MCvScalar(0, 0, 0));


                VectorOfVectorOfPoint convexHulls = new VectorOfVectorOfPoint(contours.Size);
                for (int i = 0; i < contours.Size; i++)
                {
                    CvInvoke.ConvexHull(contours[i], convexHulls[i]);
                }

                Rectangle? largest = null;
                double largestArea = 0;

                //Filter contours
                for (int i = 0; i < convexHulls.Size; i++)
                {
                    VectorOfPoint contour = convexHulls[i];
                    VectorOfPoint polygon = new VectorOfPoint(convexHulls.Size);
                    CvInvoke.ApproxPolyDP(contour, polygon, 10, true);

                    if (polygon.Size != 4) continue;
                    if (!CvInvoke.IsContourConvex(polygon)) continue;
                    
                    int numVertical = 0;
                    int numHorizontal = 0;
                    for (int j = 0; j < 4; j++)
                    {
                        double dx = polygon[j].X - polygon[(j + 1)%4].X;
                        double dy = polygon[j].Y - polygon[(j + 1) % 4].Y;
                        double slope = double.MaxValue;

                        if (dx != 0) slope = Math.Abs(dy / dx);

                        double nearlyHorizontalSlope = Math.Tan(ToRadians(20));
                        double nearlyVerticalSlope = Math.Tan(ToRadians(70));

                        if (slope > nearlyVerticalSlope) numVertical++;
                        if (slope < nearlyHorizontalSlope) numHorizontal++;
                    }

                    if (numVertical != 2 || numHorizontal < 1) continue;

                    Rectangle bounds = CvInvoke.BoundingRectangle(polygon);

                    double ratio = (double)bounds.Height/(double)bounds.Width;
                    if (ratio > 1.0 || ratio < 0.3) continue;

                    double area = CvInvoke.ContourArea(contour);

                    if (area < 100) continue;

                    if (largest == null || area > largestArea)
                    {
                        largest = bounds;
                        largestArea = area;
                    }

                    filteredContours.Push(contour);

                    polygon.Dispose();
                }

                //report to NetworkTables

                //Draw image to viewer
                if (largest != null)
                {
                    CvInvoke.Rectangle(image.Image, largest.Value, red, 2);
                }

                CvInvoke.Imshow("Main", image.Image);
                CvInvoke.Imshow("HSV", HsvOut);
                image.Dispose();

                //Cleanup

                for (int i = 0; i < contours.Size; i++)
                {
                    contours[i].Dispose();
                }
                contours.Clear();
                
                convexHulls.Dispose();

                for (int i = 0; i < filteredContours.Size; i++)
                {
                    filteredContours[i].Dispose();
                }
                filteredContours.Clear();

                CvInvoke.WaitKey(1);
            };
        }

        private static double ToRadians(double degrees)
        {
            return degrees*Math.PI/180.0;
        }
    }
}
