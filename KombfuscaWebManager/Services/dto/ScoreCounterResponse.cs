namespace KombfuscaWebManager.Services.dto
{
    public class ScoreCounterResponse
    {
        public Dictionary<string, ResultScoreCounterAPI> Players { get; set; }

        public string? ProcessedImage { get; set; }
        
    }
}
