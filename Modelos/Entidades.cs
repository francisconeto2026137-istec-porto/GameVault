namespace GameVault.Modelos;

public class Categoria
{
    public int IdCategoria { get; set; }
    public string Nome { get; set; } = string.Empty;
}

public class Editora
{
    public int IdEditora { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Pais { get; set; }
}

public class Plataforma
{
    public int IdPlataforma { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Fabricante { get; set; }
}

public class Utilizador
{
    public int IdUtilizador { get; set; }
    public string NomeUtilizador { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DataRegisto { get; set; }
}

public class Jogo
{
    public int IdJogo { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public int AnoLancamento { get; set; }
    public int IdCategoria { get; set; }
    public int IdEditora { get; set; }

    // Campos apenas de leitura, preenchidos pelas junções (JOIN)
    public string NomeCategoria { get; set; } = string.Empty;
    public string NomeEditora { get; set; } = string.Empty;
}

/// <summary>Uma linha da tabela Bibliotecas: um jogo na biblioteca de um utilizador.</summary>
public class ItemBiblioteca
{
    public int IdBiblioteca { get; set; }
    public int IdUtilizador { get; set; }
    public int IdJogo { get; set; }
    public int IdPlataforma { get; set; }
    public decimal HorasJogadas { get; set; }
    public string Estado { get; set; } = EstadosJogo.PorJogar;
    public DateTime DataAdicao { get; set; }

    public string TituloJogo { get; set; } = string.Empty;
    public string NomePlataforma { get; set; } = string.Empty;
}

public class Avaliacao
{
    public int IdAvaliacao { get; set; }
    public int IdUtilizador { get; set; }
    public int IdJogo { get; set; }
    public decimal Nota { get; set; }
    public string? Comentario { get; set; }
    public DateTime DataAvaliacao { get; set; }

    public string TituloJogo { get; set; } = string.Empty;
}

public class Conquista
{
    public int IdConquista { get; set; }
    public int IdUtilizador { get; set; }
    public int IdJogo { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int Pontos { get; set; }
    public DateTime DataDesbloqueio { get; set; }

    public string TituloJogo { get; set; } = string.Empty;
}
