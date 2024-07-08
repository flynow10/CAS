namespace CAS.Algebra;

public readonly struct Polynomial
{
    public Term[] Terms { get; }

    public Polynomial() : this([Term.Zero()])
    {
    }
    
    public Polynomial(params Term[] terms)
    {
        terms = terms.Where(t => !Term.IsZero(t)).ToArray();
        if (terms.Length == 0)
        {
            terms = [Term.Zero()];
        }

        Array.Sort(terms, (a, b) => b.Degree.CompareTo(a.Degree));
        Terms = terms;
    }

    public static Polynomial operator +(Polynomial p, int n)
    {
        return p + new Term(n, 0);
    }

    public static Polynomial operator +(int n, Polynomial p)
    {
        return p + new Term(n, 0);
    }

    public static Polynomial operator +(Term t, Polynomial p)
    {
        return p + t;
    }

    public static Polynomial operator +(Polynomial p, Term t)
    {
        if (t.Degree > Degree(p))
        {
            return new Polynomial([t, ..p.Terms]);
        }

        List<Term> terms = new List<Term>();
        bool hasAdded = false;
        for (var i = 0; i < p.Terms.Length; i++)
        {
            Term currentTerm = p.Terms[i];
            if (!hasAdded && currentTerm.Degree <= t.Degree)
            {
                int coeff = currentTerm.Coeff;
                if (currentTerm.Degree == t.Degree)
                {
                    coeff += t.Coeff;
                }
                else
                {
                    terms.Add(t);
                }
                terms.Add(new Term(coeff, currentTerm.Degree));
                hasAdded = true;
            }
            else
            {
                terms.Add(currentTerm);
            }
        }

        if (!hasAdded)
        {
            terms.Add(t);
        }

        return new Polynomial(terms.ToArray());
    }

    public static Polynomial operator +(Polynomial p1, Polynomial p2)
    {
        Polynomial p = p1;
        foreach (Term term in p2.Terms)
        {
            p += term;
        }

        return p;
    }

    public static Polynomial operator -(Polynomial p)
    {
        return new Polynomial(p.Terms.Select(t => -t).ToArray());
    }

    public static Polynomial operator -(Polynomial p1, Polynomial p2)
    {
        return p1 + -p2;
    }

    public static Polynomial operator -(Polynomial p, Term t)
    {
        return p + -t;
    }

    public static Polynomial operator -(Term t, Polynomial p)
    {
        return t + -p;
    }

    public static Polynomial operator *(int n, Polynomial p)
    {
        return p * new Term(n, 0);
    }

    public static Polynomial operator *(Polynomial p, int n)
    {
        return n * p;
    }

    public static Polynomial operator *(Term t, Polynomial p)
    {
        return Term.IsZero(t) ? new Polynomial() : new Polynomial(p.Terms.Select(pt => t * pt).ToArray());
    }

    public static Polynomial operator *(Polynomial p, Term t)
    {
        return t * p;
    }

    public static Polynomial operator *(Polynomial p1, Polynomial p2)
    {
        Polynomial p = new Polynomial();
        foreach (Term t in p1.Terms)
        {
            p += t * p2;
        }

        return p;
    }
    
    public static Polynomial Modulo(Polynomial f, int p)
    {
        List<Term> terms = new ();
        foreach (Term term in f.Terms)
        {
            terms.Add(term % p);
        }

        return new Polynomial(terms.ToArray());
    }

    public static Polynomial PowMod(Polynomial p, int exp, int prime)
    {
        if (exp < 0)
        {
            throw new Exception("No negative power!");
        }
        
        Polynomial result = One();
        while (exp > 0)
        {
            if (exp % 2 == 1)
                result = Modulo(p * result, prime);
            exp >>= 1;
            p = Modulo(p * p, prime);
        }

        return result;
    }

    public static Func<int, (Polynomial, Polynomial)> Divide(Polynomial num, Polynomial den)
    {
        return prime =>
        {
            Polynomial f = Modulo(num, prime), g = Modulo(den, prime);
            if (Degree(f) < Degree(num))
            {
                throw new Exception("Coeff of leading term was a multiple of the chosen prime number");
            }

            if (IsZero(g))
            {
                throw new DivideByZeroException("Cannot divide polynomial by zero");
            }

            Polynomial q = new Polynomial();
            int prevDegree = Degree(f);
            while (Degree(f) >= Degree(g))
            {
                Polynomial h = new Polynomial((Leading(f) / Leading(g))(prime));
                f = Modulo(f - h * g, prime);
                q = Modulo(q + h, prime);
                if (prevDegree == Degree(f))
                {
                    break;
                }

                prevDegree = Degree(f);
            }

            if (!IsZero(Modulo(num - (q * g + f), prime)))
            {
                throw new Exception("");
            }

            return (q, f);
        };
    }

    public static Func<int, Polynomial> operator /(Polynomial p, int n)
    {
        return prime => new Polynomial(p.Terms.Select(pt => ((pt / n)(prime))).ToArray());
    }

    public static Func<int, Polynomial> operator /(Polynomial num, Polynomial dem)
    {
        return prime => Divide(num, dem)(prime).Item1;
    }

    public static Polynomial Pow(Polynomial p, int n)
    {
        Polynomial result = One();
        for (int i = 0; i < n; i++)
        {
            result *= p;
        }

        return result;
    }

    public static Func<int, Polynomial> Remainder(Polynomial num, Polynomial dem)
    {
        return prime => Divide(num, dem)(prime).Item2;
    }
    public static Polynomial X()
    {
        return new Polynomial(new Term(1, 1));
    }

    public static Polynomial Cyclotonic(int p)
    {
        return new Polynomial(new Term(1, p), new Term(-1, 1));
    }

    public static Polynomial LinearMonic(int n)
    {
        return new Polynomial(new Term(1, 1), new Term(n, 0));
    }

    public static Polynomial Zero()
    {
        return new Polynomial();
    }

    public static Polynomial One()
    {
        return new Polynomial(Term.One());
    }

    public static Polynomial Random(int degree = -1, int terms = -1, int maxCoeff = 100, double meanDegree = 5.0,
        double probTerm = 0.7, bool monic = false, Func<Polynomial, bool>? condition = null)
    {
        Random random = new Random();
        while (true)
        {
            int _degree = degree == -1 ? Common.RandomPoisson(meanDegree, random) : degree;
            int _terms = terms == -1 ? Common.RandomBinomial(_degree, probTerm, random) : terms;
            int[] degrees = Enumerable.Repeat(-1, _terms + 1).ToArray();
            int[] range = Enumerable.Range(0, _degree).ToArray();
            for (int i = 0; i < _terms; i++)
            {
                range = range.Where(i => !degrees.Contains(i)).ToArray();
                if (range.Length == 0)
                {
                    Console.WriteLine(_degree);
                    Console.WriteLine(_terms);
                    Console.WriteLine(String.Join(", ", degrees));
                }
                int rangeIdx = random.Next(0, range.Length);
                int term = range.ElementAt(rangeIdx);
                degrees[i] = term;
            }

            degrees[^1] = _degree;
            int[] coeffs = new int[_terms + 1];
            for (int i = 0; i < _terms + 1; i++)
            {
                coeffs[i] = random.Next(0, maxCoeff);
            }

            if (monic)
            {
                coeffs[^1] = 1;
            }

            Polynomial p = new Polynomial(degrees.Select((d, i) => new Term(coeffs[i], d)).ToArray());
            if (condition == null || condition(p))
            {
                return p;
            }
        }
    }

    public static int Length(Polynomial p)
    {
        return p.Length();
    }

    public int Length()
    {
        return Terms.Length;
    }

    public static Term Leading(Polynomial p)
    {
        return p.Leading();
    }

    public Term Leading()
    {
        return Length() == 0 ? Term.Zero() : Terms[0];
    }

    public static int[] Coeffs(Polynomial p)
    {
        return p.Coeffs();
    }

    public int[] Coeffs()
    {
        return Terms.Select(t => t.Coeff).ToArray();
    }
    
    public static int Degree(Polynomial p)
    {
        return p.Degree();
    }

    public int Degree()
    {
        return Leading().Degree;
    }

    public static int Content(Polynomial p)
    {
        return p.Content();
    }

    public int Content()
    {
        return Common.GCD(Coeffs());
    }

    public static int Evaluate(Polynomial p, int x)
    {
        return p.Evaluate(x);
    }

    public int Evaluate(int x)
    {
        return Terms.Select(t => Term.Evaluate(t, x)).Sum();
    }
    
    public static Func<int, Polynomial> PrimPart(Polynomial p)
    {
        return prime => p.PrimPart(prime);
    }

    public Polynomial PrimPart(int prime)
    {
        return (this / Content())(prime);
    }

    public static Polynomial Derivative(Polynomial p)
    {
        return p.Derivative();
    }

    public Polynomial Derivative()
    {
        return new Polynomial(Terms.Select(Term.Derivative).ToArray());
    }
    
    public static (Polynomial, Polynomial, Polynomial) GCDX(Polynomial a, Polynomial b, int prime)
    {
        Polynomial oldR = Modulo(a, prime), r = Modulo(b, prime);
        Polynomial oldS = One(), s = Zero();
        Polynomial oldT = Zero(), t = One();
    
        while (!IsZero(Modulo(r, prime)))
        {
            Polynomial q = (oldR / r)(prime);
            (oldR, r) = (r, Modulo(oldR - q * r, prime));
            (oldS, s) = (s, Modulo(oldS - q * s, prime));
            (oldT, t) = (t, Modulo(oldT - q * t, prime));
        }

        return (oldR, oldS, oldR);
    }

    public static Polynomial GCD(Polynomial a, Polynomial b, int prime)
    {
        return GCDX(a, b, prime).Item1;
    }
    
    public static bool IsZero(Polynomial p)
    {
        return p.Terms.Length == 1 && Term.IsZero(p.Terms[0]);
    }
    
    public override string ToString()
    {
        return String.Join(" + ", Terms);
    }
}