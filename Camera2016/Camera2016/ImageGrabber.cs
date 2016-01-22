using System.Threading;
using Emgu.CV;

namespace CameraThing
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

        private Mat m_buf1, m_buf2;
        private bool m_switch;
        private Thread m_captureThread;

        /// <summary>
        /// Sets up camera and starts thread
        /// </summary>
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

        /// <summary>
        /// Get the image not currently being written to
        /// </summary>
        /// <returns></returns>
        public Mat Image()
        {
            return !m_switch ? m_buf1 : m_buf2;
        }
    }
}
