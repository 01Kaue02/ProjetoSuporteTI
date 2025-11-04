using System.Text;
using System.Text.Json;
using ProjetoSuporteTI.Models;
using System.Net.Http;

namespace ProjetoSuporteTI.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://api.suporteti.com"; // SUBSTITUA PELA SUA API

    public ApiService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "SuporteTI-Mobile/1.0");
    }

    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        try
        {
            // SIMULAÇÃO PARA DESENVOLVIMENTO - REMOVA EM PRODUÇÃO
            await Task.Delay(1500); // Simular delay de rede

            // Login de desenvolvimento
            if (email.Contains("@") && password.Length >= 4)
            {
                var user = new Usuario
                {
                    Id = 1,
                    Nome = "João Silva",
                    Email = email,
                    Cargo = "Analista de TI",
                    DataCriacao = DateTime.Now
                };

                return new LoginResult
                {
                    Success = true,
                    User = user,
                    Token = "dev-token-123",
                    Message = "Login realizado com sucesso!"
                };
            }

            return new LoginResult
            {
                Success = false,
                Message = "Email ou senha inválidos!"
            };

            /* CÓDIGO PARA API REAL - DESCOMENTE QUANDO TIVER API
            var loginData = new 
            {
                email = email,
                password = password
            };

            var json = JsonSerializer.Serialize(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{BaseUrl}/auth/login", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiLoginResponse>(responseContent);
                return new LoginResult
                {
                    Success = true,
                    User = result.User,
                    Token = result.Token,
                    Message = "Login realizado com sucesso!"
                };
            }
            else
            {
                return new LoginResult
                {
                    Success = false,
                    Message = "Credenciais inválidas!"
                };
            }
            */
        }
        catch (Exception ex)
        {
            return new LoginResult
            {
                Success = false,
                Message = $"Erro de conexão: {ex.Message}"
            };
        }
    }

    public async Task<List<Chamado>> GetChamadosAsync()
    {
        // SIMULAÇÃO PARA DESENVOLVIMENTO
        await Task.Delay(1000);

        return new List<Chamado>
        {
            new Chamado
            {
                Id = 1,
                Titulo = "Problema no computador",
                Descricao = "Computador não liga",
                Status = "Aberto",
                Prioridade = "Alta",
                DataCriacao = DateTime.Now.AddDays(-2)
            },
            new Chamado
            {
                Id = 2,
                Titulo = "Impressora com defeito",
                Descricao = "Impressora não imprime",
                Status = "Em Andamento",
                Prioridade = "Média",
                DataCriacao = DateTime.Now.AddDays(-1)
            }
        };
    }

    public async Task<bool> CreateChamadoAsync(Chamado chamado)
    {
        // SIMULAÇÃO PARA DESENVOLVIMENTO
        await Task.Delay(1500);
        return true; // Simular sucesso
    }
}
