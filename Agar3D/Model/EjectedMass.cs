using Microsoft.Xna.Framework;

namespace Agar3D.Model
{
    public class EjectedMass
    {
        public double X, Y, Z;
        public double Radius;
        public Color Color;

        // Schwung, mit dem das Pellet wegfliegt (klingt ab)
        public double Vx, Vy, Vz;

        // Kurze Schonfrist direkt nach dem Ausstossen
        public int OwnerImmuneTicks;

        public EjectedMass(double x, double y, double z, double radius, Color color,
                           double vx, double vy, double vz, int ownerImmuneTicks)
        {
            X = x;
            Y = y;
            Z = z;
            Radius = radius;
            Color = color;
            Vx = vx;
            Vy = vy;
            Vz = vz;
            OwnerImmuneTicks = ownerImmuneTicks;
        }
    }
}
