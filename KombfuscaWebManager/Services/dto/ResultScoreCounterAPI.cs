using System.Text.Json.Serialization;

namespace KombfuscaWebManager.Services.dto
{
    public class ResultScoreCounterAPI
    {
        [JsonPropertyName("fusca")]
        public int Fusca { get; set; }

        [JsonPropertyName("kombi")]
        public int Kombi { get; set; }

        [JsonPropertyName("new beetle")]
        public int NewBeetle { get; set; }
    }
}
