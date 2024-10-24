
using System.Data;
using System.Numerics;

namespace Chess.Board.BitBoard
{
    static class BitBoardMasks 
    {
        public static (int row, int col)? MaskToCoordinate(ulong mask)
        {
            if ((mask & (mask-1)) != 0 || (mask == 0))
                return null;

            int index = BitOperations.TrailingZeroCount(mask);
            int row = index / 8;
            int col = index % 8;
            return (row, col);
        }

        public static ulong? CoordinateToMask(int row, int col) 
        {
            if (row < 0 || 7 < row || col < 0 || 7 < col)
                return null;

            return (1UL << ((row * 8) + col));
        }

        public static ulong AllPositionsMask(this BitBoardPieces bb) {
            return bb.WhitePositionsMask() | bb.BlackPositionsMask();
        }

        public static ulong WhitePositionsMask(this BitBoardPieces bb) {
            return bb.WhiteBishop | bb.WhiteKing | bb.WhiteKnight | bb.WhitePawn | bb.WhiteQueen | bb.WhiteRook;
        }

        public static ulong BlackPositionsMask(this BitBoardPieces bb) {
            return bb.BlackBishop | bb.BlackKing | bb.BlackKnight | bb.BlackPawn | bb.BlackQueen | bb.BlackRook;
        }
    }
}