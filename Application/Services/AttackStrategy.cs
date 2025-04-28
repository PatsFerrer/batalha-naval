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
    private readonly HashSet<string> _discardedPositions;
    private readonly List<Position> _nearbyPositions;
    private List<Position> _enemyShipPositions;
    private const int PROXIMITY_RADIUS = 7;

    public AttackStrategy()
    {
      _random = new Random();
      _attackHistory = new List<Position>();
      _attackedPositions = new HashSet<string>();
      _discardedPositions = new HashSet<string>();
      _nearbyPositions = new List<Position>();
      _enemyShipPositions = new List<Position>();
      _minDistance = decimal.MaxValue;
    }

    public void SetEnemyPositions(List<Position> positions)
    {
      _enemyShipPositions = positions;
      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine($">>> POSIÇÕES DO NAVIO INIMIGO DESCRIPTOGRAFADAS! Total: {positions.Count}");
      foreach (var pos in positions)
      {
        Console.WriteLine($"- X:{pos.PosX}, Y:{pos.PosY}");
      }
      Console.ResetColor();
    }

    public Position GetNextAttackPosition()
    {
      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.WriteLine($"Obtendo próxima posição. HasHit: {_hasHit}, LastHitPosition: {(_lastHitPosition != null ? $"X:{_lastHitPosition.PosX}, Y:{_lastHitPosition.PosY}" : "null")}, IsNearby: {_isNearby}, LastDistance: {_lastDistance}, MinDistance: {_minDistance}, BestPosition: {(_bestPosition != null ? $"X:{_bestPosition.PosX}, Y:{_bestPosition.PosY}" : "null")}");
      Console.ResetColor();
      
      // Se temos posições do navio inimigo descriptografadas, atacamos elas
      if (_enemyShipPositions.Any())
      {
        var posicaoNaoAtacada = _enemyShipPositions
            .FirstOrDefault(p => !_attackedPositions.Contains($"{p.PosX},{p.PosY}"));
        
        if (posicaoNaoAtacada != null)
        {
          Console.ForegroundColor = ConsoleColor.Green;
          Console.WriteLine($"Atacando posição conhecida do inimigo: X:{posicaoNaoAtacada.PosX}, Y:{posicaoNaoAtacada.PosY}");
          Console.ResetColor();
          return posicaoNaoAtacada;
        }
      }

      if (_hasHit && _lastHitPosition != null)
      {
        // Se acertamos, vamos atacar nas posições adjacentes
        return GetAdjacentPosition();
      }

      // Se temos uma posição com distância menor que 7, continuamos focando nela
      if (_minDistance <= 7 && _bestPosition != null)
      {
        _isNearby = true;
        var nearbyPosition = GetNearbyPosition();
        if (nearbyPosition != null)
        {
          return nearbyPosition;
        }
      }

      // Se não encontrou posições próximas válidas, tenta atacar a melhor posição conhecida
      if (_bestPosition != null && !_attackedPositions.Contains($"{_bestPosition.PosX},{_bestPosition.PosY}"))
      {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Voltando para melhor posição conhecida: X:{_bestPosition.PosX}, Y:{_bestPosition.PosY}");
        Console.ResetColor();
        return _bestPosition;
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
            .Where(p => p.IsValid() && 
                       !_attackedPositions.Contains($"{p.PosX},{p.PosY}") &&
                       !_discardedPositions.Contains($"{p.PosX},{p.PosY}"))
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
      
      // Se ainda não há posições válidas após recalcular, volta para ataque aleatório
      if (!_nearbyPositions.Any())
      {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Não há mais posições válidas próximas, voltando para ataque aleatório");
        Console.ResetColor();
        _isNearby = false;
        return GetRandomPosition();
      }

      // Tenta novamente com as novas posições calculadas
      var newValidPositions = _nearbyPositions
          .Where(p => p.IsValid() && 
                     !_attackedPositions.Contains($"{p.PosX},{p.PosY}") &&
                     !_discardedPositions.Contains($"{p.PosX},{p.PosY}"))
          .ToList();

      if (newValidPositions.Any())
      {
        var position = newValidPositions[_random.Next(newValidPositions.Count)];
        _nearbyPositions.Remove(position);
        return position;
      }

      // Se ainda não encontrou posições válidas, volta para ataque aleatório
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine("Não foi possível encontrar posições válidas após recalcular, voltando para ataque aleatório");
      Console.ResetColor();
      _isNearby = false;
      return GetRandomPosition();
    }

    private void CalculateNearbyPositions(Position center)
    {
      _nearbyPositions.Clear();
      
      // Garante um raio mínimo de busca mesmo quando a distância é 0
      int radius = Math.Max((int)Math.Ceiling(_minDistance), 1);
      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.WriteLine($"Calculando posições próximas com raio {radius} a partir de X:{center.PosX}, Y:{center.PosY}");
      Console.ResetColor();
      
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
            
            if (position.IsValid() && 
                !_attackedPositions.Contains($"{newX},{newY}") &&
                !_discardedPositions.Contains($"{newX},{newY}"))
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
      
      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.WriteLine($"Calculadas {_nearbyPositions.Count} posições próximas para ataque");
      Console.ResetColor();
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

      // Filtra posições válidas, não atacadas e não descartadas
      var validPositions = possiblePositions
          .Where(p => p.IsValid() && 
                     !_attackedPositions.Contains($"{p.PosX},{p.PosY}") &&
                     !_discardedPositions.Contains($"{p.PosX},{p.PosY}"))
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
            _random.Next(1, 100),
            _random.Next(1, 30)
        );
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Tentando posição: X={position.PosX}, Y={position.PosY}");
        Console.ResetColor();
      } while (_attackedPositions.Contains($"{position.PosX},{position.PosY}") || 
               _discardedPositions.Contains($"{position.PosX},{position.PosY}"));

      return position;
    }

    public void RecordAttack(Position position, bool hit, decimal distanciaAproximada = 0)
    {
      // Registra a posição como atacada apenas quando recebemos o resultado do controlador
      _attackedPositions.Add($"{position.PosX},{position.PosY}");
      _lastAttackPosition = position;
      _lastDistance = distanciaAproximada;

      // Se a distância for 1000 E não tivermos uma distância menor que 7, descarta as posições
      if (distanciaAproximada >= 1000 && _minDistance > 7)
      {
        DiscardPositionsAroundAttack(position);
      }

      // Atualiza a menor distância histórica e a melhor posição se a atual for menor
      if (distanciaAproximada < _minDistance)
      {
        _minDistance = distanciaAproximada;
        _bestPosition = position;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($">>> NOVA MENOR DISTÂNCIA HISTÓRICA: {_minDistance} em X:{position.PosX}, Y:{position.PosY}");
        Console.ResetColor();
      }

      Console.WriteLine($">>> Registrando ataque em X:{position.PosX}, Y:{position.PosY}, Acertou: {hit}, Distância: {distanciaAproximada}");

      if (hit)
      {
        _hasHit = true;
        _lastHitPosition = position;
        _isNearby = false;
        _nearbyPositions.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($">>> ACERTO CONFIRMADO! Próximo ataque será ao redor de X:{position.PosX}, Y:{position.PosY}");
        Console.ResetColor();
      }
      else if (_minDistance <= 7)
      {
        _isNearby = true;
        CalculateNearbyPositions(_bestPosition);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($">>> NAVIO PRÓXIMO DETECTADO! Distância mínima: {_minDistance}. Próximo ataque será próximo a X:{_bestPosition.PosX}, Y:{_bestPosition.PosY}");
        Console.ResetColor();
      }
      else
      {
        _hasHit = false;
        _lastHitPosition = null;
        _isNearby = false;
        _nearbyPositions.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(">>> Sem acertos ainda, continuando busca aleatória");
        Console.ResetColor();
      }
    }

    private void DiscardPositionsAroundAttack(Position position)
    {
      // Descarta posições no eixo X (7 posições para cada lado)
      for (int i = Math.Max(1, position.PosX - PROXIMITY_RADIUS); i <= Math.Min(100, position.PosX + PROXIMITY_RADIUS); i++)
      {
        _discardedPositions.Add($"{i},{position.PosY}");
      }

      // Descarta posições no eixo Y (7 posições para cada lado)
      for (int j = Math.Max(1, position.PosY - PROXIMITY_RADIUS); j <= Math.Min(30, position.PosY + PROXIMITY_RADIUS); j++)
      {
        _discardedPositions.Add($"{position.PosX},{j}");
      }

      Console.ForegroundColor = ConsoleColor.Red;
      Console.WriteLine($">>> Descartando posições ao redor de X:{position.PosX}, Y:{position.PosY}");
      Console.ResetColor();
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