using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CameraProcessRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            CameraWatcher m_watcher = new CameraWatcher("10.44.88.11");
            ProcessRunner m_visionProcess = new ProcessRunner();

            Timer t = new Timer((o) =>
            {
                if(m_watcher.Alive)
                    m_visionProcess.Start();
                else
                    m_visionProcess.Stop();
            }, null, 5000, 5000);

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
