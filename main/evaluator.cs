using System;
namespace HULK ;

public class Evaluator
{
    SyntaxExpression Root ;

    public Evaluator(SyntaxExpression root)
    {
        Root = root ;
    }

    public int Evaluate()
    {
        return EvaluateExpression(Root);
    }

    private int EvaluateExpression(SyntaxExpression root)
    {
        if(root is LiteralExpression n)
        {    
            return (int)n.LiteralToken._value ;
        }

        if(root is UnaryOperatorExpression u)
        {
            var value = EvaluateExpression(u.Operand);
            switch (u.OperatorToken._kind)
            {
                case(SyntaxKind.PlusSignToken):
                    return value ;
                case(SyntaxKind.MinusToken):
                    return -value ;
                default:
                    throw new Exception($"<SEMANTIC ERROR> : Operador Unario \'{u.OperatorToken}\' no esperado.");
            }
        }

        if(root is BinaryOperatorExpression b)
        {
            var left = EvaluateExpression(b.Left) ;
            var right = EvaluateExpression(b.Right) ;

            switch (b.OperatorToken._kind)
            {
                case(SyntaxKind.PlusSignToken):
                    return left + right ;

                case(SyntaxKind.MinusToken):
                    return left - right ;

                case(SyntaxKind.StarToken):
                    return left * right ;

                case(SyntaxKind.SlashToken):
                    return left / right ;
                
                default:
                    throw new Exception($"<SEMANTIC ERROR>: Operador Binario \'{b.OperatorToken}\' no esperado.") ;
            }
        }

        if(root is ParenthesizedExpression p)
        {
            return EvaluateExpression(p.Expression);
        }
        
        throw new Exception($"<SEMANTIC ERROR> : Expresion \'{root.Kind}\' no esperada.") ;
    }
}
