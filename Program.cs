using CAS.Algebra;

namespace CAS;

class Program
{
    static void Main(string[] args)
    {
        // Polynomial p1 = new Polynomial(new Term(3, 2), new Term(-10, 1), new Term(3, 0));
        Polynomial p2 = Polynomial.Random(degree:2, maxCoeff:10);
        // Console.WriteLine(p1);
        Console.WriteLine(p2);
        int prime = 17;
        (Polynomial, int)[] factors = Factor.CantorZassenhausFactor(p2, prime);
        Console.WriteLine($"({String.Join("), (", factors.Select(pm => pm.Item2 != 1 ? $"{pm.Item1}: {pm.Item2}": pm.Item1.ToString()).ToArray())})");
        Console.WriteLine(Polynomial.Modulo(Factor.ExpandFactorization(factors, prime), prime));
        
        // string? test = Console.ReadLine();
        // if (test == null)
        // {
        //     Console.WriteLine("Failed to read line!");
        //     return;
        // }
        //
        // List<Lexer.Token> tokens = Lexer.Tokenize(test);
        // Console.WriteLine(String.Join(", ",tokens.Select(token => token.TokenType.ToString())));
    }
    static void PrintGCD(int a, int b)
    {
        Console.WriteLine($"gcd({a}, {b}) => {Common.GCD(a, b)}");
    }

    static void PrintGCDX(int a, int b)
    {
        int g, b1, b2;
        (g, b1, b2) = Common.GCDX(a, b);
        Console.WriteLine($"gcdx({a}, {b}) => ({g}, {b1}, {b2})");
    }
}