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

    private SyntaxToken Match(SyntaxKind kind)
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
        var expression = ParseE();
        var EOFtoken = Match(SyntaxKind.EOFToken); // chequea que la linea termine correctamente

        return new SyntaxTree(_bugs , expression , EOFtoken) ;
    }

    private SyntaxExpression ParseE()
    {
        // aqui esta la magia : E ---->  F + E | F - E | F

        var left = ParseF();   // Parsea factores(es lo que hace que se ejecute primero la multiplicacion o division)
        
        while(Current._kind == SyntaxKind.PlusToken || Current._kind == SyntaxKind.MinusToken)
        {
            var operatorToken = NextToken();  // guarda el operador
            var right = ParseF() ;            // parsea el otro factor
            left = new BinaryOpSyntaxExpression(left , operatorToken , right); // todo lo parseado se ira guardando en la expresion left
        }

        return left ;
    }

    private SyntaxExpression ParseF()
    {
        // Sigue la magia :  F ---> *T | /T | T

        var left = ParseT();   // Parsea Terminos(numeros)
        
        while(Current._kind == SyntaxKind.StarToken || Current._kind == SyntaxKind.SlashToken)
        {
            var operatorToken = NextToken();  // guarda el operador
            var right = ParseT() ;            // parsea el otro termino
            left = new BinaryOpSyntaxExpression(left , operatorToken , right); // todo lo parseado se ira guardando en la expresion left
        }

        return left ;
    }

    private SyntaxExpression ParseT()
    {
        if(Current._kind == SyntaxKind.OpenParenthesisToken)
        {
            SyntaxToken open = NextToken();
            var expression = ParseE() ;
            SyntaxToken close = Match(SyntaxKind.CloseParenthesisToken);

            return new ParenthesizedExpression(open, expression , close); 
        }
        var numberToken = Match(SyntaxKind.NumberToken) ; // chequea que el termino sea un numero
        return new NumberSyntaxExpression(numberToken) ;
    }    
}
