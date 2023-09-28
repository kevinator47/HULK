namespace HULK ;

public static class SemanticAnalisys
{
    public static bool CheckUnaryOperator(object value , SyntaxKind kind)
    {
        switch (kind)
        {
            // Operadores Aritmeticos(- , +) se ejecutan sobre un valor numerico.
            case(SyntaxKind.PlusSignToken):
            case(SyntaxKind.MinusToken):
                return (value is int); 
            
            // Operador Logico (!) se ejecuta sobre un valor booleano.
            case(SyntaxKind.NotToken):
                return (value is bool);

            // Operadores sin restricciones de tipo
            default:
                return true ; 
        }
    }

    public static bool CheckBinaryOperator(object left , SyntaxKind kind , object right)
    {
        switch (kind)
        {
            // Operadores Aritmeticos(- , + , * , /) se ejecutan sobre valores numericos.
            case(SyntaxKind.PlusSignToken):
            case(SyntaxKind.MinusToken):
            case(SyntaxKind.StarToken):
            case(SyntaxKind.SlashToken):
                return (left is int && right is int);
             
            // Operadores Logicos (& , |) se ejecutan sobre valores booleanos.
            case(SyntaxKind.AndToken):
            case(SyntaxKind.OrToken):
            case(SyntaxKind.DobleAndToken):
            case(SyntaxKind.DobleOrToken):
                return(left is bool && right is bool);
            // Operadores sin restricciones de tipo

            case(SyntaxKind.EqualEqualToken):
            case(SyntaxKind.NotEqualToken):
                return(left.GetType() == right.GetType());
            default:
                return true ; 
        }
    }
    public static bool CheckVariable(DeclaratedFunctionExpression father)
    {
        return (father != null);
    }

}