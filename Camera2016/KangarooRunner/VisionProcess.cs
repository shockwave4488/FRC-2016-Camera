using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KangarooRunner
{
    public class VisionProcess
    {
        private bool m_started;
        private Process m_process;
        private readonly object m_lockObject = new object();


        public VisionProcess()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "Camera2016.exe";
            m_process = new Process();
            m_process.StartInfo = startInfo;
            m_process.EnableRaisingEvents = true;
            m_process.Exited += (sender, args) => m_started = false;
        }

        public void Start()
        {
            lock (m_lockObject)
            {
                if (!m_started)
                {
                    m_process.Start();
                    m_started = true;
                    Console.WriteLine("Process Started");
                }
            }
        }

        public void Stop()
        {
            lock (m_lockObject)
            {
                if (m_started)
                {
                    try
                    {
                        m_process.Kill();
                        m_started = false;
                        Console.WriteLine("Process Stopped");
                    }
                    catch
                    {
                        Console.WriteLine("Error Stopping");
                    }
                }
            }
        }

        
    }
}
