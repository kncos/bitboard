
using System.Numerics;

int inc = 5;
int x = 0;
while (true)
{
    x += inc;
    Console.WriteLine($"the number is: {x}");
    Thread.Sleep(500);
}
