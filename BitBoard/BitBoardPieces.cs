namespace Chess.Board.BitBoard {
    class BitBoardPieces {

        private const ulong PawnRank   = 0xFF;
        private const ulong RookRank   = 0b10000001;
        private const ulong KnightRank = 0b01000010;
        private const ulong BishopRank = 0b00100100;
        private const ulong QueenRank  = 0b00010000;
        private const ulong KingRank   = 0b00001000;

        // pieces
        public ulong BlackPawn   = PawnRank << 8;
        public ulong BlackRook   = RookRank;
        public ulong BlackKnight = KnightRank;
        public ulong BlackBishop = BishopRank;
        public ulong BlackQueen  = QueenRank;
        public ulong BlackKing   = KingRank;
        public ulong WhitePawn   = PawnRank << 48;
        public ulong WhiteRook   = RookRank << 56;
        public ulong WhiteKnight = KnightRank << 56;
        public ulong WhiteBishop = BishopRank << 56;
        public ulong WhiteQueen  = QueenRank << 56;
        public ulong WhiteKing   = KingRank << 56;

        public BitBoardPieces() {}
    }
}
