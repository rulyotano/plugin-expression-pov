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
    private int _lookAhead = 0;
    private ReadOnlySpan<char> _span = expression.AsSpan();
    private ReadOnlySpan<char> _ignore = " \n\t\r".AsSpan();
    
    private bool IsEof => _lookAhead == expression.Length;

    public ParseResult Parse(out PluginExpressionNode node)
    {
        
    }

    private ParseResult ParseOr(out PluginExpressionNode node)
    {
        node = PluginExpressionNode.FailNode;
        ReadIgnoreCharacters();
        var leftResult = ParseAnd(out var leftNode);
        if (!leftResult.IsSuccess)
            return leftResult;
        
        ReadIgnoreCharacters();

        if (_span[_lookAhead] != OrCharacter)
        {
            node = new PluginExpressionOrNode(leftNode, new PluginTrueNode());
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
        
    }

    private ParseResult ParseTerm(out PluginExpressionNode node)
    {
        
    }

    private ParseResult ParseNegation(out PluginExpressionNode node)
    {
        
    }

    private ParseResult ParseParenthesis(out PluginExpressionNode node)
    {
        
    }

    private ParseResult ParseEverybody(out PluginExpressionNode node)
    {
        
    }

    private ParseResult ParseNobody(out PluginExpressionNode node)
    {
        
    }

    private void ReadIgnoreCharacters()
    {
        while (!IsEof && _ignore.Contains(_span[_lookAhead]))
        {
            _lookAhead++;
        }
    }

    private string GetErrorString(char expectedChar) => $"Position: {_lookAhead}. Expected '{expectedChar}' but found '{_span[_lookAhead]}'.";
}

public record ParseResult(bool IsSuccess, IEnumerable<string> Errors)
{
    public static ParseResult Success { get; } = new(true, Array.Empty<string>());
}