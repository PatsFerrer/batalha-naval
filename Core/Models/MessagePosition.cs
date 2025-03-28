using System.Text.Json.Serialization;

namespace NavalBattle.Core.Models
{
    /// <summary>
    /// Classe para transferência de dados de posição nas mensagens.
    /// Usado apenas para serialização/deserialização das mensagens do Service Bus.
    /// Aceita tanto X/Y maiúsculos (formato do controlador) quanto x/y minúsculos.
    /// </summary>
    public class MessagePosition
    {
        [JsonPropertyName("X")]
        public int x { get; set; }

        [JsonPropertyName("Y")]
        public int y { get; set; }
    }
}