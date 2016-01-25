using Emgu.CV.Util;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Camera2016
{
    class ContoursReport
    {

        public int Rows
        {
            get;
        }
        public int Cols { get; }
        public VectorOfVectorOfPoint Contours
        {
            get;
        }

        private Rectangle[] m_boundingBoxes = null;

        public ContoursReport() : this(new VectorOfVectorOfPoint(), 0, 0)
        {

        }

        public ContoursReport(VectorOfVectorOfPoint contours, int rows, int cols)
        {
            Contours = contours;
            Rows = rows;
            Cols = cols;
        }

        

        private System.Drawing.Rectangle[] ComputeBoundingBoxes()
        {
            if (m_boundingBoxes == null)
            {
                Rectangle[] bb = new Rectangle[Contours.Size];
                for (int i = 0; i < Contours.Size; i++)
                {
                    bb[i] = CvInvoke.BoundingRectangle(Contours[i]);
                }
                m_boundingBoxes = bb;
            }

            return m_boundingBoxes;
        }

        public double[] GetArea()
        {
            double[] areas = new double[Contours.Size];
            for (int i = 0; i < Contours.Size; i++)
            {
                areas[i] = CvInvoke.ContourArea(Contours[i]);
            }
            return areas;
        }

        public double[] GetCenterX()
        {
            double[] centers = new double[Contours.Size];
            Rectangle[] boundingBoxes = ComputeBoundingBoxes();
            for (int i = 0; i < Contours.Size; i++)
            {
                centers[i] = boundingBoxes[i].X + boundingBoxes[i].Width / 2;
            }
            return centers;
        }

        public double[] GetCenterY()
        {
            double[] centers = new double[Contours.Size];
            Rectangle[] boundingBoxes = ComputeBoundingBoxes();
            for (int i = 0; i < Contours.Size; i++)
            {
                centers[i] = boundingBoxes[i].Y + boundingBoxes[i].Height / 2;
            }
            return centers;
        }

        public double[] GetWidth()
        {
            double[] widths = new double[Contours.Size];
            Rectangle[] boundingBoxes = ComputeBoundingBoxes();
            for (int i = 0; i < Contours.Size; i++)
            {
                widths[i] = boundingBoxes[i].Width;
            }
            return widths;

        }

        public double[] GetHeights()
        {
            double[] heights = new double[Contours.Size];
            Rectangle[] boundingBoxes = ComputeBoundingBoxes();
            for (int i = 0; i < Contours.Size; i++)
            {
                heights[i] = boundingBoxes[i].Height;
            }
            return heights;

        }

        public double[] GetSolidity()
        {
            double[] solidities = new double[Contours.Size];
            VectorOfPoint hull = new VectorOfPoint();
            for (int i = 0; i < Contours.Size; i++)
            {
                CvInvoke.ConvexHull(Contours[i], hull);
                solidities[i] = CvInvoke.ContourArea(Contours[i]) / CvInvoke.ContourArea(hull);
            }
            return solidities;
        }
    }
}
