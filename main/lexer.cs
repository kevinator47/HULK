using System.ComponentModel;
using System.Text;
namespace HULK ;

/* Lexer
Es la parte del programa encargada de recibir una cadena de caracteres e identificar los distintos tokens que conforman el lenguaje de HULK

Es capaz de reconocer los siguientes Tokens :
.NumberLiteral Token
.StringLiteral Token (no implementado aun)
.WhiteSpace Token
.Identifier Token
.Keyword Token
.Operator Token
.EOF Token
.Bad Token(tokens incorrectos)

Contiene las siguientes propiedades : 
    _text     :  para almacenar la cadena de caracteres que representa el codigo HULK.
    _position :  para ir iterando sobre los caracteres.
    Current   :  una propiedad que se refiere al token actual.
    Bugs      :  una lista para almacenar los errores encontrados.

Contiene los siguientes metodos :
    Peek(n)   :  permite ver el caracter que se encuentra a una distancia "n" del actual.
    GetToken():  donde se lleva a cabo el proceso de identificar y obtener los tokens

Para identificar los tokens se intenta reconocer el tipo de token segun el primer caracter encontrado, luego se siguen mirando caracteres hasta completar
el token en cada caso.

Ejemplo : 

if(a <= b then "menor" else "mayor")
|if| |(| |a| |<=| |b| |then| |"menor"| |else| |"mayor| |)|

Poner las instrucciones de completar los tokens en funciones aparte 
*/

public class Lexer
{
    string _text  ;
    int _position ;
    char Current{
    // devuelve el caracter actual
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

    private char Peek(int distance)
    {
        // Permite ver el caracter que se encuentra a cierta distancia del actual
        int index = _position + distance ;
        
        if(index >= _text.Length)
            return '\0';

        return _text[index];
    }

    public SyntaxToken GetToken()
    {
        // End of Line tokens
        if(Current == ';')
            return new SyntaxToken(SyntaxKind.EOLToken , _position++ , ";", null);

        // End Of File tokens
        if(_position >= _text.Length)
            return new SyntaxToken(SyntaxKind.EOFToken , _position , "\0", null) ;
        
        // Number Token
        if(char.IsDigit(Current))
        {
            int start = _position ;   // para poder recuperar mas tarde todo el token

            bool IsDecimal = false ; 
            
            // Sigue leyendo caracteres hasta que se encuentra uno que no sea un digito
            while(char.IsDigit(Current) || (Current == '.' && ! IsDecimal) )
            {
                if(Current == '.')
                    IsDecimal = true ;
                _position ++ ;
            }

            int length = _position - start ;
            string text = _text.Substring(start , length ) ;
            
            if(!double.TryParse(text , out double value))
                CompilatorTools.Bugs.Add($"<Lexical Error>: {text} cannot be represented by type Number.");
                
            return new SyntaxToken(SyntaxKind.LiteralToken , start , text , value) ;
        }

        // String Literals
        if(Current == '"')
        {
            int start = _position ++ ;

            var sb = new StringBuilder();
            bool done = false ;

            while(!done)
            {
                switch (Current)
                {
                    case '\0' :
                        CompilatorTools.Bugs.Add($"<Lexical Error> : Unterminated string literal");
                        done = true ;
                        break ;
                    
                    case '"' :
                        done = true ;
                        _position ++ ;
                        break ;
                    
                    default:
                        sb.Append(Current) ;
                        _position ++ ;
                        break ;
                }
            }
            
            string value = sb.ToString();
            string text = "\"" + value + "\"" ;

            return new SyntaxToken(SyntaxKind.LiteralToken , start , text , value);
        }

        // WhiteSpace Token
        if(char.IsWhiteSpace(Current))
        {
            int start = _position ;   

            while(char.IsWhiteSpace(Current)  )
            {
                _position ++ ;
            }

            int length = _position - start ;
            string text = _text.Substring(start , length ) ;

            return new SyntaxToken(SyntaxKind.WhiteSpaceToken , start , text , null) ;
        }

        // Keywords && Identifiers
        if(char.IsLetter(Current) || Current == '_')
        {
            int start = _position ;
            
            while (char.IsLetterOrDigit(Current) || Current == '_')
            {
                _position ++ ;
            }
            
            int length = _position - start ;
            string text = _text.Substring(start , length);
            
            var kind = CompilatorTools.GetKwKind(text);
            var value = CompilatorTools.GetKwValue(kind); 

            return new SyntaxToken(kind , start , text  , value);

        }

        // Operadores
        switch (Current)
        {
            case('+'):
                return new SyntaxToken(SyntaxKind.PlusSignToken , _position++ , "+" , null) ;
            
            case('-'):
                return new SyntaxToken(SyntaxKind.MinusToken , _position++ , "-", null) ;
            
            case('*'):
                return new SyntaxToken(SyntaxKind.StarToken , _position++ , "*", null) ;
            
            case('/'):
                return new SyntaxToken(SyntaxKind.SlashToken , _position++ , "/", null) ;
            
            case('^') :
                return new SyntaxToken(SyntaxKind.ExponentToken, _position++ , "^", null);
            
            case('%'):
                return new SyntaxToken(SyntaxKind.PercentageToken , _position++ , "%" , null);

            case('@'):
               return new SyntaxToken(SyntaxKind.ArrobaToken , _position++ , "%" , null);


            case('('):
                return new SyntaxToken(SyntaxKind.OpenParenthesisToken , _position++ , "(", null) ;
            
            case(')'):
                return new SyntaxToken(SyntaxKind.CloseParenthesisToken , _position++ , ")", null) ;

            case(','):
                return new SyntaxToken(SyntaxKind.CommaSeparatorToken , _position++ , "," , null);
            
            case('!'):
                if(Peek(1) == '=')
                {
                    return new SyntaxToken(SyntaxKind.NotEqualToken , _position += 2 , "!=" , null);    
                }
                return new SyntaxToken(SyntaxKind.NotToken , _position++ , "!" , null);
                
            
            case('&'):
                if(Peek(1) == '&')
                {
                    return new SyntaxToken(SyntaxKind.DobleAndToken , _position += 2 , "&&" , null);    
                }
                return new SyntaxToken(SyntaxKind.AndToken , _position++ , "&" , null);
                            
            case('|'):
                if(Peek(1) == '|')
                {
                    return new SyntaxToken(SyntaxKind.DobleOrToken , _position += 2 , "||" , null);    
                }
                return new SyntaxToken(SyntaxKind.OrToken , _position++ , "|" , null);
                
            case('='):
                switch(Peek(1))
                {
                    case('='):
                        return new SyntaxToken(SyntaxKind.EqualEqualToken , _position += 2 , "==" , null);
                    
                    case('>'):
                        return new SyntaxToken(SyntaxKind.ArrowToken , _position += 2 , "=>" , null);

                    default:
                        return new SyntaxToken(SyntaxKind.EqualToken , _position++ , "=" , null);                    
                        
                }
            
            case('<') :
                switch (Peek(1))
                {
                    case('='):
                    return new SyntaxToken(SyntaxKind.LessOrEqualToken , _position += 2 , "<=" , null);
                
                    default:
                        return new SyntaxToken(SyntaxKind.LessToken , _position++ , "<" , null);
                }
            
            case('>') :
                switch (Peek(1))
                {
                    case('='):
                        return new SyntaxToken(SyntaxKind.MoreOrEqualToken , _position += 2 , ">=" , null);
                
                    default:
                        return new SyntaxToken(SyntaxKind.MoreToken , _position++ , ">" , null);
                }
                
            default:  // Tokens Incorrectos
                CompilatorTools.Bugs.Add($"<LexicalError> : unexpected character \"{Current}\"") ;
                return new SyntaxToken(SyntaxKind.BadToken , _position ++ , _text.Substring(_position - 1 , 1), null) ;
        }
    }
}
    