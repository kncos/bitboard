
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
            // ? pattern matching with `is`
            if (Pieces.PieceTypeAtCoordinate(startpos) is not PieceType startType)
                return false;

            if (startType.IsWhite() != State.WhiteActive)
                return false;

            PieceType? endType = Pieces.PieceTypeAtCoordinate(endpos);

            // get friendly/enemy piece masks
            ulong friendly = startType.IsWhite() ? BitBoardMasks.WhitePositionsMask(Pieces) : BitBoardMasks.BlackPositionsMask(Pieces);
            ulong enemy = startType.IsWhite() ? BitBoardMasks.BlackPositionsMask(Pieces) : BitBoardMasks.WhitePositionsMask(Pieces);

            var masks = AttackMasks.PieceTypeMask(startType, startpos, friendly, enemy, State.EnPassantTarget);

            // invalid move. Can't reach end position
            if ((endpos & masks.validMoves) == 0)
                return false;
            
            // handle en passant
            State.EnPassantTarget = 0UL; // reset to 0 since we already got masks.

            if ((startType == PieceType.BlackPawn) && ((endpos >> 16) == startpos))
                State.EnPassantTarget = endpos >> 8; // 1 row above

            if ((startType == PieceType.WhitePawn) && ((endpos << 16) == startpos))
                State.EnPassantTarget = endpos << 8; // 1 row below

            // assign new piece places
            BitBoardMasks.UnsetCoordinate(startpos, ref Pieces.FromPieceType(startType));
            BitBoardMasks.SetCoordinate(endpos, ref Pieces.FromPieceType(startType));

            // if there was something at the old end coordinate, remove it
            if (endType.HasValue)
                BitBoardMasks.UnsetCoordinate(endpos, ref Pieces.FromPieceType(endType.Value));

            // update full move clock after black's turn
            if (!startType.IsWhite())
                State.FullmoveCount += 1;

            State.WhiteActive = !State.WhiteActive;
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