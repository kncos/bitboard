
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

        public bool Move(ulong startpos, ulong endpos)
        {
            PieceType? startType = Pieces.PieceTypeAtCoordinate(startpos);
            PieceType? endType = Pieces.PieceTypeAtCoordinate(endpos);

            // no piece in start pos, nothing to move
            if (!startType.HasValue)
                return false;


            return true;
        }

        public bool Move(int start_row, int start_col, int end_row, int end_col)
        {
            // convert r,c coordinates to masks
            ulong startpos = BitBoardMasks.CoordinateToMask(start_row, start_col) ?? 0UL;
            ulong endpos = BitBoardMasks.CoordinateToMask(end_row, end_col) ?? 0UL;
            return Move(startpos, endpos);
        }
    }
}