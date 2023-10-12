using System.Diagnostics;
namespace HULK ;
using System ;
using System.Diagnostics ;

/* Punto de Entrada del Programa
Carga las funciones predefinidas
Recibe el string ingresado por el usuario que representa la linea de codigo HULK.
Pasa dicha linea como argumento al Parser(este a su vez llama al Lexer) para convertir el string en expresiones del lenguaje HULK
Si no hubo ningun problema con la gramatica o la semantica ejecuta el programa
Si no hay ningun error capturable durante el tiempo de ejecucion(hay algunos que parten el programa para que no se salga de control) imprime el resultado
Estos pasos se repiten hasta que el usuario ingrese una linea vacia

Tiene colorcitos(verde para el programa normalmente y rojo para mostrar los errores)

*/

class Program
{
    static void Main(string[] args)
    {
        Console.Clear();
        FunctionPool.LoadPredefinedFunctions();
        var crono = new Stopwatch();
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Green ;
            CompilatorTools.Bugs = new List<string>();

            Console.Write("> ") ;
            string line = Console.ReadLine();
            crono.Start();

            if(string.IsNullOrWhiteSpace(line))
                return ;

            if(line == "#clear")
            {
                Console.Clear();
                crono.Stop();
                continue ;
            }

            if(line == "#functions")
            {
                Console.ForegroundColor = ConsoleColor.Gray ;
                FunctionPool.ShowFunctions();
                
                crono.Stop();
                Console.ForegroundColor = ConsoleColor.Yellow ;
                Console.WriteLine("~ Execution time : {0} ms ~" , crono.ElapsedMilliseconds);
                
                continue;
            }
            
            var parser = new Parser(line) ;
            var tree = parser.Parse();
            
            // Revisa si se encontro algun bug durante la compilacion.
            if(CompilatorTools.Bugs.Any())   
            {
                Console.ForegroundColor = ConsoleColor.DarkRed ;
                Console.WriteLine(CompilatorTools.Bugs[0]);
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
                        Console.WriteLine(CompilatorTools.Bugs[0]);
                
                        Console.ForegroundColor = ConsoleColor.Green ;
                        break ;
                    }
                    // Imprime el resultado del programa
                    else
                    {
                        if(result != null)
                        {
                            Console.ForegroundColor = ConsoleColor.Blue ;
                            Console.WriteLine(result);
                        }        
                    }
                }
           }
           crono.Stop();
           Console.ForegroundColor = ConsoleColor.Yellow ;
           Console.WriteLine("~ Execution time : {0} ms ~" , crono.ElapsedMilliseconds);
        }
    }
}


