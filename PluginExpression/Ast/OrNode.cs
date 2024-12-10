namespace PluginExpression.Ast;

public class OrNode(NodeBase left, NodeBase right) : BinaryNodeBase(left, right)
{
    protected override Func<IList<string>, NodeBase, NodeBase, bool> EvaluateFunction { get; } = (tags, left, right) => left.Evaluate(tags) || right.Evaluate(tags);
}