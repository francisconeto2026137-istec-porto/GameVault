namespace GameVault.Servicos;

/// <summary>
/// Regras da biblioteca pessoal: jogos do utilizador, horas jogadas, avaliações e conquistas.
/// Regra principal: só se pode registar horas, avaliar ou desbloquear conquistas
/// de jogos que estejam na biblioteca do próprio utilizador.
/// </summary>
public class BibliotecaServico
{
    private readonly BibliotecaRepositorio _biblioteca = new();
    private readonly AvaliacaoRepositorio _avaliacoes = new();
    private readonly ConquistaRepositorio _conquistas = new();

    private ItemBiblioteca ObterOuFalhar(int idUtilizador, int idJogo) =>
        _biblioteca.Obter(idUtilizador, idJogo)
        ?? throw new ValidacaoException("Este jogo não está na sua biblioteca.");

    // ---------- Biblioteca ----------
    public List<ItemBiblioteca> Listar(int idUtilizador) => _biblioteca.ListarPorUtilizador(idUtilizador);

    public void Adicionar(int idUtilizador, int idJogo, int idPlataforma, string estado)
    {
        estado = Validacao.Estado(estado);
        if (_biblioteca.Obter(idUtilizador, idJogo) is not null)
            throw new ValidacaoException("Este jogo já está na sua biblioteca.");

        _biblioteca.Inserir(new ItemBiblioteca
        {
            IdUtilizador = idUtilizador,
            IdJogo = idJogo,
            IdPlataforma = idPlataforma,
            Estado = estado
        });
    }

    /// <summary>Soma horas ao total do jogo e devolve o novo total.</summary>
    public decimal RegistarHoras(int idUtilizador, int idJogo, decimal horas)
    {
        horas = Validacao.Horas(horas);
        ObterOuFalhar(idUtilizador, idJogo);
        _biblioteca.SomarHoras(idUtilizador, idJogo, horas);
        return ObterOuFalhar(idUtilizador, idJogo).HorasJogadas;
    }

    public void Atualizar(int idUtilizador, int idJogo, int idPlataforma, string estado)
    {
        estado = Validacao.Estado(estado);
        ObterOuFalhar(idUtilizador, idJogo);
        _biblioteca.Atualizar(idUtilizador, idJogo, idPlataforma, estado);
    }

    /// <summary>Remove o jogo da biblioteca e apaga a avaliação e as conquistas desse jogo.</summary>
    public void Remover(int idUtilizador, int idJogo)
    {
        ObterOuFalhar(idUtilizador, idJogo);
        _biblioteca.Remover(idUtilizador, idJogo);
    }

    // ---------- Avaliações ----------
    public List<Avaliacao> ListarAvaliacoes(int idUtilizador) => _avaliacoes.ListarPorUtilizador(idUtilizador);

    /// <summary>Cria a avaliação ou, se já existir, atualiza-a. Devolve true se foi criada.</summary>
    public bool Avaliar(int idUtilizador, int idJogo, decimal nota, string? comentario)
    {
        var avaliacao = new Avaliacao
        {
            IdUtilizador = idUtilizador,
            IdJogo = idJogo,
            Nota = Validacao.Nota(nota),
            Comentario = Validacao.TextoOpcional(comentario, "Comentário", 500)
        };
        ObterOuFalhar(idUtilizador, idJogo);

        if (_avaliacoes.Obter(idUtilizador, idJogo) is null)
        {
            _avaliacoes.Inserir(avaliacao);
            return true;
        }

        _avaliacoes.Atualizar(avaliacao);
        return false;
    }

    public void RemoverAvaliacao(int idUtilizador, int idJogo)
    {
        if (!_avaliacoes.Eliminar(idUtilizador, idJogo))
            throw new ValidacaoException("Não existe avaliação sua para este jogo.");
    }

    // ---------- Conquistas ----------
    public List<Conquista> ListarConquistas(int idUtilizador) => _conquistas.ListarPorUtilizador(idUtilizador);

    public int TotalPontos(int idUtilizador) => _conquistas.TotalPontos(idUtilizador);

    public int DesbloquearConquista(int idUtilizador, int idJogo, string titulo, string? descricao, int pontos)
    {
        var conquista = new Conquista
        {
            IdUtilizador = idUtilizador,
            IdJogo = idJogo,
            Titulo = Validacao.Texto(titulo, "Título da conquista", 100),
            Descricao = Validacao.TextoOpcional(descricao, "Descrição", 250),
            Pontos = Validacao.Pontos(pontos)
        };
        ObterOuFalhar(idUtilizador, idJogo);
        return _conquistas.Desbloquear(conquista); // título repetido no mesmo jogo → erro UNIQUE tratado na interface
    }
}
