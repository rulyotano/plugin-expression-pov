namespace PluginExpression.Nodes;

public class PluginNegationNode(PluginExpressionNode node) : PluginExpressionNode
{
    public override bool Evaluate(IList<string> tags) => !node.Evaluate(tags);
}