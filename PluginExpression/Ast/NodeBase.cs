namespace PluginExpression.Ast;

public abstract class NodeBase
{
    public abstract bool Evaluate(IList<string> tags);
    
    public bool HasParsedFailed => this is ParsingFailed;
    
    public static NodeBase FailNodeBase { get; } = new ParsingFailed();
}