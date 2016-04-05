using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;

namespace Camera2016
{
    public class ImageSaver
    {
        private BlockingCollection<Mat> m_queue = new BlockingCollection<Mat>();
        private Thread m_thread;


        public ImageSaver()
        {
            m_thread = new Thread(Run);
            m_thread.Name = "SaveThread";
            m_thread.IsBackground = true;
            m_thread.Start();
        }

        private void Run()
        {
            int i = 0;
            Directory.CreateDirectory("Images");
            while (true)
            {
                var image = m_queue.Take();
                var stamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                string filename = "Images" + Path.DirectorySeparatorChar +  stamp + ".jpg";
                i++;
                CvInvoke.Imwrite(filename, image);
                image.Dispose();
            }
        }


        public void AddToQueue(Mat mat)
        {
            m_queue.Add(mat.Clone());
        }
    }
}
