﻿namespace HULK ;

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

            if(line == "#clear")
            {
                Console.Clear();
                continue ;
            }
            

            var parser = new Parser(line) ;
            var tree = parser.Parse();
            

            if(tree._bugs.Any())  // chequea si durante la compilacion se encontro algun error y los imprime
            {
                Console.ForegroundColor = ConsoleColor.DarkRed ;
                foreach (string bug in tree._bugs)
                {
                    Console.WriteLine(bug);
                }
                
                Console.ResetColor();
                
            }
            else    // si no encontro ningun error ejecuta el programa
            {
                var e = new Evaluator(tree.Root) ;
                var result = e.Evaluate() ;
                if(e.Bugs.Any())
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed ;
                    foreach (string bug in e.Bugs)
                    {
                        Console.WriteLine(bug);
                    }
                
                    Console.ResetColor();
                }
                else
                {
                    if(result != null)
                        Console.WriteLine(result);
                }
            }

        }
    }
}


