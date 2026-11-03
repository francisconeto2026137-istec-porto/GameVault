using System.Text;
using GameVault.Interface;
using Microsoft.Extensions.Configuration;
using Spectre.Console;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;
CultureInfo.CurrentCulture = new CultureInfo("pt-PT");

// 1) Ler a cadeia de ligação de appsettings.json
var configuracao = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var cadeiaLigacao = configuracao.GetConnectionString("GameVault");
if (string.IsNullOrWhiteSpace(cadeiaLigacao))
{
    Consola.Erro("Falta a cadeia de ligação 'GameVault' em appsettings.json.");
    return;
}

// 2) Verificar a ligação antes de mostrar os menus
BaseDados.Configurar(cadeiaLigacao);
try
{
    AnsiConsole.Status().Start("A ligar à base de dados...", _ => BaseDados.TestarLigacao());
}
catch (SqlException ex)
{
    Consola.Erro($"Erro de base de dados: {TratamentoErros.Traduzir(ex)}");
    return;
}

// 3) Arrancar a aplicação
new Menus().Executar();
AnsiConsole.MarkupLine("[aqua]Até à próxima![/]");
