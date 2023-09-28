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
public sealed class DeclaratedFunctionExpression: SyntaxExpression{

    public override SyntaxKind Kind {get;set;}
    public string Name ;
    public SyntaxExpression Body ;

    public SyntaxToken[] Parameters ;
    public List<SyntaxToken> Scope ;

    public DeclaratedFunctionExpression(string name , List<SyntaxToken> parameters )
    {
        Kind = SyntaxKind.DeclaratedFunctionExpression ;
        Name = name ;
        Parameters = parameters.ToArray();
        Scope = new List<SyntaxToken>(parameters);   
    }

    public override bool Equals(object obj)
    {
        // dos funciones son iguales si tienen el mismo nombre y misma cantidad de parametros
        if(obj == null || GetType() != obj.GetType())
            return false ;
        
        var other = (DeclaratedFunctionExpression)obj ;

        return (Name == other.Name && Parameters.Length == other.Parameters.Length) ;
    }

    public void SetValues(object[] values)
    {
        for(int i = 0 ; i < values.Length ; i ++)
        {
            Scope.ElementAt(i)._value = values[i];
        }
    }

}
public sealed class FunctionCallExpression : SyntaxExpression{
    public override SyntaxKind Kind {get;set;}
    public DeclaratedFunctionExpression ExecutableFunction ;
    public SyntaxExpression[] Args ;

    public FunctionCallExpression(DeclaratedFunctionExpression executableFunction , SyntaxExpression[] args)
    {
        Kind = SyntaxKind.FunctionCallExpression ;
        ExecutableFunction = executableFunction ;
        Args = args ;
    }
}
public sealed class VariableExpression : SyntaxExpression{
    public SyntaxToken IdToken ;
    public DeclaratedFunctionExpression Father ; // crear un tipo de expresion llamado ScopedExpression
    public override SyntaxKind Kind {get;set;}

    public VariableExpression(SyntaxToken idtoken , DeclaratedFunctionExpression father)
    {
        IdToken = idtoken ;
        Father = father ;
        Kind = SyntaxKind.VariableExpression ;
    }

    public object GetValue()
    {
        // busca en el Scope del padre el token que tenga el mismo nombre
        SyntaxToken matchingToken = Father.Scope.FirstOrDefault(token => token._text == IdToken._text);
        
        if(matchingToken != null)
        {
            return matchingToken._value ;  // si lo encuentra devuelve el valor
        }
        else
        {
            // si no lo encuentra no existe y por tanto lanza una excepcion
            throw new Exception($"<RunTime Error> : Variable {IdToken._text} no existe en el contexto actual.");
        }
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


