namespace ProjetoSuporteTI.Models;

public class Chamado
{
    public int Id { get; set; }
    public string Titulo { get; set; } = "";
    public string Descricao { get; set; } = "";
    public string Status { get; set; } = "";
    public string Prioridade { get; set; } = "";
    public DateTime DataCriacao { get; set; }
    public DateTime? DataResolucao { get; set; }
    public int UsuarioId { get; set; }
}
