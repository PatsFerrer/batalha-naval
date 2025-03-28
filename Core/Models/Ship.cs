using NavalBattle.Core.Enums;

namespace NavalBattle.Core.Models
{
    public class Ship
    {
        public string Name { get; set; }
        public Position[] Positions { get; set; }
        public ShipOrientation Orientation { get; set; }
        public string CryptoKey { get; set; }

        public Ship(string name, Position centerPosition, ShipOrientation orientation, string cryptoKey)
        {
            if (string.IsNullOrEmpty(name) || name.Length > 20)
                throw new ArgumentException("Nome do navio deve ter entre 1 e 20 caracteres");

            Name = name;
            Orientation = orientation;
            CryptoKey = cryptoKey;

            Positions = new Position[5];

            // Calcula as posições do navio baseado na posição central e orientação
            Positions[2] = centerPosition; // Posição central (3)

            if (orientation == ShipOrientation.Horizontal)
            {
                Positions[0] = new Position(centerPosition.X - 2, centerPosition.Y);
                Positions[1] = new Position(centerPosition.X - 1, centerPosition.Y);
                Positions[3] = new Position(centerPosition.X + 1, centerPosition.Y);
                Positions[4] = new Position(centerPosition.X + 2, centerPosition.Y);
            }
            else
            {
                Positions[0] = new Position(centerPosition.X, centerPosition.Y - 2);
                Positions[1] = new Position(centerPosition.X, centerPosition.Y - 1);
                Positions[3] = new Position(centerPosition.X, centerPosition.Y + 1);
                Positions[4] = new Position(centerPosition.X, centerPosition.Y + 2);
            }
        }

        public bool IsPositionValid(Position position)
        {
            return position.IsValid();
        }
    }
}