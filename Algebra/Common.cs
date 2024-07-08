namespace CAS.Algebra;

public class Common
{

    public static int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    public static int Pow(int num, int exp)
    {
        int result = 1;
        while (exp > 0)
        {
            if (exp % 2 == 1)
                result *= num;
            exp >>= 1;
            num *= num;
        }

        return result;
    }

    public static int PowMod(int num, int exp, int m)
    {
        int result = 1;
        while (exp > 0)
        {
            if (exp % 2 == 1)
                result = Mod(result * num, m);
            exp >>= 1;
            num = Mod(num * num, m);
        }

        return Mod(result, m);
    }
    public static int GCD(int a, int b)
    {
        while (b != 0)
        {
            (a, b) = (b, Mod(a, b));
        }

        return a;
    }

    public static int GCD(params int[] nums)
    {
        return nums.Aggregate(0, (acc, ele) => GCD(acc, ele));
    }

    public static (int, int, int) GCDX(int a, int b)
    {
        int s = 0, oldS = 1, t = 1, oldT = 0, r = b, oldR = a;
        while (r != 0)
        {
            int quotient = oldR / r;
            (oldR, r) = (r, oldR - quotient * r);
            (oldS, s) = (s, oldS - quotient * s);
            (oldT, t) = (t, oldT - quotient * t);
        }
        
        return (oldR, oldS, oldT);
    }

    public static int InverseMod(int a, int m)
    {
        if (a % m == 0)
        {
            throw new Exception($"Can't find inverse of {a} mod {m} because {m} divides {a}");
        }

        return Mod(GCDX(a, m).Item2, m);
    }

    // https://stackoverflow.com/questions/1728736/numerical-algorithm-to-generate-numbers-from-binomial-distribution
    public static int RandomBinomial(int n, double p, Random? random = null)
    {
        random ??= new Random();

        if (n < 1000)
        {
            int result = 0;
            for (int i = 0; i < n; ++i)
            {
                if (random.NextDouble() < p)
                {
                    result++;
                }
            }

            return result;
        }

        if (n * p < 10) return RandomPoisson(n * p, random);
        if (n * (1 - p) < 10) return n - RandomPoisson(n * p, random);

        int v = (int)(0.5 + RandomNormal(n * p, Math.Sqrt(n * p * (1 - p))));
        if (v < 0) v = 0;
        else if (v > n) v = n;
        return v;

    }

    // https://stackoverflow.com/questions/218060/random-gaussian-variables
    public static double RandomNormal(double mean = 0, double stdDev = 1, Random? random = null)
    {
        random ??= new Random();
        double u1 = 1.0 - random.NextDouble();
        double u2 = 1.0 - random.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2 * Math.PI * u2);
        return mean + stdDev * randStdNormal;
    } 

    public static int RandomPoisson(double lambda, Random? random = null)
    {
        random ??= new Random();
        return (lambda < 30.0) ? PoissonSmall(lambda, random) : PoissonLarge(lambda, random);
    }

    static int PoissonSmall(double lambda, Random random)
    {
        double p = 1, L = Math.Exp(-lambda);
        int k = 0;
        do
        {
            k++;
            p *= random.NextDouble();
        } while (p > L);

        return k - 1;
    }

    static int PoissonLarge(double lambda, Random random)
    {
        double c = 0.767 - 3.36 / lambda;
        double beta = Math.PI / Math.Sqrt(3 * lambda);
        double alpha = beta * lambda;
        double k = Math.Log(c) - lambda - Math.Log(beta);

        for (;;)
        {
            double u = random.NextDouble();
            double x = (alpha - Math.Log((1.0 - u) / u)) / beta;
            int n = (int)Math.Floor(x + 0.5);
            if(n < 0)
                continue;
            double v = random.NextDouble();
            double y = alpha - beta * x;
            double temp = 1.0 * Math.Exp(y);
            double lhs = y + Math.Log(v / (temp * temp));
            double rhs = k + n * Math.Log(lambda) - Gamma.logGamma(n);
            if (lhs <= rhs)
            {
                return n;
            }
        }
    }
}