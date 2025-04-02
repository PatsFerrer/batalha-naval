using System.Text.Json.Serialization;

namespace NavalBattle.Core.Models
{
    public class Message
    {
        [JsonPropertyName("CorrelationId")]
        public string correlationId { get; set; }

        [JsonPropertyName("Origem")]
        public string origem { get; set; }

        [JsonPropertyName("NavioDestino")]
        public string navioDestino { get; set; }

        [JsonPropertyName("Evento")]
        public string evento { get; set; }

        [JsonPropertyName("Conteudo")]
        public string conteudo { get; set; }

        [JsonPropertyName("PontuacaoNavios")]
        public string pontuacaoNavios { get; set; }
    }
} 