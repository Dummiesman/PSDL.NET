using System;

namespace PSDL
{
    public static class Constants
    {
        public const int MaxSubtype = 7;
        public const int MaxType = 12;
    }
    
    [Flags]
    public enum RoomFlags:byte
    {
        Unknown = 1,
        Subterranean = 2,
        Standard = 4,
        Road = 8,
        Intersection = 16,
        SpecialBound = 32,
        Warp = 64,
        Instance = 128
    }
    
    public class Vertex
    {
        public float x;
        public float y;
        public float z;

        public Vertex(float _x, float _y, float _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        public override string ToString()
        {
            return "(x:" +  x + ", y:" + y + ", z:" + z + ")";
        }

        public override int GetHashCode()
        {
            return Convert.ToInt32(x * y * z);
        }

        public override bool Equals(object obj)
        {
            if(obj is Vertex)
            {
                Vertex v = (Vertex)obj;
                if(v.x.Equals(x) && v.y.Equals(y) && v.z.Equals(z))
                {
                    return true;
                }
            }
            return false;
        }

        public float  Distance(Vertex b)
        {
            return (b - this).Magnitude();
        }

        public float Magnitude()
        {
            return (float)Math.Sqrt(x * x + y * y + z * z);
        }

        public static Vertex operator -(Vertex v1, Vertex v2)
        {
            return new Vertex(v2.x - v1.x, v2.y - v1.y, v2.z - v1.z);
        }

        public static Vertex operator +(Vertex v1, Vertex v2)
        {
            return new Vertex(v2.x + v1.x, v2.y + v1.y, v2.z + v1.z);
        }

        public static Vertex operator *(Vertex v1, Vertex v2)
        {
            return new Vertex(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
        }

        public static Vertex operator *(Vertex vtx, float amount)
        {
            return new Vertex(vtx.x * amount, vtx.y * amount, vtx.z * amount);
        }

        public static Vertex operator /(Vertex v1, Vertex v2)
        {
            return new Vertex(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z);
        }

        public static Vertex operator /(Vertex vtx, float amount)
        {
            return new Vertex(vtx.x / amount, vtx.y / amount, vtx.z / amount);
        }
    }

    public class PerimeterPoint
    {
        public Vertex Vertex;
        public Room ConnectedRoom;

        public PerimeterPoint(Vertex vtx, Room connection)
        {
            ConnectedRoom = connection;
            Vertex = vtx;
        }
    }
}
