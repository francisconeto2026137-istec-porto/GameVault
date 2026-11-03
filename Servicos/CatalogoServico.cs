namespace GameVault.Servicos;

/// <summary>Gestão do catálogo: Jogos, Categorias, Editoras e Plataformas.</summary>
public class CatalogoServico
{
    private readonly JogoRepositorio _jogos = new();
    private readonly CategoriaRepositorio _categorias = new();
    private readonly EditoraRepositorio _editoras = new();
    private readonly PlataformaRepositorio _plataformas = new();

    private static void Garantir(bool sucesso)
    {
        if (!sucesso) throw new ValidacaoException("Registo não encontrado.");
    }

    // ---------- Jogos ----------
    public List<Jogo> ListarJogos() => _jogos.Listar();

    private static Jogo CriarJogoValidado(int id, string titulo, int ano, int idCategoria, int idEditora) => new()
    {
        IdJogo = id,
        Titulo = Validacao.Texto(titulo, "Título", 150),
        AnoLancamento = Validacao.Ano(ano),
        IdCategoria = idCategoria,
        IdEditora = idEditora
    };

    public int CriarJogo(string titulo, int ano, int idCategoria, int idEditora) =>
        _jogos.Inserir(CriarJogoValidado(0, titulo, ano, idCategoria, idEditora));

    public void AtualizarJogo(int id, string titulo, int ano, int idCategoria, int idEditora) =>
        Garantir(_jogos.Atualizar(CriarJogoValidado(id, titulo, ano, idCategoria, idEditora)));

    /// <summary>Elimina o jogo e (em cascata) as entradas de biblioteca, avaliações e conquistas ligadas a ele.</summary>
    public void EliminarJogo(int id) => Garantir(_jogos.Eliminar(id));

    // ---------- Categorias ----------
    public List<Categoria> ListarCategorias() => _categorias.Listar();

    public int CriarCategoria(string nome) =>
        _categorias.Inserir(new Categoria { Nome = Validacao.Texto(nome, "Nome", 50) });

    public void AtualizarCategoria(int id, string nome) =>
        Garantir(_categorias.Atualizar(new Categoria { IdCategoria = id, Nome = Validacao.Texto(nome, "Nome", 50) }));

    public void EliminarCategoria(int id) => Garantir(_categorias.Eliminar(id));

    // ---------- Editoras ----------
    public List<Editora> ListarEditoras() => _editoras.Listar();

    public int CriarEditora(string nome, string? pais) =>
        _editoras.Inserir(new Editora
        {
            Nome = Validacao.Texto(nome, "Nome", 100),
            Pais = Validacao.TextoOpcional(pais, "País", 60)
        });

    public void AtualizarEditora(int id, string nome, string? pais) =>
        Garantir(_editoras.Atualizar(new Editora
        {
            IdEditora = id,
            Nome = Validacao.Texto(nome, "Nome", 100),
            Pais = Validacao.TextoOpcional(pais, "País", 60)
        }));

    public void EliminarEditora(int id) => Garantir(_editoras.Eliminar(id));

    // ---------- Plataformas ----------
    public List<Plataforma> ListarPlataformas() => _plataformas.Listar();

    public int CriarPlataforma(string nome, string? fabricante) =>
        _plataformas.Inserir(new Plataforma
        {
            Nome = Validacao.Texto(nome, "Nome", 50),
            Fabricante = Validacao.TextoOpcional(fabricante, "Fabricante", 60)
        });

    public void AtualizarPlataforma(int id, string nome, string? fabricante) =>
        Garantir(_plataformas.Atualizar(new Plataforma
        {
            IdPlataforma = id,
            Nome = Validacao.Texto(nome, "Nome", 50),
            Fabricante = Validacao.TextoOpcional(fabricante, "Fabricante", 60)
        }));

    public void EliminarPlataforma(int id) => Garantir(_plataformas.Eliminar(id));
}
