namespace ProjetoSuporteTI.Models;

public class Chamado
{
    // Campos REAIS da API (baseado no retorno de RetornarChamados)
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public string Descricao { get; set; } = "";
    public int Status { get; set; } // Número: 1=Aberto, 2=Em andamento, 3=Fechado
    public int Prioridade { get; set; } // 1=Baixa, 2=Média, 3=Alta
    public DateTime DataAbertura { get; set; }
    public DateTime? DataFechamento { get; set; }
    public int Dispositivo { get; set; } // Número: 1=Desktop, 2=Notebook, 3=Mobile
    
    // Campos para compatibilidade com a criação (API pode esperar estes na criação)
    public string Objeto { get; set; } = ""; // Para criação
    public string Nome { get; set; } = ""; // Para criação
    public string Email { get; set; } = ""; // Para criação
    public DateTime DataCadastramento 
    { 
        get => DataAbertura; 
        set => DataAbertura = value; 
    }
    public DateTime? DataConclusao 
    { 
        get => DataFechamento; 
        set => DataFechamento = value; 
    }
    public int Cargo { get; set; } // Para criação
    public int Ativo { get; set; } = 1; // Para criação
    
    // Propriedades de conveniência
    public int UsuarioId 
    { 
        get => IdUsuario; 
        set => IdUsuario = value; 
    }
    
    public string Titulo 
    { 
        get => Objeto; 
        set => Objeto = value; 
    }
    
    public DateTime DataCriacao 
    { 
        get => DataAbertura; 
        set => DataAbertura = value; 
    }
    
    public DateTime? DataResolucao 
    { 
        get => DataFechamento; 
        set => DataFechamento = value; 
    }
    
    // Propriedades auxiliares para interface
    public string StatusTexto => Status switch
    {
        1 => "Aberto",
        2 => "Resolvido por IA",
        3 => "Resolvido por Suporte",
        _ => "Desconhecido"
    };    public string PrioridadeTexto => Prioridade switch
    {
        1 => "Baixa",
        2 => "Média",
        3 => "Alta", 
        _ => "Desconhecida"
    };
    
    public string DispositivoTexto => Dispositivo switch
    {
        1 => "Teclado",
        2 => "Mouse",
        3 => "Monitor",
        4 => "Impressora",
        5 => "Outros",
        _ => "Desconhecido"
    };
}
