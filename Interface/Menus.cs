using Spectre.Console;

namespace GameVault.Interface;

/// <summary>Menus da aplicação de consola. Só fala com a camada de serviços.</summary>
public class Menus
{
    private readonly UtilizadorServico _utilizadores = new();
    private readonly CatalogoServico _catalogo = new();
    private readonly BibliotecaServico _biblioteca = new();
    private Utilizador? _sessao;

    // =====================================================================
    // Menu principal
    // =====================================================================
    public void Executar() =>
        Consola.ExecutarMenuPrincipal(
            () => _sessao is null ? "Sem sessão iniciada — escolha um utilizador" : $"Sessão de {_sessao.NomeUtilizador}",
            ("Escolher utilizador (iniciar sessão)", () =>
            {
                TratamentoErros.Executar(EscolherUtilizador);
                Consola.Pausa();
            }),
            ("Gerir utilizadores", MenuUtilizadores),
            ("Catálogo (jogos, categorias, editoras, plataformas)", MenuCatalogo),
            ("A minha biblioteca e horas jogadas", MenuBiblioteca),
            ("As minhas avaliações", MenuAvaliacoes),
            ("As minhas conquistas", MenuConquistas));

    private Utilizador Sessao() =>
        _sessao ?? throw new ValidacaoException("Inicie sessão primeiro (menu principal, opção 'Escolher utilizador').");

    private void EscolherUtilizador()
    {
        _sessao = Consola.Escolher("Escolha o utilizador", _utilizadores.Listar(),
            u => u.IdUtilizador, u => u.NomeUtilizador);
        Consola.Sucesso($"Sessão iniciada como {_sessao.NomeUtilizador}.");
    }

    private static string Cortar(string texto, int maximo) =>
        texto.Length <= maximo ? texto : texto[..(maximo - 1)] + "…";

    // =====================================================================
    // Utilizadores
    // =====================================================================
    private void MenuUtilizadores() =>
        Consola.ExecutarMenu(() => "Utilizadores",
            ("Listar utilizadores", () =>
                Consola.MostrarTabela("Utilizadores",
                    new[] { "ID", "Utilizador", "Email", "Registo" },
                    _utilizadores.Listar().Select(u => new[]
                    {
                        u.IdUtilizador.ToString(), Consola.E(u.NomeUtilizador), Consola.E(u.Email),
                        u.DataRegisto.ToString("dd/MM/yyyy")
                    }), 0)),
            ("Criar utilizador", () =>
            {
                var id = _utilizadores.Criar(Consola.LerTexto("Nome de utilizador"), Consola.LerTexto("Email"));
                Consola.Sucesso($"Utilizador criado com o ID {id}.");
            }),
            ("Atualizar utilizador", () =>
            {
                var u = EscolherUtilizadorDaLista();
                _utilizadores.Atualizar(u.IdUtilizador,
                    Consola.LerTextoComPadrao("Nome de utilizador", u.NomeUtilizador),
                    Consola.LerTextoComPadrao("Email", u.Email));
                Consola.Sucesso("Utilizador atualizado.");
            }),
            ("Eliminar utilizador", () =>
            {
                var u = EscolherUtilizadorDaLista();
                if (!Consola.Confirmar($"Eliminar '{u.NomeUtilizador}' e toda a sua biblioteca, avaliações e conquistas?"))
                    throw new OperacaoCanceladaException();
                _utilizadores.Eliminar(u.IdUtilizador);
                if (_sessao?.IdUtilizador == u.IdUtilizador) _sessao = null;
                Consola.Sucesso("Utilizador eliminado.");
            }));

    private Utilizador EscolherUtilizadorDaLista() =>
        Consola.Escolher("Escolha o utilizador", _utilizadores.Listar(), u => u.IdUtilizador,
            u => $"{u.NomeUtilizador}  ({u.Email})");

    // =====================================================================
    // Catálogo
    // =====================================================================
    private void MenuCatalogo() =>
        Consola.ExecutarSubmenu(() => "Catálogo",
            ("Jogos", MenuJogos),
            ("Categorias", MenuCategorias),
            ("Editoras", MenuEditoras),
            ("Plataformas", MenuPlataformas));

    // ---------- Jogos ----------
    private void MenuJogos() =>
        Consola.ExecutarMenu(() => "Jogos",
            ("Listar jogos", () =>
                Consola.MostrarTabela("Jogos",
                    new[] { "ID", "Título", "Ano", "Categoria", "Editora" },
                    _catalogo.ListarJogos().Select(j => new[]
                    {
                        j.IdJogo.ToString(), Consola.E(j.Titulo), j.AnoLancamento.ToString(),
                        Consola.E(j.NomeCategoria), Consola.E(j.NomeEditora)
                    }), 0, 2)),
            ("Criar jogo", () =>
            {
                var titulo = Consola.LerTexto("Título");
                var ano = Consola.LerInteiro("Ano de lançamento", 1970, 2100);
                var categoria = EscolherCategoria();
                var editora = EscolherEditora();
                var id = _catalogo.CriarJogo(titulo, ano, categoria.IdCategoria, editora.IdEditora);
                Consola.Sucesso($"Jogo criado com o ID {id}.");
            }),
            ("Atualizar jogo", () =>
            {
                var jogo = EscolherJogo();
                var titulo = Consola.LerTextoComPadrao("Título", jogo.Titulo);
                var ano = Consola.LerInteiro($"Ano de lançamento (atual: {jogo.AnoLancamento})", 1970, 2100);
                var categoria = EscolherCategoria();
                var editora = EscolherEditora();
                _catalogo.AtualizarJogo(jogo.IdJogo, titulo, ano, categoria.IdCategoria, editora.IdEditora);
                Consola.Sucesso("Jogo atualizado.");
            }),
            ("Eliminar jogo", () =>
            {
                var jogo = EscolherJogo();
                if (!Consola.Confirmar($"Eliminar '{jogo.Titulo}'? Será removido das bibliotecas de todos os utilizadores."))
                    throw new OperacaoCanceladaException();
                _catalogo.EliminarJogo(jogo.IdJogo);
                Consola.Sucesso("Jogo eliminado.");
            }));

    private Jogo EscolherJogo() =>
        Consola.Escolher("Escolha o jogo", _catalogo.ListarJogos(), j => j.IdJogo, j => $"{j.Titulo} ({j.AnoLancamento})");

    private Categoria EscolherCategoria() =>
        Consola.Escolher("Escolha a categoria", _catalogo.ListarCategorias(), c => c.IdCategoria, c => c.Nome);

    private Editora EscolherEditora() =>
        Consola.Escolher("Escolha a editora", _catalogo.ListarEditoras(), e => e.IdEditora, e => e.Nome);

    private Plataforma EscolherPlataforma() =>
        Consola.Escolher("Escolha a plataforma", _catalogo.ListarPlataformas(), p => p.IdPlataforma, p => p.Nome);

    // ---------- Categorias ----------
    private void MenuCategorias() =>
        Consola.ExecutarMenu(() => "Categorias",
            ("Listar categorias", () =>
                Consola.MostrarTabela("Categorias",
                    new[] { "ID", "Nome" },
                    _catalogo.ListarCategorias().Select(c => new[] { c.IdCategoria.ToString(), Consola.E(c.Nome) }), 0)),
            ("Criar categoria", () =>
                Consola.Sucesso($"Categoria criada com o ID {_catalogo.CriarCategoria(Consola.LerTexto("Nome"))}.")),
            ("Atualizar categoria", () =>
            {
                var c = EscolherCategoria();
                _catalogo.AtualizarCategoria(c.IdCategoria, Consola.LerTextoComPadrao("Nome", c.Nome));
                Consola.Sucesso("Categoria atualizada.");
            }),
            ("Eliminar categoria", () =>
            {
                var c = EscolherCategoria();
                if (!Consola.Confirmar($"Eliminar a categoria '{c.Nome}'?")) throw new OperacaoCanceladaException();
                _catalogo.EliminarCategoria(c.IdCategoria);
                Consola.Sucesso("Categoria eliminada.");
            }));

    // ---------- Editoras ----------
    private void MenuEditoras() =>
        Consola.ExecutarMenu(() => "Editoras",
            ("Listar editoras", () =>
                Consola.MostrarTabela("Editoras",
                    new[] { "ID", "Nome", "País" },
                    _catalogo.ListarEditoras().Select(e => new[]
                        { e.IdEditora.ToString(), Consola.E(e.Nome), Consola.E(e.Pais) }), 0)),
            ("Criar editora", () =>
            {
                var id = _catalogo.CriarEditora(Consola.LerTexto("Nome"), Consola.LerTexto("País (opcional)"));
                Consola.Sucesso($"Editora criada com o ID {id}.");
            }),
            ("Atualizar editora", () =>
            {
                var e = EscolherEditora();
                _catalogo.AtualizarEditora(e.IdEditora,
                    Consola.LerTextoComPadrao("Nome", e.Nome),
                    Consola.LerTextoComPadrao("País", e.Pais));
                Consola.Sucesso("Editora atualizada.");
            }),
            ("Eliminar editora", () =>
            {
                var e = EscolherEditora();
                if (!Consola.Confirmar($"Eliminar a editora '{e.Nome}'?")) throw new OperacaoCanceladaException();
                _catalogo.EliminarEditora(e.IdEditora);
                Consola.Sucesso("Editora eliminada.");
            }));

    // ---------- Plataformas ----------
    private void MenuPlataformas() =>
        Consola.ExecutarMenu(() => "Plataformas",
            ("Listar plataformas", () =>
                Consola.MostrarTabela("Plataformas",
                    new[] { "ID", "Nome", "Fabricante" },
                    _catalogo.ListarPlataformas().Select(p => new[]
                        { p.IdPlataforma.ToString(), Consola.E(p.Nome), Consola.E(p.Fabricante) }), 0)),
            ("Criar plataforma", () =>
            {
                var id = _catalogo.CriarPlataforma(Consola.LerTexto("Nome"), Consola.LerTexto("Fabricante (opcional)"));
                Consola.Sucesso($"Plataforma criada com o ID {id}.");
            }),
            ("Atualizar plataforma", () =>
            {
                var p = EscolherPlataforma();
                _catalogo.AtualizarPlataforma(p.IdPlataforma,
                    Consola.LerTextoComPadrao("Nome", p.Nome),
                    Consola.LerTextoComPadrao("Fabricante", p.Fabricante));
                Consola.Sucesso("Plataforma atualizada.");
            }),
            ("Eliminar plataforma", () =>
            {
                var p = EscolherPlataforma();
                if (!Consola.Confirmar($"Eliminar a plataforma '{p.Nome}'?")) throw new OperacaoCanceladaException();
                _catalogo.EliminarPlataforma(p.IdPlataforma);
                Consola.Sucesso("Plataforma eliminada.");
            }));

    // =====================================================================
    // Biblioteca e horas jogadas
    // =====================================================================
    private void MenuBiblioteca() =>
        Consola.ExecutarMenu(() => "A minha biblioteca",
            ("Ver a minha biblioteca", MostrarBiblioteca),
            ("Adicionar jogo à biblioteca", () =>
            {
                var utilizador = Sessao();
                var jogo = EscolherJogo();
                var plataforma = EscolherPlataforma();
                var estado = Consola.EscolherEstado();
                _biblioteca.Adicionar(utilizador.IdUtilizador, jogo.IdJogo, plataforma.IdPlataforma, estado);
                Consola.Sucesso($"'{jogo.Titulo}' adicionado à sua biblioteca.");
            }),
            ("Registar horas jogadas", () =>
            {
                var item = EscolherItemDaBiblioteca();
                var horas = Consola.LerDecimal("Horas jogadas nesta sessão");
                var total = _biblioteca.RegistarHoras(item.IdUtilizador, item.IdJogo, horas);
                Consola.Sucesso($"Horas registadas. Total em '{item.TituloJogo}': {total:0.0} h.");
            }),
            ("Alterar plataforma / estado", () =>
            {
                var item = EscolherItemDaBiblioteca();
                var plataforma = EscolherPlataforma();
                var estado = Consola.EscolherEstado();
                _biblioteca.Atualizar(item.IdUtilizador, item.IdJogo, plataforma.IdPlataforma, estado);
                Consola.Sucesso("Entrada da biblioteca atualizada.");
            }),
            ("Remover jogo da biblioteca", () =>
            {
                var item = EscolherItemDaBiblioteca();
                if (!Consola.Confirmar($"Remover '{item.TituloJogo}'? A sua avaliação e conquistas deste jogo também serão apagadas."))
                    throw new OperacaoCanceladaException();
                _biblioteca.Remover(item.IdUtilizador, item.IdJogo);
                Consola.Sucesso("Jogo removido da biblioteca.");
            }));

    private void MostrarBiblioteca()
    {
        var itens = _biblioteca.Listar(Sessao().IdUtilizador);

        Consola.MostrarTabela("A minha biblioteca",
            new[] { "Jogo", "Plataforma", "Horas", "Estado", "Adicionado" },
            itens.Select(i => new[]
            {
                Consola.E(i.TituloJogo), Consola.E(i.NomePlataforma), i.HorasJogadas.ToString("0.0"),
                Consola.EstadoColorido(i.Estado), i.DataAdicao.ToString("dd/MM/yyyy")
            }), 2);

        if (itens.Count == 0) return;

        var totalHoras = itens.Sum(i => i.HorasJogadas);
        var concluidos = itens.Count(i => i.Estado == EstadosJogo.Concluido);

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Panel(new Markup(
                $"[bold]{itens.Count}[/] jogo(s)    [bold green]{concluidos}[/] concluído(s)    [bold aqua]{totalHoras:0.0}[/] horas jogadas"))
            .Header("[bold] Resumo [/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Grey));

        if (totalHoras > 0)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new BarChart()
                .Width(70)
                .Label("[bold aqua]Horas por jogo[/]")
                .CenterLabel()
                .AddItems(itens, i => new BarChartItem(
                    Consola.E(Cortar(i.TituloJogo, 28)), (double)i.HorasJogadas, Color.Aqua)));
        }
    }

    private ItemBiblioteca EscolherItemDaBiblioteca() =>
        Consola.Escolher("Escolha um dos seus jogos", _biblioteca.Listar(Sessao().IdUtilizador),
            i => i.IdJogo, i => $"{i.TituloJogo}  ({i.NomePlataforma}, {i.HorasJogadas:0.0} h)");

    // =====================================================================
    // Avaliações
    // =====================================================================
    private void MenuAvaliacoes() =>
        Consola.ExecutarMenu(() => "As minhas avaliações",
            ("Ver as minhas avaliações", () =>
                Consola.MostrarTabela("As minhas avaliações",
                    new[] { "Jogo", "Nota", "Data", "Comentário" },
                    _biblioteca.ListarAvaliacoes(Sessao().IdUtilizador).Select(a => new[]
                    {
                        Consola.E(a.TituloJogo), Consola.NotaColorida(a.Nota),
                        a.DataAvaliacao.ToString("dd/MM/yyyy"), Consola.E(a.Comentario)
                    }))),
            ("Avaliar jogo (ou alterar avaliação)", () =>
            {
                var item = EscolherItemDaBiblioteca();
                var nota = Consola.LerDecimal("Nota (0 a 10)");
                var comentario = Consola.LerTexto("Comentário (opcional)");
                var criada = _biblioteca.Avaliar(item.IdUtilizador, item.IdJogo, nota, comentario);
                Consola.Sucesso(criada ? "Avaliação registada." : "Avaliação atualizada.");
            }),
            ("Remover avaliação", () =>
            {
                var avaliacao = Consola.Escolher("Escolha a avaliação", _biblioteca.ListarAvaliacoes(Sessao().IdUtilizador),
                    a => a.IdJogo, a => $"{a.TituloJogo}  ({a.Nota:0.0}/10)");
                _biblioteca.RemoverAvaliacao(avaliacao.IdUtilizador, avaliacao.IdJogo);
                Consola.Sucesso("Avaliação removida.");
            }));

    // =====================================================================
    // Conquistas
    // =====================================================================
    private void MenuConquistas() =>
        Consola.ExecutarMenu(() => "As minhas conquistas",
            ("Ver as minhas conquistas", () =>
            {
                var utilizador = Sessao();
                var conquistas = _biblioteca.ListarConquistas(utilizador.IdUtilizador);

                Consola.MostrarTabela("As minhas conquistas",
                    new[] { "Jogo", "Conquista", "Descrição", "Pontos", "Data" },
                    conquistas.Select(c => new[]
                    {
                        Consola.E(c.TituloJogo), $"[yellow]★[/] {Consola.E(c.Titulo)}", Consola.E(c.Descricao),
                        c.Pontos.ToString(), c.DataDesbloqueio.ToString("dd/MM/yyyy")
                    }), 3);

                if (conquistas.Count > 0)
                {
                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine(
                        $"[bold]{conquistas.Count}[/] conquista(s)   |   [bold yellow]{_biblioteca.TotalPontos(utilizador.IdUtilizador)}[/] pontos");
                }
            }),
            ("Desbloquear conquista", () =>
            {
                var item = EscolherItemDaBiblioteca();
                var titulo = Consola.LerTexto("Título da conquista");
                var descricao = Consola.LerTexto("Descrição (opcional)");
                var pontos = Consola.LerInteiro("Pontos", 1, 1000);
                _biblioteca.DesbloquearConquista(item.IdUtilizador, item.IdJogo, titulo, descricao, pontos);
                Consola.Sucesso($"Conquista '{titulo.Trim()}' desbloqueada!");
            }));
}
