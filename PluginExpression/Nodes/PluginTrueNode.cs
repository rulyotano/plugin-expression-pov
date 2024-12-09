namespace PluginExpression.Nodes;

public class PluginTrueNode() : PluginExpressionNode
{
    public override bool Evaluate(IList<string> tags) => true;
}