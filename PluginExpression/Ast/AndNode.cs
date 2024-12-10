namespace PluginExpression.Ast;

public class AndNode(NodeBase left, NodeBase right) : BinaryNodeBase(left, right)
{
    protected override Func<IList<string>, NodeBase, NodeBase, bool> EvaluateFunction { get; } = (tags, left, right) => left.Evaluate(tags) && right.Evaluate(tags);

    public override string ToString() => $"({left}) and ({right})";
}