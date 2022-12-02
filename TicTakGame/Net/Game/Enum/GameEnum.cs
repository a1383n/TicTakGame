namespace TicTakGame.Net.Enum
{
    enum GameStatus
    {
        WaitingForHandShake,
        CellSelected,
        PlayerTurn,
        End
    }

    enum GameResult
    {
        InProgress,
        Draw,
        PlayerWin
    }

    enum Role : byte
    {
        Null,
        X,
        O
    }
}