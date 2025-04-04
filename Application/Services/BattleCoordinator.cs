using System.Text.Json;
using NavalBattle.Application.Interfaces;
using NavalBattle.Core.Enums;
using NavalBattle.Core.Models;
using NavalBattle.Core.Models.MessageContent;
using NavalBattle.Core.Helpers;

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
            // Registra o navio assim que a aplicação inicia
            await RegistrarNavio();
            
            // Inicia a escuta de mensagens
            await _messageService.StartListeningAsync(async (message) => await HandleMessageAsync(message));
        }

        private async Task HandleMessageAsync(Message message)
        {
            // Verifica se a mensagem é para nosso navio ou para todos
            if (!string.IsNullOrEmpty(message.navioDestino) && message.navioDestino != _shipName)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"Mensagem ignorada - Destinada para outro navio: {message.navioDestino}");
                Console.ResetColor();
                return;
            }

            // Processa a pontuação dos navios se existir
            if (message.pontuacaoNavios != null)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\nPontuação atual dos navios:");
                foreach (var pontuacao in message.pontuacaoNavios)
                {
                    Console.WriteLine($"Navio: {pontuacao.Key} - Pontos: {pontuacao.Value}");
                }
                Console.ResetColor();
            }

            switch (message.evento)
            {
                case "CampoLiberadoParaRegistro":
                case "RegistrarNovamente":
                    await RegistrarNavio();
                    break;

                case "LiberacaoAtaque":
                    try
                    {
                        var liberacao = message.conteudo.Deserialize<LiberacaoAtaqueContent>();
                        if (liberacao.nomeNavio == _shipName)
                        {
                            _lastLiberacaoAtaqueCorrelationId = message.correlationId;
                            await RealizarAtaque();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Não é nossa vez de atacar. Vez do navio: {liberacao.nomeNavio}");
                            Console.ResetColor();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Erro ao processar liberação de ataque: {ex.Message}");
                        Console.ResetColor();
                    }
                    break;

                case "ResultadoAtaqueEfetuado":
                    try
                    {
                        // Verifica se a mensagem é do POSSEIDON e destinada para nosso navio
                        if (message.origem != "POSSEIDON" || message.navioDestino != _shipName)
                        {
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine($"Ignorando resultado de ataque - não é para nosso navio");
                            Console.ResetColor();
                            break;
                        }

                        var resultado = message.conteudo.Deserialize<ResultadoAtaqueContent>();
                        if (resultado.Acertou)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Resultado do ataque: ACERTO! Distância: {resultado.DistanciaAproximada}");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Resultado do ataque: ERRO! Distância: {resultado.DistanciaAproximada}");
                        }
                        Console.ResetColor();

                        if (resultado.PositionMessage != null)
                        {
                            var position = new Position(resultado.PositionMessage.X, resultado.PositionMessage.Y);
                            _attackStrategy.RecordAttack(position, resultado.Acertou, resultado.DistanciaAproximada);
                            Console.WriteLine($"Registrado ataque em x:{position.PosX}, y:{position.PosY}, Acertou: {resultado.Acertou}, Distância: {resultado.DistanciaAproximada}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Erro ao processar resultado do ataque: {ex.Message}");
                        Console.ResetColor();
                    }
                    break;

                case "NavioAbatido":
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Nosso navio foi abatido!");
                    Console.ResetColor();
                    break;

                case "Vitoria":
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Vitória! Ganhamos a batalha!");
                    Console.ResetColor();
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
                conteudo = registroContent.Serialize()
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
                navioDestino = "", // Vazio pois é um ataque
                evento = EventType.Ataque.ToString(),
                conteudo = ataqueContent.Serialize(),
                pontuacaoNavios = null
            };

            await _messageService.SendMessageAsync(message);
            
            // Não registramos a posição aqui, apenas quando recebermos o ResultadoAtaqueEfetuado
            Console.WriteLine($"Ataque enviado para X:{nextPosition.PosX}, Y:{nextPosition.PosY}");
        }
    }
}