/* ==========================================================================
   GameVault - Biblioteca Pessoal de Videojogos
   Script de criação da base de dados (SQL Server 2016 ou superior)
   Ficheiro em UTF-8. Pode ser executado várias vezes (recria as tabelas).
   ========================================================================== */

IF DB_ID(N'GameVault') IS NULL
    CREATE DATABASE GameVault;
GO

USE GameVault;
GO

/* ---------- Limpeza (ordem inversa das dependências) ---------- */
DROP TABLE IF EXISTS dbo.Conquistas;
DROP TABLE IF EXISTS dbo.Avaliacoes;
DROP TABLE IF EXISTS dbo.Bibliotecas;
DROP TABLE IF EXISTS dbo.Jogos;
DROP TABLE IF EXISTS dbo.Utilizadores;
DROP TABLE IF EXISTS dbo.Plataformas;
DROP TABLE IF EXISTS dbo.Editoras;
DROP TABLE IF EXISTS dbo.Categorias;
GO

/* ---------- Tabelas de referência ---------- */
CREATE TABLE dbo.Categorias (
    IdCategoria INT IDENTITY(1,1) NOT NULL,
    Nome        NVARCHAR(50)      NOT NULL,
    CONSTRAINT PK_Categorias      PRIMARY KEY (IdCategoria),
    CONSTRAINT UQ_Categorias_Nome UNIQUE (Nome)
);

CREATE TABLE dbo.Editoras (
    IdEditora INT IDENTITY(1,1) NOT NULL,
    Nome      NVARCHAR(100)     NOT NULL,
    Pais      NVARCHAR(60)      NULL,
    CONSTRAINT PK_Editoras      PRIMARY KEY (IdEditora),
    CONSTRAINT UQ_Editoras_Nome UNIQUE (Nome)
);

CREATE TABLE dbo.Plataformas (
    IdPlataforma INT IDENTITY(1,1) NOT NULL,
    Nome         NVARCHAR(50)      NOT NULL,
    Fabricante   NVARCHAR(60)      NULL,
    CONSTRAINT PK_Plataformas      PRIMARY KEY (IdPlataforma),
    CONSTRAINT UQ_Plataformas_Nome UNIQUE (Nome)
);

/* ---------- Utilizadores ---------- */
CREATE TABLE dbo.Utilizadores (
    IdUtilizador   INT IDENTITY(1,1) NOT NULL,
    NomeUtilizador NVARCHAR(50)      NOT NULL,
    Email          NVARCHAR(150)     NOT NULL,
    DataRegisto    DATETIME2(0)      NOT NULL CONSTRAINT DF_Utilizadores_DataRegisto DEFAULT SYSDATETIME(),
    CONSTRAINT PK_Utilizadores       PRIMARY KEY (IdUtilizador),
    CONSTRAINT UQ_Utilizadores_Nome  UNIQUE (NomeUtilizador),
    CONSTRAINT UQ_Utilizadores_Email UNIQUE (Email),
    CONSTRAINT CK_Utilizadores_Email CHECK (Email LIKE N'_%@_%._%')
);

/* ---------- Jogos ---------- */
CREATE TABLE dbo.Jogos (
    IdJogo        INT IDENTITY(1,1) NOT NULL,
    Titulo        NVARCHAR(150)     NOT NULL,
    AnoLancamento SMALLINT          NOT NULL,
    IdCategoria   INT               NOT NULL,
    IdEditora     INT               NOT NULL,
    CONSTRAINT PK_Jogos PRIMARY KEY (IdJogo),
    CONSTRAINT UQ_Jogos_Titulo_Editora UNIQUE (Titulo, IdEditora),
    CONSTRAINT CK_Jogos_Ano CHECK (AnoLancamento BETWEEN 1970 AND 2100),
    CONSTRAINT FK_Jogos_Categorias FOREIGN KEY (IdCategoria) REFERENCES dbo.Categorias (IdCategoria),
    CONSTRAINT FK_Jogos_Editoras   FOREIGN KEY (IdEditora)   REFERENCES dbo.Editoras (IdEditora)
);
CREATE INDEX IX_Jogos_IdCategoria ON dbo.Jogos (IdCategoria);
CREATE INDEX IX_Jogos_IdEditora   ON dbo.Jogos (IdEditora);

/* ---------- Bibliotecas: liga Utilizador + Jogo + Plataforma ---------- */
CREATE TABLE dbo.Bibliotecas (
    IdBiblioteca INT IDENTITY(1,1) NOT NULL,
    IdUtilizador INT               NOT NULL,
    IdJogo       INT               NOT NULL,
    IdPlataforma INT               NOT NULL,
    HorasJogadas DECIMAL(8,1)      NOT NULL CONSTRAINT DF_Bibliotecas_Horas  DEFAULT 0,
    Estado       NVARCHAR(20)      NOT NULL CONSTRAINT DF_Bibliotecas_Estado DEFAULT N'Por jogar',
    DataAdicao   DATETIME2(0)      NOT NULL CONSTRAINT DF_Bibliotecas_Data   DEFAULT SYSDATETIME(),
    CONSTRAINT PK_Bibliotecas PRIMARY KEY (IdBiblioteca),
    CONSTRAINT UQ_Bibliotecas_Utilizador_Jogo UNIQUE (IdUtilizador, IdJogo),
    CONSTRAINT CK_Bibliotecas_Horas  CHECK (HorasJogadas >= 0),
    CONSTRAINT CK_Bibliotecas_Estado CHECK (Estado IN (N'Por jogar', N'A jogar', N'Concluído', N'Abandonado')),
    CONSTRAINT FK_Bibliotecas_Utilizadores FOREIGN KEY (IdUtilizador) REFERENCES dbo.Utilizadores (IdUtilizador) ON DELETE CASCADE,
    CONSTRAINT FK_Bibliotecas_Jogos        FOREIGN KEY (IdJogo)       REFERENCES dbo.Jogos (IdJogo)             ON DELETE CASCADE,
    CONSTRAINT FK_Bibliotecas_Plataformas  FOREIGN KEY (IdPlataforma) REFERENCES dbo.Plataformas (IdPlataforma)
);
CREATE INDEX IX_Bibliotecas_IdJogo ON dbo.Bibliotecas (IdJogo);

/* ---------- Avaliações (uma por utilizador e jogo) ---------- */
CREATE TABLE dbo.Avaliacoes (
    IdAvaliacao   INT IDENTITY(1,1) NOT NULL,
    IdUtilizador  INT               NOT NULL,
    IdJogo        INT               NOT NULL,
    Nota          DECIMAL(3,1)      NOT NULL,
    Comentario    NVARCHAR(500)     NULL,
    DataAvaliacao DATETIME2(0)      NOT NULL CONSTRAINT DF_Avaliacoes_Data DEFAULT SYSDATETIME(),
    CONSTRAINT PK_Avaliacoes PRIMARY KEY (IdAvaliacao),
    CONSTRAINT UQ_Avaliacoes_Utilizador_Jogo UNIQUE (IdUtilizador, IdJogo),
    CONSTRAINT CK_Avaliacoes_Nota CHECK (Nota BETWEEN 0 AND 10),
    CONSTRAINT FK_Avaliacoes_Utilizadores FOREIGN KEY (IdUtilizador) REFERENCES dbo.Utilizadores (IdUtilizador) ON DELETE CASCADE,
    CONSTRAINT FK_Avaliacoes_Jogos        FOREIGN KEY (IdJogo)       REFERENCES dbo.Jogos (IdJogo)             ON DELETE CASCADE
);
CREATE INDEX IX_Avaliacoes_IdJogo ON dbo.Avaliacoes (IdJogo);

/* ---------- Conquistas desbloqueadas por utilizador e jogo ---------- */
CREATE TABLE dbo.Conquistas (
    IdConquista      INT IDENTITY(1,1) NOT NULL,
    IdUtilizador     INT               NOT NULL,
    IdJogo           INT               NOT NULL,
    Titulo           NVARCHAR(100)     NOT NULL,
    Descricao        NVARCHAR(250)     NULL,
    Pontos           INT               NOT NULL CONSTRAINT DF_Conquistas_Pontos DEFAULT 10,
    DataDesbloqueio  DATETIME2(0)      NOT NULL CONSTRAINT DF_Conquistas_Data   DEFAULT SYSDATETIME(),
    CONSTRAINT PK_Conquistas PRIMARY KEY (IdConquista),
    CONSTRAINT UQ_Conquistas_Utilizador_Jogo_Titulo UNIQUE (IdUtilizador, IdJogo, Titulo),
    CONSTRAINT CK_Conquistas_Pontos CHECK (Pontos > 0),
    CONSTRAINT FK_Conquistas_Utilizadores FOREIGN KEY (IdUtilizador) REFERENCES dbo.Utilizadores (IdUtilizador) ON DELETE CASCADE,
    CONSTRAINT FK_Conquistas_Jogos        FOREIGN KEY (IdJogo)       REFERENCES dbo.Jogos (IdJogo)             ON DELETE CASCADE
);
CREATE INDEX IX_Conquistas_IdJogo ON dbo.Conquistas (IdJogo);
GO

/* ==========================================================================
   Dados de exemplo (as tabelas acabaram de ser criadas, logo os IDs começam em 1)
   ========================================================================== */
INSERT INTO dbo.Categorias (Nome) VALUES
    (N'RPG'), (N'Ação'), (N'Plataformas'), (N'Simulação'), (N'Puzzle'), (N'Aventura');

INSERT INTO dbo.Editoras (Nome, Pais) VALUES
    (N'CD Projekt',        N'Polónia'),
    (N'Supergiant Games',  N'Estados Unidos'),
    (N'Maddy Makes Games', N'Canadá'),
    (N'ConcernedApe',      N'Estados Unidos'),
    (N'Valve',             N'Estados Unidos'),
    (N'Team Cherry',       N'Austrália'),
    (N'Nintendo',          N'Japão'),
    (N'FromSoftware',      N'Japão');

INSERT INTO dbo.Plataformas (Nome, Fabricante) VALUES
    (N'PC',               N'Vários'),
    (N'PlayStation 5',    N'Sony'),
    (N'Nintendo Switch',  N'Nintendo'),
    (N'Xbox Series X|S',  N'Microsoft');

INSERT INTO dbo.Utilizadores (NomeUtilizador, Email) VALUES
    (N'ana.silva',    N'ana.silva@exemplo.pt'),
    (N'bruno.costa',  N'bruno.costa@exemplo.pt'),
    (N'carla.matos',  N'carla.matos@exemplo.pt');

-- Titulo, Ano, IdCategoria, IdEditora
INSERT INTO dbo.Jogos (Titulo, AnoLancamento, IdCategoria, IdEditora) VALUES
    (N'The Witcher 3: Wild Hunt',                 2015, 1, 1),
    (N'Hades',                                    2020, 2, 2),
    (N'Celeste',                                  2018, 3, 3),
    (N'Stardew Valley',                           2016, 4, 4),
    (N'Portal 2',                                 2011, 5, 5),
    (N'Hollow Knight',                            2017, 3, 6),
    (N'The Legend of Zelda: Breath of the Wild',  2017, 6, 7),
    (N'Elden Ring',                               2022, 1, 8);

-- IdUtilizador, IdJogo, IdPlataforma, HorasJogadas, Estado
INSERT INTO dbo.Bibliotecas (IdUtilizador, IdJogo, IdPlataforma, HorasJogadas, Estado) VALUES
    (1, 1, 1, 120.5, N'Concluído'),
    (1, 2, 3,  35.0, N'A jogar'),
    (1, 3, 1,  12.5, N'Concluído'),
    (1, 8, 2,  60.0, N'A jogar'),
    (2, 1, 2,  80.0, N'Concluído'),
    (2, 4, 3, 200.5, N'A jogar'),
    (2, 7, 3,  95.0, N'Concluído'),
    (3, 5, 1,  14.0, N'Concluído'),
    (3, 6, 1,  28.5, N'A jogar'),
    (3, 3, 3,   0.0, N'Por jogar');

-- IdUtilizador, IdJogo, Nota, Comentario
INSERT INTO dbo.Avaliacoes (IdUtilizador, IdJogo, Nota, Comentario) VALUES
    (1, 1,  9.5, N'História e missões secundárias excecionais.'),
    (1, 3,  9.0, N'Desafiante e emocionante.'),
    (1, 2,  8.5, N'Combate viciante.'),
    (2, 1, 10.0, N'Uma obra-prima.'),
    (2, 7,  9.5, N'Liberdade de exploração incrível.'),
    (3, 5,  9.5, N'Puzzles brilhantes e muito humor.');

-- IdUtilizador, IdJogo, Titulo, Descricao, Pontos
INSERT INTO dbo.Conquistas (IdUtilizador, IdJogo, Titulo, Descricao, Pontos) VALUES
    (1, 1, N'Caçador de Monstros',  N'Derrotar 50 monstros.', 20),
    (1, 3, N'Cume da Montanha',     N'Chegar ao topo da Montanha Celeste.', 30),
    (1, 2, N'Primeira Fuga',        N'Escapar do Submundo pela primeira vez.', 15),
    (2, 1, N'Mestre Bruxo',         N'Concluir a história principal.', 40),
    (2, 7, N'Explorador de Hyrule', N'Descobrir todos os santuários.', 50),
    (3, 5, N'Pensar com Portais',   N'Concluir a câmara 10.', 10);
GO

PRINT N'Base de dados GameVault criada com sucesso.';
GO
