namespace GameVault.Dados;

public class UtilizadorRepositorio
{
    private const string Selecao =
        "SELECT IdUtilizador, NomeUtilizador, Email, DataRegisto FROM dbo.Utilizadores";

    private static Utilizador Mapear(SqlDataReader r) => new()
    {
        IdUtilizador = r.GetInt32(0),
        NomeUtilizador = r.GetString(1),
        Email = r.GetString(2),
        DataRegisto = r.GetDateTime(3)
    };

    public List<Utilizador> Listar() =>
        BaseDados.Consultar(Selecao + " ORDER BY NomeUtilizador", Mapear);

    public Utilizador? Obter(int id) =>
        BaseDados.Consultar(Selecao + " WHERE IdUtilizador = @Id", Mapear,
            BaseDados.Param("@Id", id)).FirstOrDefault();

    public int Inserir(Utilizador u) => Convert.ToInt32(BaseDados.Escalar(
        "INSERT INTO dbo.Utilizadores (NomeUtilizador, Email) OUTPUT INSERTED.IdUtilizador VALUES (@Nome, @Email)",
        BaseDados.Param("@Nome", u.NomeUtilizador),
        BaseDados.Param("@Email", u.Email)));

    public bool Atualizar(Utilizador u) => BaseDados.Executar(
        "UPDATE dbo.Utilizadores SET NomeUtilizador = @Nome, Email = @Email WHERE IdUtilizador = @Id",
        BaseDados.Param("@Nome", u.NomeUtilizador),
        BaseDados.Param("@Email", u.Email),
        BaseDados.Param("@Id", u.IdUtilizador)) > 0;

    public bool Eliminar(int id) => BaseDados.Executar(
        "DELETE FROM dbo.Utilizadores WHERE IdUtilizador = @Id",
        BaseDados.Param("@Id", id)) > 0;
}
