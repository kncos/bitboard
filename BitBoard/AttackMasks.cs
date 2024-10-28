
using System.Net;
using System.Numerics;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.AccessControl;

namespace Chess.Board.BitBoard
{
    static class AttackMasks
    {
        /// <summary>
        /// Traces a path along a bitboard, stopping before it encounters a friendly chess
        /// piece that is blocking the path, or stopping at an enemy piece that can be captured.
        /// DOES NOT include starting row/column in the returned mask, only the steps from 1.. onward
        /// </summary>
        /// <param name="friendly">bitmask representing locations of all friendly pieces on the bitboard</param>
        /// <param name="enemy">bitmask representing locations of all enemy pieces on the bitboard</param>
        /// <param name="r">row to start from (not included in the returned masks)</param>
        /// <param name="dr">row differential, i.e. how many rows to travel in each step</param>
        /// <param name="c">column to start from (not included in the returned masks)</param>
        /// <param name="dc">column differential, i.e. how many columns to travel in each step</param>
        /// <param name="num_steps">number of steps to take along this path</param>
        /// <returns>
        /// A tuple containing:
        /// - <c>validMoves</c>, all moves along the path the piece can travel to
        /// - <c>blockedBy</c>, friendly piece along the path that blocked it from going further, if there were any
        /// - <c>canAttack</c>, enemy piece along the path that this piece can capture.
        /// </returns>
        private static (ulong validMoves, ulong blockedBy, ulong canAttack) dirHelper(ulong friendly, ulong enemy, int r, int dr, int c, int dc, int num_steps)
        {
            ulong validMoves = 0UL, blockedBy = 0UL, canAttack = 0UL;
            for (int i = 0; i < num_steps; i++)
            {
                r += dr;
                c += dc;
                if (r < 0 || r > 7 || c < 0 || c > 7)
                    break;

                ulong pos = 1UL << (c + (r * 8));
                if ((pos & friendly) != 0) {
                    blockedBy |= pos;
                    break;
                } 

                validMoves |= pos;
                if ((pos & enemy) != 0) {
                    canAttack |= pos;
                    break;
                }
            }
            return (validMoves, blockedBy, canAttack);
        }

        /// <summary>
        /// Given a list of directions, returns a bitmask representing all valid moves
        /// a piece can make that travels in those directions, as well as what friendly
        /// pieces are blocking the path, and what enemy pieces can be captured.
        /// </summary>
        /// <param name="pos">bitmask position of the chess piece</param>
        /// <param name="friendly">bitmask position of all friendly pieces</param>
        /// <param name="enemy">bitmask position of all enemy pieces</param>
        /// <param name="dirs">
        /// an array of <c>(int,int,int)</c> tuples representing row differential,
        /// column differential, and number of steps to take. Basically, each direction
        /// the chess piece can go in, and how far.
        /// </param>
        /// <returns>
        /// A tuple contianing:
        /// - <c>validMoves</c> a bitmask representing all valid positions the piece can travel to
        /// - <c>blockedBy</c> a bitmask representing positions of all friendly pieces blocking the paths
        /// - <c>canAttack</c> a bitmask representing positions of all enemy pieces that can be captured
        /// </returns>
        private static (ulong validMoves, ulong blockedBy, ulong canAttack) CreateMask(ulong pos, ulong friendly, ulong enemy, (int row_dir, int col_dir, int steps)[] dirs)
        {
            var coord = BitBoardMasks.MaskToCoordinate(pos);
            if (!coord.HasValue)
                return (0UL, 0UL, 0UL);

            int row = coord.Value.row;
            int col = coord.Value.col;

            ulong validMoves = 0UL, blockedBy = 0UL, canAttack = 0UL;
            foreach (var d in dirs)
            {
                var thisdir = dirHelper(friendly, enemy, row, d.row_dir, col, d.col_dir, d.steps);
                validMoves |= thisdir.validMoves;
                blockedBy |= thisdir.blockedBy;
                canAttack |= thisdir.canAttack;
            }
            return (validMoves, blockedBy, canAttack);
        }

        public static (ulong validMoves, ulong blockedBy, ulong canAttack) RookMask(ulong rook_pos, ulong friendly_pieces, ulong enemy_pieces)
        {
            var directions = new (int dr, int dc, int steps)[] {
                (1, 0, 8),  // down
                (-1, 0, 8), // up
                (0, 1, 8),  // right
                (0, -1, 8)  // left
            };
            return CreateMask(rook_pos, friendly_pieces, enemy_pieces, directions);
        }

        public static (ulong validMoves, ulong blockedBy, ulong canAttack) BishopMask(ulong bishop_pos, ulong friendly_pieces, ulong enemy_pieces)
        {
            var directions = new (int, int, int)[] {
                (1, 1, 8),    // down right
                (-1, -1, 8),  // up left
                (1, -1, 8),   // down left
                (-1, 1, 8),   // up right
            };
            return CreateMask(bishop_pos, friendly_pieces, enemy_pieces, directions);
        }

        public static (ulong validMoves, ulong blockedBy, ulong canAttack) QueenMask(ulong queen_pos, ulong friendly_pieces, ulong enemy_pieces)
        {
            var directions = new (int, int, int)[] {
                (1, 1, 8),    // down right
                (-1, -1, 8),  // up left
                (1, -1, 8),   // down left
                (-1, 1, 8),   // up right
                (1, 0, 8),  // down
                (-1, 0, 8), // up
                (0, 1, 8),  // right
                (0, -1, 8)  // left
            };
            return CreateMask(queen_pos, friendly_pieces, enemy_pieces, directions);
        }

        public static (ulong validMoves, ulong blockedBy, ulong canAttack) KingMask(ulong king_pos, ulong friendly_pieces, ulong enemy_pieces)
        {
            var directions = new (int, int, int)[] {
                (1, 1, 1),    // down right
                (-1, -1, 1),  // up left
                (1, -1, 1),   // down left
                (-1, 1, 1),   // up right
                (1, 0, 1),  // down
                (-1, 0, 1), // up
                (0, 1, 1),  // right
                (0, -1, 1)  // left
            };
            return CreateMask(king_pos, friendly_pieces, enemy_pieces, directions);
        }

        public static (ulong validMoves, ulong blockedBy, ulong canAttack) KnightMask(ulong knight_pos, ulong friendly_pieces, ulong enemy_pieces)
        {
            var directions = new (int, int, int)[] {
                (2, 1, 1),   // down right
                (2, -1, 1),  // down left
                (-2, 1, 1),  // up right
                (-2, -1, 1), // up left
                (1, 2, 1),   // right down
                (-1, 2, 1),  // right up
                (1, -2, 1),  // left down
                (-1, -2, 1), // left up
            };
            return CreateMask(knight_pos, friendly_pieces, enemy_pieces, directions);
        }

        private static (ulong validMoves, ulong blockedBy, ulong canAttack) PawnHelper(ulong pos, ulong friendly, ulong enemy, ulong eptarget, bool white)
        {
            
            int step = 1;
            // white pawn on starting rank
            if (white && ((pos & (0xFFUL << 48)) != 0))
                step = 2;
            // black pawn on starting rank
            if (!white && ((pos & (0xFFUL << 8)) != 0))
                step = 2;

            // if white, we decrease the row by 1 or 2
            // if black, we increase the row by 1 or 2
            ulong onestep = white ? pos >> 8 : pos << 8;
            ulong twostep = white ? pos >> 16 : pos << 16;

            (ulong vm, ulong bb, ulong ca) masks;

            // friendly piece directly in front. can't move 
            if ((onestep & friendly) != 0)
                masks = (0UL, onestep, 0UL);
            // enemy piece directly in front. can capture
            else if ((onestep & enemy) != 0) 
                masks = (onestep, 0UL, onestep);
            // nothing occluding, but can only take one step.
            else if (step == 1) 
                masks = (onestep, 0UL, 0UL);
            // friendly piece blocking 2nd step
            else if ((twostep & friendly) != 0)
                masks = (onestep, twostep, 0UL); 
            // enemy piece can be captured on 2nd step
            else if ((twostep & enemy) != 0)
                masks = (onestep | twostep, 0UL, twostep);
            // can take 2 steps, nothing occluding
            else
                masks = (onestep | twostep, 0UL, 0UL); 

            // get coordinate of en passant target
            var ep_coord = BitBoardMasks.MaskToCoordinate(eptarget);
            // if there is none, simply return masks we already found
            if (!ep_coord.HasValue)
                return masks;

            // en passant target coords
            int ep_row = ep_coord.Value.row;
            int ep_col = ep_coord.Value.col;

            var pawn_coord = BitBoardMasks.MaskToCoordinate(pos);
            if (!pawn_coord.HasValue)
                // 10x engineer moment
                throw new NotImplementedException("this message should never appear"); 

            // pawn coords
            int pawn_row = pawn_coord.Value.row;
            int pawn_col = pawn_coord.Value.col;

            // for en passant, the pawn needs to be to the left or
            // to the right of the en passant target square by 1.
            if (Math.Abs(pawn_col - ep_col) != 1)
                return masks;

            // and it needs to be in the direction of advance, which
            // in this case, means ep_row should be greater than the
            // pawn's row by 1 (for black pawn).
            if (!white && (ep_row - pawn_row != -1))
                return masks;

            // for white, ep row needs to be 1 less than pawn row
            if (white && (pawn_row - ep_row != 1))
                return masks;

            // pawn can move to the valid square
            masks.vm |= eptarget;

            // which results in attacking this pawn which
            // is on the row above. This might get tricky later
            // since it's handled differently from all other logic
            masks.ca |= eptarget >> 8;

            return masks;
        }

        public static (ulong validMoves, ulong blockedBy, ulong canAttack) BlackPawnMask(ulong pawn_pos, ulong friendly_pieces, ulong enemy_pieces, ulong en_passant_target)
        {
            return PawnHelper(pawn_pos, friendly_pieces, enemy_pieces, en_passant_target, false);
        }

        public static (ulong validMoves, ulong blockedBy, ulong canAttack) WhitePawnMask(ulong pawn_pos, ulong friendly_pieces, ulong enemy_pieces, ulong en_passant_target)
        {
            return PawnHelper(pawn_pos, friendly_pieces, enemy_pieces, en_passant_target, true);
        }

        public static (ulong validMoves, ulong blockedBy, ulong canAttack) PieceTypeMask(PieceType type, ulong pos, ulong friendly_pieces, ulong enemy_pieces, ulong en_passant_target=0UL)
        {
            switch (type)
            {
                // handle rooks
                case PieceType.BlackRook:
                case PieceType.WhiteRook:
                    return RookMask(pos, friendly_pieces, enemy_pieces);
                // handle bishops
                case PieceType.BlackBishop:
                case PieceType.WhiteBishop:
                    return BishopMask(pos, friendly_pieces, enemy_pieces);
                // handle queens
                case PieceType.BlackQueen:
                case PieceType.WhiteQueen:
                    return QueenMask(pos, friendly_pieces, enemy_pieces);
                // handle kings
                case PieceType.BlackKing:
                case PieceType.WhiteKing:
                    return KingMask(pos, friendly_pieces, enemy_pieces);
                // handle knights
                case PieceType.BlackKnight:
                case PieceType.WhiteKnight:
                    return KnightMask(pos, friendly_pieces, enemy_pieces);
                // handle Black Pawns
                case PieceType.BlackPawn:
                    return BlackPawnMask(pos, friendly_pieces, enemy_pieces, en_passant_target);
                // handle White Pawns
                case PieceType.WhitePawn:
                    return WhitePawnMask(pos, friendly_pieces, enemy_pieces, en_passant_target);
                default:
                    throw new ArgumentException("Invalid piece type received to PieceTypeAttackMask?");
            }
        }

        private static (ulong valid, ulong blocking, ulong attacking) AttackMask(BitBoard bb, bool whiteActive)
        {
            // store the result here
            (ulong v, ulong b, ulong a) res = (0UL, 0UL, 0UL); 
            
            // determine friendly/enemy pieces based on whether
            // we're calculating white's mask or black's mask
            ulong friendly = whiteActive ? bb.Pieces.WhitePositionsMask() : bb.Pieces.BlackPositionsMask();
            ulong enemy = whiteActive ? bb.Pieces.BlackPositionsMask() : bb.Pieces.WhitePositionsMask();

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
                var type_at = bb.Pieces.PieceTypeAtCoordinate(pos);
                if (!type_at.HasValue)
                    continue;

                // combine mask for this piece into the total masks 
                var masks = PieceTypeMask(type_at.Value, pos, friendly, enemy, bb.State.EnPassantTarget);
                res.v |= masks.validMoves;
                res.b |= masks.blockedBy;
                res.a |= masks.canAttack;
            }

            return res;
        }

        public static (ulong validMoves, ulong blockedBy, ulong canAttack) WhiteAttackMask(this BitBoard bb) => AttackMask(bb, true);
        public static (ulong validMoves, ulong blockedBy, ulong canAttack) BlackAttackMask(this BitBoard bb) => AttackMask(bb, false);

    }
}