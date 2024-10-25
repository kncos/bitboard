
using Chess.Board.BitBoard;

BitBoard bb = FenStringHelper.ParseFenString("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
Console.WriteLine(bb.Pieces);

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