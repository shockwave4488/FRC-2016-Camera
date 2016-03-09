using NetworkTables;
using NetworkTables.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KangarooRunner
{
    class Program
    {

        static void Main(string[] args)
        {
            NetworkTable.SetClientMode();
            NetworkTable.SetIPAddress("10.44.88.2");
            NetworkTable.SetNetworkIdentity("KangarooRunner");
            ITable runnerTable = NetworkTable.GetTable("SmartDashboard");

            CameraWatcher watcher = new CameraWatcher();
            VisionProcess proc = new VisionProcess();

            Timer timer = new Timer((o) =>
            {
                runnerTable.PutNumber("Kangaroo Battery", System.Windows.Forms.SystemInformation.PowerStatus.BatteryLifePercent);
                bool connected = watcher.Alive;
                if (connected)
                {
                    proc.Start();
                }
                else
                {
                    proc.Stop();
                }
            }, null, 5000, 5000);

            Thread.Sleep(Timeout.Infinite);


        }
    }
}
