using System.Text.RegularExpressions;

namespace CAS;

public class Lexer
{
    public static List<Token> Tokenize(string input)
    {
        // State 0
        //  LParen -> 0
        //  UnaryOperator -> 0
        //  Number -> 1
        //  Identifier -> 1
        //  EOL -> N/A
        
        // State 1
        //  BinaryOperator -> 0
        //  RParen -> 1
        int state = 0;

        int currentPos = 0;

        List<Token> tokens = new List<Token>();

        while (true)
        {
            Token? optionalToken = GetToken(input, ref currentPos, state);
            if (!optionalToken.HasValue)
            {
                continue;
            }

            Token token = optionalToken.Value;
            bool isState0Token = new[] { TokenType.Identifier, TokenType.LParen, TokenType.Number, TokenType.UnaryOperator }
                .Contains(token.TokenType);
            bool isState1Token = new[] { TokenType.RParen, TokenType.BinaryOperator }.Contains(token.TokenType);
            if ((state == 1 && isState0Token) || (state == 0 && isState1Token))
            {
                throw new Exception($"Token type '{token.TokenType}' not allowed in this state!");
            }
            tokens.Add(token);

            if (token.TokenType == TokenType.EOL)
            {
                break;
            }
            
            if (state == 0)
            {
                if (token.TokenType == TokenType.Number || token.TokenType == TokenType.Identifier)
                {
                    state = 1;
                }
            }
            else
            {
                if (token.TokenType == TokenType.BinaryOperator)
                {
                    state = 0;
                }
            }
        }

        return tokens;
    }

    static Token? GetToken(string input, ref int currentPos, int state)
    {
        if (currentPos == input.Length)
        {
            return new Token(TokenType.EOL, "");
        }
        
        char currentChar = input[currentPos++];

        if (Char.IsWhiteSpace(currentChar))
        {
            return null;
        }

        if (Char.IsNumber(currentChar))
        {
            string rest = input.Substring(currentPos - 1);
            Match match = new Regex("^[0-9]+").Match(rest);
            currentPos += match.Length - 1;
            return new Token(TokenType.Number, match.Value);
        }

        if ("+-*/^".Contains(currentChar))
        {
            if (state == 0 && currentChar != '-')
            {
                throw new Exception($"Invalid unary operator '{currentChar}' found at position {currentPos}");
            }

            return new Token(state == 0 ? TokenType.UnaryOperator : TokenType.BinaryOperator, currentChar.ToString());
        }

        if (currentChar == '(')
        {
            return new Token(TokenType.LParen, "(");
        }

        if (currentChar == ')')
        {
            return new Token(TokenType.RParen, ")");
        }

        if (Char.IsLetter(currentChar))
        {
            return new Token(TokenType.Identifier, currentChar.ToString());
        }

        throw new Exception($"Invalid character '{currentChar}' at position {currentPos}");
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
    }

    public enum TokenType
    {
        Number,
        UnaryOperator,
        BinaryOperator,
        LParen,
        RParen,
        Identifier,
        EOL
    }
}