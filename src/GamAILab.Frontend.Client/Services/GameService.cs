using System.Net.Http.Json;
using GamAILab.Shared.Models.Game.DTOs;

namespace GamAILab.Frontend.Client.Services;

public class GameService : IGameService
{
    private readonly HttpClient _httpClient;

    public GameService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
}