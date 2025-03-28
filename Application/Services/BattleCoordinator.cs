using System.Text.Json;
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

    public BattleCoordinator(IBattleService battleService, IMessageService messageService, string shipName)
    {
        _battleService = battleService;
        _messageService = messageService;
        _shipName = shipName;
        _random = new Random();
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
        var posicaoCentral = new Posicao 
        { 
            x = _random.Next(2, 98), // Evita bordas
            y = _random.Next(2, 28)  // Evita bordas
        };

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
        // Gera uma posição aleatória que não seja onde nosso navio está
        Posicao posicaoAtaque;
        do
        {
            posicaoAtaque = new Posicao
            {
                x = _random.Next(0, 100),
                y = _random.Next(0, 30)
            };
        } while (_ship != null && _ship.Positions.Any(p => p.X == posicaoAtaque.x && p.Y == posicaoAtaque.y));

        var ataqueContent = new AtaqueContent
        {
            nomeNavio = _shipName,
            posicaoAtaque = posicaoAtaque
        };

        var message = new Message
        {
            correlationId = _lastLiberacaoAtaqueCorrelationId, // Usa o mesmo correlationId do LiberacaoAtaque
            origem = _shipName,
            evento = ((int)EventType.Ataque).ToString(),
            conteudo = JsonSerializer.Serialize(ataqueContent)
        };

        await _messageService.SendMessageAsync(message);
    }
} 