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


        private Mat m_image1 = new Mat();
        private Mat m_image2 = new Mat();
        private bool m_switch = true;
        private readonly object m_lockObject = new object();


        private Thread m_captureThread;

        /// <summary>
        /// Sets up camera and starts thread
        /// </summary>
        public ImageGrabber()
        {
            //m_grabber = new Capture("http://10.44.88.11/axis-cgi/mjpg/video.cgi?resolution=320x240&.mjpg");
            m_grabber = new Capture();
            //m_switch = false;
            m_captureThread = new Thread(run);
            m_captureThread.Start();
        }

        private void run()
        {
            while (true)
            {
                m_grabber.Grab();
                Mat image;
                lock (m_lockObject)
                {
                    if (m_switch)
                    {
                        image = m_image1;
                    }
                    else
                    {
                        image = m_image2;
                    }
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
            lock (m_lockObject)
            {
                if (m_switch)
                {
                    //If its using1, m_image2 is the current image being grabbed.
                    //Use image 1
                    if (m_image1.IsEmpty) return null;
                    return m_image1.Clone();
                }
                else
                {
                    //Otherwise its capturing to image1
                    //return image 2
                    if (m_image2.IsEmpty) return null;
                    return m_image2.Clone();
                }

            }
        }
    }
}
