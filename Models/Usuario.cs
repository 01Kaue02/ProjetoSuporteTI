namespace ProjetoSuporteTI.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
    public string Senha { get; set; } = "";
    public DateTime DataCadastro { get; set; }
    public int Cargo { get; set; }
    public int Ativo { get; set; }
}

public class LoginResult
{
    public bool Success { get; set; }
    public Usuario? User { get; set; }
    public string Token { get; set; } = "";
    public string Message { get; set; } = "";
}

// Resultado da criação de chamado
public class CreateChamadoResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public int ChamadoId { get; set; }
}

// Classes para respostas da API
public class ApiLoginResponse
{
    public Usuario? User { get; set; }
    public string Token { get; set; } = "";
    public string Message { get; set; } = "";
    
    // Campos alternativos que podem vir da API
    public Usuario? Data { get; set; }
    public bool Success { get; set; }
}

public class ApiErrorResponse
{
    public string Message { get; set; } = "";
    public string Error { get; set; } = "";
    public string Type { get; set; } = "";
    public string Title { get; set; } = "";
    public int Status { get; set; }
    public object? Errors { get; set; }
}
