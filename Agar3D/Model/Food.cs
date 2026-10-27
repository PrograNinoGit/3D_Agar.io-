using Microsoft.Xna.Framework;

namespace Agar3D.Model
{
    public class Food
    {
        // Mittelpunkt der Kugel
        public double X, Y, Z;
        public double Radius;
        public Color Color;

        // Nur fuer die Optik: Futterkugeln schweben leicht auf und ab
        public float BobPhase;

        public Food(double x, double y, double z, double radius, Color color, float bobPhase)
        {
            X = x;
            Y = y;
            Z = z;
            Radius = radius;
            Color = color;
            BobPhase = bobPhase;
        }

        // Kompatibilitaet zum alten 2D-Aufruf
        public Food(double x, double z, double radius, Color color, float bobPhase)
            : this(x, radius + 2.5, z, radius, color, bobPhase)
        {
        }
    }
}
