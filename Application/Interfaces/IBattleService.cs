using NavalBattle.Domain.Models;

namespace NavalBattle.Domain.Services.Interfaces
{
    public interface IBattleService
    {
        Task RegisterShipAsync(Ship ship);
        Task<bool> AttackAsync(Position position);
        Task<bool> IsAttackAllowedAsync();
    }
} 