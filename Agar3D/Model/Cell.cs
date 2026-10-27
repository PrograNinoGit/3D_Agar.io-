using System;
using Microsoft.Xna.Framework;

namespace Agar3D.Model
{
    /// <summary>
    /// Eine Zelle (Kugel). X/Y/Z ist der MITTELPUNKT der Kugel.
    /// </summary>
    public class Cell
    {
        public double X;
        public double Y;
        public double Z;
        public double Radius;
        public Color Color;
        public string Name;

        // Bewegungsrichtung (normalisiert), z. B. fuer Bots
        public double Dx;
        public double Dy;
        public double Dz;

        // Schwung durch Split / Virus-Explosion (klingt ab)
        public double BoostX;
        public double BoostY;
        public double BoostZ;

        public int MergeCooldown;
        public int AiActionCooldown;

        public Cell(double x, double z, double radius, Color color, string name)
            : this(x, radius, z, radius, color, name)
        {
        }

        public Cell(double x, double y, double z, double radius, Color color, string name)
        {
            X = x;
            Y = y;
            Z = z;
            Radius = radius;
            Color = color;
            Name = name;
        }

        public double Mass
        {
            get { return Radius * Radius; }
        }

        public double Speed
        {
            get { return Math.Max(1.5, 6.0 - Radius / 15.0); }
        }
    }
}
