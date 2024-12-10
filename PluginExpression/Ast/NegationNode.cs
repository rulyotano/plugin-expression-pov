namespace PluginExpression.Ast;

public class NegationNode(NodeBase nodeBase) : NodeBase
{
    public override bool Evaluate(IList<string> tags) => !nodeBase.Evaluate(tags);
}