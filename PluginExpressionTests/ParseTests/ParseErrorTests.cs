using PluginExpression;
using PluginExpression.Ast;

namespace PluginExpressionTests.ParseTests;

public class ParseErrorTests
{
    [Fact]
    public void WhenEmptyRuleShouldGetParseError()
    {
        AssertErrorAt(string.Empty, 0);
    }

    [Fact]
    public void WhenNullRuleShouldGetParseError()
    {
        AssertErrorAt(null!, 0);
    }

    [Theory]
    [InlineData('.')]
    [InlineData(',')]
    public void WhenOperatorDoesntClose(char op)
    {
        AssertErrorAt($"ABC{op}", 4);
    }

    [Theory]
    [InlineData('.')]
    [InlineData(',')]
    public void WhenOperatorDoesntHaveRightPart(char op)
    {
        AssertErrorAt($"{op}ABC", 0);
    }

    [Theory]
    [InlineData('.', '.')]
    [InlineData('.', ',')]
    [InlineData(',', ',')]
    [InlineData(',', '.')]
    public void WhenOperatorsAppearsTogether(char leftOp, char rightOp)
    {
        AssertErrorAt($"ABC{leftOp}{rightOp}EFG", 4);
    }

    [Fact]
    public void WhenParenthesisIsEmptyShouldGetParseError()
    {
        AssertErrorAt("ABC.()", 5);
    }

    [Fact]
    public void WhenParenthesisDidntClose()
    {
        AssertErrorAt("ABC.(EFG", 8);
    }

    [Fact]
    public void WhenParenthesisDidntOpen()
    {
        AssertErrorAt("ABC.EFG)", 7);
    }

    [Fact]
    public void WhenParenthesisDidntOpen2()
    {
        AssertErrorAt("ABC.(E)).FG", 7);
    }

    [Fact]
    public void WhenClosingParenthesisAtTheBeginning()
    {
        AssertErrorAt(")ABC.EFG", 0);
    }

    [Fact]
    public void WhenNegatingEmpty()
    {
        AssertErrorAt("!", 1);
    }

    [Theory]
    [InlineData("ABC.!.ADS", 5)]
    [InlineData("ABC . ! . ADS", 8)]
    public void WhenNegatingNotFollowedByTerm(string term, int expectedPosition)
    {
        AssertErrorAt(term, expectedPosition);
    }

    [Theory]
    [InlineData("ABC.*~.ABC", 5)]
    [InlineData("ABC.*  ~.ABC", 7)]
    public void WhenAllAndNoneTogether(string term, int expectedPosition)
    {
        AssertErrorAt(term, expectedPosition);
    }

    [Theory]
    [InlineData("ABC DEF", 4)]
    public void WhenWordSeparated(string term, int expectedPosition)
    {
        AssertErrorAt(term, expectedPosition);
    }

    private void AssertErrorAt(string expression, int position)
    {
        var parser = new ExpressionParser(expression);
        
        var result = parser.Parse(out NodeBase root);
        
        Assert.False(result.IsSuccess);
        Assert.IsType<ParsingFailed>(root);
        Assert.Single(result.Errors);
        Assert.Equal(position, result.Errors.First().Position);
    }
}