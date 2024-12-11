namespace PluginExpression.Ast;

public abstract class BinaryNodeBase(NodeBase left, NodeBase right)
    : NodeBase
{
    protected abstract Func<IList<string>, NodeBase, NodeBase, bool> EvaluateFunction { get; }
    
    public override bool Evaluate(IList<string>? tags) => EvaluateFunction(tags ?? [], left, right);
}