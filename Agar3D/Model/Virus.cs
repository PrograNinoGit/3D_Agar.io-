namespace Agar3D.Model
{
    public class Virus
    {
        public double X, Y, Z;
        public double Radius;

        // Wie oft dieser Virus bereits mit ausgestossener Masse gefuettert wurde
        public int FeedCount;

        // Schwung eines frisch entstandenen Virus
        public double Vx, Vy, Vz;

        // Nur fuer die Optik: Drehwinkel
        public float Spin;

        public Virus(double x, double y, double z, double radius)
        {
            X = x;
            Y = y;
            Z = z;
            Radius = radius;
        }
    }
}
