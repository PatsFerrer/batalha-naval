using NavalBattle.Core.Models;

namespace NavalBattle.Application.Interfaces
{
    public interface IBattleService
    {
        Task RegisterShipAsync(Ship ship);
        Task<bool> AttackAsync(Position position);
        Task<bool> IsAttackAllowedAsync();
    }
} 