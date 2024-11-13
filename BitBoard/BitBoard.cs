
namespace Chess.Board.BitBoard
{
    class BitBoard
    {
        public BitBoardPieces Pieces { get; private set; } = new();
        public BitBoardState State { get; private set; } = new();
        public BitBoard() {}
        public BitBoard(BitBoardPieces pieces, BitBoardState state)
        {
            Pieces = pieces;
            State = state;
        }
        private (ulong valid, ulong blocking, ulong attacking) AttackMask(ulong friendly, ulong enemy)
        {
            // store the result here
            (ulong v, ulong b, ulong a) res = (0UL, 0UL, 0UL); 

            // go through each of the board's 64 squares
            for (int i = 0; i < 64; i++)
            {
                // position in the bitboard. 
                ulong pos = 1UL << i;

                // only consider friendly pieces. Can skip empty
                // squares and enemy pieces for this logic.
                if ((pos & friendly) == 0)
                    continue;

                // this should never be null in the context of this method,
                // if it is there is a bug in this method or the bitboard method
                var type_at = Pieces.PieceTypeAtCoordinate(pos);
                if (!type_at.HasValue)
                    continue;

                // combine mask for this piece into the total masks 
                var masks = AttackMasks.PieceTypeMask(type_at.Value, pos, friendly, enemy, State.EnPassantTarget);
                res.v |= masks.validMoves;
                res.b |= masks.blockedBy;
                res.a |= masks.canAttack;
            }

            return res;
        }
        private (ulong friendly, ulong enemy) GetFriendlyAndEnemyPositions()
        {
            ulong friendly = State.WhiteActive ? BitBoardMasks.WhitePositionsMask(Pieces) : BitBoardMasks.BlackPositionsMask(Pieces);
            ulong enemy = State.WhiteActive ? BitBoardMasks.BlackPositionsMask(Pieces) : BitBoardMasks.WhitePositionsMask(Pieces);
            return (friendly, enemy);
        }
        private bool KingInCheck() 
        {
            (ulong friendly, ulong enemy) = GetFriendlyAndEnemyPositions();

            // we want to treat the enemy as friendly and ourselves as the enemy,
            // so that we can see what is being attacked from the enemy's perspective
            var fullEnemyAttackMask = AttackMask(enemy, friendly);

            for (int i = 0; i < 64; i++) {
                ulong pos = 1UL << i;
                // only want to consider the pieces that the enemy is attacking
                if ((pos & fullEnemyAttackMask.attacking) == 0)
                    continue;

                // we can do this because `attacking` from the enemy's perspective is
                // guaranteed to only include friendly pieces. So regardless of whether
                // it's a black king or a white king, as long as it's in the enemy's attack
                // vector, we know it's the friendly king and can return true that the king
                // is under attack.
                var type_at = Pieces.PieceTypeAtCoordinate(pos);
                if (type_at is PieceType.BlackKing || type_at is PieceType.WhiteKing)
                    return true;
            }

            // if none of the enemy pieces were attacking the king, then it's not in check.
            return false;
        }
        private void AddBitboardMove(ulong startpos, ulong endpos, PieceType startType, PieceType? endType) 
        {
            BitBoardMasks.UnsetCoordinate(startpos, ref Pieces.FromPieceType(startType));
            BitBoardMasks.SetCoordinate(endpos, ref Pieces.FromPieceType(startType));
            if (endType.HasValue)
                BitBoardMasks.UnsetCoordinate(endpos, ref Pieces.FromPieceType(endType.Value));
            if ((endpos & State.EnPassantTarget) != 0) {
                if (State.WhiteActive)
                    BitBoardMasks.UnsetCoordinate(endpos << 8, ref Pieces.FromPieceType(PieceType.BlackPawn));
                else
                    BitBoardMasks.UnsetCoordinate(endpos >> 8, ref Pieces.FromPieceType(PieceType.WhitePawn));
            }
        }
        private void RemoveBitboardMove(ulong startpos, ulong endpos, PieceType startType, PieceType? endType)
        {
            BitBoardMasks.SetCoordinate(startpos, ref Pieces.FromPieceType(startType));
            BitBoardMasks.UnsetCoordinate(endpos, ref Pieces.FromPieceType(startType));
            if (endType.HasValue)
                BitBoardMasks.SetCoordinate(endpos, ref Pieces.FromPieceType(endType.Value));

            if ((endpos & State.EnPassantTarget) != 0) {
                if (State.WhiteActive)
                    BitBoardMasks.SetCoordinate(endpos << 8, ref Pieces.FromPieceType(PieceType.BlackPawn));
                else
                    BitBoardMasks.SetCoordinate(endpos >> 8, ref Pieces.FromPieceType(PieceType.WhitePawn));
            }
        }
        private bool TryBitboardMove(ulong startpos, ulong endpos, PieceType startType, PieceType? endType)
        {
            AddBitboardMove(startpos, endpos, startType, endType);
            if (KingInCheck()) {
                RemoveBitboardMove(startpos, endpos, startType, endType);
                return false;
            }
            return true;
        }
        private void UpdateState(ulong startpos, ulong endpos, PieceType startType, PieceType? endType)
        {
            State.EnPassantTarget = 0UL; // reset to 0 since we already got masks.
            if ((startType == PieceType.BlackPawn) && ((endpos >> 16) == startpos))
                State.EnPassantTarget = endpos >> 8; // 1 row above
            else if ((startType == PieceType.WhitePawn) && ((endpos << 16) == startpos))
                State.EnPassantTarget = endpos << 8; // 1 row below

            // update clocks
            if (!startType.IsWhite())
                State.FullmoveCount += 1;
            
            // handle capture and reset halfMove clock if capture occurred or pawn moved
            State.HalfmoveClock += 1; 
            if (endType.HasValue || startType == PieceType.BlackPawn || startType == PieceType.WhitePawn) 
                State.HalfmoveClock = 0;

            // update whose turn it is 
            State.WhiteActive = !State.WhiteActive;
        }
        private bool Move(ulong startpos, ulong endpos)
        {
            // ensure there is a piece to move at the starting square
            if (Pieces.PieceTypeAtCoordinate(startpos) is not PieceType startType)
                return false;

            // ensure that the correct player is making the move
            if (startType.IsWhite() != State.WhiteActive)
                return false;

            (ulong friendly, ulong enemy) = GetFriendlyAndEnemyPositions();

            // ensure that this is a valid move (pseudolegal)
            var masks = AttackMasks.PieceTypeMask(startType, startpos, friendly, enemy, State.EnPassantTarget);
            if ((endpos & masks.validMoves) == 0)
                return false; 

            // assign new piece places
            PieceType? endType = Pieces.PieceTypeAtCoordinate(endpos);
            if(!TryBitboardMove(startpos, endpos, startType, endType))
                return false;

            UpdateState(startpos, endpos, startType, endType);

            return true;
        }
        public bool Move(int start_row, int start_col, int end_row, int end_col)
        {
            // convert r,c coordinates to masks
            ulong startpos = BitBoardMasks.CoordinateToMask(start_row, start_col) ?? 0UL;
            ulong endpos = BitBoardMasks.CoordinateToMask(end_row, end_col) ?? 0UL;
            return Move(startpos, endpos);
        }
    }
}