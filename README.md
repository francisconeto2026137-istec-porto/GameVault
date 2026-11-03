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
Opção A — SQL Server em Docker:
```
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=GameVault#2026" -p 1433:1433 --name sql-gamevault -d mcr.microsoft.com/mssql/server:2022-latest
```
Opção B — LocalDB (Windows): cadeia `Server=(localdb)\\MSSQLLocalDB;Database=GameVault;Trusted_Connection=True;TrustServerCertificate=True`

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

## 3. Guia de teste (manual)
Utilizadores de exemplo: `ana.silva`, `bruno.costa`, `carla.matos`.

| # | Ação | Resultado esperado |
|---|------|--------------------|
| 1 | Menu 4 sem sessão | "Inicie sessão primeiro" |
| 2 | Opção 1 → escolher `ana.silva`; Menu 4 → *Ver biblioteca* | 4 jogos e total de 228,0 horas |
| 3 | *Registar horas* em Hades com `2,5` | Total passa de 35,0 para 37,5 h |
| 4 | *Registar horas* com `-3` ou `0` | Recusado: horas têm de ser > 0 |
| 5 | Menu 5 → avaliar Elden Ring com `11` | Recusado: nota entre 0 e 10 |
| 6 | Avaliar Elden Ring com `9,0` e depois com `9,5` | "Avaliação registada" e depois "atualizada" |
| 7 | Menu 6 → desbloquear conquista repetida (mesmo jogo e título) | Erro "valor duplicado" |
| 8 | Adicionar à biblioteca um jogo que já lá está | "Já está na sua biblioteca" |
| 9 | Menu 3 → Categorias → eliminar "RPG" | Recusado (está em uso por jogos) |
| 10 | Criar utilizador com email `abc` | Recusado: formato inválido |
| 11 | Remover Celeste da biblioteca da Ana | Avaliação e conquista do Celeste também desaparecem |
| 12 | Parar o SQL Server e arrancar a aplicação | Mensagem clara de falha de ligação |

Verificação direta na base de dados:
```sql
SELECT u.NomeUtilizador, j.Titulo, b.HorasJogadas, b.Estado
FROM dbo.Bibliotecas b
JOIN dbo.Utilizadores u ON u.IdUtilizador = b.IdUtilizador
JOIN dbo.Jogos j ON j.IdJogo = b.IdJogo;

-- Deve falhar (CHECK): UPDATE dbo.Bibliotecas SET HorasJogadas = -1 WHERE IdBiblioteca = 1;
-- Deve falhar (CHECK): UPDATE dbo.Avaliacoes   SET Nota = 11 WHERE IdAvaliacao = 1;
```
