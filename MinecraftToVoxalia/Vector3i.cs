using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinecraftToVoxalia
{
    class Vector3i : IEquatable<Vector3i>
    {
        public Vector3i(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static readonly Vector3i Zero = new Vector3i(0, 0, 0);

        public int X;
        public int Y;
        public int Z;

        public override int GetHashCode()
        {
            return X + Y + Z;
        }

        public override bool Equals(object other)
        {
            return Equals((Vector3i)other);
        }

        public bool Equals(Vector3i other)
        {
            return other.X == X && other.Y == Y && other.Z == Z;
        }
        
        public override string ToString()
        {
            return X + "," + Y + "," + Z;
        }

        public static bool operator !=(Vector3i one, Vector3i two)
        {
            return !one.Equals(two);
        }

        public static bool operator ==(Vector3i one, Vector3i two)
        {
            return one.Equals(two);
        }

        public static Vector3i operator +(Vector3i one, Vector3i two)
        {
            return new Vector3i(one.X + two.X, one.Y + two.Y, one.Z + two.Z);
        }

        public static Vector3i operator *(Vector3i one, int two)
        {
            return new Vector3i(one.X * two, one.Y * two, one.Z * two);
        }
    }
}
