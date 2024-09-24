using System.Text.RegularExpressions;

namespace CAS;

public class Lexer
{
    public static List<Token> Tokenize(string input)
    {
        Dictionary<char, TokenType> singleCharTokens = new Dictionary<char, TokenType>
        {
            {'(', TokenType.LParen},
            {')', TokenType.RParen},
            {'+', TokenType.Addition},
            {'-', TokenType.Subtraction},
            {'*', TokenType.Multiplication},
            {'^', TokenType.Exponent}
        };
        List<Token> tokens = new List<Token>();
        char? variable = null;

        int position = 0;
        while (position < input.Length)
        {
            char c = input[position];
            // Console.WriteLine(c);
            if (c == '\n' || c == '\r')
            {
                break;
            }

            if (char.IsWhiteSpace(c))
            {
                position++;
                continue;
            }

            if (char.IsDigit(c))
            {
                string remainingInput = input.Substring(position);
                string number = new Regex("^\\d+").Match(remainingInput).Value;
                if (number.Length == 0)
                {
                    throw new FormatException("Could not parse number!");
                }
                position += number.Length;
                tokens.Add(new Token(TokenType.Number, number));
                continue;
            }
            bool matched = false;
            foreach (KeyValuePair<char,TokenType> singleCharToken in singleCharTokens)
            {
                if (c == singleCharToken.Key)
                {
                    tokens.Add(new Token(singleCharToken.Value, c.ToString()));
                    matched = true;
                    break;
                }
            }

            if (matched)
            {
                position++;
                continue;
            }

            if (char.IsLetter(c))
            {
                if (variable == null)
                {
                    variable = c;
                } else if (variable != c)
                {
                    throw new FormatException("Cannot have more than one variable!");
                }
                tokens.Add(new Token(TokenType.Identifier, c.ToString()));
                position++;
                continue;
            }
            
            throw new Exception($"Unrecognized character '{c}'");
        }
        tokens.Add(new Token(TokenType.EOL, string.Empty));
        return tokens;
    }

    public struct Token
    {
        public TokenType TokenType { get; }
        public string Value { get; }

        public Token(TokenType tokenType, string value)
        {
            TokenType = tokenType;
            Value = value;
        }

        public override string ToString()
        {
            return TokenType.ToString();
        }
    }

    public enum TokenType
    {
        Number,
        Addition,
        Multiplication,
        Subtraction,
        Exponent,
        LParen,
        RParen,
        Identifier,
        EOL
    }
}