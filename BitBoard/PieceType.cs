
namespace Chess.Board.BitBoard
{
    enum PieceType {
        BlackPawn = 0, BlackRook, BlackKnight, BlackBishop, BlackQueen, BlackKing,
        WhitePawn, WhiteRook, WhiteKnight, WhiteBishop, WhiteQueen, WhiteKing,
    }

    static class PieceTypeExtensions
    {
        public static bool IsWhite(this PieceType pt) => pt switch 
        {
            PieceType.WhitePawn or PieceType.WhiteRook or PieceType.WhiteKnight
            or PieceType.WhiteBishop or PieceType.WhiteQueen or PieceType.WhiteKing => true,
            _ => false,
        };
    }

}