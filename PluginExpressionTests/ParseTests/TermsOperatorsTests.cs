using PluginExpression;
using PluginExpression.Ast;

namespace PluginExpressionTests.ParseTests;

public class TermsOperatorsTests
{
    [Fact]
    public void ShouldParseWord()
    {
        const string input = "Hello$World-gGuys@hello";
        var parser = new ExpressionParser(input);
        
        var result = parser.Parse(out NodeBase root);
        
        Assert.True(result.IsSuccess);
        Assert.IsType<WordNode>(root);
        Assert.Equal(input, root.ToString());
    }

    [Fact]
    public void ShouldParseWordsAndWords()
    {
        var parser = new ExpressionParser("BR.DOCTOR.VIP,BR.DOCTOR.STARTER");
        
        var result = parser.Parse(out NodeBase root);
        
        Assert.True(result.IsSuccess);
        Assert.IsType<OrNode>(root);
        Assert.Equal("((BR) and ((DOCTOR) and (VIP))) or ((BR) and ((DOCTOR) and (STARTER)))", root.ToString());
    }

    [Theory]
    [InlineData("VIP,PL.DOCTOR")]
    [InlineData("   VIP  ,   PL   .   DOCTOR   ")]
    [InlineData("\n\r VIP \t\t\t ,   PL   .   DOCTOR   ")]
    public void ShouldIgnoreCharacters(string input)
    {
        var parser = new ExpressionParser(input);
        
        var result = parser.Parse(out NodeBase root);
        
        Assert.True(result.IsSuccess);
        Assert.IsType<OrNode>(root);
        Assert.Equal("(VIP) or ((PL) and (DOCTOR))", root.ToString());
    }
}