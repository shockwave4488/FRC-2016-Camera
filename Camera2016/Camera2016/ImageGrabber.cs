using System;
using System.Runtime.InteropServices;
using System.Threading;
using Emgu.CV;
using NetworkTables.Tables;

namespace Camera2016
{
    /// <summary>
    /// Double-buffered class for grabbing images from the axis camera
    /// </summary>
    class ImageGrabber
    {
        /// <summary>
        /// Camera to grab the image from
        /// </summary>
        private Capture m_grabber;

        private readonly object m_mutex = new object();
        private ImageBuffer m_buf1 = new ImageBuffer(), m_buf2 = new ImageBuffer();
        private bool m_switch;
        private Thread m_captureThread;
        private ITable m_table;

        private bool m_updated = false;

        /// <summary>
        /// Sets up camera and starts thread
        /// </summary>
        public ImageGrabber(ITable table)
        {
            do
            {
#if KANGAROO
                m_grabber = new Capture(1);
#else
                m_grabber = new Capture(0);
#endif
            } while (m_grabber.Height == 0); 
            Console.WriteLine("Found Image");

            //m_grabber.FlipHorizontal = true;
            m_grabber.FlipVertical = true;

            m_grabber.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 1280);
            m_grabber.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 720);
            //m_grabber.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Exposure, 1000000);
            //m_grabber = new Capture();
            m_switch = false;
            m_table = table;
            m_captureThread = new Thread(run);
            m_captureThread.Start();
        }

        private void run()
        {
            while (true)
            {
                m_grabber.Grab();
                ImageBuffer image;
                lock (m_mutex)
                {
                    if (m_switch)
                        image = m_buf1;
                    else
                        image = m_buf2;

                    m_switch = !m_switch;
                    m_updated = true;
                }
                image.GyroAngle = m_table.GetNumber("Gyro", 0.0);
                image.ShooterAngle = m_table.GetNumber("TurretPot", 0.0);
                m_grabber.Retrieve(image.Image);
                
            }
        }

        /// <summary>
        /// Get the image not currently being written to
        /// </summary>
        /// <returns></returns>
        public ImageBuffer Image()
        {
            lock (m_mutex)
            {
                if (!m_updated)
                {
                    return null;
                }
                m_updated = false;
                return m_switch ? m_buf1.Clone() : m_buf2.Clone();
            }
        }
    }
}
