namespace NavalBattle.Core.Models
{
    public class Position
    {
        public int PosX { get; set; }
        public int PosY { get; set; }

        public Position(int x, int y)
        {
            PosX = x;
            PosY = y;
        }

        public double CalculateDistance(Position other)
        {
            return Math.Sqrt(Math.Pow(PosX - other.PosX, 2) + Math.Pow(PosY - other.PosY, 2));
        }

        public bool IsValid()
        {
            return PosX > 0 && PosX <= 100 && PosY > 0 && PosY <= 30;
        }
    }
} 