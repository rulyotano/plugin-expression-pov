using System.Text;
using PluginExpression.Nodes;

namespace PluginExpression;

public ref struct PluginExpressionParser(string expression)
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

    public ParseResult Parse(out PluginExpressionNode node)
    {
        return ParseOr(out node);
    }

    private ParseResult ParseOr(out PluginExpressionNode node)
    {
        node = PluginExpressionNode.FailNode;
        ReadIgnoreCharacters();
        var leftResult = ParseAnd(out var leftNode);
        if (!leftResult.IsSuccess)
            return leftResult;
        
        ReadIgnoreCharacters();

        if (_expressionSpan[_lookAhead] != OrCharacter)
        {
            node = leftNode;
            return ParseResult.Success;
        }
        
        _lookAhead++;
        
        ReadIgnoreCharacters();
        var rightResult = ParseOr(out var rightNode);
        if (!rightResult.IsSuccess)
            return rightResult;
        
        node = new PluginExpressionOrNode(leftNode, rightNode);
        return ParseResult.Success;
    }

    private ParseResult ParseAnd(out PluginExpressionNode node)
    {
        node = PluginExpressionNode.FailNode;
        ReadIgnoreCharacters();
        var leftResult = ParseTerm(out var leftNode);
        if (!leftResult.IsSuccess)
            return leftResult;

        ReadIgnoreCharacters();

        if (_expressionSpan[_lookAhead] != AndCharacter)
        {
            node = leftNode;
            return ParseResult.Success;
        }

        _lookAhead++;

        ReadIgnoreCharacters();
        var rightResult = ParseOr(out var rightNode);
        if (!rightResult.IsSuccess)
            return rightResult;

        node = new PluginExpressionAndNode(leftNode, rightNode);
        return ParseResult.Success;
    }

    private ParseResult ParseTerm(out PluginExpressionNode node)
    {
        ReadIgnoreCharacters();
        return _expressionSpan[_lookAhead] switch
        {
            OpenParenthesisCharacter => ParseParenthesis(out node),
            NegationCharacter => ParseNegation(out node),
            NobodyCharacter => ParseNobody(out node),
            EverybodyCharacter => ParseEverybody(out node),
            _ => ParseWord(out node)
        };
    }

    private ParseResult ParseParenthesis(out PluginExpressionNode node)
    {
        node = PluginExpressionNode.FailNode;
        _lookAhead++;
        
        var result = ParseOr(out node);
        if (!result.IsSuccess)
            return result;
        
        ReadIgnoreCharacters();

        if (_expressionSpan[_lookAhead] != CloseParenthesisCharacter)
            return new ParseResult(false, [GetErrorString($"{CloseParenthesisCharacter}")]);
        
        _lookAhead++;
        return ParseResult.Success;
    }

    private ParseResult ParseNegation(out PluginExpressionNode node)
    {
        _lookAhead++;
        var result = ParseOr(out node);
        if (!result.IsSuccess)
            return result;
        node = new PluginNegationNode(node);
        return ParseResult.Success;
    }

    private ParseResult ParseEverybody(out PluginExpressionNode node)
    {
        _lookAhead++;
        node = new PluginTrueNode();
        return ParseResult.Success;
    }

    private ParseResult ParseNobody(out PluginExpressionNode node)
    {
        _lookAhead++;
        node = new PluginNegationNode(new PluginTrueNode());
        return ParseResult.Success;
    }

    private ParseResult ParseWord(out PluginExpressionNode node)
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
            node = PluginExpressionNode.FailNode;
            return new ParseResult(false, [GetErrorString($"Characters not in collection: '{NoWordCharacters}'")]);
        }
        
        node = new PluginWordNode(wordBuilder.ToString());
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