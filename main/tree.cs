namespace HULK ;

/* TREE'S SHEET
Aqui se implementa el arbol de expresiones validas en hulk el cual seria

        Nodo
      /
  Expression
   /      \
 NumExp   BinaryOPExp

*/
public sealed class SyntaxTree{

    public SyntaxExpression _root ;
    public SyntaxToken EOF ;
    public List<string> _bugs ;

    public SyntaxTree(List<string> diagnostic , SyntaxExpression root , SyntaxToken eofileToken)
    {
        _root = root ;
        EOF = eofileToken ;
        _bugs = diagnostic ; 
    }


}
public abstract class Node{
    public abstract SyntaxKind Kind {get;set;} 
}

public abstract class SyntaxExpression : Node{}

public sealed class NumberSyntaxExpression : SyntaxExpression{

    public SyntaxToken NumberToken ;
    public override SyntaxKind Kind {get;set;}

    public NumberSyntaxExpression(SyntaxToken numToken)
    {
        Kind = SyntaxKind.NumExpression ;
        NumberToken = numToken ;
    }
}

public sealed class BinaryOpSyntaxExpression : SyntaxExpression{

    public override SyntaxKind Kind {get;set;}
    public SyntaxExpression _left ;
    public SyntaxToken _optoken ;
    public SyntaxExpression _right ;

    public BinaryOpSyntaxExpression(SyntaxExpression left , SyntaxToken optoken , SyntaxExpression right)
    {
        Kind = SyntaxKind.BinaryOperatorExpression ;
        _left = left ;
        _optoken = optoken ;
        _right = right ;
    }
}

public sealed class ParenthesizedExpression : SyntaxExpression
{
    public override SyntaxKind Kind {get;set;}
    public SyntaxToken _OpParenthesis ;
    public SyntaxExpression _expression ;
    public SyntaxToken _ClsParenthesis ;

    public ParenthesizedExpression(SyntaxToken OpParenthesis , SyntaxExpression expression , SyntaxToken ClsParenthesis)
    {
        Kind = SyntaxKind.ParenthesisExpression ;
        _OpParenthesis = OpParenthesis ;
        _expression = expression ;
        _ClsParenthesis = ClsParenthesis ;
    }
}