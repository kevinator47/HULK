using System;
namespace HULK ;

/* Este es el sheet del lexer, el objetivo del lexer es recibir las lineas de codigo introducidas
por el usuario y separarla en tokens(palabras, simbolos que tendran un significado para el compilador)

Para ello, al recibir un nuevo caracter identifica el tipo de token y luego va agregando los siguientes
hasta que este completo

De momento reconocera los siguientes tokens :
. EOF
. <numeros>
. <operadores> + - * / = ( )
. <espacios>

*/

public class Lexer
{
    string _text ;
    int _position    ;

    char Current
    {
        get
        {
            if(_position >= _text.Length)
                return '\0' ;

            return _text[_position];
        }
    }

    public Lexer(string codeline)
    {
        _text = codeline ;
        _position = 0 ; // ??
    }

    public SyntaxToken GetToken()
    {
        // EOF Token(sencillo)
        if(_position >= _text.Length)
            return new SyntaxToken(TokenKind.EOFToken , _position , "\0", null) ;
        
        // Number Token
        if(char.IsDigit(Current))
        {
            int start = _position ;   // para poder recuperar mas tarde todo el token

            while(char.IsDigit(Current)  )
            {
                // sigue leyendo caracteres hasta que se encuentra uno que no sea un digito
                _position ++ ;
            }

            int length = _position - start ;
            string text = _text.Substring(start , length ) ;
            int.TryParse(text , out int value);

            return new SyntaxToken(TokenKind.NumberToken , start , text , value) ;
        }

        // WhiteSpace Token(mismo razonamiento que NumberToken)
        if(char.IsWhiteSpace(Current))
        {
            int start = _position ;   // para poder recuperar mas tarde todo el token

            while(char.IsWhiteSpace(Current)  )
            {
                // sigue leyendo caracteres hasta que se encuentra uno que no sea un digito
                _position ++ ;
            }

            int length = _position - start ;
            string text = _text.Substring(start , length ) ;

            return new SyntaxToken(TokenKind.WhiteSpaceToken , start , text , null) ;
        }

        // Operadores + - * / ( )
        if(Current == '+')
            return new SyntaxToken(TokenKind.PlusToken , _position++ , "+" , null) ;
        if(Current == '-')
            return new SyntaxToken(TokenKind.MinusToken , _position++ , "-", null) ;
        if(Current == '*')
            return new SyntaxToken(TokenKind.StarToken , _position++ , "*", null) ;
        if(Current == '/')
            return new SyntaxToken(TokenKind.SlashToken , _position++ , "/", null) ;
        if(Current == '(')
            return new SyntaxToken(TokenKind.OpenParenthesisToken , _position++ , "(", null) ;
        if(Current == ')')
            return new SyntaxToken(TokenKind.CloseParenthesisToken , _position++ , ")", null) ;
 
        // Token Invalido
        return new SyntaxToken(TokenKind.BadToken , _position ++ , _text.Substring(_position - 1 , 1), null) ;
    }
}