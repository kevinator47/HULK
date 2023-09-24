using System.ComponentModel;
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

    public SyntaxExpression Root ;
    public SyntaxToken EOF ;
    public List<string> _bugs ;

    public SyntaxTree(List<string> diagnostic , SyntaxExpression root , SyntaxToken eofileToken)
    {
        Root = root ;
        EOF = eofileToken ;
        _bugs = diagnostic ; 
    }


}
public abstract class Node{
    public abstract SyntaxKind Kind {get;set;} 
}

public abstract class SyntaxExpression : Node{}

public sealed class BinaryOperatorExpression : SyntaxExpression{

    public override SyntaxKind Kind {get;set;}
    public SyntaxExpression Left ;
    public SyntaxToken OperatorToken ;
    public SyntaxExpression Right ;

    public BinaryOperatorExpression(SyntaxExpression left , SyntaxToken optoken , SyntaxExpression right)
    {
        Kind = SyntaxKind.BinaryOperatorExpression ;
        Left = left ;
        OperatorToken = optoken ;
        Right = right ;
    }
}

public sealed class UnaryOperatorExpression : SyntaxExpression
{
    public override SyntaxKind Kind {get;set;}
    public SyntaxToken OperatorToken ;
    public SyntaxExpression Operand ;

    public UnaryOperatorExpression(SyntaxToken operatorToken , SyntaxExpression operand)
    {
        Kind = SyntaxKind.UnaryOperatorExpression ;
        OperatorToken = operatorToken ;
        Operand = operand ;
    }

}

public sealed class ParenthesizedExpression : SyntaxExpression
{
    public override SyntaxKind Kind {get;set;}
    public SyntaxToken OpenParenthesis ;
    public SyntaxExpression Expression ;
    public SyntaxToken CloseParenthesis ;

    public ParenthesizedExpression(SyntaxToken openParenthesis , SyntaxExpression expression , SyntaxToken closeParenthesis)
    {
        Kind = SyntaxKind.ParenthesizedExpression ;
        OpenParenthesis = openParenthesis ;
        Expression = expression ;
        CloseParenthesis = closeParenthesis ;
    }
}

public sealed class LiteralExpression : SyntaxExpression{

    public SyntaxToken LiteralToken ;
    public override SyntaxKind Kind {get;set;}

    public LiteralExpression(SyntaxToken litToken)
    {
        Kind = SyntaxKind.LiteralExpression ;
        LiteralToken = litToken ;
    }
}


