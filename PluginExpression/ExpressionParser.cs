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
    private ParseError? _error = null;
    
    private bool IsEof => _lookAhead == (expression?.Length ?? 0);
    private bool IsError => _error is not null;

    public ParseResult Parse(out NodeBase nodeBase)
    {
        Initialize();
        nodeBase = NodeBase.FailNodeBase;
        if (expression is null)
            return new ParseResult(false, [new ParseError(0, "Expression is empty.")]);
        nodeBase = ParseOr();
        if (IsError)
            return new ParseResult(false, [_error!]);
        
        ReadIgnoreCharacters();
        if (IsEof) return ParseResult.Success;
        
        nodeBase = NodeBase.FailNodeBase;
        return new ParseResult(false, [GetError("End of expression")]);
    }

    private void Initialize()
    {
        _lookAhead = 0;
        _error = null;
    }

    private NodeBase ParseOr()
    {
        ReadIgnoreCharacters();
        var leftResult = ParseAnd();
        if (IsError)
            return leftResult;
        
        ReadIgnoreCharacters();

        if (IsEof || _expressionSpan[_lookAhead] != OrCharacter)
            return leftResult;
        
        _lookAhead++;
        
        ReadIgnoreCharacters();
        var rightResult = ParseOr();
        if (IsError)
            return rightResult;
        
        return new OrNode(leftResult, rightResult);
    }

    private NodeBase ParseAnd()
    {
        ReadIgnoreCharacters();
        var leftResult = ParseTerm();
        if (IsError)
            return leftResult;

        ReadIgnoreCharacters();

        if (IsEof ||_expressionSpan[_lookAhead] != AndCharacter)
            return leftResult;

        _lookAhead++;

        ReadIgnoreCharacters();
        var rightResult = ParseAnd();
        if (IsError)
            return rightResult;

        return new AndNode(leftResult, rightResult);
    }

    private NodeBase ParseTerm()
    {
        ReadIgnoreCharacters();
        if (IsEof)
        {
            _error = GetError($"'{OpenParenthesisCharacter}' or '{NegationCharacter}' or '{NobodyCharacter}' or '{EverybodyCharacter}' or Value");
            return NodeBase.FailNodeBase;
        }

        return _expressionSpan[_lookAhead] switch
        {
            OpenParenthesisCharacter => ParseParenthesis(),
            NegationCharacter => ParseNegation(),
            NobodyCharacter => ParseNobody(),
            EverybodyCharacter => ParseEverybody(),
            _ => ParseWord()
        };
    }

    private NodeBase ParseParenthesis()
    {
        _lookAhead++;
        
        var result = ParseOr();
        if (IsError)
            return result;
        
        ReadIgnoreCharacters();

        if (IsEof || _expressionSpan[_lookAhead] != CloseParenthesisCharacter)
        {
            _error = GetError($"{CloseParenthesisCharacter}");
            return NodeBase.FailNodeBase;
        }
        
        _lookAhead++;
        return result;
    }

    private NodeBase ParseNegation()
    {
        _lookAhead++;
        var term = ParseTerm();
        if (IsError)
            return term;
        return new NegationNode(term);
    }

    private NodeBase ParseEverybody()
    {
        _lookAhead++;
        return new TrueNode();
    }

    private NodeBase ParseNobody()
    {
        _lookAhead++;
        return new NegationNode(new TrueNode());
    }

    private NodeBase ParseWord()
    {
        ReadIgnoreCharacters();
        var wordBuilder = new StringBuilder();
        while (!IsEof && !_nonWordSpan.Contains(_expressionSpan[_lookAhead]))
        {
            wordBuilder.Append(_expressionSpan[_lookAhead]);
            _lookAhead++;
        }

        if (wordBuilder.Length != 0) return new WordNode(wordBuilder.ToString());

        _error = GetError("Value");
        return NodeBase.FailNodeBase;
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