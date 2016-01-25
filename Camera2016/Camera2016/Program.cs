using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using System.Windows.Forms;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using System.Drawing;


namespace CameraThing
{
    class Program
    {
        static void Main(string[] args)
        {
            ImageViewer viewer = new ImageViewer();

            ImageGrabber i = new ImageGrabber();

            Application.Idle += (o, s) =>
            {
                //Switch between buf1 and buf2 when passing by reference
                //use output on the final operation
                Mat image = i.Image();
                Mat HsvIn = new Mat();
                Mat HsvOut = new Mat();
                Mat output = new Mat();

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
                
                //Filter contours


                //Convex hull

                
                //report to NetworkTables



                viewer.Image = output;
            };

            viewer.ShowDialog();
        }
    }
}
