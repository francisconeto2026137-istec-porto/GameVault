namespace GameVault.Dados;

public class ConquistaRepositorio
{
    private static Conquista Mapear(SqlDataReader r) => new()
    {
        IdConquista = r.GetInt32(0),
        IdUtilizador = r.GetInt32(1),
        IdJogo = r.GetInt32(2),
        TituloJogo = r.GetString(3),
        Titulo = r.GetString(4),
        Descricao = r.TextoOuNulo(5),
        Pontos = r.GetInt32(6),
        DataDesbloqueio = r.GetDateTime(7)
    };

    public List<Conquista> ListarPorUtilizador(int idUtilizador) =>
        BaseDados.Consultar(@"
            SELECT c.IdConquista, c.IdUtilizador, c.IdJogo, j.Titulo, c.Titulo, c.Descricao, c.Pontos, c.DataDesbloqueio
            FROM dbo.Conquistas AS c
            INNER JOIN dbo.Jogos AS j ON j.IdJogo = c.IdJogo
            WHERE c.IdUtilizador = @U
            ORDER BY j.Titulo, c.DataDesbloqueio", Mapear,
            BaseDados.Param("@U", idUtilizador));

    public int Desbloquear(Conquista c) => Convert.ToInt32(BaseDados.Escalar(
        @"INSERT INTO dbo.Conquistas (IdUtilizador, IdJogo, Titulo, Descricao, Pontos)
          OUTPUT INSERTED.IdConquista
          VALUES (@U, @J, @Titulo, @Descricao, @Pontos)",
        BaseDados.Param("@U", c.IdUtilizador),
        BaseDados.Param("@J", c.IdJogo),
        BaseDados.Param("@Titulo", c.Titulo),
        BaseDados.Param("@Descricao", c.Descricao),
        BaseDados.Param("@Pontos", c.Pontos)));

    public int TotalPontos(int idUtilizador) => Convert.ToInt32(BaseDados.Escalar(
        "SELECT COALESCE(SUM(Pontos), 0) FROM dbo.Conquistas WHERE IdUtilizador = @U",
        BaseDados.Param("@U", idUtilizador)));
}
