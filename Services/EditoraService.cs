using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MongoDB.Bson;
using System.Text.Json;
using System.Text.Json.Serialization;

public class EditoraService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public EditoraService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<List<EditoraModel>> GetEditoras()
    {
        string apiUrl = _configuration["ApiUrl"];
        var editorasResponse = await _httpClient.GetFromJsonAsync<List<EditoraResponse>>(apiUrl + "Editora/listar-editoras");

        // Mapeando a resposta da API para EditoraModel
        var editoras = editorasResponse.Select(e => new EditoraModel
        {
            Id = e.Id, // Id é uma string simples
            Nome = e.NomeEditora
        }).ToList();

        return editoras;
    }

    public async Task<PaginatedResult<EditoraModel>> GetEditorasPaginado(int pagina, int tamanhoPagina)
    {
        try
        {
            string apiUrl = _configuration["ApiUrl"];

            // Calcula o índice inicial do primeiro item na página
            int indiceInicial = (pagina - 1) * tamanhoPagina;

            // Chama a API para obter os personagens da página desejada
            var response = await _httpClient.GetAsync($"{apiUrl}Editora/listar-editoras-paginado?pagina={pagina}&tamanhoPagina={tamanhoPagina}");

            // Verifica se a solicitação foi bem-sucedida
            response.EnsureSuccessStatusCode();

            // Lê a resposta e desserializa os dados dos personagens e o total de páginas
            var responseData = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

            // Adicione estas linhas para imprimir o JSON de resposta
            var jsonResponse = JsonSerializer.Serialize(responseData);
            Console.WriteLine(jsonResponse);

            // Extrai a lista de personagens e o total de páginas
            var editorasResponseJson = (JsonElement)responseData["item1"];
            var editorasResponse = JsonSerializer.Deserialize<List<EditoraResponse>>(editorasResponseJson.GetRawText());
            var totalPaginas = ((JsonElement)responseData["item2"]).GetInt32();

            // Mapeia a resposta da API para EditoraModel
            var editoras = editorasResponse.Select(e => new EditoraModel
            {
                Id = e.Id, // Id é uma string simples
                Nome = e.NomeEditora
            }).ToList();

            // Retorna as editoras da página e o número total de páginas
            return new PaginatedResult<EditoraModel> { Editoras = editoras, TotalPaginas = totalPaginas };
        }
        catch (Exception ex)
        {
            throw new Exception("Erro ao obter editoras paginadas: " + ex.Message);
        }
    }

    public async Task<EditoraModel> GetEditoraById(string id)
    {
        string apiUrl = _configuration["ApiUrl"];
        var response = await _httpClient.GetFromJsonAsync<EditoraResponse>(apiUrl + $"Editora/detalhes-editora/{id}");
        return new EditoraModel
        {
            Id = response.Id,
            Nome = response.NomeEditora
        };
    }

    public async Task<EditoraModel> CreateEditora(string nomeEditora)
    {
        try
        {
            // Verifica se o nome da editora não está vazio
            if (string.IsNullOrWhiteSpace(nomeEditora))
            {
                throw new ArgumentException("O nome da editora não pode estar vazio.");
            }

            string apiUrl = _configuration["ApiUrl"];
            
            // Crie o objeto de requisição com os dados corretamente formatados
            var request = new
            {
                nomeEditora
            };

            // Envie a solicitação
            var response = await _httpClient.PostAsJsonAsync(apiUrl + "Editora/cadastrar-editora", request);
            
            // Verifique se a solicitação foi bem-sucedida
            response.EnsureSuccessStatusCode();
            
            // Leia a resposta e retorne os dados da editora criada
            return await response.Content.ReadFromJsonAsync<EditoraModel>();
        }
        catch (HttpRequestException ex)
        {
            // Captura exceções relacionadas à comunicação com o servidor
            throw new Exception("Erro ao se comunicar com o servidor: " + ex.Message);
        }
        catch (ArgumentException ex)
        {
            // Captura exceções relacionadas a argumentos inválidos
            throw new Exception("Erro ao criar editora: " + ex.Message);
        }
        catch (Exception ex)
        {
            // Captura outras exceções
            throw new Exception("Erro ao criar editora: " + ex.Message);
        }
    }

    public async Task<bool> UpdateEditora(string id, EditoraModel editora)
    {
        try
        {
            string apiUrl = _configuration["ApiUrl"];
            var response = await _httpClient.PutAsJsonAsync(apiUrl + $"Editora/atualizar-editora/{id}", new
            {
                NomeEditora = editora.Nome // Apenas o nome é enviado no corpo da requisição
            });

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro ao atualizar editora: {response.StatusCode} - {errorMessage}");
            }

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("Erro ao atualizar editora: " + ex.Message);
        }
    }

    public async Task DeleteEditora(string id)
    {
        try
        {
            string apiUrl = _configuration["ApiUrl"];
            var response = await _httpClient.DeleteAsync(apiUrl + $"Editora/excluir-editora/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro ao excluir editora: {response.StatusCode} - {errorMessage}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Erro ao excluir editora: " + ex.Message);
        }
    }

    private class EditoraResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } // Alterado para string
        [JsonPropertyName("nomeEditora")]
        public string NomeEditora { get; set; }
    }

    public class PaginatedResult<T>
    {
        public List<T> Editoras { get; set; }
        public int TotalPaginas { get; set; }

        public void Deconstruct(out List<T> editoras, out int totalPaginas)
        {
            editoras = Editoras;
            totalPaginas = TotalPaginas;
        }
    }
}