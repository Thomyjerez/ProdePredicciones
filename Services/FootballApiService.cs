using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ProdePrediccionesAPI.Services
{
    public class FootballApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public FootballApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["ApiFootball:ApiKey"] ?? string.Empty;
            _baseUrl = configuration["ApiFootball:BaseUrl"] ?? string.Empty;
        }

        public async Task<List<EquipoExternoDto>> GetEquiposPorLigaAsync(int ligaId, int temporada)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{_baseUrl}teams?league={ligaId}&season={temporada}"),
            };
            
            request.Headers.Add("x-apisports-key", _apiKey);

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            
            using var document = JsonDocument.Parse(jsonString);
            var root = document.RootElement;
            
            var listaEquipos = new List<EquipoExternoDto>();

            if (root.TryGetProperty("response", out var responseArray))
            {
                foreach (var item in responseArray.EnumerateArray())
                {
                    var team = item.GetProperty("team");
                    listaEquipos.Add(new EquipoExternoDto
                    {
                        Nombre = team.GetProperty("name").GetString() ?? string.Empty,
                        Pais = team.GetProperty("country").GetString() ?? string.Empty,
                        EscudoUrl = team.GetProperty("logo").GetString() ?? string.Empty
                    });
                }
            }

            return listaEquipos;
        }
    }

    public class EquipoExternoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public string EscudoUrl { get; set; } = string.Empty;
    }
}