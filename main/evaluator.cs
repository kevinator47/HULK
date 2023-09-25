using System;
namespace HULK ;

public class Evaluator
{
    SyntaxExpression Root ;
    private List<string> _bugs = new List<string> ();
    public IEnumerable<string> Bugs => _bugs ;

    public Evaluator(SyntaxExpression root)
    {
        Root = root ;
    }

    public object Evaluate()
    {
        return EvaluateExpression(Root);
    }

    private object EvaluateExpression(SyntaxExpression root)
     {
        if(root is LiteralExpression n)
        {    
            return n.LiteralToken._value ;
        }

        if(root is UnaryOperatorExpression u)
        {
            var value = EvaluateExpression(u.Operand);
            SyntaxKind kind = u.OperatorToken._kind ;
           

            // chequea que la expresion tenga sentido semanticamente
            if(! SemanticAnalisys.CheckUnaryOperator(value , kind)) 
                {
                    _bugs.Add($"<Semantic Error> : Operador Unario \"{u.OperatorToken._text}\" no valido para {value.GetType()}");
                    return 0 ;
                }

            switch (kind)
            {
                case(SyntaxKind.PlusSignToken):
                    return (int)value ;
                case(SyntaxKind.MinusToken):
                    return -(int)value ;
                case(SyntaxKind.NotToken):
                    return !(bool)value ;
                default:
                    throw new Exception($"<Lexical Error> : Operador Unario \'{kind}\' no definido.");
            }
        }

        if(root is BinaryOperatorExpression b)
        {
            var left = EvaluateExpression(b.Left) ;
            var right = EvaluateExpression(b.Right) ;
            SyntaxKind kind = b.OperatorToken._kind ;

            // chequea que la expresion tenga sentido semanticamente
            if(! SemanticAnalisys.CheckBinaryOperator(left , kind , right ))
            {
                _bugs.Add($"<Semantic Error> : Operador Binario \"{b.OperatorToken._text}\" no valido para tipos : {left.GetType()} & {right.GetType()} ");
                return 0 ;
            }

            switch (b.OperatorToken._kind)
            {
                case(SyntaxKind.PlusSignToken):
                    return (int)left + (int)right ;

                case(SyntaxKind.MinusToken):
                    return (int)left - (int)right ;

                case(SyntaxKind.StarToken):
                    return (int)left * (int)right ;

                case(SyntaxKind.SlashToken):
                    return (int)left / (int)right ;
                
                case(SyntaxKind.AndToken):
                    return (bool)left & (bool)right ;
                
                case(SyntaxKind.OrToken):
                    return (bool)left | (bool) right;
                
                default:
                    throw new Exception($"<Lexical Error>: Operador Binario \'{b.OperatorToken}\' no definido.") ;
            }
        }

        if(root is ParenthesizedExpression p)
        {
            return EvaluateExpression(p.Expression);
        }
        
        throw new Exception($"<Lexical Error> : Expresion \'{root.Kind}\' no definida.") ;
    }
}
