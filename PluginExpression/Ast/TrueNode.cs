namespace PluginExpression.Ast;

public class TrueNode : NodeBase
{
    public override bool Evaluate(IList<string> tags) => true;
}