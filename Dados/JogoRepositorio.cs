namespace GameVault.Dados;

public class JogoRepositorio
{
    private const string Selecao = @"
        SELECT j.IdJogo, j.Titulo, j.AnoLancamento, j.IdCategoria, j.IdEditora, c.Nome, e.Nome
        FROM dbo.Jogos AS j
        INNER JOIN dbo.Categorias AS c ON c.IdCategoria = j.IdCategoria
        INNER JOIN dbo.Editoras   AS e ON e.IdEditora   = j.IdEditora";

    private static Jogo Mapear(SqlDataReader r) => new()
    {
        IdJogo = r.GetInt32(0),
        Titulo = r.GetString(1),
        AnoLancamento = r.GetInt16(2),
        IdCategoria = r.GetInt32(3),
        IdEditora = r.GetInt32(4),
        NomeCategoria = r.GetString(5),
        NomeEditora = r.GetString(6)
    };

    public List<Jogo> Listar() =>
        BaseDados.Consultar(Selecao + " ORDER BY j.Titulo", Mapear);

    public Jogo? Obter(int id) =>
        BaseDados.Consultar(Selecao + " WHERE j.IdJogo = @Id", Mapear,
            BaseDados.Param("@Id", id)).FirstOrDefault();

    public int Inserir(Jogo j) => Convert.ToInt32(BaseDados.Escalar(
        @"INSERT INTO dbo.Jogos (Titulo, AnoLancamento, IdCategoria, IdEditora)
          OUTPUT INSERTED.IdJogo
          VALUES (@Titulo, @Ano, @IdCategoria, @IdEditora)",
        BaseDados.Param("@Titulo", j.Titulo),
        BaseDados.Param("@Ano", j.AnoLancamento),
        BaseDados.Param("@IdCategoria", j.IdCategoria),
        BaseDados.Param("@IdEditora", j.IdEditora)));

    public bool Atualizar(Jogo j) => BaseDados.Executar(
        @"UPDATE dbo.Jogos
          SET Titulo = @Titulo, AnoLancamento = @Ano, IdCategoria = @IdCategoria, IdEditora = @IdEditora
          WHERE IdJogo = @Id",
        BaseDados.Param("@Titulo", j.Titulo),
        BaseDados.Param("@Ano", j.AnoLancamento),
        BaseDados.Param("@IdCategoria", j.IdCategoria),
        BaseDados.Param("@IdEditora", j.IdEditora),
        BaseDados.Param("@Id", j.IdJogo)) > 0;

    public bool Eliminar(int id) => BaseDados.Executar(
        "DELETE FROM dbo.Jogos WHERE IdJogo = @Id",
        BaseDados.Param("@Id", id)) > 0;
}
