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
    NumberToken ,
    PlusToken ,
    MinusToken ,
    SlashToken ,
    StarToken ,
    EqualToken , 
    OpenParenthesisToken ,
    CloseParenthesisToken ,
    WhiteSpaceToken ,
    IdToken ,
    LetToken,
    IfToken ,
    InToken ,
    ThenToken,
    ElseToken,
    BadToken ,
    EOFToken ,
    NumExpression ,
    BinaryOperatorExpression ,
    ParenthesisExpression 
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