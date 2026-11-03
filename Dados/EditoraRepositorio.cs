namespace GameVault.Dados;

public class EditoraRepositorio
{
    private static Editora Mapear(SqlDataReader r) => new()
    {
        IdEditora = r.GetInt32(0),
        Nome = r.GetString(1),
        Pais = r.TextoOuNulo(2)
    };

    public List<Editora> Listar() =>
        BaseDados.Consultar("SELECT IdEditora, Nome, Pais FROM dbo.Editoras ORDER BY Nome", Mapear);

    public int Inserir(Editora e) => Convert.ToInt32(BaseDados.Escalar(
        "INSERT INTO dbo.Editoras (Nome, Pais) OUTPUT INSERTED.IdEditora VALUES (@Nome, @Pais)",
        BaseDados.Param("@Nome", e.Nome),
        BaseDados.Param("@Pais", e.Pais)));

    public bool Atualizar(Editora e) => BaseDados.Executar(
        "UPDATE dbo.Editoras SET Nome = @Nome, Pais = @Pais WHERE IdEditora = @Id",
        BaseDados.Param("@Nome", e.Nome),
        BaseDados.Param("@Pais", e.Pais),
        BaseDados.Param("@Id", e.IdEditora)) > 0;

    public bool Eliminar(int id) => BaseDados.Executar(
        "DELETE FROM dbo.Editoras WHERE IdEditora = @Id",
        BaseDados.Param("@Id", id)) > 0;
}
