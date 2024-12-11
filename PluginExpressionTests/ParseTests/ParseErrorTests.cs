using PluginExpression;
using PluginExpression.Ast;

namespace PluginExpressionTests.ParseTests;

public class ParseErrorTests
{
    [Fact]
    public void WhenEmptyRuleShouldGetParseError()
    {
        var parser = new ExpressionParser(string.Empty);
        
        var result = parser.Parse(out NodeBase root);
        
        Assert.False(result.IsSuccess);
        Assert.IsType<ParsingFailed>(root);
        Assert.Single(result.Errors);
        Assert.Equal(0, result.Errors.First().Position);
    }

    [Fact]
    public void WhenNullRuleShouldGetParseError()
    {
        var parser = new ExpressionParser(null!);
        
        var result = parser.Parse(out NodeBase root);
        
        Assert.False(result.IsSuccess);
        Assert.IsType<ParsingFailed>(root);
        Assert.Single(result.Errors);
        Assert.Equal(0, result.Errors.First().Position);
    }

    [Theory]
    [InlineData('.')]
    [InlineData(',')]
    public void WhenOperatorDoesntClose(char op)
    {
        var parser = new ExpressionParser($"ABC{op}");
        
        var result = parser.Parse(out NodeBase root);
        
        Assert.False(result.IsSuccess);
        Assert.IsType<ParsingFailed>(root);
        Assert.Single(result.Errors);
        Assert.Equal(4, result.Errors.First().Position);
    }

    [Theory]
    [InlineData('.')]
    [InlineData(',')]
    public void WhenOperatorDoesntHaveRightPart(char op)
    {
        var parser = new ExpressionParser($"{op}ABC");
        
        var result = parser.Parse(out NodeBase root);
        
        Assert.False(result.IsSuccess);
        Assert.IsType<ParsingFailed>(root);
        Assert.Single(result.Errors);
        Assert.Equal(0, result.Errors.First().Position);
    }

    [Theory]
    [InlineData('.', '.')]
    [InlineData('.', ',')]
    [InlineData(',', ',')]
    [InlineData(',', '.')]
    public void WhenOperatorsAppearsTogether(char leftOp, char rightOp)
    {
        var parser = new ExpressionParser($"ABC{leftOp}{rightOp}EFG");
        
        var result = parser.Parse(out NodeBase root);
        
        Assert.False(result.IsSuccess);
        Assert.IsType<ParsingFailed>(root);
        Assert.Single(result.Errors);
        Assert.Equal(4, result.Errors.First().Position);
    }

    [Fact]
    public void WhenParenthesisIsEmptyShouldGetParseError()
    {
        var parser = new ExpressionParser("ABC.()");
        
        var result = parser.Parse(out NodeBase root);
        
        Assert.False(result.IsSuccess);
        Assert.IsType<ParsingFailed>(root);
        Assert.Single(result.Errors);
        Assert.Equal(5, result.Errors.First().Position);
    }

    [Fact]
    public void WhenParenthesisDidntClose()
    {
        var parser = new ExpressionParser("ABC.(EFG");
        
        var result = parser.Parse(out NodeBase root);
        
        Assert.False(result.IsSuccess);
        Assert.IsType<ParsingFailed>(root);
        Assert.Single(result.Errors);
        Assert.Equal(8, result.Errors.First().Position);
    }

    [Fact]
    public void WhenParenthesisDidntOpen()
    {
        var parser = new ExpressionParser("ABC.EFG)");
        
        var result = parser.Parse(out NodeBase root);
        
        Assert.False(result.IsSuccess);
        Assert.IsType<ParsingFailed>(root);
        Assert.Single(result.Errors);
        Assert.Equal(7, result.Errors.First().Position);
    }

    [Fact]
    public void WhenParenthesisDidntOpen2()
    {
        var parser = new ExpressionParser("ABC.(E)).FG");
        
        var result = parser.Parse(out NodeBase root);
        
        Assert.False(result.IsSuccess);
        Assert.IsType<ParsingFailed>(root);
        Assert.Single(result.Errors);
        Assert.Equal(7, result.Errors.First().Position);
    }

    [Fact]
    public void WhenClosingParenthesisAtTheBeginning()
    {
        var parser = new ExpressionParser(")ABC.EFG");
        
        var result = parser.Parse(out NodeBase root);
        
        Assert.False(result.IsSuccess);
        Assert.IsType<ParsingFailed>(root);
        Assert.Single(result.Errors);
        Assert.Equal(0, result.Errors.First().Position);
    }
}