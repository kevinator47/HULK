namespace HULK ;

/* Parser
Es la parte del programa encargada de recibir un grupo de tokens creada por el Lexer y convertirlas en expresiones.

(La informacion sobre los Tokens esta en el sheet del Lexer y de las expresiones en el del Arbol Sintactico)

Contiene las siguientes propiedades :
    _tokens   :  un array que representa el grupo de tokens.
    _position :  para ir moviendose entre los tokens. 
    Current   :  se refiere al token actual.
    
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

    parentPrecedence : almacena la precedencia de la expresion anterior, para compararla con la actual y saber cual expresion se debe parsear primero.
    Por ejemplo :  x + 2 * 7  ==> como la precedencia de "*" es mayor que la de "+" se parsea de la forma : x + (2 * 7)
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
        
        
        if(kind == SyntaxKind.LiteralToken)
            CompilatorTools.Bugs.Add($"<Syntactic Error> : Received {Current._kind} while expecting Expression ") ;    
        
        else
            CompilatorTools.Bugs.Add($"<Syntactic Error> : Received {Current._kind} while expecting {kind} ") ;
        
        return new SyntaxToken(kind , Current._position , null , null);
    }

    private bool ErrorHasOcurred => CompilatorTools.Bugs.Count != 0 ;

    private void GetToRecoveryPoint()
    {
        while(Current._kind != SyntaxKind.EOLToken && Current._kind != SyntaxKind.EOFToken)
        {
            NextToken();
        }
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
            if(Current._kind == SyntaxKind.CommaSeparatorToken || Current._kind == SyntaxKind.InKwToken  )
            {
                EndToken = new SyntaxToken(SyntaxKind.EOFToken , _position , "\0", null) ;
            }
            else
            {   // caso normal, chequea que la linea de codigo termine correctamente.
                EndToken = MatchKind(SyntaxKind.EOLToken); 
            }
            Roots.Add(expression);
            
            if(ErrorHasOcurred)
            {
                GetToRecoveryPoint();
                return new SyntaxTree(null , null);
            }
           
        }

        return new SyntaxTree( Roots.ToArray() , MatchKind(SyntaxKind.EOFToken)) ;
    }

    private SyntaxExpression ParseExpression(int parentPrecedence = 0)
    {
        if(ErrorHasOcurred)
        {
            GetToRecoveryPoint();
            return null ;
        }

        SyntaxExpression left ;
        int unaryprecedence = CompilatorTools.GetUnaryOpPrecedence(Current._kind) ;

        if(unaryprecedence != 0 && unaryprecedence >= parentPrecedence )
        {
            var operatorToken = NextToken() ;
            var operand = ParseExpression(unaryprecedence) ;
            left = new UnaryOperatorExpression(operatorToken , operand) ;
        }
        else
        {    
            left = ParseTerm();
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
            var right = ParseExpression(precedence);
            left = new BinaryOperatorExpression(left , operatorToken , right) ;
        }
        return left ;
    }

    private SyntaxExpression ParseTerm()
    {
        if(ErrorHasOcurred)
        {
            GetToRecoveryPoint();
            return null ;
        }
        switch(Current._kind)
        {
            // Expresiones entre parentesis
            case(SyntaxKind.OpenParenthesisToken):
                return ParseParenthesizedExpression();
                
            // Bool Literal
            case(SyntaxKind.TrueToken):
            case(SyntaxKind.FalseToken):
                return ParseBoolLiteralExpression();

            // Funcion Call y variables
            case(SyntaxKind.IdentifierToken):
                if(Peek(1)._kind == SyntaxKind.OpenParenthesisToken)  
                    return ParseFunctionCallExpression();
                else 
                    return ParseVariableExpression();
                
            // Let In Expressions
            case(SyntaxKind.LetKwToken):
                return ParseLetInExpression();
            
            // If Expresions
            case(SyntaxKind.IfKwToken):
                return ParseIfExpression();
            
            // PI
            case(SyntaxKind.PIKwToken):
                return ParsePIExpression();
                
            // literales numericos y string
            default :
                return ParseLiteralExpression();
        }
    }

    private SyntaxExpression ParseLiteralExpression()
    {
        var litToken = MatchKind(SyntaxKind.LiteralToken) ;
        return new LiteralExpression(litToken);
    }
    private SyntaxExpression ParsePIExpression()
    {
        var PItoken = NextToken();
        return new LiteralExpression(PItoken);
    }
    private SyntaxExpression ParseBoolLiteralExpression()
    {
        var booltoken = NextToken();
        return new LiteralExpression(booltoken);
    }
    private SyntaxExpression ParseVariableExpression()
    {
        string name = NextToken()._text ;
        return new VariableExpression(name);
    }
    private SyntaxExpression ParseParenthesizedExpression()
    {
        var open = NextToken() ;                                     
        var expression = ParseExpression(0) ;            
        var close = MatchKind(SyntaxKind.CloseParenthesisToken) ; 

        return new ParenthesizedExpression(open, expression , close);
    }
    private SyntaxExpression ParseIfExpression()
    {
        NextToken(); // saltando el if

        MatchKind(SyntaxKind.OpenParenthesisToken);
        SyntaxExpression condition = ParseExpression();
        MatchKind(SyntaxKind.CloseParenthesisToken);

        SyntaxExpression truebranch = ParseExpression();
        MatchKind(SyntaxKind.ElseKwToken);
        SyntaxExpression falsebranch = ParseExpression();

        return new IfExpression(condition , truebranch , falsebranch);
    }
    private SyntaxExpression ParseLetInExpression()
    {
        Dictionary<string , SyntaxExpression> asigments = new Dictionary<string, SyntaxExpression>() ;
            
        NextToken() ; // saltando el Let
            
        while(Current._kind != SyntaxKind.InKwToken)
        {
            string varName = MatchKind(SyntaxKind.IdentifierToken)._text ;
            MatchKind(SyntaxKind.EqualToken);

            if(ErrorHasOcurred)
            {                    
                while(Current._kind != SyntaxKind.EOFToken)
                {
                    NextToken();
                }
                return new LetInExpression(new Dictionary<string , SyntaxExpression> (), null);
            }
            var expression = ParseExpression();
            asigments.Add(varName,expression) ;
                
            if(Current._kind == SyntaxKind.InKwToken)
                break ;
            if(Current._kind == SyntaxKind.EOFToken)
            {
                CompilatorTools.Bugs.Add($"<Syntax Error> : Missing keyword \"in\" at Let-In expression.");
                return new LetInExpression(new Dictionary<string , SyntaxExpression> (), null);
            }
            MatchKind(SyntaxKind.CommaSeparatorToken);
        }
        NextToken() ; // saltando el 'in'
        var body = ParseExpression();

        return new LetInExpression(asigments , body);            
    }
    private SyntaxExpression ParseFunctionDeclaration()
    {
        string name = MatchKind(SyntaxKind.IdentifierToken)._text ; 
        MatchKind(SyntaxKind.OpenParenthesisToken) ;

        List<string> parameters = new List<string>() ;
        
        while(Current._kind != SyntaxKind.CloseParenthesisToken)
        {
            string varName = MatchKind(SyntaxKind.IdentifierToken)._text ; 
            parameters.Add(varName) ;
            
            if(Current._kind != SyntaxKind.CloseParenthesisToken) // chequea que los parametros esten separados por comas excepto el ultimo.
            {
                MatchKind(SyntaxKind.CommaSeparatorToken) ;
            }
            
            if(Current._kind == SyntaxKind.EOFToken) // Si se llega al final del archivo no se cerro el parentesis.
            {
                CompilatorTools.Bugs.Add($"<Syntactic Error> : Irregular use of parenthesis on function declaration : {name}(...") ;
            }

            if(ErrorHasOcurred)
            {                    
                GetToRecoveryPoint();  
                return new DeclaredFunctionExpression(null , new List<string>() , null);
            }
        }

        NextToken();
        MatchKind(SyntaxKind.ArrowToken);

        var body = ParseExpression();
        return new DeclaredFunctionExpression(name , parameters , body) ;
    }
    private SyntaxExpression ParseFunctionCallExpression()
    {
        var name = NextToken()._text ;   // Nombre de la funcion
        MatchKind(SyntaxKind.OpenParenthesisToken);                     
                                
        List<SyntaxExpression> args = new List<SyntaxExpression>() ;

        while(Current._kind != SyntaxKind.CloseParenthesisToken)
        {
            args.Add(ParseExpression()) ;
                       
            if(Current._kind == SyntaxKind.EOFToken)
            {
                // Si se llega al final del archivo no se cerro el parentesis.
                CompilatorTools.Bugs.Add($"<Syntactic Error> : Irregular use of parenthesis on function call : {name}(...") ;
            }
                        
            if(Current._kind != SyntaxKind.CloseParenthesisToken)
            {   
                // chequea que a cada argumento le siga una coma excepto el ultimo       
                MatchKind(SyntaxKind.CommaSeparatorToken);
            }

            if(ErrorHasOcurred)
            {
                GetToRecoveryPoint();
                return new FunctionCallExpression(null , new List<SyntaxExpression>()) ;                 
            }
        }
        NextToken() ;  // Saltando el ) 
                     
        return new FunctionCallExpression(name , args);
    }    
}

    