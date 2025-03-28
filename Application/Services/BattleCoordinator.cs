using System.Text.Json;
using Application.Services;
using NavalBattle.Domain.Enums;
using NavalBattle.Domain.Models;
using NavalBattle.Domain.Services.Interfaces;


public class BattleCoordinator
{
    private readonly IBattleService _battleService;
    private readonly IMessageService _messageService;
    private readonly string _shipName;
    private string _lastLiberacaoAtaqueCorrelationId;
    private Random _random;
    private Ship _ship;
    private readonly AttackStrategy _attackStrategy;

    public BattleCoordinator(IBattleService battleService, IMessageService messageService, string shipName)
    {
        _battleService = battleService;
        _messageService = messageService;
        _shipName = shipName;
        _random = new Random();
        _attackStrategy = new AttackStrategy();
    }

    public async Task StartAsync()
    {
        await _messageService.StartListeningAsync(HandleMessageAsync);
    }

    private async Task HandleMessageAsync(Message message)
    {
        switch (int.Parse(message.evento))
        {
            case (int)EventType.CampoLiberadoParaRegistro:
            case (int)EventType.RegistrarNovamente:
                await RegistrarNavio();
                break;

            case (int)EventType.LiberacaoAtaque:
                _lastLiberacaoAtaqueCorrelationId = message.correlationId;
                await RealizarAtaque();
                break;

            case (int)EventType.ResultadoAtaqueEfetuado:
                var resultado = JsonSerializer.Deserialize<ResultadoAtaqueContent>(message.conteudo);
                Console.WriteLine($"Resultado do ataque: Acertou: {resultado.Acertou}, Distância: {resultado.DistanciaAproximada}");
                
                // Registra o resultado do ataque na estratégia
                _attackStrategy.RecordAttack(
                    new Position(resultado.Posicao.x, resultado.Posicao.y),
                    resultado.Acertou
                );
                break;

            case (int)EventType.NavioAbatido:
                Console.WriteLine("Nosso navio foi abatido!");
                break;

            case (int)EventType.Vitoria:
                Console.WriteLine("Vitória! Ganhamos a batalha!");
                break;
        }
    }

    private async Task RegistrarNavio()
    {
        // Gera uma nova posição que não seja a mesma da anterior
        Posicao posicaoCentral;
        do
        {
            posicaoCentral = new Posicao 
            { 
                x = _random.Next(2, 98), // Evita bordas
                y = _random.Next(2, 28)  // Evita bordas
            };
        } while (_ship != null && _ship.Positions.Any(p => p.X == posicaoCentral.x && p.Y == posicaoCentral.y));

        var orientacao = _random.Next(2) == 0 ? ShipOrientation.Vertical : ShipOrientation.Horizontal;
        _ship = new Ship(_shipName, new Position(posicaoCentral.x, posicaoCentral.y), orientacao, "chave");

        var registroContent = new RegistroNavioContent
        {
            nomeNavio = _shipName,
            posicaoCentral = posicaoCentral,
            orientacao = orientacao == ShipOrientation.Vertical ? "vertical" : "horizontal"
        };

        var message = new Message
        {
            correlationId = Guid.NewGuid().ToString(),
            origem = _shipName,
            evento = ((int)EventType.RegistroNavio).ToString(),
            conteudo = JsonSerializer.Serialize(registroContent)
        };

        await _messageService.SendMessageAsync(message);
    }

    private async Task RealizarAtaque()
    {
        // Usa a estratégia para determinar a próxima posição de ataque
        var nextPosition = _attackStrategy.GetNextAttackPosition();

        var ataqueContent = new AtaqueContent
        {
            nomeNavio = _shipName,
            posicaoAtaque = new Posicao { x = nextPosition.X, y = nextPosition.Y }
        };

        var message = new Message
        {
            correlationId = _lastLiberacaoAtaqueCorrelationId,
            origem = _shipName,
            evento = ((int)EventType.Ataque).ToString(),
            conteudo = JsonSerializer.Serialize(ataqueContent)
        };

        await _messageService.SendMessageAsync(message);
    }
} 