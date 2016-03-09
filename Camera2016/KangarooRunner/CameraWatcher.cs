using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

namespace KangarooRunner
{
    public class CameraWatcher
    {
        private Timer timer;
        private bool m_alive = false;

        private readonly object m_lockObject = new object();
        public bool Alive
        {
            get
            {
                lock (m_lockObject)
                {
                    bool temp = m_alive;
                    m_alive = false;
                    return temp;
                }
            }
            set
            {
                lock (m_lockObject)
                {
                    m_alive = value;
                }
            }
        }

        public CameraWatcher()
        {
            Ping ping = new Ping();

            timer = new Timer((o) =>
            {
                try
                {
                    var result = ping.Send("10.44.88.11", 300);
                    if (result.Status == IPStatus.Success)
                    {
                        //Update Watchdog
                        Alive = true;
                    }
                }
                catch
                {
                    Console.WriteLine("Error Pinging");
                }
                
            }, null, 1000, 1000);
        }
    }
}
