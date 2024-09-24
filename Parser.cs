using CAS.Algebra;

namespace CAS;

public class Parser(List<Lexer.Token> tokens)
{
    List<Lexer.Token> tokens = tokens;
    private int currentTokenIndex;

    private Lexer.Token currentToken => tokens[currentTokenIndex];

    private Polynomial Parse(List<Lexer.Token>? newTokens = null)
    {
        if (newTokens is not null)
        {
            tokens = newTokens;
        }

        currentTokenIndex = 0;

        // Console.WriteLine(String.Join(", ", tokens));

        // AST ast = ParseAST();
        // Console.WriteLine("Base AST:");
        // Console.WriteLine(ast);
        //
        // ast.ExpandExponents();
        // Console.WriteLine("Expand exponents:");
        // Console.WriteLine(ast);
        //
        // ast.Distribute();
        // Console.WriteLine("Distribute:");
        // Console.WriteLine(ast);
        //
        // ast.RemoveAssociative();
        // Console.WriteLine("Remove associative parenthesis:");
        // Console.WriteLine(ast);
        //
        // ast.CombineConstants();
        // Console.WriteLine("Combine Constants:");
        // Console.WriteLine(ast);

        return ConvertAST(ParseAST());
    }

    private Polynomial ConvertAST(AST ast)
    {
        Polynomial polynomial = new Polynomial();
        ast.ExpandExponents();
        ast.Distribute();
        ast.RemoveAssociative();
        ast.CombineConstants();

        Node baseNode = ast.GetBaseNode();
        Addition addition;
        if (baseNode is Addition)
        {
            addition = (Addition)baseNode;
        }
        else
        {
            addition = new Addition(baseNode);
        }

        foreach (Node node in addition.Children)
        {
            if (node is Multiplication multiplication)
            {
                polynomial += new Term(multiplication.GetCoefficient(), multiplication.GetDegree());
                continue;
            }

            if (node is Identifier)
            {
                polynomial += new Term(1, 1);
                continue;
            }

            if (node is Number number)
            {
                polynomial += new Term(number.value, 0);
                continue;
            }
            
            throw new Exception("Failed to convert AST to Polynomial");
        }

        return polynomial;
    }

    private Lexer.Token EatToken(Lexer.TokenType tokenType)
    {
        if (currentToken.TokenType != tokenType)
        {
            throw new Exception($"Unexpected token {tokenType}.");
        }

        return tokens[currentTokenIndex++];
    }

    private AST ParseAST()
    {
        return new AST(ParseExpression());
    }

    /**
     * Expression -> Term
     * Expression -> Term + Term + Term ...
     * Expression -> Term - Term - Term ...
     * Term -> Factor
     * Term -> Factor * Factor * Factor ...
     * Factor -> Literal
     * Factor -> Literal^Num
     * Literal -> (Expression)
     * Literal -> Num
     * Literal -> ID
     */
    private Node ParseExpression()
    {
        Addition addition = new Addition();

        while (currentToken.TokenType != Lexer.TokenType.EOL && currentToken.TokenType != Lexer.TokenType.RParen)
        {
            if (currentToken.TokenType == Lexer.TokenType.Subtraction)
            {
                EatToken(Lexer.TokenType.Subtraction);
                addition.Children.Add(Negate(ParseTerm()));
            }
            else
            {
                Node term = ParseTerm();
                if (term is Addition additionTerm)
                {
                    addition.Children.AddRange(additionTerm.Children);
                }
                else
                {
                    addition.Children.Add(term);
                }
            }

            if (currentToken.TokenType == Lexer.TokenType.Addition)
            {
                EatToken(Lexer.TokenType.Addition);
            }
        }

        if (addition.Children.Count == 1)
        {
            return addition.Children.First();
        }

        return addition;
    }

    private Node ParseTerm()
    {
        Multiplication multiplication = new Multiplication(ParseFactor());

        Lexer.TokenType[] literalTypes = { Lexer.TokenType.Identifier, Lexer.TokenType.Number, Lexer.TokenType.LParen };

        while (literalTypes.Contains(currentToken.TokenType) ||
               currentToken.TokenType == Lexer.TokenType.Multiplication)
        {
            if (currentToken.TokenType == Lexer.TokenType.Multiplication)
            {
                EatToken(Lexer.TokenType.Multiplication);
            }

            multiplication.Children.Add(ParseFactor());
        }

        if (multiplication.Children.Count == 1)
        {
            return multiplication.Children.First();
        }

        return multiplication;
    }

    private Node ParseFactor()
    {
        Node literal = ParseLiteral();
        if (currentToken.TokenType == Lexer.TokenType.Exponent)
        {
            EatToken(Lexer.TokenType.Exponent);
            Number exponent = ParseNumber();
            return new Exponentiation(literal, exponent);
        }

        return literal;
    }

    private Node ParseLiteral()
    {
        switch (currentToken.TokenType)
        {
            case Lexer.TokenType.Identifier:
            {
                Lexer.Token token = EatToken(Lexer.TokenType.Identifier);
                return new Identifier(token.Value);
            }
            case Lexer.TokenType.Number:
            {
                return ParseNumber();
            }
            case Lexer.TokenType.LParen:
            {
                EatToken(Lexer.TokenType.LParen);
                Node expression = ParseExpression();
                EatToken(Lexer.TokenType.RParen);
                return expression;
            }
            default:
                throw new Exception($"Unexpected token {currentToken.TokenType}.");
        }
    }

    private Number ParseNumber()
    {
        Lexer.Token token = EatToken(Lexer.TokenType.Number);
        return new Number(int.Parse(token.Value));
    }

    private Node Negate(Node node)
    {
        return new Negative(node);
    }

    public static Polynomial Parse(string input)
    {
        return new Parser(Lexer.Tokenize(input)).Parse();
    }

    private abstract class Node
    {
        public abstract override string? ToString();
        public abstract Node Clone();
        public abstract void ExpandExponents();
        public abstract Node Distribute();
        public abstract void RemoveAssociative();
        public abstract Node CombineConstants();

        protected Node ExpandExponent(Exponentiation exponentiation)
        {
            Node[] nodes = new Node[exponentiation.exponent.value];
            for (int j = 0; j < exponentiation.exponent.value; j++)
            {
                nodes[j] = exponentiation.baseExpression.Clone();
            }

            return new Multiplication(nodes);
        }
    }

    private class AST(Node baseNode) : Node
    {
        public Node GetBaseNode()
        {
            return baseNode;
        }

        public override string ToString()
        {
            return baseNode.ToString() ?? "";
        }

        public override Node Clone()
        {
            return baseNode.Clone();
        }

        public override void ExpandExponents()
        {
            baseNode.ExpandExponents();
            if (baseNode is Exponentiation exponentiation)
            {
                baseNode = ExpandExponent(exponentiation);
            }
        }

        public override Node Distribute()
        {
            baseNode = baseNode.Distribute();
            return this;
        }

        public override Node CombineConstants()
        {
            baseNode = baseNode.CombineConstants();
            return this;
        }

        public override void RemoveAssociative()
        {
            baseNode.RemoveAssociative();
        }
    }

    private abstract class OperationNode(params Node[] nodes) : Node
    {
        public List<Node> Children = [..nodes];

        public override void ExpandExponents()
        {
            for (int i = 0; i < Children.Count; i++)
            {
                Node child = Children[i];
                child.ExpandExponents();
                if (child is Exponentiation exponentiation)
                {
                    Children[i] = ExpandExponent(exponentiation);
                }
            }
        }
    }

    private class Number(int value) : Node
    {
        public int value = value;

        public override string ToString()
        {
            return value.ToString();
        }

        public override Node Clone()
        {
            return new Number(value);
        }

        public override Node Distribute()
        {
            return this;
        }

        public override Node CombineConstants()
        {
            return this;
        }

        public override void ExpandExponents()
        {
        }

        public override void RemoveAssociative()
        {
        }
    }

    private class Identifier(string value) : Node
    {
        public string value = value;

        public override string ToString()
        {
            return value;
        }

        public override Node Clone()
        {
            return new Identifier(value);
        }

        public override Node Distribute()
        {
            return this;
        }

        public override Node CombineConstants()
        {
            return this;
        }

        public override void ExpandExponents()
        {
        }

        public override void RemoveAssociative()
        {
        }
    }

    private class Negative(Node node) : Node
    {
        public Node node = node;

        public override string ToString()
        {
            return $"-({node})";
        }

        public override Node Clone()
        {
            return new Negative(node.Clone());
        }

        public override Node Distribute()
        {
            if (node is OperationNode || node is Exponentiation)
            {
                return new Multiplication(new Number(-1), node.Distribute()).Distribute();
            }

            if (node is Number number)
            {
                return new Number(-1 * number.value);
            }

            throw new Exception("Cannot distribute negative number");
        }

        public override Node CombineConstants()
        {
            node = node.CombineConstants();
            return this;
        }

        public override void ExpandExponents()
        {
            node.ExpandExponents();
            if (node is Exponentiation exponentiation)
            {
                node = ExpandExponent(exponentiation);
            }
        }

        public override void RemoveAssociative()
        {
            node.RemoveAssociative();
        }
    }

    private class Addition(params Node[] nodes) : OperationNode(nodes)
    {
        public override string ToString()
        {
            return "Add(" + String.Join(" + ", Children.Select(node => node.ToString())) + ")";
        }

        public override Node Distribute()
        {
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i] = Children[i].Distribute();
            }

            return this;
        }

        public override Node CombineConstants()
        {
            int sum = 0;
            List<Node> nodes = new List<Node>();
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i] = Children[i].CombineConstants();
                Node node = Children[i];
                if (node is Number number)
                {
                    sum += number.value;
                }
                else
                {
                    nodes.Add(node);
                }
            }

            if (nodes.Count == 0)
            {
                return new Number(sum);
            }

            Children = nodes;
            if (sum != 0)
            {
                Children.Add(new Number(sum));
            }

            return this;
        }

        public override Node Clone()
        {
            return new Addition(Children.Select(node => node.Clone()).ToArray());
        }

        public override void RemoveAssociative()
        {
            List<Node> newChildren = new List<Node>();
            foreach (Node child in Children)
            {
                child.RemoveAssociative();
                if (child is Addition addition)
                {
                    newChildren.AddRange(addition.Children);
                }
                else
                {
                    newChildren.Add(child);
                }
            }

            Children = newChildren;
        }
    }

    private class Multiplication(params Node[] nodes) : OperationNode(nodes)
    {
        public override string ToString()
        {
            return "Mult(" + String.Join(" * ", Children.Select(node => node.ToString())) + ")";
        }

        public override OperationNode Distribute()
        {
            while (Children.Count > 1)
            {
                int firstAddition = Children.FindIndex(node => node is Addition);
                if (firstAddition == -1)
                {
                    return this;
                }

                int next = firstAddition == 0 ? 1 : 0;
                Addition addition = (Addition)Children[firstAddition];
                Node nextNode = Children[next];
                Addition newAddition = Distribute(nextNode, addition);
                if (firstAddition > next)
                {
                    Children.RemoveAt(firstAddition);
                    Children.RemoveAt(next);
                }
                else
                {
                    Children.RemoveAt(next);
                    Children.RemoveAt(firstAddition);
                }

                Children.Add(newAddition);
            }

            return (OperationNode)Children.First();
        }

        public Addition Distribute(Node node, Addition addition)
        {
            List<Node> terms = new List<Node>();
            foreach (Node child in addition.Children)
            {
                Multiplication multiplication = new Multiplication(child.Distribute(), node.Clone());
                Node distributed = multiplication.Distribute();
                if (distributed is Addition distributedAddition)
                {
                    terms.AddRange(distributedAddition.Children);
                }
                else
                {
                    terms.Add(distributed);
                }
            }

            return new Addition(terms.ToArray());
        }

        public override Node CombineConstants()
        {
            int product = 1;
            List<Node> nodes = new List<Node>();
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i] = Children[i].CombineConstants();
                Node node = Children[i];
                if (node is Number number)
                {
                    product *= number.value;
                }
                else
                {
                    nodes.Add(node);
                }
            }

            if (nodes.Count == 0)
            {
                return new Number(product);
            }

            Children = nodes;
            if (product != 1)
            {
                Children.Add(new Number(product));
            }

            return this;
        }

        public override void RemoveAssociative()
        {
            List<Node> newChildren = new List<Node>();
            foreach (Node child in Children)
            {
                child.RemoveAssociative();
                if (child is Multiplication multiplication)
                {
                    newChildren.AddRange(multiplication.Children);
                }
                else
                {
                    newChildren.Add(child);
                }
            }

            Children = newChildren;
        }

        public int GetDegree()
        {
            int degree = 0;
            for (int i = 0; i < Children.Count; i++)
            {
                if (Children[i] is Identifier)
                {
                    degree++;
                }
            }

            return degree;
        }

        public int GetCoefficient()
        {
            int coeff = 1;
            for (int i = 0; i < Children.Count; i++)
            {
                if (Children[i] is Number number)
                {
                    coeff *= number.value;
                }
            }

            return coeff;
        }

        public override Node Clone()
        {
            return new Multiplication(Children.Select(node => node.Clone()).ToArray());
        }
    }

    private class Exponentiation(Node baseExpression, Number exponent) : Node
    {
        public Node baseExpression = baseExpression;
        public Number exponent = exponent;

        public override string ToString()
        {
            return $"Exp({baseExpression}^{exponent})";
        }

        public override Node Distribute()
        {
            baseExpression = baseExpression.Distribute();
            return this;
        }

        public override Node Clone()
        {
            return new Exponentiation(baseExpression.Clone(), (Number)exponent.Clone());
        }

        public override Node CombineConstants()
        {
            baseExpression = baseExpression.CombineConstants();
            if (baseExpression is Number baseNumber)
            {
                return new Number(Common.Pow(baseNumber.value, exponent.value));
            }

            return this;
        }

        public override void RemoveAssociative()
        {
            baseExpression.RemoveAssociative();
        }

        public override void ExpandExponents()
        {
            baseExpression.ExpandExponents();
            if (baseExpression is Exponentiation baseExponent)
            {
                baseExpression = ExpandExponent(baseExponent);
            }
        }
    }
}