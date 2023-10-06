namespace HULK ;

/* Los tokens son unidades que tienen un significado especifico para el compilador

Contienen las siguientes propiedades :
    ._kind      :  Representa el tipo de token
    ._position  :  Representa la posicion del token en la linea de codigo
    ._text      :  Representa el "nombre" del token ("x" , ">=" , "function", etc)
    ._value     :  Representa el valor del token(util en las variables)
*/

// Conjunto de Tokens y Expresiones del HULK
public enum SyntaxKind
{
    // Operadores Aritmeticos
    PlusSignToken ,
    MinusToken ,
    SlashToken ,
    StarToken ,
    ExponentToken ,
    PercentageToken ,
    EqualToken ,
    ArrobaToken ,

    // Operadores Logicos
    NotToken ,
    AndToken ,
    OrToken ,
    DobleAndToken ,
    DobleOrToken ,
    NotEqualToken ,
    EqualEqualToken ,
    MoreToken,
    LessToken ,
    MoreOrEqualToken ,
    LessOrEqualToken ,
    
    // Tokens
    LiteralToken , 
    OpenParenthesisToken ,
    CloseParenthesisToken ,
    WhiteSpaceToken ,
    CommaSeparatorToken,
    ArrowToken,
    BadToken ,
    EOLToken ,
    EOFToken ,


    // Keywords & Identifier
    LetKwToken,
    IfKwToken ,
    InKwToken ,
    ThenKwToken,
    ElseKwToken,
    TrueToken,
    FalseToken ,
    IdentifierToken ,
    FunctionKwToken,

    // Expresiones
    LiteralExpression ,
    BinaryOperatorExpression ,
    UnaryOperatorExpression ,
    ParenthesizedExpression ,
    VariableExpression,
    DeclaredFunctionExpression ,
    FunctionCallExpression ,
    LetInExpression
}

public class SyntaxToken
{
    public SyntaxKind _kind ;
    public int _position ;
    public string _text  ;
    public object _value ;

    public SyntaxToken(SyntaxKind kind , int position , string text , object value)
    {
        _kind = kind ;
        _position = position ;
        _text = text ;
        _value = value ;             
    }
}