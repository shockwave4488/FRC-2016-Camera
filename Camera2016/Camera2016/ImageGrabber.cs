using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Emgu.CV;
using Emgu.CV.UI;

namespace CameraThing
{
    class ImageGrabber
    {
        private Capture m_grabber;
        private Mat m_buf1, m_buf2;
        private bool m_switch;
        private Thread m_captureThread;

        public ImageGrabber()
        {
            m_grabber = new Capture("http://10.44.88.11/axis-cgi/mjpg/video.cgi?resolution=320x240&.mjpg");
            m_switch = false;
            m_captureThread = new Thread(run);
            m_captureThread.Start();
        }

        private void run()
        {
            while (true)
            {
                Mat image = m_grabber.QueryFrame();
                if (m_switch)
                    m_buf1 = image;
                else
                    m_buf2 = image;

                m_switch = !m_switch;
            }
        }

        public Mat Image()
        {
            return !m_switch ? m_buf1 : m_buf2;
        }
    }
}
