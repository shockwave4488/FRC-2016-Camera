using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camera2016
{
    public class ImageBuffer : IDisposable
    {
        public Mat Image { get; set; }
        public double GyroAngle { get; set; }
        public double ShooterAngle { get; set; }

        public ImageBuffer(Mat image)
        {
            Image = image;
        }

        public ImageBuffer()
        {
            Image = new Mat();
        }

        public ImageBuffer Clone()
        {
            Mat mat = new Mat();
            CvInvoke.Transpose(Image, mat);

            ImageBuffer buf = new ImageBuffer(mat);
            buf.GyroAngle = GyroAngle;
            buf.ShooterAngle = ShooterAngle;
            return buf;
        }

        public bool IsEmpty => Image.IsEmpty;

        public void Dispose()
        {
            Image.Dispose();
        }


    }
}
