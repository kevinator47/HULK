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
. <identificadores>
. <kwords>

*/

public class Lexer
{
    string _text  ;
    int _position ;
    private List<string> _bugs = new List<string> ();
    public IEnumerable<string> Bugs => _bugs ;

    char Current
    {
        get
        {
            if(_position >= _text.Length)
                return '\0' ;

            return _text[_position];
        }
    }

    private char Peek(int distance)
    {
        // Permite ver el caracter que se encuentra a cierta distancia del actual
        int index = _position + distance ;
        
        if(index >= _text.Length)
            return '\0';

        return _text[index];
    }

    // Las palabras claves del HULK
    Dictionary<string , SyntaxKind> KWords = new Dictionary<string , SyntaxKind>()
    {
        {"let" , SyntaxKind.LetToken},
        {"if" , SyntaxKind.IfToken} ,
        {"in" , SyntaxKind.InToken} ,
        {"then" , SyntaxKind.ThenToken},
        {"else" , SyntaxKind.ElseToken} , 
        {"true" , SyntaxKind.TrueToken} ,
        {"false" , SyntaxKind.FalseToken} ,
        {"function" , SyntaxKind.FunctionKwToken}
    };

    public Lexer(string codeline)
    {
        _text = codeline ;
        _position = 0 ; // ??
    }

    public SyntaxToken GetToken()
    {
        // EOF Token(sencillo)
        if(_position >= _text.Length)
            return new SyntaxToken(SyntaxKind.EOFToken , _position , "\0", null) ;
        
        // Number Token
        if(char.IsDigit(Current))
        {
            int start = _position ;   // para poder recuperar mas tarde todo el token
            
            while(char.IsDigit(Current) )
            {
                // sigue leyendo caracteres hasta que se encuentra uno que no sea un digito
                _position ++ ;
            }

            int length = _position - start ;
            string text = _text.Substring(start , length ) ;
            
            if(!int.TryParse(text , out int value))
                _bugs.Add("<Lexical Error>: no es posible representar {0} como Int.");
            return new SyntaxToken(SyntaxKind.LiteralToken , start , text , value) ;
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
            
            var kind = GetKwKind(text);
            var value = GetKwValue(kind); 

            return new SyntaxToken(kind , start , text  , value);

        }

        // Operadores + - * / ( )
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

            case('('):
                return new SyntaxToken(SyntaxKind.OpenParenthesisToken , _position++ , "(", null) ;
            
            case(')'):
                return new SyntaxToken(SyntaxKind.CloseParenthesisToken , _position++ , ")", null) ;
            
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
                
            case(','):
            {
                return new SyntaxToken(SyntaxKind.CommaSeparatorToken , _position++ , "," , null);
            }
            
            default:
                // Bad Token
                _bugs.Add($"<LexicalError> : unexpected character \"{Current}\"") ;
                return new SyntaxToken(SyntaxKind.BadToken , _position ++ , _text.Substring(_position - 1 , 1), null) ;
        }
    }

    private SyntaxKind GetKwKind(string text)
    {
        // Si el texto es una palabra clave devuelve su tipo de token, en otro caso devuelve IdentifierToken
        if(KWords.ContainsKey(text))
            return KWords[text];                // usar operador '?'
        return SyntaxKind.IdentifierToken ;
    }

    private object GetKwValue(SyntaxKind Kind)
    {
        if(Kind == SyntaxKind.TrueToken || Kind == SyntaxKind.FalseToken)
            return Kind == SyntaxKind.TrueToken ; // si es true devuelve true, si es false devuelve false
        
        return null ;
    }
}