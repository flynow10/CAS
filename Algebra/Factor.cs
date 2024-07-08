namespace CAS.Algebra;

public class Factor
{
    public static (Polynomial, int)[] CantorZassenhausFactor(Polynomial f, int prime)
    {
        Polynomial fModP = Polynomial.Modulo(f, prime);
        if (fModP.Degree() <= 1) return [(fModP, 1)];

        Polynomial ff = fModP.PrimPart(prime);

        Polynomial squarePoly = Polynomial.GCD(f, ff.Derivative(), prime);
        ff = (ff / squarePoly)(prime);

        int oldCoeff = ff.Leading().Coeff;
        ff = (ff / oldCoeff)(prime);

        Polynomial[] dds = DDFactor(ff, prime);

        List<(Polynomial, int)> returnValue = new List<(Polynomial, int)>();
        
        for (var i = 1; i <= dds.Length; i++)
        {
            Polynomial dd = dds[i - 1];
            Polynomial[] sp = DDSplit(dd, i, prime);
            sp = sp.Select(p => (p / p.Leading().Coeff)(prime)).ToArray();
            foreach (Polynomial mp in sp)
            {
                returnValue.Add((mp, Multiplicity(fModP, mp, prime)));
            }
        }
        returnValue.Add((fModP.Leading().Coeff * Polynomial.One(), 1));

        return returnValue.ToArray();
    }

    public static Polynomial ExpandFactorization((Polynomial, int)[] factors, int prime)
    {
        return factors.Aggregate(Polynomial.One(), (poly, factor) => poly * Polynomial.Pow(factor.Item1, factor.Item2));
    }

    public static int Multiplicity(Polynomial f, Polynomial g, int prime)
    {
        if (Polynomial.Degree(Polynomial.GCD(f, g, prime)) == 0) return 0;
        return 1 + Multiplicity((f / g)(prime), g, prime);
    }
    
    public static Polynomial[] DDFactor(Polynomial f, int prime)
    {
        Polynomial w = Polynomial.X();

        Polynomial[] g = new Polynomial[f.Degree()];
        for(int i = 0; i < g.Length; i++)
        {
            w = Polynomial.Remainder(Polynomial.PowMod(w, prime, prime), f)(prime);
            g[i] = Polynomial.GCD(w - Polynomial.X(), f, prime);
            f = (f / g[i])(prime);
        }

        if (!(f.Terms is [{ Coeff: 1, Degree: 0 }]))
        {
            return [..g, f];
        }

        return g;
    }

    public static Polynomial[] DDSplit(Polynomial f, int d, int prime)
    {
        f = Polynomial.Modulo(f, prime);
        if (f.Degree() == d)
        {
            return [f];
        }
    
        if (f.Degree() == 0)
        {
            return [];
        }

        Polynomial w = Polynomial.Random(degree: d, monic: true);
        w = Polynomial.Modulo(w, prime);
        int nPower = (Common.Pow(prime, d) - 1) / 2;
        Polynomial g = Polynomial.GCD(Polynomial.PowMod(w, nPower, prime) - Polynomial.One(), f, prime);
        Polynomial gPrime = (f / g)(prime);
        return [..DDSplit(g, d, prime), ..DDSplit(gPrime, d, prime)];
    }
}