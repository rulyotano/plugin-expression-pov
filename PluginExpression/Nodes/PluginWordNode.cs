namespace PluginExpression.Nodes;

public class PluginWordNode(string value) : PluginExpressionNode
{
    public override bool Evaluate(IList<string> tags) => tags.Any(tag => string.Equals(tag, value, StringComparison.InvariantCultureIgnoreCase));
}