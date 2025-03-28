using System.Text.Json.Serialization;

namespace NavalBattle.Core.Models
{
    /// <summary>
    /// Classe para transferência de dados de posição nas mensagens.
    /// Usado apenas para serialização/deserialização das mensagens do Service Bus.
    /// O controlador sempre envia com X e Y maiúsculos.
    /// </summary>
    public class MessagePosition
    {
        [JsonPropertyName("X")]
        public int X { get; set; }

        [JsonPropertyName("Y")]
        public int Y { get; set; }
    }
}