using PluginExpression;
using PluginExpression.Ast;

namespace PluginExpressionTests.ParseTests;

public class LogicOperatorTests
{
    [Fact]
    public void ShouldParseAll()
    {
        var parser = new ExpressionParser("*");
        
        var result = parser.Parse(out NodeBase root);
        
        Assert.True(result.IsSuccess);
        Assert.IsType<TrueNode>(root);
        Assert.Equal("1", root.ToString());
    }

    [Fact]
    public void ShouldParseNone()
    {
        var parser = new ExpressionParser("~");
        
        var result = parser.Parse(out NodeBase root);
        
        Assert.True(result.IsSuccess);
        Assert.IsType<NegationNode>(root);
        Assert.Equal("!(1)", root.ToString());
    }

    [Theory]
    [InlineData("*,~")]
    [InlineData("  *  ,  ~   ")]
    [InlineData("  *  \n \r,  ~   \n \r \t")]
    public void ShouldParseOrSimpleWithIgnoreCharacters(string input)
    {
        var parser = new ExpressionParser(input);
        
        var result = parser.Parse(out NodeBase root);
        
        Assert.True(result.IsSuccess);
        Assert.IsType<OrNode>(root);
        Assert.Equal("(1) or (!(1))", root.ToString());
    }
    
    [Theory]
    [InlineData("~.*")]
    [InlineData("  ~  .  *   ")]
    [InlineData("  ~  \n \r.  *   \n \r \t")]
    public void ShouldParseAndSimpleWithIgnoreCharacters(string input)
    {
        var parser = new ExpressionParser(input);
        
        var result = parser.Parse(out NodeBase root);
        
        Assert.True(result.IsSuccess);
        Assert.IsType<AndNode>(root);
        Assert.Equal("(!(1)) and (1)", root.ToString());
    }
    
    [Theory]
    [InlineData("*,*,*,*")]
    [InlineData("*,  * \n ,  *  ,  *  ")]
    public void ShouldParseManyOrs(string input)
    {
        var parser = new ExpressionParser(input);
        
        var result = parser.Parse(out NodeBase root);
        
        Assert.True(result.IsSuccess);
        Assert.IsType<OrNode>(root);
        Assert.Equal("(1) or ((1) or ((1) or (1)))", root.ToString());
    }
    
    [Theory]
    [InlineData("*.*.*.*")]
    [InlineData("*.  * \n .  *  .  *  ")]
    public void ShouldParseManyAnds(string input)
    {
        var parser = new ExpressionParser(input);
        
        var result = parser.Parse(out NodeBase root);
        
        Assert.True(result.IsSuccess);
        Assert.IsType<AndNode>(root);
        Assert.Equal("(1) and ((1) and ((1) and (1)))", root.ToString());
    }
    
    [Theory]
    [InlineData("*.*,*.*.*")]
    [InlineData("  *   .  *   ,   *   .   *  .   *")]
    public void OrsShouldHaveLessForceThanAnds(string input)
    {
        var parser = new ExpressionParser(input);
        
        var result = parser.Parse(out NodeBase root);
        
        Assert.True(result.IsSuccess);
        Assert.IsType<OrNode>(root);
        Assert.Equal("((1) and (1)) or ((1) and ((1) and (1)))", root.ToString());
    }
    
    [Theory]
    [InlineData("*,*.*.*,*")]
    public void OrsShouldHaveLessForceThanAndsSecondTests(string input)
    {
        var parser = new ExpressionParser(input);
        
        var result = parser.Parse(out NodeBase root);
        
        Assert.True(result.IsSuccess);
        Assert.IsType<OrNode>(root);
        Assert.Equal("(1) or (((1) and ((1) and (1))) or (1))", root.ToString());
    }
}