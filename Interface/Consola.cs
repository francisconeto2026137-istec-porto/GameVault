using Spectre.Console;

namespace GameVault.Interface;

/// <summary>
/// Apresentação e leitura na consola, com Spectre.Console (cores, tabelas e menus com setas).
/// Os textos dos parâmetros são texto simples: aqui são escapados antes de irem para o "markup".
/// As células das tabelas, essas, já são markup (use <see cref="E"/> para texto vindo da base de dados).
/// </summary>
public static class Consola
{
    private static readonly Style EstiloSelecao = new(Color.Aqua, decoration: Decoration.Bold);

    /// <summary>Escapa texto para que caracteres como [ e ] não sejam interpretados como formatação.</summary>
    public static string E(string? texto) => Markup.Escape(texto ?? string.Empty);

    // =====================================================================
    // Mensagens
    // =====================================================================
    public static void Sucesso(string mensagem) => AnsiConsole.MarkupLine($"[green]✔[/] {E(mensagem)}");
    public static void Aviso(string mensagem) => AnsiConsole.MarkupLine($"[yellow]![/] {E(mensagem)}");
    public static void Erro(string mensagem) => AnsiConsole.MarkupLine($"[red]✖[/] {E(mensagem)}");
    public static void Info(string mensagem) => AnsiConsole.MarkupLine($"[grey]{E(mensagem)}[/]");

    public static void Banner()
    {
        AnsiConsole.Write(new FigletText("GameVault").Color(Color.Aqua));
        AnsiConsole.MarkupLine("[grey]A sua biblioteca pessoal de videojogos[/]");
        AnsiConsole.WriteLine();
    }

    // =====================================================================
    // Formatação reutilizável
    // =====================================================================
    public static string EstadoColorido(string estado)
    {
        var cor = estado switch
        {
            EstadosJogo.Concluido => "green",
            EstadosJogo.AJogar => "aqua",
            EstadosJogo.Abandonado => "red",
            _ => "grey"
        };
        return $"[{cor}]{E(estado)}[/]";
    }

    /// <summary>Nota de 0 a 10 mostrada com estrelas (de 0 a 5) e cor.</summary>
    public static string NotaColorida(decimal nota)
    {
        var cor = nota >= 8 ? "green" : nota >= 5 ? "yellow" : "red";
        var cheias = (int)Math.Round(nota / 2);
        var estrelas = new string('★', cheias) + new string('☆', 5 - cheias);
        return $"[{cor}]{estrelas}  {nota:0.0}[/]";
    }

    /// <summary>Mostra uma tabela. As células são markup; "colunasADireita" são índices de colunas numéricas.</summary>
    public static void MostrarTabela(string titulo, string[] colunas, IEnumerable<string[]> linhas, params int[] colunasADireita)
    {
        var lista = linhas.ToList();
        if (lista.Count == 0)
        {
            Aviso("Sem registos para mostrar.");
            return;
        }

        var tabela = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .Title($"[bold aqua]{E(titulo)}[/]");

        for (var i = 0; i < colunas.Length; i++)
        {
            var coluna = new TableColumn($"[bold]{E(colunas[i])}[/]");
            if (colunasADireita.Contains(i)) coluna.Alignment = Justify.Right;
            tabela.AddColumn(coluna);
        }

        foreach (var linha in lista)
            tabela.AddRow(linha);

        AnsiConsole.Write(tabela);
    }

    // =====================================================================
    // Leitura
    // =====================================================================
    public static string LerTexto(string pergunta) =>
        AnsiConsole.Prompt(new TextPrompt<string>($"[cyan]{E(pergunta)}:[/]") { AllowEmpty = true }).Trim();

    /// <summary>Se o utilizador só carregar em Enter, mantém o valor atual.</summary>
    public static string LerTextoComPadrao(string pergunta, string? atual)
    {
        var prompt = new TextPrompt<string>($"[cyan]{E(pergunta)}[/]") { AllowEmpty = true };
        if (!string.IsNullOrEmpty(atual))
            prompt.DefaultValue(atual);
        return AnsiConsole.Prompt(prompt).Trim();
    }

    public static int LerInteiro(string pergunta, int minimo, int maximo) =>
        AnsiConsole.Prompt(
            new TextPrompt<int>($"[cyan]{E(pergunta)}[/] [grey]({minimo}-{maximo})[/]:")
                .ValidationErrorMessage("[red]Indique um número inteiro válido.[/]")
                .Validate(valor => valor >= minimo && valor <= maximo
                    ? ValidationResult.Success()
                    : ValidationResult.Error($"[red]O valor tem de estar entre {minimo} e {maximo}.[/]")));

    private static bool TentarLerDecimal(string? texto, out decimal valor) =>
        decimal.TryParse((texto ?? string.Empty).Trim().Replace(',', '.'),
            NumberStyles.Number, CultureInfo.InvariantCulture, out valor);

    /// <summary>Aceita vírgula ou ponto como separador decimal.</summary>
    public static decimal LerDecimal(string pergunta)
    {
        var texto = AnsiConsole.Prompt(
            new TextPrompt<string>($"[cyan]{E(pergunta)}:[/]")
                .Validate(t => TentarLerDecimal(t, out _)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Número inválido. Exemplo: 7,5[/]")));

        TentarLerDecimal(texto, out var valor);
        return valor;
    }

    public static bool Confirmar(string pergunta) =>
        AnsiConsole.Prompt(new ConfirmationPrompt($"[yellow]{E(pergunta)}[/]")
        {
            Yes = 's',
            No = 'n',
            DefaultValue = false,
            InvalidChoiceMessage = "[red]Responda com 's' ou 'n'.[/]"
        });

    public static void Pausa()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Prima uma tecla para continuar...[/]");
        Console.ReadKey(true);
    }

    // =====================================================================
    // Escolha numa lista (setas ↑/↓ + Enter)
    // =====================================================================
    public static T Escolher<T>(string titulo, List<T> itens, Func<T, int> obterId, Func<T, string> descrever)
        where T : class
    {
        if (itens.Count == 0)
            throw new ValidacaoException("Não existem registos para escolher.");

        const string cancelar = "← Cancelar";
        var etiquetas = itens.Select(i => $"[grey]{obterId(i),4}[/]  {E(descrever(i))}").ToList();

        var escolha = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[bold aqua]{E(titulo)}[/]")
                .PageSize(12)
                .MoreChoicesText("[grey](↑/↓ para ver mais)[/]")
                .HighlightStyle(EstiloSelecao)
                .AddChoices(etiquetas.Append(cancelar)));

        if (escolha == cancelar)
            throw new OperacaoCanceladaException();

        return itens[etiquetas.IndexOf(escolha)];
    }

    public static string EscolherEstado() =>
        AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold aqua]Estado do jogo[/]")
                .HighlightStyle(EstiloSelecao)
                .AddChoices(EstadosJogo.Todos));

    // =====================================================================
    // Menus
    // =====================================================================
    /// <summary>Menu principal: mostra o banner e abre outros menus.</summary>
    public static void ExecutarMenuPrincipal(Func<string> subtitulo, params (string Texto, Action Acao)[] opcoes) =>
        Ciclo(subtitulo, opcoes, executarComPausa: false, mostrarBanner: true);

    /// <summary>Menu cujas opções executam uma ação num ecrã limpo e param para ler o resultado.</summary>
    public static void ExecutarMenu(Func<string> titulo, params (string Texto, Action Acao)[] opcoes) =>
        Ciclo(titulo, opcoes, executarComPausa: true, mostrarBanner: false);

    /// <summary>Menu cujas opções abrem outros menus.</summary>
    public static void ExecutarSubmenu(Func<string> titulo, params (string Texto, Action Acao)[] opcoes) =>
        Ciclo(titulo, opcoes, executarComPausa: false, mostrarBanner: false);

    private static void Ciclo(Func<string> titulo, (string Texto, Action Acao)[] opcoes, bool executarComPausa, bool mostrarBanner)
    {
        var textoSaida = mostrarBanner ? "Sair" : "← Voltar";

        while (true)
        {
            AnsiConsole.Clear();
            if (mostrarBanner) Banner();
            AnsiConsole.Write(new Rule($"[bold aqua]{E(titulo())}[/]") { Justification = Justify.Left });
            AnsiConsole.WriteLine();

            var textos = opcoes.Select(o => o.Texto).Append(textoSaida).ToList();
            var escolha = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[grey]Use ↑/↓ e Enter para escolher[/]")
                    .PageSize(12)
                    .HighlightStyle(EstiloSelecao)
                    .AddChoices(textos));

            if (escolha == textoSaida)
                return;

            var acao = opcoes[textos.IndexOf(escolha)].Acao;

            if (executarComPausa)
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(new Rule($"[bold aqua]{E(escolha)}[/]") { Justification = Justify.Left });
                AnsiConsole.WriteLine();
                TratamentoErros.Executar(acao);
                Pausa();
            }
            else
            {
                acao();
            }
        }
    }
}
