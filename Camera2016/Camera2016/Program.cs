using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using System.Windows.Forms;

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
                Mat buf1 = i.Image();
                Mat buf2 = new Mat();
                Mat output = new Mat();

                if (null == buf1)
                    return;

                //HSV Filter
                CvInvoke.CvtColor(buf1, buf2, Emgu.CV.CvEnum.ColorConversion.Bgr2Hsv);
                MCvScalar low = new MCvScalar(63, 44, 193);
                MCvScalar high = new MCvScalar(97, 255, 255);
                CvInvoke.InRange(buf2, new ScalarArray(low), new ScalarArray(high), output);

                //Contours


                //Filter contours


                //Convex hull

                
                //report to NetworkTables



                viewer.Image = output;
            };

            viewer.ShowDialog();
        }
    }
}
