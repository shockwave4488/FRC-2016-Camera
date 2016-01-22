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
                Mat image = i.Image();
                Mat output = new Mat();
                Mat hsv = new Mat();

                if (null == image)
                    return;

                CvInvoke.CvtColor(image, hsv, Emgu.CV.CvEnum.ColorConversion.Bgr2Hsv);
                MCvScalar low = new MCvScalar(81, 103, 186);
                MCvScalar high = new MCvScalar(96, 255, 255);
                CvInvoke.InRange(hsv, new ScalarArray(low), new ScalarArray(high), output);
                //cvtColor(input, hsv, COLOR_BGR2HSV);
                //inRange(hsv, low, high, output);
                //outputSocket.setValue(output);

                viewer.Image = output;

                //Thread.Sleep(100);
            };

            viewer.ShowDialog();
        }
    }
}
