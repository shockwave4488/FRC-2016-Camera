using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

namespace Camera2016
{
    public class CameraWatcher
    {
        private Timer timer;
        private UsbManager usbInfo = new UsbManager();
        private int m_cameraState = 0;
        private bool m_cameraFound = false;

        // camera states:
        // 0 = Camera is found and working
        // 1 = Camera is not found, waiting for reconnect to reinitialize
        // 2 = Camera was found again, re-init was kicked off

        private readonly object m_lockObject = new object();
        public int CheckState
        {
            get
            {
                lock (m_lockObject)
                {
                    int temp = m_cameraState;
                    return temp;
                }
            }
            set
            {
                lock (m_lockObject)
                {
                    m_cameraState = value;
                }
            }
        }

        public CameraWatcher()
        {
            timer = new Timer((o) =>
            {
                var usbDevices = usbInfo.GetUSBDevices();

                m_cameraFound = false;
                foreach (var usbDevice in usbDevices)
                {
                    if (usbDevice.Description.Equals("Logitech USB Camera (HD Pro Webcam C920)"))
                    {
                        Console.WriteLine("Camera found");
                        m_cameraFound = true;
                    }                 
                }

                if (m_cameraFound && m_cameraState == 1)
                {
                    m_cameraState = 2;
                }else if (!m_cameraFound && m_cameraState == 0)
                {
                    // camera has been disconnected
                    // set flag to 1 and wait for camera reconnect
                    m_cameraState = 1;
                }

            }, null, 1000, 1000);
        }
    }
}
