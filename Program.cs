
using System.Text;
using Chess.Board.BitBoard;

string start_pos = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
string ep_pos1 = "rnbqkbnr/1ppppppp/8/p3P3/8/8/PPPP1PPP/RNBQKBNR w KQkq - 0 1";
string ep_pos2 = "rnbqkbnr/1pppp1pp/p4P2/8/5pP1/8/PPPP1P1P/RNBQKBNR w KQkq g3 0 1";

BitBoard bb = FenStringHelper.ParseFenString(start_pos);
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

    if (tokens.Length == 1 && (tokens[0].Length == 4 || tokens[0].Length == 5)) {
        ulong? startMask = BitBoardMasks.AlgebraicNotationToMask(tokens[0][0..2]);
        ulong? endMask = BitBoardMasks.AlgebraicNotationToMask(tokens[0][2..4]);
        if (!startMask.HasValue || !endMask.HasValue)
            continue;

        var sc = BitBoardMasks.MaskToCoordinate(startMask.Value);
        var ec = BitBoardMasks.MaskToCoordinate(endMask.Value);

        if (sc.HasValue && ec.HasValue)
            bb.Move(sc.Value.row, sc.Value.col, ec.Value.row, ec.Value.col);    
        
    } else if (tokens.Length > 1 && (String.Equals(tokens[0], "fen"))) {
        bb = FenStringHelper.ParseFenString(String.Join(" ", tokens[1..]));
    }

    Console.WriteLine(bb.Pieces);
    Console.WriteLine(bb.ToFenString());
 
}