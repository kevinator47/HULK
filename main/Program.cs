namespace HULK ;

/* Este es el sheet principal, donde se va a iniciar el programa, hasta que se implemente el parser voy
a probar que el Lexer funcione corretamente en la funcion main

*/

class Program
{
    static void Main()
    {
        while (true)
        {
            Console.Write("> ") ;
            string line = Console.ReadLine();

            if(string.IsNullOrWhiteSpace(line))
                return ;
            
            var lexer = new Lexer(line);
            
            while (true)
            {
                var token = lexer.GetToken();
                
                if(token._kind == TokenKind.EOFToken)
                    break ;

                Console.Write("{0} : \'{1}\' [{2}]" , token._kind.ToString() , token._text , token._position) ;
                
                if(token._value != null)
                    Console.Write(" ({0})",token._value);

                Console.WriteLine();    
            }
        }
    }
}


