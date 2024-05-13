using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MongoDB.Bson;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

public class HqService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public HqService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<List<HqModel>> GetHqs()
    {
        string apiUrl = _configuration["ApiUrl"];
        var hqsResponse = await _httpClient.GetFromJsonAsync<List<HqResponse>>(apiUrl + "Hq/listar-hqs");

        // Mapeando a resposta da API para HqModel
        var hqs = hqsResponse.Select(e => new HqModel
        {
            Id = e.Id, // Id é uma string simples
            Titulo = e.Titulo,
            TituloOriginal = e.TituloOriginal,
            Categoria = e.Categoria,
            TotalEdicoes = e.TotalEdicoes,
            Status = e.Status,
            Observacao = e.Observacao,
            Edicoes = e.Edicoes,
            Editoras = e.Editoras,
            Personagens = e.Personagens
        }).ToList();

        return hqs;
    }

    public async Task<PaginatedResult<HqModel>> GetHqsPaginado(int pagina, int tamanhoPagina)
    {
        try
        {
            string apiUrl = _configuration["ApiUrl"];

            // Calcula o índice inicial do primeiro item na página
            int indiceInicial = (pagina - 1) * tamanhoPagina;

            // Chama a API para obter os personagens da página desejada
            var response = await _httpClient.GetAsync($"{apiUrl}Hq/listar-hqs-paginado?pagina={pagina}&tamanhoPagina={tamanhoPagina}");

            // Verifica se a solicitação foi bem-sucedida
            response.EnsureSuccessStatusCode();

            // Lê a resposta e desserializa os dados dos hqs e o total de páginas
            var responseData = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

            // Adicione estas linhas para imprimir o JSON de resposta
            var jsonResponse = JsonSerializer.Serialize(responseData);
            Console.WriteLine(jsonResponse);

            // Extrai a lista de hqs e o total de páginas
            var hqsResponseJson = (JsonElement)responseData["item1"];
            var hqsResponse = JsonSerializer.Deserialize<List<HqResponse>>(hqsResponseJson.GetRawText());
            var totalPaginas = ((JsonElement)responseData["item2"]).GetInt32();

            // Mapeia a resposta da API para HqModel
            var hqs = hqsResponse.Select(e => new HqModel
            {
                Id = e.Id, // Id é uma string simples
                Titulo = e.Titulo,
                TituloOriginal = e.TituloOriginal,
                Categoria = e.Categoria,
                TotalEdicoes = e.TotalEdicoes,
                Status = e.Status,
                Observacao = e.Observacao,
                Edicoes = e.Edicoes,
                Editoras = e.Editoras,
                Personagens = e.Personagens
            }).ToList();

            // Retorna os hqs da página e o número total de páginas
            return new PaginatedResult<HqModel> { Hqs = hqs, TotalPaginas = totalPaginas };
        }
        catch (Exception ex)
        {
            throw new Exception("Erro ao obter hqs paginados: " + ex.Message);
        }
    }

    public async Task<HqModel> GetHqById(string id)
    {
        string apiUrl = _configuration["ApiUrl"];
        var response = await _httpClient.GetFromJsonAsync<HqResponse>(apiUrl + $"Hq/detalhes-hq/{id}");
        return new HqModel
        {
            Id = response.Id,
            Titulo = response.Titulo,
            TituloOriginal = response.TituloOriginal,
            Categoria = response.Categoria,
            TotalEdicoes = response.TotalEdicoes,
            Status = response.Status,
            Observacao = response.Observacao,
            Edicoes = response.Edicoes,
            Editoras = response.Editoras,
            Personagens = response.Personagens
        };
    }

    public async Task<HqModel> CreateHq(string titulo, string? tituloOriginal, Categoria categoria, string? totalEdicoes, Status status, string? observacao, List<string> edicoes, List<string> editoras, List<string> personagens)
    {
        try
        {
            // Verifica se o nome do Personagem não está vazio
            if (string.IsNullOrWhiteSpace(titulo))
            {
                throw new ArgumentException("O titulo da HQ não pode estar vazio.");
            }

            string apiUrl = _configuration["ApiUrl"];
            
            // Crie o objeto de requisição com os dados corretamente formatados
            var request = new
            {
                titulo,
                tituloOriginal,
                categoria,
                totalEdicoes,
                status,
                observacao,
                edicoes,
                editoras,
                personagens
            };

            // Envie a solicitação
            var response = await _httpClient.PostAsJsonAsync(apiUrl + "Hq/cadastrar-hq", request);
            
            // Verifique se a solicitação foi bem-sucedida
            if (!response.IsSuccessStatusCode)
            {
                // Se a solicitação não for bem-sucedida, leia a resposta e gere uma mensagem de erro específica
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Erro ao criar hq. Resposta do servidor: {response.StatusCode} - {errorMessage}");
            }
            
            // Leia a resposta e retorne os dados da personagem criada
            return await response.Content.ReadFromJsonAsync<HqModel>();
        }
        catch (HttpRequestException ex)
        {
            // Captura exceções relacionadas à comunicação com o servidor
            throw new Exception("Erro ao se comunicar com o servidor: " + ex.Message);
        }
        catch (ArgumentException ex)
        {
            // Captura exceções relacionadas a argumentos inválidos
            throw new Exception("Erro ao criar hq: " + ex.Message);
        }
        catch (Exception ex)
        {
            // Captura outras exceções
            throw new Exception("Erro ao criar hq: " + ex.Message);
        }
    }

    public async Task<bool> UpdateHq(string id, HqModel hq)
    {
        try
        {
            string apiUrl = _configuration["ApiUrl"];
            var response = await _httpClient.PutAsJsonAsync(apiUrl + $"Hq/atualizar-hq/{id}", new
            {
                Titulo = hq.Titulo,
                TituloOriginal = hq.TituloOriginal,
                Categoria = hq.Categoria,
                TotalEdicoes = hq.TotalEdicoes,
                Status = hq.Status,
                Observacao = hq.Observacao,
                Edicoes = hq.Edicoes,
                Editoras = hq.Editoras,
                Personagens = hq.Personagens
            });

            if (!response.IsSuccessStatusCode)
            {
                // Se a solicitação não for bem-sucedida, leia a resposta e gere uma mensagem de erro específica
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Erro ao alterar hq. Resposta do servidor: {response.StatusCode} - {errorMessage}");
            }

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("Erro ao atualizar hq: " + ex.Message);
        }
    }

    public async Task DeleteHq(string id)
    {
        try
        {
            string apiUrl = _configuration["ApiUrl"];
            var response = await _httpClient.DeleteAsync(apiUrl + $"Hq/excluir-hq/{id}");

            if (!response.IsSuccessStatusCode)
            {
                // Se a solicitação não for bem-sucedida, leia a resposta e gere uma mensagem de erro específica
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Erro ao deletar hq. Resposta do servidor: {response.StatusCode} - {errorMessage}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Erro ao excluir hq: " + ex.Message);
        }
    }

    public class HqResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("titulo")] // Adicione essas anotações para especificar os nomes das propriedades no JSON
        public string Titulo { get; set; }
        [JsonPropertyName("tituloOriginal")] // Adicione essas anotações para especificar os nomes das propriedades no JSON
        public string? TituloOriginal { get; set; }

        [JsonPropertyName("categoria")] // Adicione essas anotações para especificar os nomes das propriedades no JSON
        public Categoria Categoria { get; set; }
        [JsonPropertyName("totalEdicoes")] // Adicione essas anotações para especificar os nomes das propriedades no JSON
        public String? TotalEdicoes { get; set; }

        [JsonPropertyName("status")] // Adicione essas anotações para especificar os nomes das propriedades no JSON
        public Status Status { get; set; }

        [JsonPropertyName("observacao")] // Adicione essas anotações para especificar os nomes das propriedades no JSON
        public String? Observacao { get; set; }

        [JsonPropertyName("edicoes")] // Adicione essas anotações para especificar os nomes das propriedades no JSON
        public List<string> Edicoes { get; set; }

        [JsonPropertyName("editoras")] // Adicione essas anotações para especificar os nomes das propriedades no JSON
        public List<string> Editoras { get; set; }

        [JsonPropertyName("personagens")] // Adicione essas anotações para especificar os nomes das propriedades no JSON
        public List<string> Personagens { get; set; }
    }


    public class PaginatedResult<T>
    {
        public List<T> Hqs { get; set; }
        public int TotalPaginas { get; set; }

        public void Deconstruct(out List<T> hqs, out int totalPaginas)
        {
            hqs = Hqs;
            totalPaginas = TotalPaginas;
        }
    }
}