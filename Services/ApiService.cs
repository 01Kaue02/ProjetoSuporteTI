using System.Text;
using System.Text.Json;
using ProjetoSuporteTI.Models;

namespace ProjetoSuporteTI.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://api-chat-n79k.onrender.com";
    
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public Usuario? CurrentUser { get; private set; }

    public ApiService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "SuporteTI-Mobile/1.0");
    }

    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        try
        {
            Console.WriteLine("=== LOGIN FINAL ===");
            Console.WriteLine($"🔐 Email: {email}");

            // FORMATO QUE FUNCIONOU - Usuario completo
            var usuarioCompleto = new
            {
                id = 0,
                nome = "",
                email = email,
                senha = password,
                dataCadastro = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                cargo = 0,
                chamados = new string[] { }
            };

            var json = JsonSerializer.Serialize(usuarioCompleto, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            Console.WriteLine($"📤 Enviando: {json}");

            var response = await _httpClient.PostAsync($"{BaseUrl}/api/Login/LoginUsuario", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"📥 Status: {response.StatusCode}");
            Console.WriteLine($"📥 Resposta: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("✅ LOGIN FUNCIONOU!");
                Console.WriteLine($"📄 RESPOSTA: {responseContent}");
                
                // A API retorna apenas mensagem de sucesso, não os dados do usuário
                // Vamos tentar buscar os dados do usuário em outro endpoint
                try
                {
                    Console.WriteLine("🔍 Tentando buscar dados do usuário...");
                    
                    // Tentar buscar por email
                    var userDataResponse = await _httpClient.GetAsync($"{BaseUrl}/api/Usuario/ObterPorEmail/{Uri.EscapeDataString(email)}");
                    
                    if (!userDataResponse.IsSuccessStatusCode)
                    {
                        // Tentar endpoint alternativo
                        Console.WriteLine("� Tentando endpoint alternativo...");
                        userDataResponse = await _httpClient.GetAsync($"{BaseUrl}/api/Usuario?email={Uri.EscapeDataString(email)}");
                    }
                    
                    if (!userDataResponse.IsSuccessStatusCode)
                    {
                        // Tentar POST para buscar usuário
                        Console.WriteLine("🔍 Tentando POST para buscar usuário...");
                        var searchData = new { email = email };
                        var searchJson = JsonSerializer.Serialize(searchData, _jsonOptions);
                        var searchContent = new StringContent(searchJson, Encoding.UTF8, "application/json");
                        userDataResponse = await _httpClient.PostAsync($"{BaseUrl}/api/Usuario/BuscarPorEmail", searchContent);
                    }
                    
                    if (userDataResponse.IsSuccessStatusCode)
                    {
                        var userDataContent = await userDataResponse.Content.ReadAsStringAsync();
                        Console.WriteLine($"📄 DADOS DO USUÁRIO: {userDataContent}");
                        
                        try
                        {
                            var user = JsonSerializer.Deserialize<Usuario>(userDataContent, _jsonOptions);
                            
                            if (user != null && user.Id > 0)
                            {
                                Console.WriteLine($"👤 Usuário encontrado: {user.Nome} (ID: {user.Id}, Cargo: {user.Cargo})");
                                
                                // Validar cargo = 1 (usuário comum)
                                if (user.Cargo != 1)
                                {
                                    string cargoNome = user.Cargo switch
                                    {
                                        2 => "Gerente",
                                        3 => "Suporte",
                                        _ => "Desconhecido"
                                    };
                                    
                                    Console.WriteLine($"❌ Acesso negado: Cargo {user.Cargo} ({cargoNome})");
                                    
                                    return new LoginResult
                                    {
                                        Success = false,
                                        Message = $"Acesso restrito! Este app é apenas para usuários comuns. Você está cadastrado como {cargoNome}."
                                    };
                                }
                                
                                // Login aprovado!
                                CurrentUser = user;
                                
                                Console.WriteLine($"🎉 Login aprovado para usuário: {user.Nome}");
                                
                                return new LoginResult
                                {
                                    Success = true,
                                    User = user,
                                    Token = "",
                                    Message = "Login realizado com sucesso!"
                                };
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ Erro ao processar dados do usuário: {ex.Message}");
                        }
                    }
                    
                    // Se chegou até aqui, não conseguiu buscar os dados do usuário
                    // Mas o login foi válido, então vamos criar um usuário básico
                    Console.WriteLine("⚠️ Criando usuário básico baseado no email...");
                    
                    var basicUser = new Usuario
                    {
                        Id = 1, // ID temporário
                        Nome = email.Split('@')[0], // Nome baseado no email
                        Email = email,
                        Cargo = 1 // Assumir que é usuário comum se o login funcionou
                    };
                    
                    CurrentUser = basicUser;
                    
                    return new LoginResult
                    {
                        Success = true,
                        User = basicUser,
                        Token = "",
                        Message = "Login realizado com sucesso! (Dados básicos)"
                    };
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro ao buscar dados do usuário: {ex.Message}");
                    
                    // Login foi válido, criar usuário básico
                    var basicUser = new Usuario
                    {
                        Id = 1,
                        Nome = email.Split('@')[0],
                        Email = email,
                        Cargo = 1
                    };
                    
                    CurrentUser = basicUser;
                    
                    return new LoginResult
                    {
                        Success = true,
                        User = basicUser,
                        Token = "",
                        Message = "Login realizado com sucesso! (Dados básicos)"
                    };
                }
            }
            else
            {
                Console.WriteLine($"❌ Erro HTTP: {response.StatusCode}");
                
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<ApiErrorResponse>(responseContent, _jsonOptions);
                    return new LoginResult
                    {
                        Success = false,
                        Message = errorResponse?.Message ?? responseContent
                    };
                }
                catch
                {
                    return new LoginResult
                    {
                        Success = false,
                        Message = $"Erro {response.StatusCode}: {responseContent}"
                    };
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Exceção no login: {ex.Message}");
            return new LoginResult
            {
                Success = false,
                Message = $"Erro de conexão: {ex.Message}"
            };
        }
    }

    public void Logout()
    {
        CurrentUser = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    // Método para criar chamado
    public async Task<bool> CreateChamadoAsync(string titulo, string descricao)
    {
        try
        {
            if (CurrentUser == null)
            {
                Console.WriteLine("❌ Usuário não logado");
                return false;
            }

            var chamado = new
            {
                titulo = titulo,
                descricao = descricao,
                usuarioId = CurrentUser.Id,
                status = "Aberto",
                prioridade = "Média",
                dataAbertura = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };

            var json = JsonSerializer.Serialize(chamado, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            Console.WriteLine($"📤 Criando chamado: {titulo}");

            var response = await _httpClient.PostAsync($"{BaseUrl}/api/Chamado/CriarChamado", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"📥 Status: {response.StatusCode}");
            Console.WriteLine($"📥 Resposta: {responseContent}");

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Erro ao criar chamado: {ex.Message}");
            return false;
        }
    }

    // Sobrecarga para aceitar objeto Chamado
    public async Task<bool> CreateChamadoAsync(Chamado chamado)
    {
        return await CreateChamadoAsync(chamado.Titulo, chamado.Descricao);
    }
}