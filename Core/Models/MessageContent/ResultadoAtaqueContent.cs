using System.Text.Json.Serialization;

namespace NavalBattle.Core.Models.MessageContent
{
    public class ResultadoAtaqueContent
    {
        [JsonPropertyName("Posicao")]
        public MessagePosition PositionMessage { get; set; }
        public bool Acertou { get; set; }
        public int DistanciaAproximada { get; set; }
    }
} 