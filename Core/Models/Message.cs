namespace NavalBattle.Core.Models
{
    public class Message
    {
        public string correlationId { get; set; }
        public string origem { get; set; }
        public string evento { get; set; }
        public string conteudo { get; set; }
    }
} 