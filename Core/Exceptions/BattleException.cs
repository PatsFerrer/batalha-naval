namespace NavalBattle.Core.Exceptions
{
    public class BattleException : Exception
    {
        public BattleException(string message) : base(message)
        {
        }

        public BattleException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
} 