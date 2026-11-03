namespace GameVault.Modelos;

/// <summary>Estados possíveis de um jogo na biblioteca (têm de coincidir com o CHECK da base de dados).</summary>
public static class EstadosJogo
{
    public const string PorJogar = "Por jogar";
    public const string AJogar = "A jogar";
    public const string Concluido = "Concluído";
    public const string Abandonado = "Abandonado";

    public static readonly string[] Todos = { PorJogar, AJogar, Concluido, Abandonado };
}
