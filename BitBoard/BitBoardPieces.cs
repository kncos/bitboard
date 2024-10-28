using System.Collections;
using System.Security.Cryptography;
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
        public ref ulong BlackPawn   => ref FromPieceType(PieceType.BlackPawn);
        public ref ulong BlackRook   => ref FromPieceType(PieceType.BlackRook);
        public ref ulong BlackKnight => ref FromPieceType(PieceType.BlackKnight);
        public ref ulong BlackBishop => ref FromPieceType(PieceType.BlackBishop);
        public ref ulong BlackQueen  => ref FromPieceType(PieceType.BlackQueen);
        public ref ulong BlackKing   => ref FromPieceType(PieceType.BlackKing);
        public ref ulong WhitePawn   => ref FromPieceType(PieceType.WhitePawn);
        public ref ulong WhiteRook   => ref FromPieceType(PieceType.WhiteRook);
        public ref ulong WhiteKnight => ref FromPieceType(PieceType.WhiteKnight);
        public ref ulong WhiteBishop => ref FromPieceType(PieceType.WhiteBishop);
        public ref ulong WhiteQueen  => ref FromPieceType(PieceType.WhiteQueen);
        public ref ulong WhiteKing   => ref FromPieceType(PieceType.WhiteKing);

        // get piece from enum
        public ref ulong FromPieceType(PieceType p) => ref PiecesArr[(int)p];

        public static PieceType? ToPieceType(int i) => (0 <= i && i < 12) ? (PieceType)i : null;

        public PieceType? PieceTypeAtCoordinate(ulong pos)
        {
            if ((pos & (pos-1)) != 0)
                return null;

            for (int i = 0; i < PiecesArr.Length; i++)
                if ((pos & PiecesArr[i]) != 0)
                    return (PieceType)i;

            return null;
        }

        public PieceType? PieceTypeAtCoordinate(int row, int col)
        {
            ulong? mask = BitBoardMasks.CoordinateToMask(row, col);
            if (!mask.HasValue)
                return null;

            return PieceTypeAtCoordinate(mask.Value);
        }

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
            for (int i = 0; i < PiecesArr.Length; i++)
            {
                // get all coordinates of that type
                var coords = BitBoardMasks.MaskToCoordinates(PiecesArr[i]);
                // for each coordinate of this piece type
                foreach (var coord in coords)
                {
                    // add the appropriate letter to the character board.
                    // ! new ability unlocked, pattern matching
                    board[coord.row, coord.col] = ((PieceType)i) switch 
                    {
                        // black pieces
                        PieceType.BlackPawn   => 'p',
                        PieceType.BlackRook   => 'r',
                        PieceType.BlackKnight => 'n',
                        PieceType.BlackBishop => 'b',
                        PieceType.BlackQueen  => 'q',
                        PieceType.BlackKing   => 'k',
                        // white pieces
                        PieceType.WhitePawn   => 'P',
                        PieceType.WhiteRook   => 'R',
                        PieceType.WhiteKnight => 'N',
                        PieceType.WhiteBishop => 'B',
                        PieceType.WhiteQueen  => 'Q',
                        PieceType.WhiteKing   => 'K',
                        _ => throw new ArgumentOutOfRangeException(nameof(i), "Invalid Piece Type")
                    };
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