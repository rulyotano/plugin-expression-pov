namespace PluginExpression.Ast;

public class WordNode(string value) : NodeBase
{
    public override bool Evaluate(IList<string>? tags) => tags?.Any(tag => string.Equals(tag, value, StringComparison.InvariantCultureIgnoreCase)) ?? false;

    public override string ToString() => value;
}