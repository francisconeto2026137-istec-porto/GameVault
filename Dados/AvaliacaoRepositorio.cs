namespace GameVault.Dados;

public class AvaliacaoRepositorio
{
    private const string Selecao = @"
        SELECT a.IdAvaliacao, a.IdUtilizador, a.IdJogo, j.Titulo, a.Nota, a.Comentario, a.DataAvaliacao
        FROM dbo.Avaliacoes AS a
        INNER JOIN dbo.Jogos AS j ON j.IdJogo = a.IdJogo";

    private static Avaliacao Mapear(SqlDataReader r) => new()
    {
        IdAvaliacao = r.GetInt32(0),
        IdUtilizador = r.GetInt32(1),
        IdJogo = r.GetInt32(2),
        TituloJogo = r.GetString(3),
        Nota = r.GetDecimal(4),
        Comentario = r.TextoOuNulo(5),
        DataAvaliacao = r.GetDateTime(6)
    };

    public List<Avaliacao> ListarPorUtilizador(int idUtilizador) =>
        BaseDados.Consultar(Selecao + " WHERE a.IdUtilizador = @U ORDER BY a.Nota DESC, j.Titulo", Mapear,
            BaseDados.Param("@U", idUtilizador));

    public Avaliacao? Obter(int idUtilizador, int idJogo) =>
        BaseDados.Consultar(Selecao + " WHERE a.IdUtilizador = @U AND a.IdJogo = @J", Mapear,
            BaseDados.Param("@U", idUtilizador),
            BaseDados.Param("@J", idJogo)).FirstOrDefault();

    public void Inserir(Avaliacao a) => BaseDados.Executar(
        @"INSERT INTO dbo.Avaliacoes (IdUtilizador, IdJogo, Nota, Comentario)
          VALUES (@U, @J, @Nota, @Comentario)",
        BaseDados.Param("@U", a.IdUtilizador),
        BaseDados.Param("@J", a.IdJogo),
        BaseDados.Param("@Nota", a.Nota),
        BaseDados.Param("@Comentario", a.Comentario));

    public bool Atualizar(Avaliacao a) => BaseDados.Executar(
        @"UPDATE dbo.Avaliacoes
          SET Nota = @Nota, Comentario = @Comentario, DataAvaliacao = SYSDATETIME()
          WHERE IdUtilizador = @U AND IdJogo = @J",
        BaseDados.Param("@Nota", a.Nota),
        BaseDados.Param("@Comentario", a.Comentario),
        BaseDados.Param("@U", a.IdUtilizador),
        BaseDados.Param("@J", a.IdJogo)) > 0;

    public bool Eliminar(int idUtilizador, int idJogo) => BaseDados.Executar(
        "DELETE FROM dbo.Avaliacoes WHERE IdUtilizador = @U AND IdJogo = @J",
        BaseDados.Param("@U", idUtilizador),
        BaseDados.Param("@J", idJogo)) > 0;
}
