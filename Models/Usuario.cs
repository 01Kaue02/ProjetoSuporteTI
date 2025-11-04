namespace ProjetoSuporteTI.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
    public string Cargo { get; set; } = "";
    public DateTime DataCriacao { get; set; }
}

public class LoginResult
{
    public bool Success { get; set; }
    public Usuario? User { get; set; }
    public string Token { get; set; } = "";
    public string Message { get; set; } = "";
}
