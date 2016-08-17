using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

// This is a different method to manage the USB connections. It uses the built in windows event handlers

namespace Camera2016
{
    internal class UsbManager2
    {
        private int m_cameraState = 0;
        private readonly object m_lockObject = new object();

        public void startWatcher()
        {
            WqlEventQuery insertQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");

            ManagementEventWatcher insertWatcher = new ManagementEventWatcher(insertQuery);
            insertWatcher.EventArrived += new EventArrivedEventHandler(DeviceInsertedEvent);
            insertWatcher.Start();

            WqlEventQuery removeQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            ManagementEventWatcher removeWatcher = new ManagementEventWatcher(removeQuery);
            removeWatcher.EventArrived += new EventArrivedEventHandler(DeviceRemovedEvent);
            removeWatcher.Start();
        }

        private void DeviceInsertedEvent(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            foreach (var property in instance.Properties)
            {
                if (property.Name.Equals("Description"))
                {
                    if (property.Value.Equals("Logitech USB Camera (HD Pro Webcam C920)"))
                    {
                        lock (m_lockObject)
                        {
                            if (m_cameraState == 1)
                            {
                                m_cameraState = 2;
                            }
                        }
                        Console.WriteLine("Camera connected");
                    }
                }
            }
        }

        private void DeviceRemovedEvent(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            foreach (var property in instance.Properties)
            {
                if (property.Name.Equals("Description"))
                {
                    if (property.Value.Equals("Logitech USB Camera (HD Pro Webcam C920)"))
                    {
                        lock (m_lockObject)
                        {
                            if (m_cameraState == 0)
                            {
                                m_cameraState = 1;
                            }
                        }
                        Console.WriteLine("Camera disconnected");
                    }
                }
            }
        }

        // camera states:
        // 0 = Camera is found and working
        // 1 = Camera is not found, waiting for reconnect to reinitialize
        // 2 = Camera was found again, re-init was kicked off
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
    }
}
