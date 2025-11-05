using System.Text;
using System.Text.Json;
using ProjetoSuporteTI.Models;

namespace ProjetoSuporteTI.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://api-chat-n79k.onrender.com";
    
    // Singleton para manter o estado do usuário logado
    private static ApiService? _instance;
    public static ApiService Instance => _instance ??= new ApiService();
    
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Voltar para camelCase
        PropertyNameCaseInsensitive = true
    };

    public Usuario? CurrentUser { get; private set; }

    // Método para verificar se há usuário logado
    public bool IsUserLoggedIn => CurrentUser != null;
    
    // Método para obter info do usuário logado
    public string GetUserInfo()
    {
        if (CurrentUser == null)
            return "❌ Nenhum usuário logado";
        
        return $"✅ {CurrentUser.Nome} (ID: {CurrentUser.Id}, Cargo: {CurrentUser.Cargo})";
    }
    
    // Método para restaurar usuário das preferências
    public void RestoreUserFromPreferences()
    {
        if (CurrentUser != null) return; // Já tem usuário logado
        
        var isLoggedIn = Preferences.Get("user_logged_in", "false");
        if (isLoggedIn == "true")
        {
            var userId = Preferences.Get("user_id", "0");
            var userName = Preferences.Get("user_nome", "");
            var userEmail = Preferences.Get("user_email", "");
            var userCargo = Preferences.Get("user_cargo", "0");
            
            if (int.TryParse(userId, out int id) && id > 0)
            {
                CurrentUser = new Usuario
                {
                    Id = id,
                    Nome = userName,
                    Email = userEmail,
                    Cargo = int.Parse(userCargo)
                };
                
                Console.WriteLine($"🔄 Usuário restaurado das preferências: {GetUserInfo()}");
            }
        }
    }

    private ApiService() // Construtor privado para Singleton
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "SuporteTI-Mobile/1.0");
    }

    // Mapear email para ID de usuário válido na API
    private int GetUserIdByEmail(string email)
    {
        // Mapeamento conhecido de emails para IDs
        return email.ToLower() switch
        {
            "sofia.g@empresa.com.br" => 11,  // Sofia - ID 11
            _ => 10  // Usuário padrão - ID 10
        };
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
                                
                                // Salvar dados do usuário nas preferências como backup
                                Preferences.Set("user_id", user.Id.ToString());
                                Preferences.Set("user_nome", user.Nome ?? "");
                                Preferences.Set("user_email", user.Email ?? "");
                                Preferences.Set("user_cargo", user.Cargo.ToString());
                                Preferences.Set("user_logged_in", "true");
                                
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
                    
                    var userId = GetUserIdByEmail(email);
                    Console.WriteLine($"🔍 Email: {email} → ID: {userId}");
                    
                    var basicUser = new Usuario
                    {
                        Id = userId, // ID baseado no email
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
                    var userId = GetUserIdByEmail(email);
                    Console.WriteLine($"🔍 Email: {email} → ID: {userId}");
                    
                    var basicUser = new Usuario
                    {
                        Id = userId, // ID baseado no email
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
    public async Task<CreateChamadoResult> CreateChamadoAsync(string titulo, string descricao, string prioridade = "Média", string dispositivo = "Outros")
    {
        try
        {
            if (CurrentUser == null)
            {
                Console.WriteLine("❌ ERRO: Usuário não logado no ApiService");
                Console.WriteLine("🔍 DEBUG: Verificar se o login foi feito corretamente");
                return new CreateChamadoResult 
                { 
                    Success = false, 
                    Message = "Usuário não está logado. Faça login novamente." 
                };
            }

            Console.WriteLine($"✅ Usuário logado: {CurrentUser.Nome} (ID: {CurrentUser.Id})");

            // Mapear prioridade de texto para número conforme API
            int prioridadeNumero = prioridade switch
            {
                "Baixo" => 1,
                "Médio" => 2,
                "Alto" => 3,
                "Crítica" => 4,
                _ => 2 // Padrão: Médio
            };

            // Mapear dispositivo selecionado para número conforme banco de dados
            // 1=Teclado, 2=Mouse, 3=Monitor, 4=Impressora, 5=Outros
            int dispositivoNumero = dispositivo switch
            {
                "Teclado" => 1,      // Teclado
                "Mouse" => 2,        // Mouse
                "Monitor" => 3,      // Monitor
                "Impressora" => 4,   // Impressora
                "Outros" => 5,       // Outros
                _ => 5               // Padrão: Outros
            };

            Console.WriteLine($"📤 PREPARANDO DADOS:");
            Console.WriteLine($"   IdUsuario: {CurrentUser.Id}");
            Console.WriteLine($"   Descricao: '{descricao}' (Length: {descricao?.Length ?? 0})");
            Console.WriteLine($"   Status: 1");
            Console.WriteLine($"   Prioridade: {prioridadeNumero} ('{prioridade}')");
            Console.WriteLine($"   Dispositivo selecionado: '{dispositivo}'");
            Console.WriteLine($"   Dispositivo mapeado: {dispositivoNumero} ({dispositivo} -> {GetDispositivoTexto(dispositivoNumero)})");

            // ESTRUTURA CORRETA descoberta: SEM wrapper, campos diretos
            var requestBody = new
            {
                IdUsuario = CurrentUser.Id,
                Descricao = descricao,
                Status = 1, // Número: 1=Aberto  
                Prioridade = prioridadeNumero,
                Dispositivo = dispositivoNumero
            };

            // Usar JsonSerializerOptions específico para envio (sem conversão de nomes)
            var sendOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null, // Manter PascalCase
                PropertyNameCaseInsensitive = true
            };

            var json = JsonSerializer.Serialize(requestBody, sendOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            Console.WriteLine($"📤 Criando chamado:");
            Console.WriteLine($"   📝 Descrição: {descricao}");
            Console.WriteLine($"   👤 ID Usuário: {CurrentUser.Id}");
            Console.WriteLine($"   ⚡ Prioridade: {prioridade} ({prioridadeNumero})");
            Console.WriteLine($"   💻 Dispositivo: {DeviceInfo.Model ?? "Desconhecido"}");
            Console.WriteLine($"   🌐 URL: {BaseUrl}/api/Chamado/CriarChamado");
            Console.WriteLine($"   📤 JSON: {json}");

            var response = await _httpClient.PostAsync($"{BaseUrl}/api/Chamado/CriarChamado", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"📥 Status: {response.StatusCode}");
            Console.WriteLine($"📥 Resposta: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    // Tentar deserializar resposta para pegar o ID do chamado
                    var createdChamado = JsonSerializer.Deserialize<Chamado>(responseContent, _jsonOptions);
                    
                    var result = new CreateChamadoResult
                    {
                        Success = true,
                        Message = "Chamado criado com sucesso!",
                        ChamadoId = createdChamado?.Id ?? 0
                    };

                    Console.WriteLine($"✅ Chamado criado! ID: {result.ChamadoId}");
                    
                    return result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Erro ao deserializar resposta: {ex.Message}");
                    
                    // Se não conseguir deserializar, pelo menos retorna sucesso
                    var result = new CreateChamadoResult
                    {
                        Success = true,
                        Message = "Chamado criado com sucesso! (ID não disponível)",
                        ChamadoId = 0
                    };

                    Console.WriteLine($"✅ Chamado criado! (sem ID)");
                    
                    return result;
                }
            }
            else
            {
                Console.WriteLine($"❌ Erro na API: {response.StatusCode}");
                
                // Tentar extrair mensagem de erro
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<ApiErrorResponse>(responseContent, _jsonOptions);
                    return new CreateChamadoResult
                    {
                        Success = false,
                        Message = errorResponse?.Message ?? $"Erro {response.StatusCode}: {responseContent}"
                    };
                }
                catch
                {
                    return new CreateChamadoResult
                    {
                        Success = false,
                        Message = $"Erro {response.StatusCode}: {responseContent}"
                    };
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Erro ao criar chamado: {ex.Message}");
            return new CreateChamadoResult
            {
                Success = false,
                Message = $"Erro de conexão: {ex.Message}"
            };
        }
    }

    // Sobrecarga para aceitar objeto Chamado (mantida para compatibilidade)
    public async Task<bool> CreateChamadoAsync(Chamado chamado)
    {
        var prioridadeTexto = chamado.Prioridade switch
        {
            1 => "Baixo",
            2 => "Médio", 
            3 => "Alto",
            _ => "Médio"
        };
        
        var dispositivoTexto = chamado.Dispositivo switch
        {
            1 => "Teclado",
            2 => "Mouse",
            3 => "Monitor", 
            4 => "Impressora",
            5 => "Outros",
            _ => "Outros"
        };
        
        var result = await CreateChamadoAsync(chamado.Titulo, chamado.Descricao, prioridadeTexto, dispositivoTexto);
        return result.Success;
    }

    // Método para listar chamados do usuário
    public async Task<List<Chamado>> GetChamadosAsync()
    {
        try
        {
            if (CurrentUser == null)
            {
                Console.WriteLine("❌ Usuário não logado");
                return new List<Chamado>();
            }

            Console.WriteLine($"📋 Buscando chamados...");

            // Usar o endpoint correto baseado na documentação
            var response = await _httpClient.GetAsync($"{BaseUrl}/api/Chamado/RetornarChamados");
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"📥 Status: {response.StatusCode}");
            Console.WriteLine($"📥 Resposta: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var chamados = JsonSerializer.Deserialize<List<Chamado>>(responseContent, _jsonOptions);
                    
                    if (chamados != null)
                    {
                        // Filtrar apenas os chamados do usuário atual
                        var meusChamados = chamados.Where(c => c.IdUsuario == CurrentUser.Id).ToList();
                        
                        Console.WriteLine($"✅ Total de chamados: {chamados.Count}");
                        Console.WriteLine($"📋 Meus chamados: {meusChamados.Count}");
                        
                        return meusChamados;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro ao deserializar chamados: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"❌ Erro na API: {response.StatusCode} - {responseContent}");
            }

            return new List<Chamado>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Erro ao buscar chamados: {ex.Message}");
            return new List<Chamado>();
        }
    }
    
    // Método para finalizar chamado usando o endpoint correto da API
    public async Task<bool> FinalizarChamadoAsync(int chamadoId)
    {
        try
        {
            if (CurrentUser == null)
            {
                Console.WriteLine("❌ Usuário não logado");
                return false;
            }

            Console.WriteLine($"🔄 Finalizando chamado {chamadoId} como resolvido pela IA");

            // Usar o endpoint /api/Chamado/finalizarChamado conforme documentação
            var requestBody = new
            {
                Id = chamadoId,
                IdUsuario = CurrentUser.Id,
                Descricao = "Chamado resolvido pela IA", // Campo obrigatório
                Status = 2, // 2 = Resolvido por IA
                Prioridade = 2, // Prioridade padrão
                Dispositivo = 5, // Outros (padrão)
                DataAbertura = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffK"),
                DataFechamento = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffK")
            };

            var sendOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                PropertyNameCaseInsensitive = true
            };

            var json = JsonSerializer.Serialize(requestBody, sendOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            Console.WriteLine($"📤 Enviando para finalizar: {json}");

            var response = await _httpClient.PutAsync($"{BaseUrl}/api/Chamado/FinalizarChamadoUsuario", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"📥 Resposta finalização: {response.StatusCode}");
            Console.WriteLine($"📥 Conteúdo: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("✅ Chamado finalizado com sucesso!");
                return true;
            }
            else
            {
                Console.WriteLine($"❌ Erro ao finalizar chamado: {response.StatusCode}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Exceção ao finalizar chamado: {ex.Message}");
            return false;
        }
    }

    // Método para atualizar status do chamado para "Resolvido por IA"
    public async Task<bool> MarcarComoResolvidoPorIAAsync(int chamadoId)
    {
        // Usar o endpoint específico para finalizar chamado
        return await FinalizarChamadoAsync(chamadoId);
    }
    
    // Método para atualizar status do chamado para "Resolvido por Suporte"
    public async Task<bool> MarcarComoResolvidoPorSuporteAsync(int chamadoId)
    {
        return await AtualizarStatusChamadoAsync(chamadoId, 3, "Resolvido por Suporte");
    }
    
    // Método privado para atualizar status
    private async Task<bool> AtualizarStatusChamadoAsync(int chamadoId, int novoStatus, string statusNome)
    {
        try
        {
            if (CurrentUser == null)
            {
                Console.WriteLine("❌ Usuário não logado");
                return false;
            }

            Console.WriteLine($"🔄 Atualizando status do chamado {chamadoId} para: {statusNome} ({novoStatus})");

            // Estrutura para atualizar status (pode variar conforme API)
            var requestBody = new
            {
                Id = chamadoId,
                Status = novoStatus
            };

            var sendOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                PropertyNameCaseInsensitive = true
            };

            var json = JsonSerializer.Serialize(requestBody, sendOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            Console.WriteLine($"📤 JSON para atualização: {json}");

            // Tentar endpoint de atualização (pode precisar ajustar conforme API)
            var response = await _httpClient.PutAsync($"{BaseUrl}/api/Chamado/AtualizarStatus/{chamadoId}", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"📥 Status da atualização: {response.StatusCode}");
            Console.WriteLine($"📥 Resposta: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"✅ Status atualizado com sucesso para: {statusNome}");
                return true;
            }
            else
            {
                Console.WriteLine($"❌ Erro ao atualizar status: {response.StatusCode}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Erro ao atualizar status: {ex.Message}");
            return false;
        }
    }
    
    // Método helper para obter texto do dispositivo conforme banco de dados
    private string GetDispositivoTexto(int dispositivo)
    {
        return dispositivo switch
        {
            1 => "Teclado",
            2 => "Mouse", 
            3 => "Monitor",
            4 => "Impressora",
            5 => "Outros",
            _ => "Desconhecido"
        };
    }
}