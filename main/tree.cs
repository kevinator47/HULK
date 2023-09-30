namespace HULK ;

// Arbol Sintactico
/* Una estructura donde cada expresion es un nodo lo que permite relacionarlas entre si 

Existen los siguientes tipos de Nodos :
(Todas las expresiones tienen una propiedad llamada Kind que se refiere al tipo de expresion).
    
. SyntaxExpression (abstact) :  Una clase solo para que todas las diferentes expresiones tengan un nombre comun.
    
. BinaryOperatorExpression   :  Contiene dos expresiones(Left y Right) relacionadas por un operador binario(OperatorToken).

. UnaryOperatorExpression    :  Contiene un operador(Operator Token) que modifica a una expresion(Operand)

. ParenthesizedExpression    :  Contiene una expresion(Expression) dentro de dos parentesis.

. ScopedExpression(abstract) :  Clase para agrupar todas las expresiones que tienen Scope (funciones, let-in , if-else)

*/


public sealed class SyntaxTree{

    public SyntaxExpression[] Roots ;
    public SyntaxToken EOF ;
    public List<string> _bugs ;

    public SyntaxTree(List<string> diagnostic , SyntaxExpression[] roots , SyntaxToken eofileToken)
    {
        Roots = roots ;
        EOF = eofileToken ;
        _bugs = diagnostic ; 
    }
}
public abstract class Node{
    public abstract SyntaxKind Kind {get;set;} 
}

public abstract class SyntaxExpression : Node{
    public abstract object Evaluate();

    public bool ErrorHasOcurred()
    {
        return (CompilatorTools.Bugs.Count != 0);
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

    public override object Evaluate()
    {
        return LiteralToken._value;
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

    public override object Evaluate()
    {
        var value = Operand.Evaluate();
        
        if(ErrorHasOcurred())
            return null ;
        
        if(SemanticErrors(value))
        {
            CompilatorTools.Bugs.Add($"<Semantic Error> : Unary operator \"{OperatorToken._text}\" cannot be used with {value.GetType()}");
            return null ;
        }

        switch (OperatorToken._kind)
            {
                case(SyntaxKind.PlusSignToken):
                    return (int)value ;
                case(SyntaxKind.MinusToken):
                    return -(int)value ;
                case(SyntaxKind.NotToken):
                    return !(bool)value ;
                default:
                    throw new Exception($"<Lexical Error> : Unary Operator \'{OperatorToken._text}\' not defined.");
            }
    }

    public bool SemanticErrors(object value)
    {
        // Busca errores entre el tipo del valor y el operador.
        switch (OperatorToken._kind)
        {
            // Operadores Aritmeticos(- , +) se ejecutan sobre un valor numerico.
            case(SyntaxKind.PlusSignToken):
            case(SyntaxKind.MinusToken):
                return !(value is int); 
                
            // Operador Logico (!) se ejecuta sobre un valor booleano.
            case(SyntaxKind.NotToken):
                return !(value is bool);

            // Operadores sin restricciones de tipo
            default:
                return false ; 
        }
    }
}


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

    public bool SemanticErrors( object left , object right )
    {
        switch (OperatorToken._kind)
        {
            // Operadores Aritmeticos(- , + , * , /) se ejecutan sobre valores numericos.
            case(SyntaxKind.PlusSignToken):
            case(SyntaxKind.MinusToken):
            case(SyntaxKind.StarToken):
            case(SyntaxKind.SlashToken):
                return !(left is int && right is int);
             
            // Operadores Logicos (& , |) se ejecutan sobre valores booleanos.
            case(SyntaxKind.AndToken):
            case(SyntaxKind.OrToken):
            case(SyntaxKind.DobleAndToken):
            case(SyntaxKind.DobleOrToken):
                return !(left is bool && right is bool);
            // Operadores sin restricciones de tipo

            case(SyntaxKind.EqualEqualToken):
            case(SyntaxKind.NotEqualToken):
                return !(left.GetType() == right.GetType());
            default:
                return false ; 
        }
    }

    public override object Evaluate()
    {
        var left = Left.Evaluate();
        var right = Right.Evaluate();

        if(ErrorHasOcurred())
            return null ;

        if(SemanticErrors(left , right))
        {
            CompilatorTools.Bugs.Add($"<Semantic Error> : Binary Operator \"{OperatorToken._text}\" no valid for types : {left.GetType()} & {right.GetType()} ");
            return null ;
        }

        switch (OperatorToken._kind)
        {
            case(SyntaxKind.PlusSignToken):
                return (int)left + (int)right ;

            case(SyntaxKind.MinusToken):
                return (int)left - (int)right ;

            case(SyntaxKind.StarToken):
                return (int)left * (int)right ;
                
            case(SyntaxKind.SlashToken):
                return (int)left / (int)right ;
                
            case(SyntaxKind.DobleAndToken):
                return (bool)left && (bool)right ;
                
            case(SyntaxKind.DobleOrToken):
                return (bool)left || (bool) right;

            case(SyntaxKind.AndToken):
                return (bool)left & (bool)right ;
                
            case(SyntaxKind.OrToken):
                return (bool)left | (bool) right;
                
            case(SyntaxKind.EqualEqualToken):
                return left.Equals(right);  

            case(SyntaxKind.NotEqualToken):
                return !(left.Equals(right));
                
            default:
                throw new Exception($"<Lexical Error>: Binary Operator \'{OperatorToken._text}\' not defined.") ;
        }
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
    public override object Evaluate()
    {
        return Expression.Evaluate();
    }
}

public sealed class VariableExpression : SyntaxExpression{
    public SyntaxToken IdToken ;
    public DeclaredFunctionExpression Father ; // crear un tipo de expresion llamado ScopedExpression
    public override SyntaxKind Kind {get;set;}

    public VariableExpression(SyntaxToken idtoken , DeclaredFunctionExpression father)
    {
        IdToken = idtoken ;
        Father = father ;
        Kind = SyntaxKind.VariableExpression ;
    }

    public override object Evaluate()
    {
        // busca en el Scope del padre el token que tenga el mismo nombre  
        SyntaxToken matchingToken = Father.Scope.FirstOrDefault(token => token._text == IdToken._text);
        
        if(matchingToken != null)
        {
            return matchingToken._value ;  // si lo encuentra devuelve el valor
        }
        else
        {
            // si no lo encuentra no esta definida en el ambito
            CompilatorTools.Bugs.Add($"<Semantic Error> : Variable \"{IdToken._text}\" not defined in {Father.Kind} {Father.Name}");
            return null ;

        }
    }
}

public abstract class ScopedExpression : SyntaxExpression{}
public sealed class DeclaredFunctionExpression: ScopedExpression{

    public override SyntaxKind Kind {get;set;}
    public string Name ;
    public SyntaxExpression Body ;

    public SyntaxToken[] Parameters ;
    public List<SyntaxToken> Scope ;

    public DeclaredFunctionExpression(string name , List<SyntaxToken> parameters )
    {
        Kind = SyntaxKind.DeclaredFunctionExpression ;
        Name = name ;
        Parameters = parameters.ToArray();
        Scope = new List<SyntaxToken>(parameters);   
    }
    public void DeclarateBody(SyntaxExpression body)
    {
        // El body se setea por separado para poder pasar la funcion como padre de las variables en él.
        Body = body ;
    }

    public override bool Equals(object obj)
    {
        // Dos funciones son iguales si tienen el mismo nombre y misma cantidad de parametros
        if(obj == null || GetType() != obj.GetType())
            return false ;
        
        var other = (DeclaredFunctionExpression)obj ;

        return (Name == other.Name && Parameters.Length == other.Parameters.Length) ;
    }

    // Hash code para que no me salga la advertencia
    public void SetValues(object[] values)
    {
        // Al recibir un llamado setea los valores en el scope.
        for(int i = 0 ; i < values.Length ; i ++)
        {
            Scope.ElementAt(i)._value = values[i];
        }
    }
    public override object Evaluate()
    {
        if(FunctionPool.CheckIfExist(this))
                CompilatorTools.Bugs.Add($"<Semantic Error> : Function \'{Name}\' receiving {Parameters.Length} argument(s) already exist.");

        FunctionPool.FunctionList.Add(this); 
        Console.WriteLine("Function added to list");  
        return null;
    }

    public object EvaluateBody()
    {
        return Body.Evaluate();
    }
}
public sealed class FunctionCallExpression : SyntaxExpression{
    public override SyntaxKind Kind {get;set;}
    public DeclaredFunctionExpression ExecutableFunction ;
    public SyntaxExpression[] Args ;

    public FunctionCallExpression(DeclaredFunctionExpression executableFunction , SyntaxExpression[] args)
    {
        Kind = SyntaxKind.FunctionCallExpression ;
        ExecutableFunction = executableFunction ;
        Args = args ;
    }
    public override object Evaluate()
    {
        List<object> values = new List<object>() ;

        for(int i = 0 ; i < Args.Length ; i ++)
        {
            values.Add(Args[i].Evaluate());
        }
        ExecutableFunction.SetValues(values.ToArray());
        return ExecutableFunction.EvaluateBody();
    }
}


