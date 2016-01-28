using System.Threading;
using Emgu.CV;

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
        private Mat m_buf1 = new Mat(), m_buf2 = new Mat();
        private bool m_switch;
        private Thread m_captureThread;

        /// <summary>
        /// Sets up camera and starts thread
        /// </summary>
        public ImageGrabber()
        {
            m_grabber = new Capture("http://10.44.88.11/axis-cgi/mjpg/video.cgi?resolution=320x240&.mjpg");
            //m_grabber = new Capture();
            m_switch = false;
            m_captureThread = new Thread(run);
            m_captureThread.Start();
        }

        private void run()
        {
            while (true)
            {
                m_grabber.Grab();
                Mat image;
                lock (m_mutex)
                {
                    if (m_switch)
                        image = m_buf1;
                    else
                        image = m_buf2;

                    m_switch = !m_switch;
                }
                m_grabber.Retrieve(image);
            }
        }

        /// <summary>
        /// Get the image not currently being written to
        /// </summary>
        /// <returns></returns>
        public Mat Image()
        {
            lock (m_mutex)
            {
                return m_switch ? m_buf1.Clone() : m_buf2.Clone();
            }
        }
    }
}
