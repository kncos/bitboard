
namespace Chess.Board.BitBoard
{
    static class BitBoardMasks 
    {
        public static UInt64 WhitePawn(UInt64 position) {
            // is in starting position
            if ((position & (0xFF << 48)) != 0) {
                return (position >> 8 | position >> 16);
            }
            
            return position >> 8;
        }

        public static UInt64 BlackPawn(UInt64 position) {
            if ((position & (0xFF << 8)) != 0)
                return (position << 8 | position << 16);

            return position << 8;
        } 

    }
}