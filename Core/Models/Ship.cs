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
                Positions[0] = new Position(centerPosition.PosX - 2, centerPosition.PosY);
                Positions[1] = new Position(centerPosition.PosX - 1, centerPosition.PosY);
                Positions[3] = new Position(centerPosition.PosX + 1, centerPosition.PosY);
                Positions[4] = new Position(centerPosition.PosX + 2, centerPosition.PosY);
            }
            else
            {
                Positions[0] = new Position(centerPosition.PosX, centerPosition.PosY - 2);
                Positions[1] = new Position(centerPosition.PosX, centerPosition.PosY - 1);
                Positions[3] = new Position(centerPosition.PosX, centerPosition.PosY + 1);
                Positions[4] = new Position(centerPosition.PosX, centerPosition.PosY + 2);
            }
        }

        public bool IsPositionValid(Position position)
        {
            return position.IsValid();
        }
    }
}