
using System.Runtime.InteropServices;

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
        private bool TryBitboardMove(ulong startpos, ulong endpos, PieceType startType, PieceType? endType, char promotion)
        {
            AddBitboardMove(startpos, endpos, startType, endType);
            // ensure move doesn't put king in check
            if (KingInCheck()) {
                RemoveBitboardMove(startpos, endpos, startType, endType);
                return false;
            }

            // handle pawn promotion
            if (startType == PieceType.BlackPawn || startType == PieceType.WhitePawn)
                if (!HandlePromotion(endpos, promotion)) {
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
        private bool HandlePromotion(ulong endpos, char promotion) {
            // get type to promote to
            PieceType? pawn_promotion = promotion switch {
                'q' => State.WhiteActive ? PieceType.WhiteQueen : PieceType.BlackQueen,
                'b' => State.WhiteActive ? PieceType.WhiteBishop : PieceType.BlackBishop,
                'r' => State.WhiteActive ? PieceType.WhiteRook : PieceType.BlackRook,
                'n' => State.WhiteActive ? PieceType.WhiteKnight : PieceType.BlackKnight,
                _ => null,
            };

            // handle pawn promotion
            if (pawn_promotion.HasValue) 
            {
                ref ulong component = ref (State.WhiteActive ? ref Pieces.WhitePawn : ref Pieces.BlackPawn);
                BitBoardMasks.UnsetCoordinate(endpos, ref component);
                BitBoardMasks.SetCoordinate(endpos, ref Pieces.FromPieceType(pawn_promotion.Value));
                return true;
            } 

            // if we're a pawn that should be promoted, but the promotion did not occur,
            // then this is an invalid move.
            if (State.WhiteActive && ((endpos & 0xFFUL) != 0)) 
                return false;
            if (!State.WhiteActive && ((endpos & (0xFFUL << 56)) != 0)) 
                return false;

            return true;
        }
        public bool Move(ulong startpos, ulong endpos, char promotion='\0')
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
            if(!TryBitboardMove(startpos, endpos, startType, endType, promotion))
                return false;

            // update state variables
            UpdateState(startpos, endpos, startType, endType);
            return true;
        }

        public List<(ulong startpos, ulong endpos, char promotion)> GetValidMoves() 
        {
            var res = new List<(ulong, ulong, char)>();

            return res;
        }

        public bool Move(int start_row, int start_col, int end_row, int end_col, char promotion='\0')
        {
            // convert r,c coordinates to masks
            ulong startpos = BitBoardMasks.CoordinateToMask(start_row, start_col) ?? 0UL;
            ulong endpos = BitBoardMasks.CoordinateToMask(end_row, end_col) ?? 0UL;
            return Move(startpos, endpos, promotion);
        }
    }
}