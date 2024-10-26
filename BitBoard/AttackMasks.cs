
using System.Numerics;
using System.Reflection.Emit;
using System.Security.AccessControl;

namespace Chess.Board.BitBoard
{
    static class AttackMasks
    {
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
            if (white && (pos & 0xFF << 48) != 0)
                step = 2;
            // black pawn on starting rank
            if (!white && (pos & 0xFF << 8) != 0)
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
            if (!white && (ep_row - pawn_row != 1))
                return masks;

            // for white, ep row needs to be 1 less than pawn row
            if (white && (pawn_row - ep_row != -1))
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
    }
}