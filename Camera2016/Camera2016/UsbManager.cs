using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace Camera2016
{
    class UsbManager
    {
        public void printUSBDevices()
        {
            var usbDevices = GetUSBDevices();

            foreach (var usbDevice in usbDevices)
            {
                Console.WriteLine("Description: {0}", usbDevice.Description);
            }

            Console.Read();
        }

        public List<USBDeviceInfo> GetUSBDevices()
        {
            List<USBDeviceInfo> devices = new List<USBDeviceInfo>();

            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_USBHub"))
                collection = searcher.Get();

            foreach (var device in collection)
            {
                devices.Add(new USBDeviceInfo(
                (string)device.GetPropertyValue("Description")
                ));
            }

            collection.Dispose();
            return devices;
        }
    }

    class USBDeviceInfo
    {
        public USBDeviceInfo(string description)
        {
            this.Description = description;
        }
        public string Description { get; private set; }
    }
}
