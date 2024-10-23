using System.Numerics;
namespace Chess.Board {
    struct BitBoardPieces {
        // pieces
        public UInt64[] Pieces = new UInt64[12];
        public ref UInt64 BlackPawn   => ref Pieces[0];
        public ref UInt64 BlackRook   => ref Pieces[1];
        public ref UInt64 BlackKnight => ref Pieces[2];
        public ref UInt64 BlackBishop => ref Pieces[3];
        public ref UInt64 BlackQueen  => ref Pieces[4];
        public ref UInt64 BlackKing   => ref Pieces[5];
        public ref UInt64 WhitePawn   => ref Pieces[6];
        public ref UInt64 WhiteRook   => ref Pieces[7];
        public ref UInt64 WhiteKnight => ref Pieces[8];
        public ref UInt64 WhiteBishop => ref Pieces[9];
        public ref UInt64 WhiteQueen  => ref Pieces[10];
        public ref UInt64 WhiteKing   => ref Pieces[11];

        public BitBoardPieces() {}
    }
}
