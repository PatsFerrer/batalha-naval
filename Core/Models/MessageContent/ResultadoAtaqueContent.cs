using System.Text.Json.Serialization;

namespace NavalBattle.Core.Models.MessageContent
{
    public class ResultadoAtaqueContent
    {
        [JsonPropertyName("posicao")]
        public MessagePosition PositionMessage { get; set; }

        [JsonPropertyName("acertou")]
        public bool Acertou { get; set; }

        [JsonPropertyName("distanciaAproximada")]
        public decimal DistanciaAproximada { get; set; }
    }
} 