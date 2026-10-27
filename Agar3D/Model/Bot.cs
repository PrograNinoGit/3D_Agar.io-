using System.Collections.Generic;

namespace Agar3D.Model
{
    /// <summary>
    /// Ein Bot kann aus mehreren Zellen bestehen (nach einem Split).
    /// </summary>
    public class Bot
    {
        public readonly List<Cell> Cells = new List<Cell>();

        // Gemeinsame Wanderrichtung (3D), wenn der Bot gerade kein Ziel verfolgt
        public double WanderDx;
        public double WanderDy;
        public double WanderDz;
        public int WanderTimer;

        public Bot(Cell firstCell)
        {
            Cells.Add(firstCell);
        }

        public double TotalMass
        {
            get
            {
                double total = 0;
                foreach (Cell c in Cells) total += c.Mass;
                return total;
            }
        }

        public double AvgX { get { return Avg(0); } }
        public double AvgY { get { return Avg(1); } }
        public double AvgZ { get { return Avg(2); } }

        private double Avg(int axis)
        {
            if (Cells.Count == 0) return 0;
            double sum = 0;
            foreach (Cell c in Cells)
            {
                sum += axis == 0 ? c.X : (axis == 1 ? c.Y : c.Z);
            }
            return sum / Cells.Count;
        }
    }
}
