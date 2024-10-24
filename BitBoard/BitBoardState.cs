
namespace Chess.Board.BitBoard
{
    class BitBoardState
    {
        // which color is active? True for white, False for black
        public bool WhiteActive = true;
        
        // Castling Availability
        public bool WhiteKingside = true;
        public bool WhiteQueenside = true;
        public bool BlackKingside = true;
        public bool BlackQueenside = true;

        public UInt64 EnPassantTarget = 0;

        // number of half moves since the last capture or pawn advance
        public int HalfmoveClock = 0;
        // total number of full moves. Starts at 1
        public int FullmoveCount = 1;

        public BitBoardState() {}
    }
}