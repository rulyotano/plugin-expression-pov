namespace PluginExpression.Ast;

public class ParsingFailed : NodeBase
{
    public override bool Evaluate(IList<string>? tags) => throw new ApplicationException("Can't evaluate since couldn't parse plugin expression."); 
}