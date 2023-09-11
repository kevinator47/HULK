namespace HULK ;

/* Los tokens son unidades que tienen un significado especifico para el compilador

Contienen las siguientes propiedades :
    .Kind    ---->  Representa el tipo de Token(NumberToken , EOFToken , etc) los cuales seran valores de
                    un enum llamado TokenKind.
    
    .Posicion  ---------->  representa la posicion del token en la linea de codigo

    .Texto(se entiende)
    .Valor(se entiende)
*/

public enum TokenKind
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
    EOFToken
}

public class SyntaxToken
{
    public TokenKind _kind ;
    public int _position ;
    public string _text  ;
    public object _value ;

    public SyntaxToken(TokenKind kind , int position , string text , object value)
    {
        _kind = kind ;
        _position = position ;
        _text = text ;
        _value = value ;
    }


}