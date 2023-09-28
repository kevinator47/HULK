namespace HULK ;

/* Los tokens son unidades que tienen un significado especifico para el compilador

Contienen las siguientes propiedades :
    ._kind    ---->  Representa el tipo de Token(NumberToken , EOFToken , etc) los cuales seran valores de
                    un enum llamado SyntaxKind.
    
    ._position  ---------->  representa la posicion del token en la linea de codigo

    ._text(se entiende)
    ._value(se entiende)
*/

public enum SyntaxKind
{
    // Operadores Aritmeticos
    PlusSignToken ,
    MinusToken ,
    SlashToken ,
    StarToken ,
    EqualToken ,

    // Operadores Logicos
    NotToken ,
    AndToken ,
    OrToken ,
    DobleAndToken ,
    DobleOrToken ,
    NotEqualToken ,
    EqualEqualToken ,
    
    // Tokens
    LiteralToken , 
    OpenParenthesisToken ,
    CloseParenthesisToken ,
    WhiteSpaceToken ,
    CommaSeparatorToken,
    ArrowToken,
    BadToken ,
    EOFToken ,

    // Keywords & Identifier
    LetToken,
    IfToken ,
    InToken ,
    ThenToken,
    ElseToken,
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
    DeclaratedFunctionExpression ,
    FunctionCallExpression
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