using System.Text.Json;
using NavalBattle.Application.Interfaces;
using NavalBattle.Core.Enums;
using NavalBattle.Core.Models;
using NavalBattle.Core.Models.MessageContent;

namespace NavalBattle.Application.Services
{
    public class BattleCoordinator
    {
        private readonly IBattleService _battleService;
        private readonly IMessageService _messageService;
        private readonly string _shipName;
        private string _lastLiberacaoAtaqueCorrelationId;
        private Random _random;
        private Ship _ship;
        private readonly AttackStrategy _attackStrategy;
        private readonly string _cryptoKey;

        public BattleCoordinator(IBattleService battleService, IMessageService messageService, string shipName, string cryptoKey)
        {
            _battleService = battleService;
            _messageService = messageService;
            _shipName = shipName;
            _cryptoKey = cryptoKey;
            _random = new Random();
            _attackStrategy = new AttackStrategy();
        }

        public async Task StartAsync()
        {
            await _messageService.StartListeningAsync(async (message) => await HandleMessageAsync(message));
        }

        private async Task HandleMessageAsync(Message message)
        {
            switch (message.evento)
            {
                case "CampoLiberadoParaRegistro":
                case "RegistrarNovamente":
                    await RegistrarNavio();
                    break;

                case "LiberacaoAtaque":
                    try
                    {
                        var liberacao = JsonSerializer.Deserialize<LiberacaoAtaqueContent>(message.conteudo);
                        if (liberacao.nomeNavio == _shipName)
                        {
                            _lastLiberacaoAtaqueCorrelationId = message.correlationId;
                            await RealizarAtaque();
                        }
                        else
                        {
                            Console.WriteLine($"Não é nossa vez de atacar. Vez do navio: {liberacao.nomeNavio}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao processar liberação de ataque: {ex.Message}");
                    }
                    break;

                case "ResultadoAtaqueEfetuado":
                    try
                    {
                        var resultado = JsonSerializer.Deserialize<ResultadoAtaqueContent>(message.conteudo);
                        Console.WriteLine($"Resultado do ataque: Acertou: {resultado.Acertou}, Distância: {resultado.DistanciaAproximada}");

                        if (resultado.PositionMessage != null)
                        {
                            var position = new Position(resultado.PositionMessage.X, resultado.PositionMessage.Y);
                            _attackStrategy.RecordAttack(position, resultado.Acertou);
                            Console.WriteLine($"Registrado ataque em x:{position.PosX}, y:{position.PosY}, Acertou: {resultado.Acertou}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao processar resultado do ataque: {ex.Message}");
                    }
                    break;

                case "NavioAbatido":
                    Console.WriteLine("Nosso navio foi abatido!");
                    break;

                case "Vitoria":
                    Console.WriteLine("Vitória! Ganhamos a batalha!");
                    break;
            }
        }

        private async Task RegistrarNavio()
        {
            // Gera uma nova posição que não seja a mesma da anterior
            MessagePosition posicaoCentral;
            do
            {
                posicaoCentral = new MessagePosition 
                { 
                    X = _random.Next(2, 98),
                    Y = _random.Next(2, 28)
                };
            } while (_ship != null && _ship.Positions.Any(p => p.PosX == posicaoCentral.X && p.PosY == posicaoCentral.Y));

            var orientacao = _random.Next(2) == 0 ? ShipOrientation.Vertical : ShipOrientation.Horizontal;
            _ship = new Ship(_shipName, new Position(posicaoCentral.X, posicaoCentral.Y), orientacao, _cryptoKey);

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
                evento = EventType.RegistroNavio.ToString(),
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
                posicaoAtaque = new MessagePosition { X = nextPosition.PosX, Y = nextPosition.PosY }
            };

            var message = new Message
            {
                correlationId = _lastLiberacaoAtaqueCorrelationId,
                origem = _shipName,
                evento = EventType.Ataque.ToString(),
                conteudo = JsonSerializer.Serialize(ataqueContent)
            };

            await _messageService.SendMessageAsync(message);
        }
    }
}