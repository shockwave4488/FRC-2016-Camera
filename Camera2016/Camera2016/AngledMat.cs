using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using NetworkTables.Tables;

namespace Camera2016
{
    class AngledMat
    {
        public static ITable NetworkTable;
        public double RobotAngle { get; private set; }
        public double ShooterAngle { get; private set; }
        public Mat Image { get; }

        public AngledMat() : this(new Mat())
        {
        }

        public AngledMat(Mat image)
        {
            RobotAngle = NetworkTable.GetNumber("GyroAngle", 0);
            ShooterAngle = NetworkTable.GetNumber("ShooterAngle", 0);
            Image = image;
        }

        public void GetValues()
        {
            RobotAngle = NetworkTable?.GetNumber("GyroAngle", 0) ?? 0;
            ShooterAngle = NetworkTable?.GetNumber("ShooterAngle", 0) ?? 0;
        }

        public AngledMat Clone()
        {
            AngledMat toReturn = new AngledMat(Image.Clone());
            toReturn.RobotAngle = RobotAngle;
            toReturn.ShooterAngle = ShooterAngle;
            return toReturn;
        }

        public bool IsEmpty => Image.IsEmpty;

        public void Dispose()
        {
            Image.Dispose();
        }
    }
}
