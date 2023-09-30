namespace HULK ;

/* Punto de Entrada del Programa

Recibe el string ingresado por el usuario que representa la linea de codigo HULK.
Pasa dicha linea como argumento al Parser(este a su vez llama al Lexer) para convertir el string en expresiones del lenguaje HULK
Si no hubo ningun problema con la gramatica o la semantica ejecuta el programa
Si no hay ningun error capturable durante el tiempo de ejecucion(hay algunos que parten el programa para que no se salga de control) imprime el resultado
Estos pasos se repiten hasta que el usuario ingrese una linea vacia

Tiene colorcitos(verde para el programa normalmente y rojo para mostrar los errores)
*/

class Program
{
    static void Main()
    {
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Green ;
            CompilatorTools.Bugs = new List<string>();

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
            
            // Revisa si se encontro algun bug durante la compilacion.
            if(tree._bugs.Any())   
            {
                Console.ForegroundColor = ConsoleColor.DarkRed ;
                foreach (string bug in tree._bugs)
                {
                    Console.WriteLine(bug);
                }
                
                Console.ForegroundColor = ConsoleColor.Green ;
                
            }
            
            // Si no se encontro ninguno , se ejecuta el programa.
            else    
            {
                for(int i = 0 ; i < tree.Roots.Length ; i++)
                {
                    var result = tree.Roots[i].Evaluate() ;
                
                    // Nuevamente revisa si encontro algun bug que no partio el programa.
                    if(CompilatorTools.Bugs.Any())
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed ;
                        foreach (string bug in CompilatorTools.Bugs)
                        {
                            Console.WriteLine(bug);
                        }
                
                        Console.ForegroundColor = ConsoleColor.Green ;
                        break ;
                    }
                    // Imprime el resultado del programa
                    else
                    {
                        if(result != null)
                            Console.WriteLine(result);
                    }
                }
           }

        }
    }
}


