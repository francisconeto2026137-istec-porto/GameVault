namespace GameVault.Interface;

/// <summary>Lançada quando o utilizador cancela uma escolha.</summary>
public class OperacaoCanceladaException : Exception { }

/// <summary>Centraliza o tratamento de erros: nenhuma exceção chega a "rebentar" a aplicação.</summary>
public static class TratamentoErros
{
    public static void Executar(Action acao)
    {
        try
        {
            acao();
        }
        catch (OperacaoCanceladaException)
        {
            Consola.Info("Operação cancelada.");
        }
        catch (ValidacaoException ex)
        {
            Consola.Aviso(ex.Message);
        }
        catch (SqlException ex)
        {
            Consola.Erro($"Erro de base de dados: {Traduzir(ex)}");
        }
        catch (Exception ex)
        {
            Consola.Erro($"Erro inesperado: {ex.Message}");
        }
    }

    public static string Traduzir(SqlException ex) => ex.Number switch
    {
        2627 or 2601 => "já existe um registo com esses dados (valor duplicado).",
        547 => "operação recusada: o registo está associado a outros dados ou um valor viola uma regra de integridade.",
        2 or 40 or 53 or 4060 or 18456 or 10060 or -1 =>
            "não foi possível ligar à base de dados. Verifique o servidor, as credenciais e a cadeia de ligação em appsettings.json.",
        _ => $"{ex.Message} (código {ex.Number})"
    };
}
