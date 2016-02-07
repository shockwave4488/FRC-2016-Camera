using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CameraProcessRunner
{
    class CameraWatcher
    {
        private bool m_alive;
        private string m_cameraAddress;
        private Ping m_cameraPing;
        private Timer m_timer;
        private object m_mutex;

        public CameraWatcher(string cameraAddress)
        {
            m_alive = false;
            m_cameraAddress = cameraAddress;
            m_cameraPing = new Ping();
            m_mutex = new object();

            m_timer = new Timer((o) =>
            {
                try
                {
                    var result = m_cameraPing.Send(m_cameraAddress, 300);
                    if (result.Status == IPStatus.Success)
                        Alive = true;
                }
                catch
                {
                }
            }, null, 1000, 1000);
        }

        public bool Alive
        {
            get
            {
                lock (m_mutex)
                {
                    bool toReturn = m_alive;
                    m_alive = false;
                    return toReturn;
                }
            }
            set
            {
                lock (m_mutex)
                    m_alive = value;
            }
        }
    }
}
