namespace HULK ;

public class Evaluator
{
    SyntaxExpression _root ;

    public Evaluator(SyntaxExpression root)
    {
        _root = root ;
    }

    public int Evaluate()
    {
        return EvaluateExpression(_root);
    }

    private int EvaluateExpression(SyntaxExpression root)
    {
        if(root is NumberSyntaxExpression n)
        {    
            return (int)n.NumberToken._value ;
        }

        if(root is BinaryOpSyntaxExpression b)
        {
            var left = EvaluateExpression(b._left) ;
            var right = EvaluateExpression(b._right) ;

            if(b._optoken._kind == SyntaxKind.PlusToken)
                return left + right ;
            if(b._optoken._kind == SyntaxKind.MinusToken)
                return left - right ;
            if(b._optoken._kind == SyntaxKind.StarToken)
                return left * right ;
            if(b._optoken._kind == SyntaxKind.SlashToken)
                return left - right ;
            else
            {
                throw new Exception($"<RunTimeError> : Operador Binario no valido {b._optoken}") ;
            }
        }

        if(root is ParenthesizedExpression p)
        {
            return EvaluateExpression(p._expression);
        }
        
        throw new Exception($"<RunTimeError> : Expresion no valida {root.Kind}") ;
    }
}
