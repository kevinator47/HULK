using System.Collections.Generic;
namespace HULK ;

// Arbol Sintactico
/* Una estructura donde cada expresion es un nodo lo que permite relacionarlas entre si 

Existen los siguientes tipos de Nodos :
(Todas las expresiones tienen una propiedad llamada Kind que se refiere al tipo de expresion).
    
. SyntaxExpression (abstact) :  Una clase solo para que todas las diferentes expresiones tengan un nombre comun.
    
. BinaryOperatorExpression   :  Contiene dos expresiones(Left y Right) relacionadas por un operador binario(OperatorToken).

. UnaryOperatorExpression    :  Contiene un operador(Operator Token) que modifica a una expresion(Operand)

. ParenthesizedExpression    :  Contiene una expresion(Expression) dentro de dos parentesis.

. FunctionExpression(abstract) :  Clase para agrupar todas las funciones(predefinidas y declaradas)

*/


public sealed class SyntaxTree{

    public SyntaxExpression[] Roots ;
    public SyntaxToken EOF ;
    public List<string> _bugs ;

    public SyntaxTree(SyntaxExpression[] roots , SyntaxToken eofileToken)
    {
        Roots = roots ;
        EOF = eofileToken ; 
    }
}
public abstract class Node{
    public abstract SyntaxKind Kind {get;set;} 
}

public abstract class SyntaxExpression : Node{
    public abstract object Evaluate(Dictionary<string , object> symbolTable = null);

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

    public override object Evaluate(Dictionary<string , object> symbolTable = null)
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

    public override object Evaluate(Dictionary<string , object> symbolTable = null)
    {
        var value = Operand.Evaluate(symbolTable);
        
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
                    return (double)value ;
                case(SyntaxKind.MinusToken):
                    return -(double)value ;
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
                return !(value is double); 
                
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
            // Operadores(- , + , * , / , > , ^ , % , > , < , <= , >=) se ejecutan sobre valores numericos.
            case(SyntaxKind.PlusSignToken):
            case(SyntaxKind.MinusToken):
            case(SyntaxKind.StarToken):
            case(SyntaxKind.SlashToken):
            case(SyntaxKind.ExponentToken):
            case(SyntaxKind.MoreToken):
            case(SyntaxKind.MoreOrEqualToken):
            case(SyntaxKind.LessToken):
            case(SyntaxKind.LessOrEqualToken):
            case(SyntaxKind.PercentageToken):
                return !(left is double && right is double);
             
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

    public override object Evaluate(Dictionary<string , object> symbolTable = null)
    {
        var left = Left.Evaluate(symbolTable);
        var right = Right.Evaluate(symbolTable);

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
                return (double)left + (double)right ;

            case(SyntaxKind.MinusToken):
                return (double)left - (double)right ;

            case(SyntaxKind.StarToken):
                return (double)left * (double)right ;
                
            case(SyntaxKind.SlashToken):
                return (double)left / (double)right ;
            
            case(SyntaxKind.ExponentToken):
                return Math.Pow((double) left , (double) right)  ;
            
            case(SyntaxKind.PercentageToken):
                return (double)left % (double)right ;
            
            case(SyntaxKind.ArrobaToken):
                return left.ToString() + right.ToString();
                
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

            case(SyntaxKind.MoreToken):
                return (double)left > (double)right ;
           
            case(SyntaxKind.MoreOrEqualToken):
                return (double)left >= (double)right ;
           
            case(SyntaxKind.LessToken):
                return (double)left < (double)right ;
           
            case(SyntaxKind.LessOrEqualToken):
                return (double)left <= (double)right ;  

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
    public override object Evaluate(Dictionary<string , object> symbolTable = null)
    {
        return Expression.Evaluate(symbolTable);
    }
}

public sealed class VariableExpression : SyntaxExpression{
    public override SyntaxKind Kind {get;set;}
    public string Name ; 
    

    public VariableExpression(string name)
    {
        Kind = SyntaxKind.VariableExpression ;
        Name = name ;
    }

    public override object Evaluate(Dictionary<string, object> symbolTable = null)
    {
        // Chequeando si existe la variable en el scope actual
        if( symbolTable != null && symbolTable.ContainsKey(Name))
            return symbolTable[Name];
        
        CompilatorTools.Bugs.Add($"<RuntimeError> : Variable {Name} does not exist in current context.");
        return null ;
    }
}

public sealed class IfExpression : SyntaxExpression{
    public override SyntaxKind Kind {get;set;}
    SyntaxExpression Condition ;
    SyntaxExpression TrueBranch ;
    SyntaxExpression FalseBranch ;

    public IfExpression(SyntaxExpression condition , SyntaxExpression truebranch , SyntaxExpression falsebranch)
    {
        Condition = condition ;
        TrueBranch = truebranch ;
        FalseBranch = falsebranch ;
    }
    private bool SemanticErrors(Dictionary<string , object> symbolTable)
    {
        return !(Condition.Evaluate(symbolTable) is bool) ;
    }
    public override object Evaluate(Dictionary<string , object> symbolTable = null)
    {
        if(SemanticErrors(symbolTable))
        {
            CompilatorTools.Bugs.Add("<Semantic Error> : if condition must be a boolean expresion.");
            return null ;
        }
        if((bool)Condition.Evaluate(symbolTable))
        {
            return TrueBranch.Evaluate(symbolTable) ;
        }
        return FalseBranch.Evaluate(symbolTable) ;
    } 

}
public abstract class FunctionExpression : SyntaxExpression{
    public string Name ;
    public List<string> Args ;
    public SyntaxExpression Body ;

    public int Len ;

    public abstract object Execute(List<object> parameters , Dictionary<string , object> symbolTable = null);

    }
    

public sealed class DeclaredFunctionExpression: FunctionExpression{

    public override SyntaxKind Kind {get;set;}

    public DeclaredFunctionExpression(string name , List<string> args , SyntaxExpression body ) 
    {
        Kind = SyntaxKind.DeclaredFunctionExpression ;
        Name = name ;
        Args = args ;
        Body = body ;   
        Len = Args.Count;
    }

    public override bool Equals(object obj)
    {
        // Dos funciones son iguales si tienen el mismo nombre y misma cantidad de parametros
        if(obj == null)
        {
            return false ;
        }
        var other = (FunctionExpression)obj ;

        return (Name == other.Name && Len == other.Len) ;
    }

    // Hash code para que no me salga la advertencia
    public override object Evaluate(Dictionary<string , object> symbolTable = null)
    {
        if(FunctionPool.CheckIfExist(this))
            CompilatorTools.Bugs.Add($"<Semantic Error> : Function \'{Name}\' receiving {Args.Count} argument(s) already exist.");

        FunctionPool.FunctionList.Add(this); 
        return null;
    }

    public override object Execute(List<object> parameters , Dictionary<string , object> symbolTable = null)
    {
        
        // Crear una nueva symbolTable para la llamada actual
        var localSymbolTable = new Dictionary<string , object>(symbolTable);

        // Setear los valores de los parametros a las variables correspondientes
        for(int i = 0 ; i < Args.Count ; i++)
        {
            localSymbolTable[Args[i]] = parameters[i];
        }

        return Body.Evaluate(localSymbolTable);
    }
}

public sealed class PredefinedFunctionExpreesion : FunctionExpression{
    public override SyntaxKind Kind {get;set;}
    

    public PredefinedFunctionExpreesion(string name , int len)
    {
        Name = name ;
        Len = len ;
    }

    public override object Evaluate(Dictionary<string, object> symbolTable = null)
    {
        return null ;
    }

    public override object Execute(List<object> ArgValues , Dictionary <string , object> symbolTable = null)
    {        
        // chequeo semantico
        
        switch(Name)
        {
        case("sin"):
            return Math.Sin((double) ArgValues.ElementAt(0));
        case("cos"):
            return Math.Cos((double) ArgValues.ElementAt(0)) ;            
        case("tan"):
            return Math.Tan((double) ArgValues.ElementAt(0)) ;
        case("print"):
            return ArgValues.ElementAt(0) ;
        default:
            return null ;
        }
    }
}
public sealed class LetInExpression : SyntaxExpression{
    public override SyntaxKind Kind {get;set;}
    public Dictionary<string , SyntaxExpression> Assigment ;
    public SyntaxExpression Body ;

    public LetInExpression(Dictionary<string , SyntaxExpression> assigment , SyntaxExpression body)
    {
        Kind = SyntaxKind.LetInExpression ;
        Body = body ;
        Assigment = assigment ;
    }    
    public override object Evaluate(Dictionary<string,object> symbolTable = null)
    {
        if(symbolTable == null)
            symbolTable = new Dictionary<string, object>();

        Dictionary<string, object> localSymbolTable = new Dictionary<string, object>();

        foreach (var item in symbolTable.Keys)
        {
           localSymbolTable[item] = symbolTable[item]; 
        }

        foreach (var variable in Assigment.Keys)
        {
            localSymbolTable[variable] = Assigment[variable].Evaluate(symbolTable);
        }
                
        return Body.Evaluate(localSymbolTable);
    }
}
public sealed class FunctionCallExpression : SyntaxExpression{
    public override SyntaxKind Kind {get;set;}
    public string Name ;
    public List<SyntaxExpression> Args ;

    public FunctionCallExpression(string name , List<SyntaxExpression> args)
    {
        Kind = SyntaxKind.FunctionCallExpression ;
        Name = name ;
        Args = args ;
    }
    public override object Evaluate(Dictionary <string , object> symbolTable = null)
    {
        // obteniendo los valores de los parametros de la llamada
        var argValues = new List<object>();
        
        foreach(var argument in Args)
        {
            argValues.Add(argument.Evaluate(symbolTable));
        }

        // buscando la funcion 
        var function = FunctionPool.Find(Name , Args.Count);

        if(function == null)
        {
            CompilatorTools.Bugs.Add($"<RuntimeError> : Function {Name} receiving {Args.Count} parameter(s) does not exist.");
            return null ;
        }

        if(symbolTable == null)
            symbolTable = new Dictionary<string , object>();

        return function.Execute(argValues , symbolTable);

    }
}


