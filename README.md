# GameVault — Biblioteca Pessoal de Videojogos

Projeto final académico em C# (.NET 8) + SQL Server, aplicação de consola com ADO.NET.

## Arquitetura
Interface (menus) → Serviços (validações e regras) → Dados (SQL parametrizado) → SQL Server.
Os erros de validação são `ValidacaoException`; os erros SQL (duplicados, chaves estrangeiras,
ligação) são traduzidos em `TratamentoErros`.

## Requisitos
- .NET SDK 8 (ou superior — nesse caso altere `TargetFramework` no `.csproj`)
- SQL Server 2016+ (Docker, LocalDB ou instância local)
- VS Code com as extensões **C# Dev Kit** (inclui C#) e **SQL Server (mssql)**

## 1. Base de dados
```
LocalDB (Windows): cadeia `Server=(localdb)\\MSSQLLocalDB;Database=GameVault;Trusted_Connection=True;TrustServerCertificate=True`

Executar o script:
- No VS Code: separador SQL Server → *Add Connection* → abrir `BaseDados/01_criar_base_dados.sql`
  → `Ctrl+Shift+E` (Execute Query); **ou**
- Na linha de comandos:
```
sqlcmd -S localhost,1433 -U sa -P "GameVault#2026" -C -f 65001 -i BaseDados/01_criar_base_dados.sql
```
(`-f 65001` garante UTF-8 e portanto os acentos.)

## 2. Configurar e executar
1. Ajustar `ConnectionStrings:GameVault` em `appsettings.json` (servidor, utilizador, palavra-passe).
2. No terminal, na pasta do projeto:
```
dotnet restore
dotnet build
dotnet run
```

## 3. Principais Funcionalidades

1. Banner e menus: aparece o banner “GameVault” em ASCII. Os menus escolhem-se com ↑/↓ e Enter.
2. Escolha de jogos, plataformas e utilizadores: passa a ser numa lista navegável, com “← Cancelar” no fim.
3. Listagens: saem em tabelas com bordas arredondadas.
4. Biblioteca: os estados têm cores (verde para Concluído, azul para A jogar, vermelho para Abandonado). Há um painel de resumo e um gráfico de barras com as horas por jogo.
5. Avaliações e conquistas: as notas aparecem com estrelas (★★★★★ 9,5) e as conquistas com pontos e total.
6. Mensagens: sucesso com ✔ a verde, avisos a amarelo e erros a vermelho. Os campos opcionais mantêm o valor atual se carregar só em Enter.

```
