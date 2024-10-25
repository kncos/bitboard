
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

        public bool Move(int start_row, int start_col, int end_row, int end_col)
        {
            PieceType? startType = Pieces.PieceTypeAtCoordinate(start_row, start_col);
            PieceType? endType = Pieces.PieceTypeAtCoordinate(end_row, end_col);

            // no piece in start pos, nothing to move
            if (!startType.HasValue)
                return false;

            // remove the start type from its start position 
            BitBoardMasks.UnsetCoordinate(start_row, start_col, ref Pieces.FromPieceType(startType.Value));
            // place it in its end position
            BitBoardMasks.SetCoordinate(end_row, end_col, ref Pieces.FromPieceType(startType.Value));
            // if the end position had something there, let this piece capture it
            if (endType.HasValue)
                BitBoardMasks.UnsetCoordinate(end_row, end_col, ref Pieces.FromPieceType(endType.Value));

            return true;
        }
    }
}