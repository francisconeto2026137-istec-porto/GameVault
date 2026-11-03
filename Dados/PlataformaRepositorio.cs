namespace GameVault.Dados;

public class PlataformaRepositorio
{
    private static Plataforma Mapear(SqlDataReader r) => new()
    {
        IdPlataforma = r.GetInt32(0),
        Nome = r.GetString(1),
        Fabricante = r.TextoOuNulo(2)
    };

    public List<Plataforma> Listar() =>
        BaseDados.Consultar("SELECT IdPlataforma, Nome, Fabricante FROM dbo.Plataformas ORDER BY Nome", Mapear);

    public int Inserir(Plataforma p) => Convert.ToInt32(BaseDados.Escalar(
        "INSERT INTO dbo.Plataformas (Nome, Fabricante) OUTPUT INSERTED.IdPlataforma VALUES (@Nome, @Fabricante)",
        BaseDados.Param("@Nome", p.Nome),
        BaseDados.Param("@Fabricante", p.Fabricante)));

    public bool Atualizar(Plataforma p) => BaseDados.Executar(
        "UPDATE dbo.Plataformas SET Nome = @Nome, Fabricante = @Fabricante WHERE IdPlataforma = @Id",
        BaseDados.Param("@Nome", p.Nome),
        BaseDados.Param("@Fabricante", p.Fabricante),
        BaseDados.Param("@Id", p.IdPlataforma)) > 0;

    public bool Eliminar(int id) => BaseDados.Executar(
        "DELETE FROM dbo.Plataformas WHERE IdPlataforma = @Id",
        BaseDados.Param("@Id", id)) > 0;
}
