using System.Text.Json.Serialization;

namespace NavalBattle.Core.Models.MessageContent
{
    public class RegistroNavioContent
    {
        [JsonPropertyName("nomeNavio")]
        public string nomeNavio { get; set; }

        [JsonPropertyName("posicaoCentral")]
        public MessagePosition posicaoCentral { get; set; }

        [JsonPropertyName("orientacao")]
        public string orientacao { get; set; }
    }
} 