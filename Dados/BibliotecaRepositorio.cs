namespace GameVault.Dados;

public class BibliotecaRepositorio
{
    private const string Selecao = @"
        SELECT b.IdBiblioteca, b.IdUtilizador, b.IdJogo, b.IdPlataforma,
               j.Titulo, p.Nome, b.HorasJogadas, b.Estado, b.DataAdicao
        FROM dbo.Bibliotecas AS b
        INNER JOIN dbo.Jogos       AS j ON j.IdJogo       = b.IdJogo
        INNER JOIN dbo.Plataformas AS p ON p.IdPlataforma = b.IdPlataforma";

    private static ItemBiblioteca Mapear(SqlDataReader r) => new()
    {
        IdBiblioteca = r.GetInt32(0),
        IdUtilizador = r.GetInt32(1),
        IdJogo = r.GetInt32(2),
        IdPlataforma = r.GetInt32(3),
        TituloJogo = r.GetString(4),
        NomePlataforma = r.GetString(5),
        HorasJogadas = r.GetDecimal(6),
        Estado = r.GetString(7),
        DataAdicao = r.GetDateTime(8)
    };

    public List<ItemBiblioteca> ListarPorUtilizador(int idUtilizador) =>
        BaseDados.Consultar(Selecao + " WHERE b.IdUtilizador = @U ORDER BY j.Titulo", Mapear,
            BaseDados.Param("@U", idUtilizador));

    public ItemBiblioteca? Obter(int idUtilizador, int idJogo) =>
        BaseDados.Consultar(Selecao + " WHERE b.IdUtilizador = @U AND b.IdJogo = @J", Mapear,
            BaseDados.Param("@U", idUtilizador),
            BaseDados.Param("@J", idJogo)).FirstOrDefault();

    public void Inserir(ItemBiblioteca item) => BaseDados.Executar(
        @"INSERT INTO dbo.Bibliotecas (IdUtilizador, IdJogo, IdPlataforma, Estado)
          VALUES (@U, @J, @P, @Estado)",
        BaseDados.Param("@U", item.IdUtilizador),
        BaseDados.Param("@J", item.IdJogo),
        BaseDados.Param("@P", item.IdPlataforma),
        BaseDados.Param("@Estado", item.Estado));

    /// <summary>Soma as horas indicadas ao total já registado.</summary>
    public bool SomarHoras(int idUtilizador, int idJogo, decimal horas) => BaseDados.Executar(
        @"UPDATE dbo.Bibliotecas SET HorasJogadas = HorasJogadas + @Horas
          WHERE IdUtilizador = @U AND IdJogo = @J",
        BaseDados.Param("@Horas", horas),
        BaseDados.Param("@U", idUtilizador),
        BaseDados.Param("@J", idJogo)) > 0;

    public bool Atualizar(int idUtilizador, int idJogo, int idPlataforma, string estado) => BaseDados.Executar(
        @"UPDATE dbo.Bibliotecas SET IdPlataforma = @P, Estado = @Estado
          WHERE IdUtilizador = @U AND IdJogo = @J",
        BaseDados.Param("@P", idPlataforma),
        BaseDados.Param("@Estado", estado),
        BaseDados.Param("@U", idUtilizador),
        BaseDados.Param("@J", idJogo)) > 0;

    /// <summary>
    /// Remove o jogo da biblioteca e, na mesma transação, a avaliação e as conquistas
    /// associadas a esse par utilizador/jogo. Se algo falhar, nada é apagado (XACT_ABORT).
    /// </summary>
    public bool Remover(int idUtilizador, int idJogo) => BaseDados.Executar(
        @"SET XACT_ABORT ON;
          BEGIN TRANSACTION;
              DELETE FROM dbo.Avaliacoes   WHERE IdUtilizador = @U AND IdJogo = @J;
              DELETE FROM dbo.Conquistas   WHERE IdUtilizador = @U AND IdJogo = @J;
              DELETE FROM dbo.Bibliotecas  WHERE IdUtilizador = @U AND IdJogo = @J;
          COMMIT TRANSACTION;",
        BaseDados.Param("@U", idUtilizador),
        BaseDados.Param("@J", idJogo)) > 0;
}
