using System.Linq.Expressions;
namespace HULK ;

/* PARSER'S SHEET

El parser se encarga de recibir los tokens generados por el lexer y construir expresiones que luego seran evaluadas

*/

public class Parser
{
    private SyntaxToken[] _tokens ;  // un array para guardar los tokens
    private int _position ;  // para ir moviendose entre los tokens
    
    private List<string> _bugs = new List<string> ();
    public IEnumerable<string> Bugs => _bugs ;

    private SyntaxToken Peek(int d)
    {
        // Permite seleccionar el token que se encuentre a una distancia d del actual
        int index = Math.Min(_position + d , _tokens.Length - 1) ;
        return _tokens[index] ; 
    }

    private SyntaxToken Current => Peek(0) ; // devuelve el token actual

    private SyntaxToken NextToken()
    {
        // esta funcion devuelve el token actual y selecciona el proximo token
        var current = Current ;
        _position ++ ;
        return current ;
    }

    private SyntaxToken MatchKind(SyntaxKind kind)
    {
        // este metodo chequea que el tipo de token sea el esperado por el programa, si falla crea un bug de tipo TypeError
        if(Current._kind == kind)
            return NextToken();
        
        _bugs.Add($"<TypeError> : received {Current._kind} while expecting {kind} ") ;
        return new SyntaxToken(kind , Current._position , null , null);
    }

    public Parser(string codeline)
    {
        // El constructor corre el lexer obteniendo un array de tokens

        var tokens = new List<SyntaxToken> ();
        var lexer = new Lexer(codeline) ;
    
        SyntaxToken token ;

        do
        {
            token = lexer.GetToken();

            if(token._kind != SyntaxKind.WhiteSpaceToken && token._kind != SyntaxKind.BadToken )
            {
                tokens.Add(token) ; // agrega a la lista todos los tokens excepto espacios en blanco y tokens invalidos
            }
        } while (token._kind != SyntaxKind.EOFToken);
        
        _tokens = tokens.ToArray();
        _bugs.AddRange(lexer.Bugs);  // agrega los bugs capturados por el lexer   
        }

    public SyntaxTree Parse()
    {
        var expression = ParseExpression();
        var EOFtoken = MatchKind(SyntaxKind.EOFToken); // chequea que la linea termine correctamente

        return new SyntaxTree(_bugs , expression , EOFtoken) ;
    }

    private SyntaxExpression ParseExpression(int parentPrecedence = 0)
    {
        SyntaxExpression left ;

        int unaryprecedence = GetUnaryOpPrecedence(Current._kind);

        if(unaryprecedence != 0 && unaryprecedence >= parentPrecedence )
        {
            var operatorToken = NextToken();
            var operand = ParseExpression(unaryprecedence);
            left = new UnaryOperatorExpression(operatorToken , operand);
        }
        else
        {
            left = ParseTerm();
        }

        while(true) // maneja la precedencia de los operadores
        {
            int precedence = GetBinaryOpPrecedence(Current._kind);
            if(precedence == 0 || precedence <= parentPrecedence)
                break ;  // si no se puede ejecutar el operador o el operador anterior tiene mayor precedencia no se hace nada(se parseara el otro primero)

            // si el operador tiene mayor precedencia se parsea primero
            var operatorToken = NextToken();
            var right = ParseExpression(precedence);

            left = new BinaryOperatorExpression(left , operatorToken , right) ;
        }
        return left ;
    }

    private SyntaxExpression ParseTerm()
    {
        // expresiones dentro de parentesis
        if(Current._kind == SyntaxKind.OpenParenthesisToken)
        {
            SyntaxToken open = NextToken();
            var expression = ParseExpression() ;
            SyntaxToken close = MatchKind(SyntaxKind.CloseParenthesisToken);

            return new ParenthesizedExpression(open, expression , close); 
        }

        // literales
        var literalToken = MatchKind(SyntaxKind.LiteralToken) ; // chequea que el termino sea un numero
        return new LiteralExpression(literalToken) ;
    }    
    
    private int GetBinaryOpPrecedence(SyntaxKind kind)
    {
        // se usa para darle una precedencia a cada operador y saber cual ejecutar primero
        switch (kind)
        {
            case(SyntaxKind.StarToken):
            case(SyntaxKind.SlashToken):
                return 2 ;

            case(SyntaxKind.PlusSignToken):
            case(SyntaxKind.MinusToken):
                return 1 ;

            default:
                return 0 ;
        }
    }
    private int GetUnaryOpPrecedence(SyntaxKind kind)
    {
        // se usa para darle una precedencia a cada operador y saber cual ejecutar primero
        switch (kind)
        {
            case(SyntaxKind.PlusSignToken):
            case(SyntaxKind.MinusToken):
                return 3 ;

            default:
                return 0 ;
        }
    }
}

    
