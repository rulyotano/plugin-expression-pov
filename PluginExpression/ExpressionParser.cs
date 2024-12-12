using System.Text;
using PluginExpression.Ast;

namespace PluginExpression;

public ref struct ExpressionParser(string? expression)
{
    private const char OrCharacter = ',';
    private const char AndCharacter = '.';
    private const char NegationCharacter = '!';
    private const char EverybodyCharacter = '*';
    private const char NobodyCharacter = '~';
    private const char OpenParenthesisCharacter = '(';
    private const char CloseParenthesisCharacter = ')';
    private const string IgnoreCharacters = " \n\t\r";
    private static readonly string? NoWordCharacters = $"{IgnoreCharacters}{OrCharacter}{AndCharacter}{NegationCharacter}{EverybodyCharacter}{NobodyCharacter}{OpenParenthesisCharacter}{CloseParenthesisCharacter}";
    private int _lookAhead = 0;
    private readonly ReadOnlySpan<char> _expressionSpan = expression.AsSpan();
    private readonly ReadOnlySpan<char> _ignoreSpan = IgnoreCharacters.AsSpan();
    private readonly ReadOnlySpan<char> _nonWordSpan = NoWordCharacters.AsSpan();
    
    private bool IsEof => _lookAhead == (expression?.Length ?? 0);

    public ParseResult Parse(out NodeBase nodeBase)
    {
        nodeBase = NodeBase.FailNodeBase;
        if (expression is null)
            return new ParseResult(false, [new ParseError(0, "Expression is empty.")]);
        var result = ParseOr(out nodeBase);
        
        ReadIgnoreCharacters();
        if (IsEof) return result;
        
        nodeBase = NodeBase.FailNodeBase;
        return new ParseResult(false, [GetError("End of expression")]);
    }

    private ParseResult ParseOr(out NodeBase nodeBase)
    {
        nodeBase = NodeBase.FailNodeBase;
        ReadIgnoreCharacters();
        var leftResult = ParseAnd(out var leftNode);
        if (!leftResult.IsSuccess)
            return leftResult;
        
        ReadIgnoreCharacters();

        if (IsEof || _expressionSpan[_lookAhead] != OrCharacter)
        {
            nodeBase = leftNode;
            return ParseResult.Success;
        }
        
        _lookAhead++;
        
        ReadIgnoreCharacters();
        var rightResult = ParseOr(out var rightNode);
        if (!rightResult.IsSuccess)
            return rightResult;
        
        nodeBase = new OrNode(leftNode, rightNode);
        return ParseResult.Success;
    }

    private ParseResult ParseAnd(out NodeBase nodeBase)
    {
        nodeBase = NodeBase.FailNodeBase;
        ReadIgnoreCharacters();
        var leftResult = ParseTerm(out var leftNode);
        if (!leftResult.IsSuccess)
            return leftResult;

        ReadIgnoreCharacters();

        if (IsEof ||_expressionSpan[_lookAhead] != AndCharacter)
        {
            nodeBase = leftNode;
            return ParseResult.Success;
        }

        _lookAhead++;

        ReadIgnoreCharacters();
        var rightResult = ParseAnd(out var rightNode);
        if (!rightResult.IsSuccess)
            return rightResult;

        nodeBase = new AndNode(leftNode, rightNode);
        return ParseResult.Success;
    }

    private ParseResult ParseTerm(out NodeBase nodeBase)
    {
        ReadIgnoreCharacters();
        if (IsEof)
        {
            nodeBase = NodeBase.FailNodeBase;
            return new ParseResult(false, [GetError($"'{OpenParenthesisCharacter}' or '{NegationCharacter}' or '{NobodyCharacter}' or '{EverybodyCharacter}' or Value")]);
        }

        return _expressionSpan[_lookAhead] switch
        {
            OpenParenthesisCharacter => ParseParenthesis(out nodeBase),
            NegationCharacter => ParseNegation(out nodeBase),
            NobodyCharacter => ParseNobody(out nodeBase),
            EverybodyCharacter => ParseEverybody(out nodeBase),
            _ => ParseWord(out nodeBase)
        };
    }

    private ParseResult ParseParenthesis(out NodeBase nodeBase)
    {
        nodeBase = NodeBase.FailNodeBase;
        _lookAhead++;
        
        var result = ParseOr(out nodeBase);
        if (!result.IsSuccess)
            return result;
        
        ReadIgnoreCharacters();

        if (IsEof || _expressionSpan[_lookAhead] != CloseParenthesisCharacter)
            return new ParseResult(false, [GetError($"{CloseParenthesisCharacter}")]);
        
        _lookAhead++;
        return ParseResult.Success;
    }

    private ParseResult ParseNegation(out NodeBase nodeBase)
    {
        _lookAhead++;
        var result = ParseTerm(out nodeBase);
        if (!result.IsSuccess)
            return result;
        nodeBase = new NegationNode(nodeBase);
        return ParseResult.Success;
    }

    private ParseResult ParseEverybody(out NodeBase nodeBase)
    {
        _lookAhead++;
        nodeBase = new TrueNode();
        return ParseResult.Success;
    }

    private ParseResult ParseNobody(out NodeBase nodeBase)
    {
        _lookAhead++;
        nodeBase = new NegationNode(new TrueNode());
        return ParseResult.Success;
    }

    private ParseResult ParseWord(out NodeBase nodeBase)
    {
        ReadIgnoreCharacters();
        var wordBuilder = new StringBuilder();
        while (!IsEof && !_nonWordSpan.Contains(_expressionSpan[_lookAhead]))
        {
            wordBuilder.Append(_expressionSpan[_lookAhead]);
            _lookAhead++;
        }

        if (wordBuilder.Length == 0)
        {
            nodeBase = NodeBase.FailNodeBase;
            return new ParseResult(false, [GetError("Value")]);
        }
        
        nodeBase = new WordNode(wordBuilder.ToString());
        return ParseResult.Success;
    }

    private void ReadIgnoreCharacters()
    {
        while (!IsEof && _ignoreSpan.Contains(_expressionSpan[_lookAhead]))
        {
            _lookAhead++;
        }
    }
     
    private ParseError GetError(string expected)
    {
        var found = IsEof ? "EOF" : _expressionSpan[_lookAhead].ToString(); 
        return new ParseError(_lookAhead, $"Expected \"{expected}\" but found \"{found}\".");
    }
}

public record ParseResult(bool IsSuccess, IEnumerable<ParseError> Errors)
{
    public static ParseResult Success { get; } = new(true, Array.Empty<ParseError>());
}

public record ParseError(int Position, string ErrorMessage)
{
    public override string ToString() => $"{Position}: {ErrorMessage}";
}