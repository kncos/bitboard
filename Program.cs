
using System.Text;
using Chess.Board.BitBoard;

BitBoard bb = FenStringHelper.ParseFenString("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
Console.WriteLine(bb.Pieces);

void printBitgrid(ulong grid) {
    StringBuilder sb = new StringBuilder();
    for (int i = 0; i < 64; i++)
    {
        if ((grid >> i) % 2 == 1)
            sb.Append("1");
        else
            sb.Append("0");

        if ((i+1) % 8 == 0)
            sb.Append('\n');
    }
    Console.WriteLine(sb.ToString());
}

//printBitgrid(AttackMasks.RookMask(1UL << 47, 1UL << 44, 1UL << 15).validMoves);
//printBitgrid(AttackMasks.RookMask(1UL << 47, 1UL << 44, 1UL << 15).blockedBy);
//printBitgrid(AttackMasks.RookMask(1UL << 47, 1UL << 44, 1UL << 15).canAttack);
//printBitgrid(AttackMasks.BishopMask(1UL << 36, 0UL, 0UL).validMoves);
Console.WriteLine("Knight mask");
printBitgrid(AttackMasks.KnightMask(1UL << 36, 0UL, 0UL).validMoves);
Console.WriteLine("King mask");
printBitgrid(AttackMasks.KingMask(1UL << 36, 0UL, 0UL).validMoves);
Console.WriteLine("Queen mask");
printBitgrid(AttackMasks.QueenMask(1UL << 36, 0UL, 0UL).validMoves);
Console.WriteLine("Black Pawn (not starting rank) mask");
printBitgrid(AttackMasks.BlackPawnMask(1UL << 36, 0UL, 0UL, 0UL).validMoves);
Console.WriteLine("White Pawn (not starting rank) mask");
printBitgrid(AttackMasks.WhitePawnMask(1UL << 36, 0UL, 0UL, 0UL).validMoves);
Console.WriteLine("Black Pawn (starting rank) mask");
printBitgrid(AttackMasks.BlackPawnMask(1UL << 8, 0UL, 0UL, 0UL).validMoves);
Console.WriteLine("White Pawn (starting rank) mask");
printBitgrid(AttackMasks.WhitePawnMask(1UL << 48, 0UL, 0UL, 0UL).validMoves);


while (true)
{
    string input = Console.ReadLine();
    if (String.Equals(input, "q"))
        break;

    string[] tokens = input.Split(' ');
    if (tokens.Length != 2)
        continue;

    ulong? startMask = BitBoardMasks.AlgebraicNotationToMask(tokens[0]);
    ulong? endMask = BitBoardMasks.AlgebraicNotationToMask(tokens[1]);
    if (!startMask.HasValue || !endMask.HasValue)
        continue;

    var sc = BitBoardMasks.MaskToCoordinate(startMask.Value);
    var ec = BitBoardMasks.MaskToCoordinate(endMask.Value);

    if (sc.HasValue && ec.HasValue)
        bb.Move(sc.Value.row, sc.Value.col, ec.Value.row, ec.Value.col);

    Console.WriteLine(bb.Pieces);
    Console.WriteLine(bb.ToFenString());
}