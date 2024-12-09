namespace PluginExpression.Nodes;

public class PluginExpressionAndNode(PluginExpressionNode left, PluginExpressionNode right) : PluginExpressionBinaryNode(left, right)
{
    protected override Func<IList<string>, PluginExpressionNode, PluginExpressionNode, bool> EvaluateFunction { get; } = (tags, left, right) => left.Evaluate(tags) && right.Evaluate(tags);
}