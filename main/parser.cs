namespace HULK ;

/* Parser
Es la parte del programa encargada de recibir un grupo de tokens creada por el Lexer y convertirlas en expresiones.

(La informacion sobre los Tokens esta en el sheet del Lexer y de las expresiones en el del Arbol Sintactico)

Contiene las siguientes propiedades :
    _tokens   :  un array que representa el grupo de tokens.
    _position :  para ir moviendose entre los tokens. 
    Current   :  se refiere al token actual.
    CompilatorTools.Bugs     :  una lista para almacenar los errores que se encuentren.

Contiene los siguientes metodos :
    constructor       :  Ejecuta el Lexer obteniendo una lista de tokens. 
    Peek(n)           :  Permite mirar el token que se encuentra a una distancia "n" del actual.
    NextToken()       :  Devuelve el token actual y cambia el puntero para el siguiente token.
    MatchKind(kind)   :  Chequea que el token actual coincida con el tipo especificado, en caso de que no : agrega un error de tipo semantico.
    
    Parse()           :  Es el parseo mas general, va parseando las expresiones segun las reglas del lenguaje y conforma un arbol sintactico.
    ParseExpression() :  Parsea las expresiones(Operadores Unarios, Operadores Binarios , Terminos) segun la precedencia de los operadores.
    ParseTerm()       :  Parsea los terminos(literales, expresiones en parentesis, llamados de funcion , variables).
    ParseFunctionD()  :  Parsea la declaracion de funciones

    Las reglas del lenguaje son las siguientes : (E : Expresion , T : Termino , F : Llamado de Funcion)
    
    E : T | T + E | T - E | T * E | T / E | -E 

    T : (E) | F(E) | variable | bool | int

    El metodo ParseExpression trabaja con dos parametros, parentPrecedence y father

    parentPrecedence : almacena la precedencia de la expresion anterior, para compararla con la actual y saber cual expresion se debe parsear primero.
    Por ejemplo :  x + 2 * 7  ==> como la precedencia de "*" es mayor que la de "+" se parsea de la forma : x + (2 * 7)

    father : almacena la funcion que contiene a la expresion actual, para que las variables que aparezcan sepan en que ambito se encuentran declaradas
    Suc(x) => x + 2  ; sin eso "x" no sabria donde buscar su valor cuando se ejecuta la expresion "x + 2"
*/

public class Parser
{
    private SyntaxToken[] _tokens ;  
    private int _position ;
    private SyntaxToken Current => Peek(0) ; // devuelve el token actual

    public Parser(string codeline)
    {
        var tokens = new List<SyntaxToken> () ;
        var lexer = new Lexer(codeline) ;
    
        SyntaxToken token ;

        /* Guarda en la lista los tokens encontrados por el lexer excepto espacios en blanco y tokens no validos
        (se ejecuta hasta encontrar el EOF Token, el cual es seguro que siempre estará)  */
        do
        {
            token = lexer.GetToken();

            if(token._kind != SyntaxKind.WhiteSpaceToken && token._kind != SyntaxKind.BadToken )
            {
                tokens.Add(token) ; 
            }
        } while (token._kind != SyntaxKind.EOFToken) ; 
        
        _tokens = tokens.ToArray();    
        }

    private SyntaxToken Peek(int d)
    {
        int index = Math.Min(_position + d , _tokens.Length - 1) ;
        return _tokens[index] ; 
    }
    private SyntaxToken NextToken()
    {
        var current = Current ;
        _position ++ ;
        return current ;
    }

    private SyntaxToken MatchKind(SyntaxKind kind)
    {
        if(Current._kind == kind)
            return NextToken();
        
        CompilatorTools.Bugs.Add($"<Syntactic Error> : Received {Current._kind} while expecting {kind} ") ;
        return new SyntaxToken(kind , Current._position , null , null);
    }
    public SyntaxTree Parse()
    {
        // para guardar todas las instrucciones en caso de que escriban varias lineas de codigo en una sola
        List <SyntaxExpression> Roots = new List<SyntaxExpression>();

        SyntaxExpression expression ;
        
        while(Current._kind != SyntaxKind.EOFToken)
        {
            // Declaracion de Funciones
            if(Current._kind == SyntaxKind.FunctionKwToken)
            {
                NextToken();
                expression = ParseFunctionDeclaration();
            }

            // Otras Expresiones
            else
            {
                expression = ParseExpression();
            }
            
            SyntaxToken EndToken ;

            // Al parsear expresiones dentro de los parentesis de una funcion el final serian las , entre los parametros
            if(Current._kind == SyntaxKind.CommaSeparatorToken )
            {
                EndToken = new SyntaxToken(SyntaxKind.EOFToken , _position , "\0", null) ;
            }
            else
            {   // caso normal, chequea que la linea de codigo termine correctamente.
                EndToken = MatchKind(SyntaxKind.EOLToken); 
            }
            Roots.Add(expression);
        }

        return new SyntaxTree(CompilatorTools.Bugs , Roots.ToArray() , MatchKind(SyntaxKind.EOFToken)) ;
    }

    private SyntaxExpression ParseExpression(int parentPrecedence = 0 , DeclaredFunctionExpression father = null)
    {
        SyntaxExpression left ;
        int unaryprecedence = CompilatorTools.GetUnaryOpPrecedence(Current._kind) ;

        if(unaryprecedence != 0 && unaryprecedence >= parentPrecedence )
        {
            var operatorToken = NextToken() ;
            var operand = ParseExpression(unaryprecedence , father) ;
            left = new UnaryOperatorExpression(operatorToken , operand) ;
        }
        else
        {
            left = ParseTerm(father);
        }

        while(true) 
        {
            int precedence = CompilatorTools.GetBinaryOpPrecedence(Current._kind);

            // Si no se puede ejecutar el operador o el operador anterior tiene mayor precedencia no se hace nada(se parseara el otro primero).
            if(precedence == 0 || precedence <= parentPrecedence)
            { 
                break ;  
            }

            // Si el operador tiene mayor precedencia se parsea primero.
            var operatorToken = NextToken();
            var right = ParseExpression(precedence , father);
            left = new BinaryOperatorExpression(left , operatorToken , right) ;
        }
        return left ;
    }

    private SyntaxExpression ParseTerm(DeclaredFunctionExpression father)
    {
        switch(Current._kind)
        {
            // (E)
            case(SyntaxKind.OpenParenthesisToken):
                var open = NextToken() ;
                var expression = ParseExpression() ;
                var close = MatchKind(SyntaxKind.CloseParenthesisToken) ; // chequea que se cierre el parentesis

                return new ParenthesizedExpression(open, expression , close);

            // bool
            case(SyntaxKind.TrueToken):
            case(SyntaxKind.FalseToken):

                var booltoken = NextToken();
                return new LiteralExpression(booltoken);
            
            // F(E) y variable
            case(SyntaxKind.IdentifierToken):

                // F(E)
                if(Peek(1)._kind == SyntaxKind.OpenParenthesisToken) 
                {
                    var name = NextToken()._text ;  
                    NextToken();   // saltando el (
                    
                    // lista de argumentos de la funcion
                    List<SyntaxExpression> args = new List<SyntaxExpression>() ;

                    while(Current._kind != SyntaxKind.CloseParenthesisToken)
                    {
                        args.Add(ParseExpression(0,father));
                        
                        // Si se llega al final de la linea no se cerro el parentesis
                        if(Current._kind == SyntaxKind.EOFToken)
                        {
                            CompilatorTools.Bugs.Add($"<Syntactic Error> : Irregular use of parenthesis on function call : {name}(...") ;
                            break ;
                        }
                        
                        if(Current._kind != SyntaxKind.CloseParenthesisToken)
                        {   
                            // chequea que a cada argumento le siga una coma excepto el ultimo       
                            MatchKind(SyntaxKind.CommaSeparatorToken);
                        }                 
                    }

                    SyntaxExpression[] arguments = args.ToArray();
                    NextToken(); // Saltando el ) 

                    var declaredFunction = FunctionPool.CheckIfExist(name , arguments.Length);

                    if(declaredFunction == null)
                        CompilatorTools.Bugs.Add($"<Syntactic Error> : Function \"{name}\" receiving {arguments.Length} argument(s) do(es) not exist.");
                    
                    return new FunctionCallExpression(declaredFunction , arguments);
                }
                
                // Variable
                else 
                { 

                    var variabletoken = NextToken();
                    var fatherExp = father ;

                    if(father == null)
                    {
                        CompilatorTools.Bugs.Add($"<Syntactic Error> : Variable \"{variabletoken._text}\" declared outside valid scope.");
                    }
                    
                    else
                    {
                        SyntaxToken matchingToken = father.Scope.FirstOrDefault(token => token._text == variabletoken._text);
                        
                        if(matchingToken == null)
                            CompilatorTools.Bugs.Add($"<Syntactic Error> : Use of undeclared variable \"{variabletoken._text}\" at {father.Kind} \"{father.Name}\" ");
                    }
                    
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
            
            if(Current._kind != SyntaxKind.CloseParenthesisToken) // chequea que los parametros esten separados por comas excepto el ultimo
            {
                MatchKind(SyntaxKind.CommaSeparatorToken) ;
            }
            
            // Si se llega al final de la linea no se cerro el parentesis
            if(Current._kind == SyntaxKind.EOFToken)
            {
                CompilatorTools.Bugs.Add($"<Syntactic Error> : Irregular use of parenthesis on function declaration : {name}(...") ;
                break ;
            }
        }

        NextToken();
        MatchKind(SyntaxKind.ArrowToken);

        // creo la funcion sin el body para poder pasarla como padre de las variables del body
        var functionExp = new DeclaredFunctionExpression(name._text , parameters); 

        // declara el cuerpo de la funcion pasandola a ella como padre de las variables.
        functionExp.DeclarateBody( ParseExpression(0,functionExp) ) ; 
        
        return functionExp ;
    }    
    
}

    
