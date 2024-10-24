using System.Collections;
using System.Text;

namespace Chess.Board.BitBoard {
    class BitBoardPieces {
        private const ulong PawnRank   = 0xFF;
        private const ulong RookRank   = 0b10000001;
        private const ulong KnightRank = 0b01000010;
        private const ulong BishopRank = 0b00100100;
        private const ulong QueenRank  = 0b00010000;
        private const ulong KingRank   = 0b00001000;

        public ulong[] PiecesArr { get; private set; } = new ulong[12];

        // PiecesArr
        public ref ulong BlackPawn   => ref PiecesArr[0];
        public ref ulong BlackRook   => ref PiecesArr[1];
        public ref ulong BlackKnight => ref PiecesArr[2];
        public ref ulong BlackBishop => ref PiecesArr[3];
        public ref ulong BlackQueen  => ref PiecesArr[4];
        public ref ulong BlackKing   => ref PiecesArr[5];
        public ref ulong WhitePawn   => ref PiecesArr[6];
        public ref ulong WhiteRook   => ref PiecesArr[7];
        public ref ulong WhiteKnight => ref PiecesArr[8];
        public ref ulong WhiteBishop => ref PiecesArr[9];
        public ref ulong WhiteQueen  => ref PiecesArr[10];
        public ref ulong WhiteKing   => ref PiecesArr[11];

        public BitBoardPieces() {}

        public void SetDefaults()
        {
            PiecesArr = [
                PawnRank << 8,
                RookRank,
                KnightRank,
                BishopRank,
                QueenRank,
                KingRank,
                PawnRank << 48,
                RookRank << 56,
                KnightRank << 56,
                BishopRank << 56,
                QueenRank << 56,
                KingRank << 56,
            ];
        }

        public void ClearBoard()
        {
            for (int i = 0; i < PiecesArr.Length; i++)
                PiecesArr[i] = 0UL;
        }

        public IEnumerable<ulong> AllPieces
        {
            get => PiecesArr.AsEnumerable();
        }

        public IEnumerable<ulong> BlackPieces
        {
            get => PiecesArr.Take(6);
        }

        public IEnumerable<ulong> WhitePieces
        {
            get => PiecesArr.Skip(6).Take(6);
        }

        public char[,] ToCharBoard()
        {
            // we'll make a chess board represented as 8x8 chars
            char[,] board = new char[8,8];
            // initialize it with spaces to represent blank squares
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    board[i,j] = ' ';

            // iterate through each piece type
            for (int i = 0; i < 12; i++)
            {
                // get all coordinates of that type
                var coords = BitBoardMasks.MaskToCoordinates(PiecesArr[i]);
                // for each coordinate of this piece type
                foreach (var coord in coords)
                {
                    // add the appropriate letter to the character board
                    switch(i) 
                    {
                        case 0: board[coord.row,coord.col]  = 'p'; break;
                        case 1: board[coord.row,coord.col]  = 'r'; break;
                        case 2: board[coord.row,coord.col]  = 'n'; break;
                        case 3: board[coord.row,coord.col]  = 'b'; break;
                        case 4: board[coord.row,coord.col]  = 'q'; break;
                        case 5: board[coord.row,coord.col]  = 'k'; break;
                        case 6: board[coord.row,coord.col]  = 'P'; break;
                        case 7: board[coord.row,coord.col]  = 'R'; break;
                        case 8: board[coord.row,coord.col]  = 'N'; break;
                        case 9: board[coord.row,coord.col]  = 'B'; break;
                        case 10: board[coord.row,coord.col] = 'Q'; break;
                        case 11: board[coord.row,coord.col] = 'K'; break;
                    }
                }
            }

            return board;
        }

        public override string ToString()
        {
            char[,] board = ToCharBoard();

            StringBuilder prettyBoard = new StringBuilder();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    prettyBoard.Append($"[{board[i,j]}] ");
                }
                prettyBoard.Append('\n');
            }

            return prettyBoard.ToString();
        }
    } 
}