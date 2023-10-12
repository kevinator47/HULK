using System.Collections.Generic;
namespace HULK ;

/*
    Function Pool : una clase que almacena todas las funciones que han sido implementadas en durante la ejecucion del programa de HULK
    Contiene una lista de objetos tipo DeclaratedFunction la cual contendra funciones autoimplementadas como sen(x) , cos(x) , PI(), etc
*/

public class FunctionPool
{
    public static List<FunctionExpression> FunctionList = new List<FunctionExpression>();

    public static bool CheckIfExist(DeclaredFunctionExpression newfunction)
    {
        return FunctionList.Any(func => func.Equals(newfunction)) ;
    }

    public static FunctionExpression Find(string name , int counter)
    {
        return FunctionList.FirstOrDefault(func => func.Name == name && func.Len == counter );
    }

    public static void LoadPredefinedFunctions()
    {
        string [] predFunctionNames = new string[] {"sin" , "cos" , "tan" , "print"};

        foreach (string nm in predFunctionNames)
        {
            FunctionList.Add(new PredefinedFunctionExpreesion(nm, 1) );
        }
    }
    public static void ShowFunctions()
    {
        foreach(var f in FunctionList)
        {
            Console.WriteLine($"{f.Name} : takes {f.Len} argument(s).");
        }
    }
}