using System.Text.Json.Serialization;

namespace NavalBattle.Core.Models
{
    public class Message
    {
        [JsonPropertyName("correlationId")]
        public string correlationId { get; set; }

        [JsonPropertyName("origem")]
        public string origem { get; set; }

        [JsonPropertyName("navioDestino")]
        public string navioDestino { get; set; }

        [JsonPropertyName("evento")]
        public string evento { get; set; }

        [JsonPropertyName("conteudo")]
        public string conteudo { get; set; }

        [JsonPropertyName("pontuacaoNavios")]
        public string pontuacaoNavios { get; set; }
    }
} 