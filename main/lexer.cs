using System.Text;
namespace HULK ;

/* Lexer
Es la parte del programa encargada de recibir una cadena de caracteres e identificar los distintos tokens que conforman el lenguaje de HULK

Es capaz de reconocer los siguientes Tokens :
.Literales (string , number , boolean)
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
    
Contiene los siguientes metodos :
    Peek(n)   :  permite ver el caracter que se encuentra a una distancia "n" del actual.
    GetToken():  donde se lleva a cabo el proceso de identificar y obtener los tokens

Para identificar los tokens se intenta reconocer el tipo de token segun el primer caracter encontrado, luego se siguen mirando caracteres hasta completar
el token en cada caso.

Ejemplo : 

if(a <= b then "menor" else "mayor")
|if| |(| |a| |<=| |b| |then| |"menor"| |else| |"mayor| |)|

Cada tipo de token tiene su metodo Complete.
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
        // Propiedades que caracterizan a los tokens
        int start = _position ;  
        SyntaxKind kind       ;
        string text           ;
        object value          ;

        // End of Line tokens
        if(Current == ';')
            return new SyntaxToken(SyntaxKind.EOLToken , _position++ , ";", null);

        // End Of File tokens
        else if(_position >= _text.Length)
            return new SyntaxToken(SyntaxKind.EOFToken , _position , "\0", null) ;

        // Number Token
        else if(char.IsDigit(Current))
        {
            CompleteNumberLiteral(out kind , out text , out value , start);
        }

        // String Literals
        else if(Current == '"')
        {
            start = _position ++ ;
            CompleteStringLiteral(out kind , out text , out value , start) ;
        }

        // WhiteSpace Token
        else if(char.IsWhiteSpace(Current))
        {
            CompleteWhiteSpace(out kind , out text , out value , start);
        }
        // KeyWords and Identifiers
        else if(char.IsLetter(Current) || Current == '_')
        {
            CompleteKWAndIdentifiers(out kind , out text , out value , start) ;
        }

        // Operadores
        else
        {
            CompleteOperators(out kind , out text , out value , out start);
        }
        return new SyntaxToken(kind, start , text , value);
        
    }
    private void CompleteNumberLiteral(out SyntaxKind kind , out string text , out object value , int start)
    {
        kind = SyntaxKind.LiteralToken ;
        bool IsDecimal = false ; 
            
        //Sigue leyendo caracteres hasta que se encuentra uno que no sea un digito
        while(char.IsDigit(Current) || (Current == '.' && ! IsDecimal) )
        {
            if(Current == '.')
                IsDecimal = true ;
            
            _position ++ ;
        }

        int length = _position - start ;
        text = _text.Substring(start , length ) ;
            
        if(!double.TryParse(text , out double dbvalue))
            CompilatorTools.Bugs.Add($"<Lexical Error>: {text} cannot be represented by type Number.");
        
        value = dbvalue ;
    }

    private void CompleteStringLiteral(out SyntaxKind kind , out string text , out object value , int start)
    {
        kind = SyntaxKind.LiteralToken ;
        
        var sb = new StringBuilder();
        var done = false ;

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
        value = sb.ToString();
        text = "\"" + value + "\"" ;
    }

    private void CompleteWhiteSpace(out SyntaxKind kind , out string text , out object value , int start)
    {
        kind = SyntaxKind.WhiteSpaceToken ;

        while(char.IsWhiteSpace(Current)  )
            {
                _position ++ ;
            }

            int length = _position - start ;
            text = _text.Substring(start , length ) ;
            value = null ;
    }
    private void CompleteKWAndIdentifiers(out SyntaxKind kind , out string text , out object value , int start)
    {

            while (char.IsLetterOrDigit(Current) || Current == '_')
            {
                _position ++ ;
            }
            
            int length = _position - start ;
            text = _text.Substring(start , length);
            
            kind = CompilatorTools.GetKwKind(text);
            value = CompilatorTools.GetKwValue(kind);
        
    }
    private void CompleteOperators(out SyntaxKind kind , out string text , out object value , out int start)
    {
        value = null ;
        switch (Current)
        {
            case('+'):
                kind =  SyntaxKind.PlusSignToken ;
                start = _position++ ;
                text = "+" ;
                break ;
            
            case('-'):
                kind =  SyntaxKind.MinusToken ;
                start = _position++ ;
                text = "-" ;
                break ;

            case('*'):
                kind =  SyntaxKind.StarToken ;
                start = _position++ ;
                text = "*" ;
                break ;

            case('/'):
                kind =  SyntaxKind.SlashToken ;
                start = _position++ ;
                text = "/" ;
                break ;

            case('^') :
                kind =  SyntaxKind.ExponentToken ;
                start = _position++ ;
                text = "^" ;
                break ;

            case('%'):
                kind =  SyntaxKind.PercentageToken ;
                start = _position++ ;
                text = "%" ;
                break ;

            case('@'):
                kind =  SyntaxKind.ArrobaToken ;
                start = _position++ ;
                text = "@" ;
                break ;

            case('('):
                kind =  SyntaxKind.OpenParenthesisToken ;
                start = _position++ ;
                text = "(" ;
                break ;

            case(')'):
                kind =  SyntaxKind.CloseParenthesisToken ;
                start = _position++ ;
                text = ")" ;
                break ;

            case(','):
                kind =  SyntaxKind.CommaSeparatorToken ;
                start = _position++ ;
                text = "," ;
                break ;

            case('!'):
                if(Peek(1) == '=')
                {
                    kind =  SyntaxKind.NotEqualToken ;
                    start = _position += 2 ;
                    text = "!=" ;
                }
                else
                {
                    kind =  SyntaxKind.NotToken ;
                    start = _position++ ;
                    text = "!" ;
                }
                break ;
            
            case('&'):
                if(Peek(1) == '&')
                {
                    kind =  SyntaxKind.DobleAndToken ;
                    start = _position += 2 ;
                    text = "&&" ;
                }
                else
                {
                    kind =  SyntaxKind.AndToken ;
                    start = _position++ ;
                    text = "&" ;
                }
                break ;

            case('|'):
                if(Peek(1) == '|')
                {
                    kind =  SyntaxKind.DobleOrToken ;
                    start = _position += 2 ;
                    text = "||" ;
                }
                else
                {
                    kind =  SyntaxKind.OrToken ;
                    start = _position++ ;
                    text = "|" ;
                }
                break ;

            case('='):
                switch(Peek(1))
                {
                    case('='):
                        kind =  SyntaxKind.EqualEqualToken ;
                        start = _position += 2 ;
                        text = "==" ;
                        break ;
                    
                    case('>'):
                        kind =  SyntaxKind.ArrowToken ;
                        start = _position += 2 ;
                        text = "=>" ;
                        break ;
                    default:
                        kind =  SyntaxKind.EqualToken ;
                        start = _position ++ ;
                        text = "=" ;
                        break ;
                }
            break ;
            
            case('<') :
                switch (Peek(1))
                {
                    case('='):
                        kind =  SyntaxKind.LessOrEqualToken ;
                        start = _position +=2 ;
                        text = "<=" ;
                        break ;
                    
                    default:
                        kind =  SyntaxKind.LessToken ;
                        start = _position++ ;
                        text = "<" ;
                        break ;
               }
            break ;
            
            case('>') :
                switch (Peek(1))
                {
                    case('='):
                        kind =  SyntaxKind.MoreOrEqualToken ;
                        start = _position += 2 ;
                        text = ">=" ;
                        break ;
                    
                    default:
                    kind =  SyntaxKind.MoreToken ;
                    start = _position++ ;
                    text = ">" ;
                    break ;
                }
            break ;
                
            default:  // Tokens Incorrectos
            kind =  SyntaxKind.BadToken ;
            text = _text.Substring(_position , 1) ;
            start = _position ++ ;
            break ;
        }
    }
}
    