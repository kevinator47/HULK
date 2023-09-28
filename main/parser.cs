using System;
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
        SyntaxExpression expression ;
        
        // function declaration
        if(Current._kind == SyntaxKind.FunctionKwToken)
        {
            NextToken();
            expression = ParseFunctionDeclaration();
        }
        // other expressions
        else
        {
            expression = ParseExpression();
        }
        
        // feo pero funciona para las comas
        SyntaxToken EOFtoken ;

        if(Current._kind == SyntaxKind.CommaSeparatorToken )
            EOFtoken = new SyntaxToken(SyntaxKind.EOFToken , _position , "\0", null) ;  
        else
            EOFtoken = MatchKind(SyntaxKind.EOFToken); // chequea que la linea termine correctamente

        return new SyntaxTree(_bugs , expression , EOFtoken) ;
    }

    private SyntaxExpression ParseExpression(int parentPrecedence = 0 , DeclaratedFunctionExpression father = null)
    {
        SyntaxExpression left ;

        int unaryprecedence = GetUnaryOpPrecedence(Current._kind);

        if(unaryprecedence != 0 && unaryprecedence >= parentPrecedence )
        {
            var operatorToken = NextToken();
            var operand = ParseExpression(unaryprecedence , father);
            left = new UnaryOperatorExpression(operatorToken , operand);
        }
        else
        {
            left = ParseTerm(father);
        }

        while(true) // maneja la precedencia de los operadores
        {
            int precedence = GetBinaryOpPrecedence(Current._kind);
            if(precedence == 0 || precedence <= parentPrecedence)
                break ;  // si no se puede ejecutar el operador o el operador anterior tiene mayor precedencia no se hace nada(se parseara el otro primero)

            // si el operador tiene mayor precedencia se parsea primero
            var operatorToken = NextToken();
            var right = ParseExpression(precedence , father);

            left = new BinaryOperatorExpression(left , operatorToken , right) ;
        }
        
        return left ;
    }

    private SyntaxExpression ParseTerm(DeclaratedFunctionExpression father)
    // ParseTerm(father)
    {
        switch(Current._kind)
        {
            // Expresiones dentro de parentesis
            case(SyntaxKind.OpenParenthesisToken):
                
                var open = NextToken();
                var expression = ParseExpression() ;
                var close = MatchKind(SyntaxKind.CloseParenthesisToken); // chequea que se cierre el parentesis

                return new ParenthesizedExpression(open, expression , close);

            // literales true y false
            case(SyntaxKind.TrueToken):
            case(SyntaxKind.FalseToken):

                var booltoken = NextToken();
                return new LiteralExpression(booltoken);
            
            // llamados de funcion y variables
            case(SyntaxKind.IdentifierToken):
                
                // Function Call
                if(Peek(1)._kind == SyntaxKind.OpenParenthesisToken) 
                {
                    var name = NextToken()._text ; // el nombre del identificador
                    NextToken();   // saltando el (
                    
                    List<SyntaxExpression> args = new List<SyntaxExpression>();

                    while(Current._kind != SyntaxKind.CloseParenthesisToken)
                    {
                        args.Add(ParseExpression(0,father));
                        if(Current._kind == SyntaxKind.CloseParenthesisToken)
                            break ;
                        
                        MatchKind(SyntaxKind.CommaSeparatorToken);
                    }
                    SyntaxExpression[] arguments = args.ToArray();
                    NextToken(); // Saltando el )

                    var declaredFunction = FunctionPool.CheckIfExist(name , arguments.Length);

                    if(declaredFunction == null)
                        _bugs.Add($"<Semantic Error> : No existe ninguna funcion \"{name}\" que reciba {arguments.Length} argumento(s).");
                    
                    return new FunctionCallExpression(declaredFunction , arguments);
                }
                
                // Variable
                else 
                {
                    var variabletoken = NextToken();
                    var fatherExp = father ;

                    SyntaxToken matchingToken = father.Scope.FirstOrDefault(token => token._text == variabletoken._text);
                    if(matchingToken == null)
                        _bugs.Add($"Uso de la variable sin declarar \"{variabletoken._text}\" en {father.Kind} \"{father.Name}\" ");

                    return new VariableExpression(variabletoken , father);
                }
            
            // literales numericos
            default :
                var numberToken = MatchKind(SyntaxKind.LiteralToken) ; // chequea que el termino sea un numero
                return new LiteralExpression(numberToken);
        }
    }

    private SyntaxExpression ParseFunctionDeclaration()
    {
        // function Sucesor(x) => x + 1

        var name = MatchKind(SyntaxKind.IdentifierToken); 
        MatchKind(SyntaxKind.OpenParenthesisToken) ;

        List<SyntaxToken> parameters = new List<SyntaxToken> ();
        while(Current._kind != SyntaxKind.CloseParenthesisToken)
        {
            parameters.Add(MatchKind(SyntaxKind.IdentifierToken)) ;
            
            if(Current._kind == SyntaxKind.CloseParenthesisToken) // esta feo pero funciona xD
                break ;
            
            MatchKind(SyntaxKind.CommaSeparatorToken) ;
        }

        NextToken();
        MatchKind(SyntaxKind.ArrowToken);

        // creo la funcion sin el body para poder pasarla como padre de las variables del body
        var functionExp = new DeclaratedFunctionExpression(name._text , parameters); 

        functionExp.Body = ParseExpression(0,functionExp); // HACERLO MAS LINDO CON UN METODO DENTRO DE LA FUNCTION EXPRESION
        
        return functionExp ;
        }    
    
    private int GetBinaryOpPrecedence(SyntaxKind kind)
    {
        // se usa para darle una precedencia a cada operador y saber cual ejecutar primero
        switch (kind)
        {
            case(SyntaxKind.StarToken):
            case(SyntaxKind.SlashToken):
            case(SyntaxKind.AndToken):
            case(SyntaxKind.OrToken):
            case(SyntaxKind.DobleAndToken):
            case(SyntaxKind.DobleOrToken):
                return 2 ;

            case(SyntaxKind.PlusSignToken):
            case(SyntaxKind.MinusToken):
            case(SyntaxKind.NotEqualToken):
            case(SyntaxKind.EqualEqualToken):
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
            case(SyntaxKind.NotToken):
                return 3 ;

            default:
                return 0 ;
        }
    }
}

    
