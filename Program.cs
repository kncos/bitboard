
using System.Text;
using Chess.Board.BitBoard;

string start_pos = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
string ep_pos1 = "rnbqkbnr/1ppppppp/8/p3P3/8/8/PPPP1PPP/RNBQKBNR w KQkq - 0 1";

BitBoard bb = FenStringHelper.ParseFenString(ep_pos1);
Console.WriteLine(bb.Pieces);

void printBitgrid(ulong grid) {
    StringBuilder sb = new StringBuilder();
    for (int i = 0; i < 64; i++)
    {
        if ((grid >> i) % 2 == 1)
            sb.Append("[1] ");
        else
            sb.Append("[ ] ");

        if ((i+1) % 8 == 0)
            sb.Append('\n');
    }
    Console.WriteLine(sb.ToString());
}


//ulong black_pawn = BitBoardMasks.AlgebraicNotationToMask("b4").Value;
//ulong white_pawn = BitBoardMasks.AlgebraicNotationTo:w
//Mask("a4").Value;
//ulong ep_target = BitBoardMasks.AlgebraicNotationToMask("a3").Value;
//var masks = AttackMasks.BlackPawnMask(black_pawn, 0UL, white_pawn, ep_target);
//Console.WriteLine("attack squares");
//printBitgrid(masks.validMoves);
//Console.WriteLine("can attack");
//printBitgrid(masks.canAttack);


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
    printBitgrid(bb.WhiteAttackMask().validMoves);
}