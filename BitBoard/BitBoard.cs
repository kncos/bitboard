
namespace Chess.Board.BitBoard
{
    class BitBoard
    {
        public BitBoardPieces Pieces { get; private set; } = new();
        public BitBoardState State { get; private set; } = new();

        public BitBoard() {}

        public BitBoard(BitBoardPieces pieces, BitBoardState state)
        {
            Pieces = pieces;
            State = state;
        }

    }
}