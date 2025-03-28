namespace NavalBattle.Core.Models
{
    public class Position
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public double CalculateDistance(Position other)
        {
            return Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
        }

        public bool IsValid()
        {
            return X >= 0 && X < 100 && Y >= 0 && Y < 30;
        }
    }
} 