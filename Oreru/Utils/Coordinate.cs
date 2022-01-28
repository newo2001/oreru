using System;

namespace Oreru.Utils {
    public readonly struct Coordinate {
        public int X { get; }
        public int Y { get; }

        public Coordinate(int x, int y) {
            X = x;
            Y = y;
        }

        public static implicit operator Coordinate((int, int) pair) {
            var (x, y) = pair;
            return new Coordinate(x, y);
        }

        public static Coordinate operator +(Coordinate a, Coordinate b) {
            return (a.X + b.X, a.Y + b.Y);
        }
        
        public static Coordinate operator +(Coordinate a, int b) {
            return (a.X + b, a.Y + b);
        }
        
        public static Coordinate operator -(Coordinate a, Coordinate b) {
            return (a.X - b.X, a.Y - b.Y);
        }
        
        public static Coordinate operator -(Coordinate a, int b) {
            return (a.X - b, a.Y - b);
        }

        public void Deconstruct(out int x, out int y) {
            x = X;
            y = Y;
        }

        public double Distance(Coordinate other) {
            var (x, y) = this - other;
            return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
        }

        public override string ToString() => $"({X}, {Y})";

        public override bool Equals(object obj) {
            return obj switch {
                null => false,
                Coordinate(var x, var y) => x == X && y == Y,
                _ => base.Equals(obj)
            };
        }

        public override int GetHashCode() => X * 31 + Y;
    }
}