using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MongoDB.Bson;
using System.Text.Json.Serialization;
using System.Text.Json;

public class PersonagemService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public PersonagemService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<List<PersonagemModel>> GetPersonagens()
    {
        string apiUrl = _configuration["ApiUrl"];
        var personagensResponse = await _httpClient.GetFromJsonAsync<List<PersonagemResponse>>(apiUrl + "Personagem/listar-personagens");

        // Mapeando a resposta da API para PersonagemModel
        var personagens = personagensResponse.Select(e => new PersonagemModel
        {
            Id = e.Id, // Id é uma string simples
            Nome = e.Nome,
            Observacao = e.Observacao,
            TipoPersonagem = e.TipoPersonagem
        }).ToList();

        return personagens;
    }

    public async Task<PaginatedResult<PersonagemModel>> GetPersonagensPaginado(int pagina, int tamanhoPagina)
    {
        try
        {
            string apiUrl = _configuration["ApiUrl"];

            // Calcula o índice inicial do primeiro item na página
            int indiceInicial = (pagina - 1) * tamanhoPagina;

            // Chama a API para obter os personagens da página desejada
            var response = await _httpClient.GetAsync($"{apiUrl}Personagem/listar-personagens-paginado?pagina={pagina}&tamanhoPagina={tamanhoPagina}");

            // Verifica se a solicitação foi bem-sucedida
            response.EnsureSuccessStatusCode();

            // Lê a resposta e desserializa os dados dos personagens e o total de páginas
            var responseData = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

            // Adicione estas linhas para imprimir o JSON de resposta
            var jsonResponse = JsonSerializer.Serialize(responseData);
            Console.WriteLine(jsonResponse);

            // Extrai a lista de personagens e o total de páginas
            var personagensResponseJson = (JsonElement)responseData["item1"];
            var personagensResponse = JsonSerializer.Deserialize<List<PersonagemResponse>>(personagensResponseJson.GetRawText());
            var totalPaginas = ((JsonElement)responseData["item2"]).GetInt32();

            // Mapeia a resposta da API para PersonagemModel
            var personagens = personagensResponse.Select(e => new PersonagemModel
            {
                Id = e.Id, // Id é uma string simples
                Nome = e.Nome,
                Observacao = e.Observacao,
                TipoPersonagem = e.TipoPersonagem
            }).ToList();

            // Retorna os personagens da página e o número total de páginas
            return new PaginatedResult<PersonagemModel> { Personagens = personagens, TotalPaginas = totalPaginas };
        }
        catch (Exception ex)
        {
            throw new Exception("Erro ao obter personagens paginados: " + ex.Message);
        }
    }

    public async Task<PersonagemModel> GetPersonagemById(string id)
    {
        string apiUrl = _configuration["ApiUrl"];
        var response = await _httpClient.GetFromJsonAsync<PersonagemResponse>(apiUrl + $"Personagem/detalhes-personagem/{id}");
        return new PersonagemModel
        {
            Id = response.Id,
            Nome = response.Nome,
            Observacao = response.Observacao,
            TipoPersonagem = response.TipoPersonagem
        };
    }

    public async Task<PersonagemModel> CreatePersonagem(string nome, string? observacao, TipoPersonagem? tipoPersonagem)
    {
        try
        {
            // Verifica se o nome do Personagem não está vazio
            if (string.IsNullOrWhiteSpace(nome))
            {
                throw new ArgumentException("O nome do Personagem não pode estar vazio.");
            }

            string apiUrl = _configuration["ApiUrl"];
            
            // Crie o objeto de requisição com os dados corretamente formatados
            var request = new
            {
                nome,
                observacao,
                tipoPersonagem
            };

            // Envie a solicitação
            var response = await _httpClient.PostAsJsonAsync(apiUrl + "Personagem/cadastrar-personagem", request);
            
            // Verifique se a solicitação foi bem-sucedida
            if (!response.IsSuccessStatusCode)
            {
                // Se a solicitação não for bem-sucedida, leia a resposta e gere uma mensagem de erro específica
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Erro ao criar personagem. Resposta do servidor: {response.StatusCode} - {errorMessage}");
            }
            
            // Leia a resposta e retorne os dados da personagem criada
            return await response.Content.ReadFromJsonAsync<PersonagemModel>();
        }
        catch (HttpRequestException ex)
        {
            // Captura exceções relacionadas à comunicação com o servidor
            throw new Exception("Erro ao se comunicar com o servidor: " + ex.Message);
        }
        catch (ArgumentException ex)
        {
            // Captura exceções relacionadas a argumentos inválidos
            throw new Exception("Erro ao criar personagem: " + ex.Message);
        }
        catch (Exception ex)
        {
            // Captura outras exceções
            throw new Exception("Erro ao criar personagem: " + ex.Message);
        }
    }

    public async Task<bool> UpdatePersonagem(string id, PersonagemModel personagem)
    {
        try
        {
            string apiUrl = _configuration["ApiUrl"];
            var response = await _httpClient.PutAsJsonAsync(apiUrl + $"Personagem/atualizar-personagem/{id}", new
            {
                Nome = personagem.Nome,
                Observacao = personagem.Observacao,
                TipoPersonagem = personagem.TipoPersonagem
            });

            if (!response.IsSuccessStatusCode)
            {
                // Se a solicitação não for bem-sucedida, leia a resposta e gere uma mensagem de erro específica
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Erro ao alterar personagem. Resposta do servidor: {response.StatusCode} - {errorMessage}");
            }

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("Erro ao atualizar personagem: " + ex.Message);
        }
    }

    public async Task DeletePersonagem(string id)
    {
        try
        {
            string apiUrl = _configuration["ApiUrl"];
            var response = await _httpClient.DeleteAsync(apiUrl + $"Personagem/excluir-personagem/{id}");

            if (!response.IsSuccessStatusCode)
            {
                // Se a solicitação não for bem-sucedida, leia a resposta e gere uma mensagem de erro específica
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Erro ao deletar personagem. Resposta do servidor: {response.StatusCode} - {errorMessage}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Erro ao excluir personagem: " + ex.Message);
        }
    }

    public class PersonagemResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("nome")] // Adicione essas anotações para especificar os nomes das propriedades no JSON
        public string Nome { get; set; }

        [JsonPropertyName("observacao")]
        public string Observacao { get; set; }

        [JsonPropertyName("tipoPersonagem")]
        public TipoPersonagem? TipoPersonagem { get; set; }
    }


    public class PaginatedResult<T>
    {
        public List<T> Personagens { get; set; }
        public int TotalPaginas { get; set; }

        public void Deconstruct(out List<T> personagens, out int totalPaginas)
        {
            personagens = Personagens;
            totalPaginas = TotalPaginas;
        }
    }
}