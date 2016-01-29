using System;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using System.Drawing;

namespace Camera2016
{
    class Program
    {
        static void Main(string[] args)
        {
            ImageViewer viewer = new ImageViewer();
            ImageGrabber imageGrabber = new ImageGrabber();

            Mat HsvIn = new Mat(), HsvOut = new Mat(), output = new Mat(), Temp = new Mat();
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            VectorOfVectorOfPoint convexHulls = new VectorOfVectorOfPoint();
            VectorOfVectorOfPoint filteredContours = new VectorOfVectorOfPoint();

            MCvScalar low = new MCvScalar(63, 44, 193);
            MCvScalar high = new MCvScalar(97, 255, 255);

            Application.Idle += (o, s) =>
            {
                Mat image = imageGrabber.Image();

                if (image.IsEmpty)
                    return;

                //HSV Filter
                CvInvoke.CvtColor(image, HsvIn, Emgu.CV.CvEnum.ColorConversion.Bgr2Hsv);
                CvInvoke.InRange(HsvIn, new ScalarArray(low), new ScalarArray(high), HsvOut);
                
                HsvOut.ConvertTo(Temp, DepthType.Cv8U);
                //Contours
                CvInvoke.FindContours(Temp, contours, null, RetrType.List, ChainApproxMethod.ChainApproxTc89Kcos);
                //CvInvoke.DrawContours(output, contours, -1, new MCvScalar(0, 0, 0));
                
                for (int i = 0; i < contours.Size; i++)
                {
                    CvInvoke.ConvexHull(contours[i], convexHulls[i]);
                }

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
                        double dx = polygon[j].X - polygon[(j - 1)%4].X;
                        double dy = polygon[j].Y - polygon[(j - 1) % 4].Y;
                        double slope = double.MaxValue;

                        if (dx != 0) Math.Abs(dy / dx);

                        double nearlyHorizontalSlope = Math.Tan(ToRadians(20));
                        double nearlyVerticalSlope = Math.Tan(ToRadians(70));

                        if (slope > nearlyVerticalSlope) numVertical++;
                        if (slope < nearlyHorizontalSlope) numHorizontal++;
                    }

                    if (numVertical != 2 || numHorizontal < 1) continue;

                    Rectangle bounds = CvInvoke.BoundingRectangle(polygon);

                    double ratio = bounds.Height/bounds.Width;
                    if (ratio > 1.0 || ratio < 0.5) continue;

                    if (CvInvoke.ContourArea(contour) < 100) continue;

                    filteredContours.Push(contour);

                    polygon.Dispose();
                }

                //report to NetworkTables



                viewer.Image = output;

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
                convexHulls.Clear();

                for (int i = 0; i < filteredContours.Size; i++)
                {
                    filteredContours[i].Dispose();
                }
                filteredContours.Clear();
            };

            viewer.ShowDialog();
        }

        private static double ToRadians(double degrees)
        {
            return degrees*Math.PI/180.0;
        }
    }
}
