using System.Text.Json;
using NavalBattle.Application.Interfaces;
using NavalBattle.Core.Enums;
using NavalBattle.Core.Models;
using NavalBattle.Core.Models.MessageContent;

namespace NavalBattle.Application.Services.Implementations
{
    public class BattleService : IBattleService
    {
        private readonly IMessageService _messageService;
        private readonly ICryptoService _cryptoService;
        private Ship _ship;
        private bool _canAttack;
        private readonly string _origin;

        public BattleService(
            IMessageService messageService,
            ICryptoService cryptoService,
            string origin)
        {
            _messageService = messageService;
            _cryptoService = cryptoService;
            _origin = origin;
            _canAttack = false;
        }

        public async Task RegisterShipAsync(Ship ship)
        {
            _ship = ship;

            var registrationMessage = new Message
            {
                origem = _origin,
                evento = ((int)EventType.RegistroNavio).ToString(),
                conteudo = JsonSerializer.Serialize(new
                {
                    ship.Name,
                    CenterPosition = ship.Positions[2],
                    ship.Orientation
                }),
                correlationId = Guid.NewGuid().ToString()
            };

            await _messageService.SendMessageAsync(registrationMessage);
        }

        public async Task<bool> AttackAsync(Position position)
        {
            if (!_canAttack)
                throw new InvalidOperationException("Ataque não permitido neste momento");

            if (!position.IsValid())
                throw new ArgumentException("Posição inválida");

            var attackMessage = new Message
            {
                origem = _origin,
                evento = ((int)EventType.Ataque).ToString(),
                conteudo = JsonSerializer.Serialize(new AtaqueContent 
                { 
                    nomeNavio = _origin,
                    posicaoAtaque = new MessagePosition { x = position.X, y = position.Y }
                }),
                correlationId = Guid.NewGuid().ToString()
            };

            await _messageService.SendMessageAsync(attackMessage);
            _canAttack = false;
            return true;
        }

        public Task<bool> IsAttackAllowedAsync()
        {
            return Task.FromResult(_canAttack);
        }

        public void SetAttackPermission(bool canAttack)
        {
            _canAttack = canAttack;
        }
    }
} 