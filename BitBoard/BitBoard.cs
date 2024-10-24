
namespace Chess.Board.BitBoard
{
    class BitBoard
    {
        public required BitBoardPieces Pieces = new();
        public required BitBoardState State = new();

        public BitBoard() {}
        
    }
}