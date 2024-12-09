namespace PluginExpression.Nodes;

public class PluginExpressionFailParsedNode : PluginExpressionNode
{
    public override bool Evaluate(IList<string> tags) => throw new ApplicationException("Can't evaluate since couldn't parse plugin expression."); 
}