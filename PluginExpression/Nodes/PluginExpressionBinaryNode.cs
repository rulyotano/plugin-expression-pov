namespace PluginExpression.Nodes;

public abstract class PluginExpressionBinaryNode(PluginExpressionNode left, PluginExpressionNode right)
    : PluginExpressionNode
{
    protected abstract Func<IList<string>, PluginExpressionNode, PluginExpressionNode, bool> EvaluateFunction { get; }
    
    public override bool Evaluate(IList<string> tags) => EvaluateFunction(tags, left, right);
}