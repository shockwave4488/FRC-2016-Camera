using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraProcessRunner
{
    class ProcessRunner
    {
        private Process m_cameraProcess;
        private bool m_started;

        public ProcessRunner()
        {
            ProcessStartInfo s = new ProcessStartInfo("Camera2016.exe");
            m_cameraProcess = new Process();
            m_cameraProcess.StartInfo = s;
        }

        public void Start()
        {
            if(!m_started)
                m_cameraProcess.Start();

            m_started = true;
        }

        public void Stop()
        {
            try
            {
                m_cameraProcess.Kill();
            }
            catch
            {
            }
            m_started = false;
        }
    }
}
