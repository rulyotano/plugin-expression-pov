using System.Text;
using PluginExpression.Ast;

namespace PluginExpression;

public ref struct ExpressionParser(string expression)
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
    
    private bool IsEof => _lookAhead == expression.Length;

    public ParseResult Parse(out NodeBase nodeBase)
    {
        return ParseOr(out nodeBase);
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
            return new ParseResult(false, [GetErrorString($"'{OpenParenthesisCharacter}' or '{NegationCharacter}' or '{NoWordCharacters}' or '{EverybodyCharacter}' or Word")]);
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

        if (_expressionSpan[_lookAhead] != CloseParenthesisCharacter)
            return new ParseResult(false, [GetErrorString($"{CloseParenthesisCharacter}")]);
        
        _lookAhead++;
        return ParseResult.Success;
    }

    private ParseResult ParseNegation(out NodeBase nodeBase)
    {
        _lookAhead++;
        var result = ParseOr(out nodeBase);
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
            return new ParseResult(false, [GetErrorString($"Characters not in collection: '{NoWordCharacters}'")]);
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

    private string GetErrorString(string expected) => $"Position: {_lookAhead}. Expected '{expected}' but found '{_expressionSpan[_lookAhead]}'.";
}

public record ParseResult(bool IsSuccess, IEnumerable<string> Errors)
{
    public static ParseResult Success { get; } = new(true, Array.Empty<string>());
}