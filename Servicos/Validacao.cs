using System.Text.RegularExpressions;

namespace GameVault.Servicos;

/// <summary>Erro de regra de negócio ou de dados inválidos introduzidos pelo utilizador.</summary>
public class ValidacaoException : Exception
{
    public ValidacaoException(string mensagem) : base(mensagem) { }
}

/// <summary>Validações de entrada reutilizadas pelos serviços (espelham os CHECK da base de dados).</summary>
public static class Validacao
{
    public static string Texto(string? valor, string campo, int maximo)
    {
        var texto = valor?.Trim() ?? string.Empty;
        if (texto.Length == 0)
            throw new ValidacaoException($"O campo '{campo}' é obrigatório.");
        if (texto.Length > maximo)
            throw new ValidacaoException($"O campo '{campo}' não pode ter mais de {maximo} caracteres.");
        return texto;
    }

    /// <summary>Texto facultativo: devolve null se estiver vazio.</summary>
    public static string? TextoOpcional(string? valor, string campo, int maximo)
    {
        var texto = valor?.Trim() ?? string.Empty;
        if (texto.Length == 0) return null;
        if (texto.Length > maximo)
            throw new ValidacaoException($"O campo '{campo}' não pode ter mais de {maximo} caracteres.");
        return texto;
    }

    public static string Email(string? valor)
    {
        var email = Texto(valor, "Email", 150);
        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new ValidacaoException("O email indicado não tem um formato válido.");
        return email;
    }

    public static int Ano(int ano)
    {
        if (ano < 1970 || ano > 2100)
            throw new ValidacaoException("O ano de lançamento tem de estar entre 1970 e 2100.");
        return ano;
    }

    public static decimal Nota(decimal nota)
    {
        if (nota < 0 || nota > 10)
            throw new ValidacaoException("A nota tem de estar entre 0 e 10.");
        if (decimal.Round(nota, 1) != nota)
            throw new ValidacaoException("A nota admite no máximo uma casa decimal (ex.: 8,5).");
        return nota;
    }

    public static decimal Horas(decimal horas)
    {
        if (horas <= 0 || horas > 1000)
            throw new ValidacaoException("As horas a registar têm de ser superiores a 0 e não podem exceder 1000 por registo.");
        if (decimal.Round(horas, 1) != horas)
            throw new ValidacaoException("As horas admitem no máximo uma casa decimal (ex.: 1,5).");
        return horas;
    }

    public static int Pontos(int pontos)
    {
        if (pontos < 1 || pontos > 1000)
            throw new ValidacaoException("Os pontos de uma conquista têm de estar entre 1 e 1000.");
        return pontos;
    }

    public static string Estado(string? estado)
    {
        if (estado is null || !EstadosJogo.Todos.Contains(estado))
            throw new ValidacaoException($"Estado inválido. Valores permitidos: {string.Join(", ", EstadosJogo.Todos)}.");
        return estado;
    }
}
