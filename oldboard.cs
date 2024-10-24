//using System.Text;
//
//class BitBoard {
//    // 12 bitboards for each piece and color
//    private UInt64 BlackPawn;
//    private UInt64 BlackRook;
//    private UInt64 BlackKnight;
//    private UInt64 BlackBishop;
//    private UInt64 BlackQueen;
//    private UInt64 BlackKing;
//    private UInt64 WhitePawn;
//    private UInt64 WhiteRook;
//    private UInt64 WhiteKnight;
//    private UInt64 WhiteBishop;
//    private UInt64 WhiteQueen;
//    private UInt64 WhiteKing;
//    
//    // which color is active? True for white, False for black
//    private bool WhiteActive;
//    
//    // Castling Availability
//    private bool WhiteKingside;
//    private bool WhiteQueenside;
//    private bool BlackKingside;
//    private bool BlackQueenside;
//
//    private UInt64 EnPassantBit;
//
//    // number of half moves since the last capture or pawn advance
//    private int Halfmove_Clock;
//    // total number of full moves. Starts at 1
//    private int Fullmove_Count;
//
//    public BitBoard(string fenString) 
//    {
//        try
//        {
//            SetBoard(fenString);
//        }
//        catch (ArgumentException ex) 
//        {
//            Console.WriteLine($"invalid FEN string passed to BitBoard constructor.\n{ex.Message}");
//            throw;
//        }
//    }
//
//    public void SetBoard(string fenString) 
//    {
//        // FEN strings have 6 fields. The piece placement data,
//        // the active color (whose turn it is), Castling availability,
//        // En Passant target square, halfmove clock, and fullmove number
//        string[] fields = fenString.Split(' ');
//        if (fields.Length != 6)
//            throw new ArgumentException($"Invalid number of fields! Expected 6, got {fields.Length}");
//        
//        // Attempt to set board positions. If position data was not valid,
//        // then return false (method did not succeed)
//        if (!SetFenPositions(fields[0]))
//            throw new ArgumentException("Failed to set board positions.");
//        
//        // active color should be w or b to designate whose turn it is
//        if (!SetActiveColor(fields[1]))
//            throw new ArgumentException("Failed to set active color.");
//
//        if (!SetCastling(fields[2]))
//            throw new ArgumentException("Failed to set castling information.");
//        
//        if (!SetEnPassantTarget(fields[3]))
//            throw new ArgumentException("Failed to set En Passant target");
//
//        bool success = int.TryParse(fields[4], out Halfmove_Clock);
//        if (!success)
//            throw new ArgumentException("Failed to parse Halfmove Clock integer.");
//
//        success = int.TryParse(fields[5], out Fullmove_Count);
//        if (!success)
//            throw new ArgumentException("Failed to parse Fullmove Count integer.");
//    }
//
//    private bool SetFenPositions(string positionData) 
//    {
//        string[] ranks = positionData.Split('/');
//        if (ranks.Length != 8)
//            return false;
//
//        for (int i = 0; i < ranks.Length; i++) 
//        {
//            int col = 0;
//            for (int j = 0; j < ranks[i].Length; j++) 
//            {
//                // handle blank spaces (represented by numbers from 1-8)
//                if ('0' < ranks[i][j] && ranks[i][j] < '9') 
//                {
//                    col += ranks[i][j] - '0';
//                    continue;
//                }
//                
//                // convert row/col into an appropriate bit to set
//                UInt64 boardpos = (1UL << ((i*8)+col));
//                switch (ranks[i][j])
//                {
//                    case 'r':
//                        BlackRook |= boardpos;
//                        break;
//                    case 'n':
//                        BlackKnight |= boardpos;
//                        break;
//                    case 'b':
//                        BlackBishop |= boardpos;
//                        break;
//                    case 'q':
//                        BlackQueen |= boardpos;
//                        break;
//                    case 'k':
//                        BlackKing |= boardpos;
//                        break;
//                    case 'p':
//                        BlackPawn |= boardpos;
//                        break;
//                    case 'R':
//                        WhiteRook |= boardpos;
//                        break;
//                    case 'N':
//                        WhiteKnight |= boardpos;
//                        break;
//                    case 'B':
//                        WhiteBishop |= boardpos;
//                        break;
//                    case 'Q':
//                        WhiteQueen |= boardpos;
//                        break;
//                    case 'K':
//                        WhiteKing |= boardpos;
//                        break;
//                    case 'P':
//                        WhitePawn |= boardpos;
//                        break;
//                    default:
//                        return false;
//                }
//                // go to next column now that piece was placed in this col
//                col += 1;
//            }
//            // if for whatever reason this rank did not end in column 8 then
//            // we know this is a malformed rank.
//            if (col != 8)
//                return false;
//        }
//
//        return true;
//    }
//
//    private bool SetActiveColor(string activeColor) {
//        // active color should be w or b to designate whose turn it is
//        if (String.Equals(activeColor, "w", StringComparison.OrdinalIgnoreCase))
//            WhiteActive = true;
//        else if (String.Equals(activeColor, "b", StringComparison.OrdinalIgnoreCase))
//            WhiteActive = false;
//        else
//            return false;
//
//        return true;
//    }
//
//    private bool SetCastling(string castling) {
//        if (castling.Length != 4) {
//            WhiteKingside = false;
//            WhiteQueenside = false;
//            BlackKingside = false;
//            BlackQueenside = false;
//        } else {
//            WhiteKingside = (castling[0] == 'K');
//            WhiteQueenside = (castling[1] == 'Q');
//            BlackKingside = (castling[2] == 'k');
//            BlackQueenside = (castling[3] == 'q');
//        }
//
//        return true;
//    }
//
//    private bool SetEnPassantTarget(string epdata) {
//        if (String.Equals(epdata, "-")) {
//            EnPassantBit = 0;
//            return true;
//        } else {
//            int row = epdata[0] - 'a';
//            int col = epdata[1] - '0';
//
//            EnPassantBit = (1UL << ((row*8)+col));
//            
//            // if it's 0, then we had an invalid string
//            if (EnPassantBit == 0)
//                return false;
//        }
//
//        return true;
//    }
//
//    public string ToFenString() {
//        return "";
//    }
//
//    public override string ToString() {
//        StringBuilder board = new StringBuilder();
//
//        for (int square = 0; square < 64; square++) {
//            int row = square / 8;
//            int col = square % 8;
//
//            UInt64 bit = 1UL << square;
//            if ((BlackPawn & bit) != 0) board.Append('p');
//            else if ((BlackRook & bit) != 0) board.Append('r');
//            else if ((BlackKnight & bit) != 0) board.Append('n');
//            else if ((BlackBishop & bit) != 0) board.Append('b');
//            else if ((BlackQueen & bit) != 0) board.Append('q');
//            else if ((BlackKing & bit) != 0) board.Append('k');
//            else if ((WhitePawn & bit) != 0) board.Append('P');
//            else if ((WhiteRook & bit) != 0) board.Append('R');
//            else if ((WhiteKnight & bit) != 0) board.Append('N');
//            else if ((WhiteBishop & bit) != 0) board.Append('B');
//            else if ((WhiteQueen & bit) != 0) board.Append('Q');
//            else if ((WhiteKing & bit) != 0) board.Append('K');
//            else board.Append('.');  // Empty square
//
//            if (col == 7)
//                board.Append('\n');
//        }
//
//        return board.ToString();
//    }
//}
//