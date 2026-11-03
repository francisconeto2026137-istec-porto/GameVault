namespace GameVault.Servicos;

public class UtilizadorServico
{
    private readonly UtilizadorRepositorio _repositorio = new();

    public List<Utilizador> Listar() => _repositorio.Listar();

    public int Criar(string nomeUtilizador, string email) =>
        _repositorio.Inserir(new Utilizador
        {
            NomeUtilizador = Validacao.Texto(nomeUtilizador, "Nome de utilizador", 50),
            Email = Validacao.Email(email)
        });

    public void Atualizar(int id, string nomeUtilizador, string email)
    {
        var atualizado = _repositorio.Atualizar(new Utilizador
        {
            IdUtilizador = id,
            NomeUtilizador = Validacao.Texto(nomeUtilizador, "Nome de utilizador", 50),
            Email = Validacao.Email(email)
        });
        if (!atualizado) throw new ValidacaoException("Utilizador não encontrado.");
    }

    /// <summary>Elimina o utilizador e (em cascata) a sua biblioteca, avaliações e conquistas.</summary>
    public void Eliminar(int id)
    {
        if (!_repositorio.Eliminar(id)) throw new ValidacaoException("Utilizador não encontrado.");
    }
}
