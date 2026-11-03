namespace GameVault.Dados;

public class CategoriaRepositorio
{
    private static Categoria Mapear(SqlDataReader r) => new()
    {
        IdCategoria = r.GetInt32(0),
        Nome = r.GetString(1)
    };

    public List<Categoria> Listar() =>
        BaseDados.Consultar("SELECT IdCategoria, Nome FROM dbo.Categorias ORDER BY Nome", Mapear);

    public int Inserir(Categoria c) => Convert.ToInt32(BaseDados.Escalar(
        "INSERT INTO dbo.Categorias (Nome) OUTPUT INSERTED.IdCategoria VALUES (@Nome)",
        BaseDados.Param("@Nome", c.Nome)));

    public bool Atualizar(Categoria c) => BaseDados.Executar(
        "UPDATE dbo.Categorias SET Nome = @Nome WHERE IdCategoria = @Id",
        BaseDados.Param("@Nome", c.Nome),
        BaseDados.Param("@Id", c.IdCategoria)) > 0;

    public bool Eliminar(int id) => BaseDados.Executar(
        "DELETE FROM dbo.Categorias WHERE IdCategoria = @Id",
        BaseDados.Param("@Id", id)) > 0;
}
