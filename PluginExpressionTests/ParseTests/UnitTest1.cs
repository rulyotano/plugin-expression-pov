using PluginExpression;
using PluginExpression.Ast;

namespace PluginExpressionTests.ParseTests;

public class UnitTest1
{
    [Fact]
    public void ShouldParseAll()
    {
        var parser = new PluginExpressionParser("*");
        
        var result = parser.Parse(out NodeBase root);
        
        Assert.True(result.IsSuccess);
        Assert.IsType<TrueNode>(root);
    }

    [Fact]
    public void ShouldParseNone()
    {
        var parser = new PluginExpressionParser("~");
        
        var result = parser.Parse(out NodeBase root);
        
        Assert.True(result.IsSuccess);
        Assert.IsType<NegationNode>(root);
    }
}