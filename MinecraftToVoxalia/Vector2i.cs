using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinecraftToVoxalia
{
    /// <summary>
    /// Represents a 2D vector of integers.
    /// </summary>
    public struct Vector2i : IEquatable<Vector2i>
    {
        /// <summary>
        /// Construct the vec2i.
        /// </summary>
        /// <param name="x">X coordinaate.</param>
        /// <param name="y">Y coordinate.</param>
        public Vector2i(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// The zero vector.
        /// </summary>
        public static readonly Vector2i Zero = new Vector2i(0, 0);

        /// <summary>
        /// The x coordinate.
        /// </summary>
        public int X;

        /// <summary>
        /// The y coordinate.
        /// </summary>
        public int Y;

        /// <summary>
        /// Gets a cheap hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return X * 23 + Y;
        }

        /// <summary>
        /// Compares equality between this and another vector.
        /// </summary>
        /// <param name="other">The other vector.</param>
        /// <returns>Whether they are equal.</returns>
        public override bool Equals(object other)
        {
            return Equals((Vector2i)other);
        }

        /// <summary>
        /// Compares equality between this and another vector.
        /// </summary>
        /// <param name="other">The other vector.</param>
        /// <returns>Whether they are equal.</returns>
        public bool Equals(Vector2i other)
        {
            return other.X == X && other.Y == Y;
        }
        
        /// <summary>
        /// Gets a simple string of the vector.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return "(" + X + ", " + Y + ")";
        }

        /// <summary>
        /// Logical comparison.
        /// </summary>
        /// <param name="one">First vec.</param>
        /// <param name="two">Second vec.</param>
        /// <returns>Result.</returns>
        public static bool operator !=(Vector2i one, Vector2i two)
        {
            return !one.Equals(two);
        }

        /// <summary>
        /// Logical comparison.
        /// </summary>
        /// <param name="one">First vec.</param>
        /// <param name="two">Second vec.</param>
        /// <returns>Result.</returns>
        public static bool operator ==(Vector2i one, Vector2i two)
        {
            return one.Equals(two);
        }

        /// <summary>
        /// Mathematical comparison.
        /// </summary>
        /// <param name="one">First vec.</param>
        /// <param name="two">Second vec.</param>
        /// <returns>Result.</returns>
        public static Vector2i operator +(Vector2i one, Vector2i two)
        {
            return new Vector2i(one.X + two.X, one.Y + two.Y);
        }

        /// <summary>
        /// Mathematical comparison.
        /// </summary>
        /// <param name="one">First vec.</param>
        /// <param name="two">Int scalar.</param>
        /// <returns>Result.</returns>
        public static Vector2i operator *(Vector2i one, int two)
        {
            return new Vector2i(one.X * two, one.Y * two);
        }

        /// <summary>
        /// Mathematical comparison.
        /// </summary>
        /// <param name="one">First vec.</param>
        /// <param name="two">Int scalar.</param>
        /// <returns>Result.</returns>
        public static Vector2i operator /(Vector2i one, int two)
        {
            return new Vector2i(one.X / two, one.Y / two);
        }
    }
}
