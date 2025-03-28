using NavalBattle.Core.Models;

namespace NavalBattle.Application.Services
{
  public class AttackStrategy
  {
    private readonly Random _random;
    private Position _lastHitPosition;
    private bool _hasHit;
    private readonly List<Position> _attackHistory;
    private readonly HashSet<string> _attackedPositions;

    public AttackStrategy()
    {
      _random = new Random();
      _attackHistory = new List<Position>();
      _attackedPositions = new HashSet<string>();
    }

    public Position GetNextAttackPosition()
    {
      Console.WriteLine($"Obtendo próxima posição. HasHit: {_hasHit}, LastHitPosition: {(_lastHitPosition != null ? $"X:{_lastHitPosition.PosX}, Y:{_lastHitPosition.PosY}" : "null")}");
      
      if (_hasHit && _lastHitPosition != null)
      {
        // Se acertamos, vamos atacar nas posições adjacentes
        return GetAdjacentPosition();
      }

      // Se não acertamos ainda ou não temos posição de referência, ataca aleatoriamente
      return GetRandomPosition();
    }

    private Position GetAdjacentPosition()
    {
      // Lista de posições adjacentes possíveis (cima, baixo, esquerda, direita)
      var possiblePositions = new List<Position>
            {
                new Position(_lastHitPosition.PosX, _lastHitPosition.PosY - 1), // cima
                new Position(_lastHitPosition.PosX, _lastHitPosition.PosY + 1), // baixo
                new Position(_lastHitPosition.PosX - 1, _lastHitPosition.PosY), // esquerda
                new Position(_lastHitPosition.PosX + 1, _lastHitPosition.PosY)  // direita
            };

      // Filtra posições válidas e não atacadas
      var validPositions = possiblePositions
          .Where(p => p.IsValid() && !_attackedPositions.Contains($"{p.PosX},{p.PosY}"))
          .ToList();

      if (validPositions.Any())
      {
        // Escolhe uma posição aleatória entre as válidas
        return validPositions[_random.Next(validPositions.Count)];
      }

      // Se não há posições adjacentes válidas, volta para ataque aleatório
      return GetRandomPosition();
    }

    private Position GetRandomPosition()
    {
      Position position;
      do
      {
        position = new Position(
            _random.Next(0, 100),
            _random.Next(0, 30)
        );
        Console.WriteLine($"Tentando posição: X={position.PosX}, Y={position.PosY}"); // Log para debug
      } while (_attackedPositions.Contains($"{position.PosX},{position.PosY}"));

      return position;
    }

    public void RecordAttack(Position position, bool hit)
    {
      _attackHistory.Add(position);
      _attackedPositions.Add($"{position.PosX},{position.PosY}");

      Console.WriteLine($">>> Registrando ataque em X:{position.PosX}, Y:{position.PosY}, Acertou: {hit}");

      if (hit)
      {
        _hasHit = true;
        _lastHitPosition = position;  // Guarda a referência direta
        Console.WriteLine($">>> ACERTO CONFIRMADO! Próximo ataque será ao redor de X:{position.PosX}, Y:{position.PosY}");
      }
      else if (_hasHit && _lastHitPosition != null)
      {
        Console.WriteLine($">>> Erro, mas continuando busca ao redor do último acerto em X:{_lastHitPosition.PosX}, Y:{_lastHitPosition.PosY}");
      }
      else
      {
        _hasHit = false;
        _lastHitPosition = null;
        Console.WriteLine(">>> Sem acertos ainda, continuando busca aleatória");
      }
    }

    public bool HasHit()
    {
      return _hasHit;
    }

    public Position GetLastHitPosition()
    {
      return _lastHitPosition;
    }
  }
}