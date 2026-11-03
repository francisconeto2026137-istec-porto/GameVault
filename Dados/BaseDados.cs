using System.Data;

namespace GameVault.Dados;

/// <summary>
/// Acesso genérico à base de dados com ADO.NET. Todas as consultas usam parâmetros
/// (nunca concatenação de texto), o que previne SQL Injection.
/// Cada operação abre e fecha a sua própria ligação (o "pool" de ligações trata do resto).
/// </summary>
public static class BaseDados
{
    private static string _cadeiaLigacao = string.Empty;

    public static void Configurar(string cadeiaLigacao) => _cadeiaLigacao = cadeiaLigacao;

    /// <summary>Abre e fecha uma ligação; lança SqlException se não for possível ligar.</summary>
    public static void TestarLigacao()
    {
        using var ligacao = new SqlConnection(_cadeiaLigacao);
        ligacao.Open();
    }

    /// <summary>Cria um parâmetro SQL. Valores nulos passam a DBNull; decimais usam escala 1.</summary>
    public static SqlParameter Param(string nome, object? valor)
    {
        if (valor is decimal d)
            return new SqlParameter(nome, SqlDbType.Decimal) { Precision = 9, Scale = 1, Value = d };

        return new SqlParameter(nome, valor ?? DBNull.Value);
    }

    /// <summary>Executa um SELECT e converte cada linha num objeto através da função "mapear".</summary>
    public static List<T> Consultar<T>(string sql, Func<SqlDataReader, T> mapear, params SqlParameter[] parametros)
    {
        var resultado = new List<T>();
        using var ligacao = new SqlConnection(_cadeiaLigacao);
        using var comando = new SqlCommand(sql, ligacao);
        comando.Parameters.AddRange(parametros);
        ligacao.Open();
        using var leitor = comando.ExecuteReader();
        while (leitor.Read())
            resultado.Add(mapear(leitor));
        return resultado;
    }

    /// <summary>Executa INSERT/UPDATE/DELETE e devolve o número de linhas afetadas.</summary>
    public static int Executar(string sql, params SqlParameter[] parametros)
    {
        using var ligacao = new SqlConnection(_cadeiaLigacao);
        using var comando = new SqlCommand(sql, ligacao);
        comando.Parameters.AddRange(parametros);
        ligacao.Open();
        return comando.ExecuteNonQuery();
    }

    /// <summary>Executa uma consulta e devolve o primeiro valor da primeira linha.</summary>
    public static object? Escalar(string sql, params SqlParameter[] parametros)
    {
        using var ligacao = new SqlConnection(_cadeiaLigacao);
        using var comando = new SqlCommand(sql, ligacao);
        comando.Parameters.AddRange(parametros);
        ligacao.Open();
        return comando.ExecuteScalar();
    }

    /// <summary>Lê uma coluna de texto que pode ser NULL.</summary>
    public static string? TextoOuNulo(this SqlDataReader leitor, int indice) =>
        leitor.IsDBNull(indice) ? null : leitor.GetString(indice);
}
