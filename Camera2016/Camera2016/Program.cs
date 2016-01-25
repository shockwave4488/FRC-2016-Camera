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
                if (image == null) return;
                
                //image = image.ConvertTo(image32S, CvType.CV_32SC1)
                var filered = GRIPOperations.HSVThreshold(image);
                
                var contourss = GRIPOperations.FindContours(filered);
                var filteredContourss = GRIPOperations.FilterContours(contourss);
                var convexHulled = GRIPOperations.ConvexHull(contourss);

                CvInvoke.DrawContours(filered, convexHulled.Contours, -1, new MCvScalar(0, 255, 255));

                contourss.Dispose();
                filteredContourss.Dispose();
                convexHulled.Dispose();

                image.Dispose();
                var oldImage = viewer.Image;

                viewer.Image = filered;
                oldImage?.Dispose();

                /*

                Mat HsvIn = new Mat();
                Mat HsvOut = new Mat();
                Mat output = new Mat();
                //output.Create(image.Height, image.Width, DepthType.Cv8U, 1); This is giving me problems for some reason

                if (null == image)
                    return;

                //HSV Filter
                CvInvoke.CvtColor(image, HsvIn, Emgu.CV.CvEnum.ColorConversion.Bgr2Hsv);
                MCvScalar low = new MCvScalar(63, 44, 193);
                MCvScalar high = new MCvScalar(97, 255, 255);
                CvInvoke.InRange(HsvIn, new ScalarArray(low), new ScalarArray(high), HsvOut);

                //Contours
                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                CvInvoke.FindContours(HsvOut, contours, null, RetrType.List, ChainApproxMethod.ChainApproxTc89Kcos);
                CvInvoke.DrawContours(output, contours, -1, new MCvScalar(0, 0, 0));
                
                //Filter contours
                VectorOfVectorOfPoint filteredContours = new VectorOfVectorOfPoint();
                for (int i = 0; i < contours.Size; i++)
                {
                    VectorOfPoint contour = contours[i];
                    //Rectangle r = CvInvoke.BoundingRectangle(contour); <- use if we need min/max width/height

                    if (CvInvoke.ContourArea(contour) < 100) continue;

                    CvInvoke.ConvexHull(contour, new Mat());

                    filteredContours.Push(contour);
                }

                //Convex hull
                VectorOfVectorOfPoint convexHulls = new VectorOfVectorOfPoint(filteredContours.Size);
                for (int i = 0; i < filteredContours.Size; i++)
                {
                    CvInvoke.ConvexHull(filteredContours[i], convexHulls[i]);
                }
                
                //report to NetworkTables

                */

                //viewer.Image = output;
            };

            viewer.ShowDialog();
        }
    }
}
