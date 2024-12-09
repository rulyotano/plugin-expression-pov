namespace PluginExpression.Nodes;

public abstract class PluginExpressionNode
{
    public abstract bool Evaluate(IList<string> tags);
    
    public bool HasParsedFailed => this is PluginExpressionFailParsedNode;
    
    public static PluginExpressionNode FailNode { get; } = new PluginExpressionFailParsedNode();
}