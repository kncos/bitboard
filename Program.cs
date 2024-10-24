
using Chess.Board.BitBoard;

BitBoard bb = FenStringHelper.ParseFenString("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
Console.WriteLine(bb.Pieces);
Console.WriteLine(FenStringHelper.ToFenString(bb));

Console.WriteLine(new BitBoard().Pieces);
Console.WriteLine(FenStringHelper.ToFenString(new BitBoard()));