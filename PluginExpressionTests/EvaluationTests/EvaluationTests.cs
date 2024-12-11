using PluginExpression;
using PluginExpression.Ast;
using Xunit.Abstractions;

namespace PluginExpressionTests.EvaluationTests;

public class EvaluationTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData("*", "DOCTOR,VIP,PL", true, "When all should return always true.")]
    [InlineData("*", "DOCTOR,VIP,PL,ALL", true, "When all should return always true.")]
    [InlineData("*", "", true, "When all and empty should return always true.")]
    [InlineData("*", null, true, "When all and null should return true.")]
    [InlineData("~", "DOCTOR,VIP,PL,ALL", false, "When none should return always false.")]
    [InlineData("~", "", false, "When none and empty should return always false.")]
    [InlineData("~", null, false, "When none and null should return always false.")]
    [InlineData("*.~", "DOCTOR,VIP,PL,ALL", false, "True and False should be false")]
    [InlineData("*,~", "DOCTOR,VIP,PL,ALL", true, "True or False should be true")]
    [InlineData("A", "b,c,d", false, "When having only one rule should return false if not present")]
    [InlineData("A", "a", true, "When having only one rule should return true if present")]
    [InlineData("A,B", "DOCTOR,VIP,PL,ALL", false, "When OR and doctor don't have it should return false")]
    [InlineData("A,B", "DOCTOR,VIP,PL,ALL,B", true, "When OR and doctor have it should return true")]
    [InlineData("A,B", "b", true, "Or should be camel case insensitive")]
    [InlineData("A,B", "DOCTOR,VIP,PL,ALL,A", true, "When OR and doctor have it should return true")]
    [InlineData("A.B", "DOCTOR,VIP,PL,ALL,A", false, "When AND and doctor only has one should return false")]
    [InlineData("A.B", "ALL,B,A", true, "When AND and doctor only has both should return true")]
    [InlineData("A.B", "b,a", true, "And should be camel case insensitive")]
    [InlineData("A.B", "", false, "And and empty should be false")]
    [InlineData("A.B", null, false, "And and null should be false")]
    [InlineData("A,B", "", false, "Or and empty should be false")]
    [InlineData("A,B", null, false, "Or and null should be false")]
    [InlineData("A", "", false, "Word and empty should be false")]
    [InlineData("A", null, false, "Word and null should be false")]
    [InlineData("!A", "", true, "Negation and empty should be true")]
    [InlineData("!A", null, true, "Negation and null should be true")]
    public void ShouldEvaluateCorrectly(string expression, string? tags, bool expectedResult, string message)
    {
        output.WriteLine(message);
        
        var parser = new ExpressionParser(expression);
        var parseResult = parser.Parse(out NodeBase ast);
        Assert.True(parseResult.IsSuccess);

        var result = ast.Evaluate(tags?.Split(','));
        Assert.Equal(expectedResult, result);
    }
}