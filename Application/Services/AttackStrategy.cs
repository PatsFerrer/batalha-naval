using NavalBattle.Core.Models;

namespace NavalBattle.Application.Services
{
  public class AttackStrategy
  {
    private readonly Random _random;
    private Position _lastHitPosition;
    private Position _lastAttackPosition;
    private bool _hasHit;
    private bool _isNearby;
    private readonly List<Position> _attackHistory;
    private readonly HashSet<string> _attackedPositions;
    private readonly List<Position> _nearbyPositions;

    public AttackStrategy()
    {
      _random = new Random();
      _attackHistory = new List<Position>();
      _attackedPositions = new HashSet<string>();
      _nearbyPositions = new List<Position>();
    }

    public Position GetNextAttackPosition()
    {
      Console.WriteLine($"Obtendo próxima posição. HasHit: {_hasHit}, LastHitPosition: {(_lastHitPosition != null ? $"X:{_lastHitPosition.PosX}, Y:{_lastHitPosition.PosY}" : "null")}, IsNearby: {_isNearby}");
      
      if (_hasHit && _lastHitPosition != null)
      {
        // Se acertamos, vamos atacar nas posições adjacentes
        return GetAdjacentPosition();
      }

      if (_isNearby && _lastAttackPosition != null)
      {
        // Se estamos próximos do navio, vamos atacar em um raio de 7 posições
        return GetNearbyPosition();
      }

      // Se não acertamos ainda ou não temos posição de referência, ataca aleatoriamente
      return GetRandomPosition();
    }

    private Position GetNearbyPosition()
    {
      // Se já temos posições próximas calculadas, usa uma delas
      if (_nearbyPositions.Any())
      {
        var validPositions = _nearbyPositions
            .Where(p => p.IsValid() && !_attackedPositions.Contains($"{p.PosX},{p.PosY}"))
            .ToList();

        if (validPositions.Any())
        {
          var position = validPositions[_random.Next(validPositions.Count)];
          _nearbyPositions.Remove(position);
          return position;
        }
      }

      // Se não há mais posições válidas próximas, volta para ataque aleatório
      _isNearby = false;
      return GetRandomPosition();
    }

    private void CalculateNearbyPositions(Position center)
    {
      _nearbyPositions.Clear();
      
      // Lista de posições possíveis em um raio de 7
      for (int x = -7; x <= 7; x++)
      {
        for (int y = -7; y <= 7; y++)
        {
          // Verifica se a posição está dentro do raio de 7
          if (Math.Sqrt(x * x + y * y) <= 7)
          {
            var newX = center.PosX + x;
            var newY = center.PosY + y;
            var position = new Position(newX, newY);
            
            if (position.IsValid() && !_attackedPositions.Contains($"{newX},{newY}"))
            {
              _nearbyPositions.Add(position);
            }
          }
        }
      }

      // Embaralha as posições para não atacar em um padrão previsível
      var shuffledPositions = _nearbyPositions.OrderBy(x => _random.Next()).ToList();
      _nearbyPositions.Clear();
      _nearbyPositions.AddRange(shuffledPositions);
      
      Console.WriteLine($"Calculadas {_nearbyPositions.Count} posições próximas para ataque");
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
      _hasHit = false;
      _lastHitPosition = null;
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
        Console.WriteLine($"Tentando posição: X={position.PosX}, Y={position.PosY}");
      } while (_attackedPositions.Contains($"{position.PosX},{position.PosY}"));

      return position;
    }

    public void RecordAttack(Position position, bool hit, decimal distanciaAproximada = 0)
    {
      _attackHistory.Add(position);
      _attackedPositions.Add($"{position.PosX},{position.PosY}");
      _lastAttackPosition = position;

      Console.WriteLine($">>> Registrando ataque em X:{position.PosX}, Y:{position.PosY}, Acertou: {hit}, Distância: {distanciaAproximada}");

      if (hit)
      {
        _hasHit = true;
        _lastHitPosition = position;
        _isNearby = false;
        _nearbyPositions.Clear();
        Console.WriteLine($">>> ACERTO CONFIRMADO! Próximo ataque será ao redor de X:{position.PosX}, Y:{position.PosY}");
      }
      else if (distanciaAproximada <= 7)
      {
        _isNearby = true;
        CalculateNearbyPositions(position);
        Console.WriteLine($">>> NAVIO PRÓXIMO DETECTADO! Distância: {distanciaAproximada}. Próximo ataque será próximo a X:{position.PosX}, Y:{position.PosY}");
      }
      else
      {
        _hasHit = false;
        _lastHitPosition = null;
        _isNearby = false;
        _nearbyPositions.Clear();
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