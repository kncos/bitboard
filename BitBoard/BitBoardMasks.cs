
using System.Data;
using System.Numerics;

namespace Chess.Board.BitBoard
{
    static class BitBoardMasks 
    {
        public static void SetCoordinate(int row, int col, ref ulong bitmask)
        {
            bitmask |= CoordinateToMask(row, col) ?? 0UL;
        }

        public static void SetCoordinate(ulong pos, ref ulong bitmask)
        {
            bitmask |= pos;
        }

        public static void UnsetCoordinate(int row, int col, ref ulong bitmask)
        {
            bitmask &= ~(CoordinateToMask(row, col) ?? 0UL);
        }

        public static void UnsetCoordinate(ulong pos, ref ulong bitmask)
        {
            bitmask &= ~pos;
        }

        public static (int row, int col)? MaskToCoordinate(ulong mask)
        {
            if ((mask & (mask-1)) != 0 || (mask == 0))
                return null;

            int index = BitOperations.TrailingZeroCount(mask);
            int row = index / 8;
            int col = index % 8;
            return (row, col);
        }

        public static List<(int row, int col)> MaskToCoordinates(ulong mask)
        {
            var res = new List<(int row, int col)>();
            for (int i = 0; i < 64; i++)
            {
                if ((mask & (1UL << i)) != 0) 
                {
                    var c = MaskToCoordinate(1UL << i);
                    if (c.HasValue)
                        res.Add(c.Value);
                }
            }
            return res;
        }

        public static ulong? CoordinateToMask(int row, int col) 
        {
            if (row < 0 || 7 < row || col < 0 || 7 < col)
                return null;

            return (1UL << ((row * 8) + col));
        }

        public static ulong? AlgebraicNotationToMask(string coord)
        {
            // col 0 is 'a', col 7 is 'h'
            // row 0 is '8', row 7 is '1'

            if (coord[0] < 'a' || 'h' < coord[0])
                return null;
            if (coord[1] < '1' || '8' < coord[1])
                return null;

            int row = '8' - coord[1]; // 8 - 1 = 7
            int col = coord[0] - 'a'; // h - a = 7
            return CoordinateToMask(row, col);
        }

        public static string MaskToAlgebraicNotation(ulong mask)
        {
            var c = MaskToCoordinate(mask);
            if (!c.HasValue)
                return "-";

            var coord = c.Value;
            
            char row = (char)('8' - coord.row);
            char col = (char)('a' + coord.col);
            return $"{col}{row}";
        }

        public static ulong AllPositionsMask(this BitBoardPieces bb) 
        {
            return bb.WhitePositionsMask() | bb.BlackPositionsMask();
        }

        public static ulong WhitePositionsMask(this BitBoardPieces bb) 
        {
            return bb.WhiteBishop | bb.WhiteKing | bb.WhiteKnight | bb.WhitePawn | bb.WhiteQueen | bb.WhiteRook;
        }

        public static ulong BlackPositionsMask(this BitBoardPieces bb) 
        {
            return bb.BlackBishop | bb.BlackKing | bb.BlackKnight | bb.BlackPawn | bb.BlackQueen | bb.BlackRook;
        }

        public static bool HasOverlappingPieces(this BitBoardPieces bb) 
        {
            ulong mask = 0xFFFFFFFFFFFFFFFF;

            foreach (ulong m in bb.AllPieces) {
                mask = mask & m;
            }

            return (mask != 0);
        }

    }
}