using System.Text.Json.Serialization;

namespace NavalBattle.Core.Models.MessageContent
{
    public class LiberacaoAtaqueContent
    {
        [JsonPropertyName("nomeNavio")]
        public string nomeNavio { get; set; }

        [JsonPropertyName("posicaoCentral")]
        public MessagePosition posicaoCentral { get; set; }

        [JsonPropertyName("orientacao")]
        public string orientacao { get; set; }

        [JsonPropertyName("posicoesNavio")]
        public List<MessagePosition> posicoesNavio { get; set; }
    }
} 