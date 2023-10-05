using System.Collections.Generic;
namespace HULK ;

/*
    Function Pool : una clase que almacena todas las funciones que han sido implementadas en durante la ejecucion del programa de HULK
    Contiene una lista de objetos tipo DeclaratedFunction la cual contendra funciones autoimplementadas como sen(x) , cos(x) , PI(), etc
*/

public class FunctionPool
{
    public static List<ScopedExpression> FunctionList = new List<ScopedExpression>();

    public static bool CheckIfExist(DeclaredFunctionExpression newfunction)
    {
        return FunctionList.Any(func => func.Equals(newfunction)) ;
    }

    public static ScopedExpression Find(string name , int counter)
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

        foreach (var item in FunctionList)
        {
            Console.WriteLine("{0} : {1}" , item.Name , item.Len);
        }
        Console.WriteLine(FunctionList.Count);
    }
}