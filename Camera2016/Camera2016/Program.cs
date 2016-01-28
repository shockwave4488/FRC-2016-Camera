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

            Application.Idle += (o, s) =>
            {
                Mat image = imageGrabber.Image();
                Mat HsvIn = new Mat();
                Mat HsvOut = new Mat();
                Mat output = new Mat();
                //output.Create(image.Height, image.Width, DepthType.Cv8U, 1); This is giving me problems for some reason

                if (image.IsEmpty)
                    return;

                //HSV Filter
                CvInvoke.CvtColor(image, HsvIn, Emgu.CV.CvEnum.ColorConversion.Bgr2Hsv);
                MCvScalar low = new MCvScalar(63, 44, 193);
                MCvScalar high = new MCvScalar(97, 255, 255);
                CvInvoke.InRange(HsvIn, new ScalarArray(low), new ScalarArray(high), HsvOut);

                Mat Temp = new Mat();
                HsvOut.ConvertTo(Temp, DepthType.Cv8U);
                //Contours
                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                CvInvoke.FindContours(Temp, contours, null, RetrType.List, ChainApproxMethod.ChainApproxTc89Kcos);
                //CvInvoke.DrawContours(output, contours, -1, new MCvScalar(0, 0, 0));

                VectorOfVectorOfPoint convexHulls = new VectorOfVectorOfPoint(filteredContours.Size);
                for (int i = 0; i < contours.Size; i++)
                {
                    CvInvoke.ConvexHull(contours[i], convexHulls[i]);
                }
                //Filter contours
                VectorOfVectorOfPoint filteredContours = new VectorOfVectorOfPoint();
                for (int i = 0; i < convexHulls.Size; i++)
                {
                    VectorOfPoint contour = convexHulls[i];
                    VectorOfPoint polygon = new VectorOfPoint(convexHulls.Size);
                    CvInvoke.ApproxPolyDP(contour, polygon, 10, true);
                    if (polygon.Size != 4) continue;
                    if (!CvInvoke.IsContourConvex(polygon)) continue;
                    for (int j = 0; j < 4; j++)
                    {
                        double dx = polygon[j].X - polygon[(j - 1)%4].X;
                        double dy = polygon[j].Y - polygon[(j - 1) % 4].Y;
                        double slope = double.MaxValue;
                        if (dx != 0) Math.Abs(dy / dx);
                        double nearlyHorizontalSlope = Math.Tan(ToRadians(20));
                        double nearlyVerticalSlope = Math.Tan(ToRadians(70));
                    }  
                    //Rectangle r = CvInvoke.BoundingRectangle(contour); <- use if we need min/max width/height

                    if (CvInvoke.ContourArea(contour) < 100) continue;

                    filteredContours.Push(contour);
                }
                //report to NetworkTables



                viewer.Image = output;
            };

            viewer.ShowDialog();
        }

        private static double ToRadians(double degrees)
        {
            return degrees*Math.PI/180.0;
        }
    }
}
