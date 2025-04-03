using NavalBattle.Core.Models;

namespace NavalBattle.Application.Services
{
  public class AttackStrategy
  {
    private readonly Random _random;
    private Position _lastHitPosition;
    private Position _lastAttackPosition;
    private Position _bestPosition;
    private bool _hasHit;
    private bool _isNearby;
    private decimal _lastDistance;
    private decimal _minDistance;
    private readonly List<Position> _attackHistory;
    private readonly HashSet<string> _attackedPositions;
    private readonly List<Position> _nearbyPositions;

    public AttackStrategy()
    {
      _random = new Random();
      _attackHistory = new List<Position>();
      _attackedPositions = new HashSet<string>();
      _nearbyPositions = new List<Position>();
      _minDistance = decimal.MaxValue;
    }

    public Position GetNextAttackPosition()
    {
      Console.WriteLine($"Obtendo próxima posição. HasHit: {_hasHit}, LastHitPosition: {(_lastHitPosition != null ? $"X:{_lastHitPosition.PosX}, Y:{_lastHitPosition.PosY}" : "null")}, IsNearby: {_isNearby}, LastDistance: {_lastDistance}, MinDistance: {_minDistance}, BestPosition: {(_bestPosition != null ? $"X:{_bestPosition.PosX}, Y:{_bestPosition.PosY}" : "null")}");
      
      if (_hasHit && _lastHitPosition != null)
      {
        // Se acertamos, vamos atacar nas posições adjacentes
        return GetAdjacentPosition();
      }

      // Se temos uma posição com distância menor que 7, continuamos focando nela
      if (_minDistance <= 7 && _bestPosition != null)
      {
        _isNearby = true;
        return GetNearbyPosition();
      }

      // Se não acertamos ainda e não temos uma posição próxima, ataca aleatoriamente
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

      // Se não há mais posições válidas próximas, recalcula
      CalculateNearbyPositions(_bestPosition);
      return GetNearbyPosition();
    }

    private void CalculateNearbyPositions(Position center)
    {
      _nearbyPositions.Clear();
      
      // Usa a menor distância histórica como raio de busca
      int radius = (int)Math.Ceiling(_minDistance);
      Console.WriteLine($"Calculando posições próximas com raio {radius} (menor distância histórica) a partir de X:{center.PosX}, Y:{center.PosY}");
      
      // Lista de posições possíveis no raio especificado
      for (int x = -radius; x <= radius; x++)
      {
        for (int y = -radius; y <= radius; y++)
        {
          // Verifica se a posição está dentro do raio
          if (Math.Sqrt(x * x + y * y) <= radius)
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
      _lastDistance = distanciaAproximada;

      // Atualiza a menor distância histórica e a melhor posição se a atual for menor
      if (distanciaAproximada < _minDistance)
      {
        _minDistance = distanciaAproximada;
        _bestPosition = position;
        Console.WriteLine($">>> NOVA MENOR DISTÂNCIA HISTÓRICA: {_minDistance} em X:{position.PosX}, Y:{position.PosY}");
      }

      Console.WriteLine($">>> Registrando ataque em X:{position.PosX}, Y:{position.PosY}, Acertou: {hit}, Distância: {distanciaAproximada}");

      if (hit)
      {
        _hasHit = true;
        _lastHitPosition = position;
        _isNearby = false;
        _nearbyPositions.Clear();
        Console.WriteLine($">>> ACERTO CONFIRMADO! Próximo ataque será ao redor de X:{position.PosX}, Y:{position.PosY}");
      }
      else if (_minDistance <= 7)
      {
        _isNearby = true;
        CalculateNearbyPositions(_bestPosition);
        Console.WriteLine($">>> NAVIO PRÓXIMO DETECTADO! Distância mínima: {_minDistance}. Próximo ataque será próximo a X:{_bestPosition.PosX}, Y:{_bestPosition.PosY}");
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