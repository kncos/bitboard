
using System.Security.Principal;

namespace Chess.Board.BitBoard
{
    class FenStringParser
    {
        public static BitBoardPieces? ParsePieceData(string pieceData) 
        {
            string baseErr = "Invalid FEN position data";
            BitBoardPieces bbpieces = new BitBoardPieces();

            string[] ranks = pieceData.Split('/');

            if (ranks.Length != 8)
                throw new ArgumentException($"{baseErr}: expected 8 ranks. Found {ranks.Length} (splitting string at '/': {pieceData}");

            for (int i = 0; i < ranks.Length; i++) 
            {
                int col = 0;
                for (int j = 0; j < ranks[i].Length; j++) 
                {
                    // handle blank spaces (represented by numbers from 1-8)
                    if ('0' < ranks[i][j] && ranks[i][j] < '9') 
                    {
                        col += ranks[i][j] - '0';
                        continue;
                    }
                    
                    // convert row/col into an appropriate bit to set
                    ulong? boardpos = BitBoardMasks.CoordinateToMask(i, col);
                    if (!boardpos.HasValue)
                        throw new ArgumentException($"{baseErr}: Invalid row/column coordinate found: ({i}, {col})");

                    switch (ranks[i][j])
                    {
                        case 'r':
                            bbpieces.BlackRook |= boardpos.Value;
                            break;
                        case 'n':
                            bbpieces.BlackKnight |= boardpos.Value;
                            break;
                        case 'b':
                            bbpieces.BlackBishop |= boardpos.Value;
                            break;
                        case 'q':
                            bbpieces.BlackQueen |= boardpos.Value;
                            break;
                        case 'k':
                            bbpieces.BlackKing |= boardpos.Value;
                            break;
                        case 'p':
                            bbpieces.BlackPawn |= boardpos.Value;
                            break;
                        case 'R':
                            bbpieces.WhiteRook |= boardpos.Value;
                            break;
                        case 'N':
                            bbpieces.WhiteKnight |= boardpos.Value;
                            break;
                        case 'B':
                            bbpieces.WhiteBishop |= boardpos.Value;
                            break;
                        case 'Q':
                            bbpieces.WhiteQueen |= boardpos.Value;
                            break;
                        case 'K':
                            bbpieces.WhiteKing |= boardpos.Value;
                            break;
                        case 'P':
                            bbpieces.WhitePawn |= boardpos.Value;
                            break;
                        default:
                            break;
                    }
                    // go to next column now that piece was placed in this col
                    col += 1;
                }
                // if for whatever reason this rank did not end in column 8 then
                // we know this is a malformed rank.
                if (col != 8)
                    throw new ArgumentException($"{baseErr}: malformed column. expected 8 columns, found {col} on row {i} (rank: {ranks[i]})");
            }

            return bbpieces;
        }

        public static BitBoardState ParseStateData(string stateData)
        {
            BitBoardState bbstate = new BitBoardState();

            string baseErr = "Invalid FEN state data";

            // ensure we have the correct # of components
            string[] components = stateData.Split(' ');
            if (components.Length != 5)
                throw new ArgumentException($"{baseErr}: Found {stateData} which splits into {components.Length} components");

            // parse whose turn it is
            bbstate.WhiteActive = ParseWhiteActive(components[0], baseErr);
            // parse castling data
            var castlingData = ParseCastlingStateData(components[1], baseErr);
            bbstate.WhiteKingside = castlingData.wk;
            bbstate.WhiteQueenside = castlingData.wq;
            bbstate.BlackKingside = castlingData.bk;
            bbstate.BlackQueenside = castlingData.bq;
            // parse en passant target square
            bbstate.EnPassantTarget = ParseEnPassantTarget(components[2], baseErr);
            // parse halfmove clock
            bbstate.HalfmoveClock = ParseHalfmoveClock(components[3], baseErr);
            // parse fullmove count
            bbstate.FullmoveCount = ParseFullmoveCount(components[4], baseErr);

            return bbstate;
        }

        private static bool ParseWhiteActive(string component, string baseErr)
        {
            if (String.Equals(component, "w", StringComparison.OrdinalIgnoreCase))
                return true;
            else if (String.Equals(component, "b", StringComparison.OrdinalIgnoreCase))
                return false;
            else
                throw new ArgumentException($"{baseErr}: Side to move component invalid. Expected \"w\" or \"b\". Found {component}");
        }

        private static (bool wk, bool wq, bool bk, bool bq) ParseCastlingStateData(string component, string baseErr)
        {
            if (component.Length > 4)
                throw new ArgumentException($"{baseErr}: Castling component longer than 4 characters. Found: {component}");

            bool WhiteKingside = false;
            bool WhiteQueenside = false;
            bool BlackKingside = false;
            bool BlackQueenside = false;
            foreach (char c in component) 
            {
                switch(c) 
                {
                    case 'K':
                        if (WhiteKingside)
                            throw new ArgumentException($"{baseErr}: Found multiple 'K' in castling component: {component}");

                        WhiteKingside = true;
                        break;
                    case 'Q':
                        if (WhiteQueenside)
                            throw new ArgumentException($"{baseErr}: Found multiple 'Q' in castling component: {component}");

                        WhiteQueenside = true;
                        break;
                    case 'k':
                        if (BlackKingside)
                            throw new ArgumentException($"{baseErr}: Found multiple 'k' in castling component: {component}");

                        BlackKingside = true;
                        break;
                    case 'q':
                        if (BlackQueenside)
                            throw new ArgumentException($"{baseErr}: Found multiple 'q' in castling component: {component}");

                        BlackQueenside = true;
                        break;
                    case '-':
                        if (WhiteKingside || WhiteQueenside || BlackKingside || BlackQueenside || component.Length > 1)
                            throw new ArgumentException($"{baseErr}: Found unexpected '-' character in castling component: {component}");

                        break;
                    default:
                        throw new ArgumentException($"{baseErr}: Invalid token found in castling availability component. Found {c} in {component}");
                }
            }

            return (WhiteKingside, WhiteQueenside, BlackKingside, BlackQueenside);
        } 

        private static ulong ParseEnPassantTarget(string component, string baseErr)
        {
            string InvalidLengthMsg = $"{baseErr}: En Passant Target Component is not a valid length. Expected 1 or 2. Found {component.Length} for string {component}";
            string InvalidAlgebraicNotationMsg = $"{baseErr}: En Passant Target Component has an invalid coordinate. Expected algebraic notation between 'a1' and 'h8'. Found {component}";

            // if we have '-', then there is no target en passant square. so the mask will be 0's.
            // if we have a component length of 1 and it is not '-', that is not a valid algebraic notation.
            if (component.Length == 1)
            {
                if (String.Equals(component, "-"))
                    return 0UL;
                else
                    throw new ArgumentException(InvalidAlgebraicNotationMsg);
            }
            // if the length is 2, then it must be algebraic notation. Validate algebraic
            // notation and return the proper mask, otherwise return an exception
            else if (component.Length == 2)
            {
                ulong? mask = BitBoardMasks.AlgebraicNotationToMask(component);
                if (!mask.HasValue)
                    throw new ArgumentException(InvalidAlgebraicNotationMsg);
                
                return mask.Value;
            }

            // any length other than 1 or 2 is invalid.
            throw new ArgumentException(InvalidLengthMsg);            
        }

        private static int ParseHalfmoveClock(string component, string baseErr)
        {
            int hc = 0;
            if (!Int32.TryParse(component, out hc))
                throw new ArgumentException($"{baseErr}: Could not parse halfmove clock component to an integer. Found: {component}");
            
            if (hc < 0)
                throw new ArgumentException($"{baseErr}: Halfmove Clock is an invalid number (negative number). Found {component}.");

            return hc;
        }

        private static int ParseFullmoveCount(string component, string baseErr)
        {
            int fc = 0;
            if (!Int32.TryParse(component, out fc))
                throw new ArgumentException($"{baseErr}: Could not parse fullmove clock component to an integer. Found: {component}");
            
            if (fc < 1)
                throw new ArgumentException($"{baseErr}: Fullmove Clock is an invalid number (must be 1 or greater). Found {component}.");

            return fc;
        }
    }
}