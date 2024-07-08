namespace CAS.Algebra;

public readonly struct Term
{
    public int Coeff { get; }
    public int Degree { get; }

    public Term(int coeff, int degree)
    {
        if (degree < 0)
        {
            throw new Exception("Degree must be non-negative!");
        }
        Coeff = coeff;
        Degree = coeff != 0 ? degree : 0;
    }

    public static Term operator +(Term t1, Term t2)
    {
        if (t1.Degree != t2.Degree)
        {
            throw new Exception("Cannot add two terms with different degrees!");
        }

        return new Term(t1.Coeff + t2.Coeff, t1.Degree);
    }

    public static Term operator -(Term t)
    {
        return new Term(-t.Coeff, t.Degree);
    }

    public static Term operator -(Term t1, Term t2)
    {
        return t1 + -t2;
    }
    
    public static Term operator *(Term t1, Term t2)
    {
        return new Term(t1.Coeff * t2.Coeff, t1.Degree + t2.Degree);
    }

    public static Func<int, Term> operator /(Term t1, Term t2)
    {
        if (t1.Degree < t2.Degree)
        {
            throw new Exception("Cannot divide by a term of greater degree than the numerator");
        }

        return prime =>
        {
            return new Term(Common.Mod(t1.Coeff * Common.InverseMod(t2.Coeff, prime), prime), t1.Degree - t2.Degree);
        };
    }

    public static Func<int, Term> operator /(Term t, int n)
    {
        return t / new Term(n, 0);
    }

    public static Term operator %(Term t, int n)
    {
        return new Term(Common.Mod(t.Coeff, n), t.Degree);
    }

    public static Term Derivative(Term t)
    {
        return new Term(t.Coeff * t.Degree, int.Max(t.Degree - 1, 0));
    }

    public static int Evaluate(Term t, int x)
    {
        return t.Coeff * Common.Pow(x, t.Degree);
    }

    public static bool IsZero(Term term)
    {
        return term.Coeff == 0;
    }

    public static Term Zero()
    {
        return new Term(0, 0);
    }

    public static Term One()
    {
        return new Term(1, 0);
    }

    public override string ToString()
    {
        if (Degree == 0)
        {
            return Coeff.ToString();
        }
        string coeff = Coeff != 1 ? $"{Coeff}*" : "";
        string degree = Degree != 1 ? $"^{Degree}" : "";
        return $"{coeff}x{degree}";
    }
}