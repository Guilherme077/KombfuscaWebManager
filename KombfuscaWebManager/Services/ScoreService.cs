using KombfuscaWebManager.Data;
using KombfuscaWebManager.Models.CupModels;
using KombfuscaWebManager.Models.CupModels.ViewModels;
using KombfuscaWebManager.Services.dto;
using Microsoft.AspNetCore.Identity;
using System.Net.NetworkInformation;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace KombfuscaWebManager.Services
{
    public class ScoreService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context;


        public ScoreService(HttpClient httpClient, ApplicationDbContext context)
        {
            _httpClient = httpClient;
            _context = context;
        }

        //public async Task ProcessImage(IFormFile image, int periodId, string LoggedUserId)
        //{
        //    using var content = new MultipartFormDataContent();

        //    using var stream = image.OpenReadStream();

        //    content.Add(new StreamContent(stream), "picture", image.FileName);

        //    var response = await _httpClient.PostAsync("http://localhost:5000/scorecounter", content);

        //    var body = await response.Content.ReadAsStringAsync();

        //    if (!response.IsSuccessStatusCode)
        //    {
        //        throw new Exception(
        //            $"Status: {response.StatusCode}\n\nResposta:\n{body}");
        //    }

        //    string json = await response.Content.ReadAsStringAsync();

        //    var resultado = JsonSerializer.Deserialize<Dictionary<string, ResultScoreCounterAPI>>(json);

        //    var model = new ScoreConfirmationViewModel
        //    {
        //        PeriodId = periodId
        //    };

        //    foreach (var item in resultado)
        //    {
        //        model.Players.Add(
        //            new PlayersScoreConfirmationViewModel
        //            {
        //                UserId = LoggedUserId, // Change to player's user ID when available
        //                Name = "Jogador Teste", // Change to player's name when available

        //                Fusca = item.Value.Fusca,
        //                Kombi = item.Value.Kombi,
        //                NewBeetle = item.Value.NewBeetle
        //            });
        //    }
        //    return View("Confirmar", model);
        //}
    }
}
