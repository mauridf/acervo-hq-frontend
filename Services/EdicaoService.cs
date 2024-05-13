using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MongoDB.Bson;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

public class EdicaoService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public EdicaoService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<List<EdicaoModel>> GetEdicoes()
    {
        string apiUrl = _configuration["ApiUrl"];
        var edicaoResponse = await _httpClient.GetFromJsonAsync<List<EdicaoResponse>>(apiUrl + "Edicao/listar-edicoes");

        // Mapeando a resposta da API para EdicaoModel
        var edicoes = edicaoResponse.Select(e => new EdicaoModel
        {
            Id = e.Id, // Id é uma string simples
            TituloEdicao = e.TituloEdicao
        }).ToList();

        return edicoes;
    }

    public async Task<EdicaoModel> GetEdicaoById(string id)
    {
        string apiUrl = _configuration["ApiUrl"];
        var response = await _httpClient.GetFromJsonAsync<EdicaoResponse>(apiUrl + $"Edicao/detalhes-edicoes/{id}");
        return new EdicaoModel
        {
            Id = response.Id,
            TituloEdicao = response.TituloEdicao
        };
    }

    public async Task<EdicaoModel> CreateEdicao(string tituloEdicao)
    {
        try
        {
            // Verifica se o titulo da Edicao não está vazio
            if (string.IsNullOrWhiteSpace(tituloEdicao))
            {
                throw new ArgumentException("O titulo da Edição não pode estar vazio.");
            }

            string apiUrl = _configuration["ApiUrl"];
            
            // Crie o objeto de requisição com os dados corretamente formatados
            var request = new
            {
                tituloEdicao
            };

            // Envie a solicitação
            var response = await _httpClient.PostAsJsonAsync(apiUrl + "Edicao/cadastrar-edicao", request);
            
            // Verifique se a solicitação foi bem-sucedida
            if (!response.IsSuccessStatusCode)
            {
                // Se a solicitação não for bem-sucedida, leia a resposta e gere uma mensagem de erro específica
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Erro ao criar edicao. Resposta do servidor: {response.StatusCode} - {errorMessage}");
            }
            
            // Leia a resposta e retorne os dados da personagem criada
            return await response.Content.ReadFromJsonAsync<EdicaoModel>();
        }
        catch (HttpRequestException ex)
        {
            // Captura exceções relacionadas à comunicação com o servidor
            throw new Exception("Erro ao se comunicar com o servidor: " + ex.Message);
        }
        catch (ArgumentException ex)
        {
            // Captura exceções relacionadas a argumentos inválidos
            throw new Exception("Erro ao criar edicao: " + ex.Message);
        }
        catch (Exception ex)
        {
            // Captura outras exceções
            throw new Exception("Erro ao criar edicao: " + ex.Message);
        }
    }

    public async Task<bool> UpdateEdicao(string id, EdicaoModel edicao)
    {
        try
        {
            string apiUrl = _configuration["ApiUrl"];
            var response = await _httpClient.PutAsJsonAsync(apiUrl + $"Edicao/atualizar-edicao/{id}", new
            {
                TituloEdicao = edicao.TituloEdicao
            });

            if (!response.IsSuccessStatusCode)
            {
                // Se a solicitação não for bem-sucedida, leia a resposta e gere uma mensagem de erro específica
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Erro ao alterar edicao. Resposta do servidor: {response.StatusCode} - {errorMessage}");
            }

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("Erro ao atualizar edicao: " + ex.Message);
        }
    }

    public async Task DeleteEdicao(string id)
    {
        try
        {
            string apiUrl = _configuration["ApiUrl"];
            var response = await _httpClient.DeleteAsync(apiUrl + $"Edicao/excluir-edicao/{id}");

            if (!response.IsSuccessStatusCode)
            {
                // Se a solicitação não for bem-sucedida, leia a resposta e gere uma mensagem de erro específica
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Erro ao deletar edicao. Resposta do servidor: {response.StatusCode} - {errorMessage}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Erro ao excluir edicao: " + ex.Message);
        }
    }

    public class EdicaoResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("tituloEdicao")] // Adicione essas anotações para especificar os nomes das propriedades no JSON
        public string TituloEdicao { get; set; }
    }
}