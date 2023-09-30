namespace HULK ;

/*
    Function Pool : una clase que almacena todas las funciones que han sido implementadas en durante la ejecucion del programa de HULK
    Contiene una lista de objetos tipo DeclaratedFunction la cual contendra funciones autoimplementadas como sen(x) , cos(x) , PI(), etc
*/

public class FunctionPool
{
    public static List<DeclaredFunctionExpression> FunctionList = new List<DeclaredFunctionExpression>();

    public static bool CheckIfExist(DeclaredFunctionExpression newfunction)
    {
        return FunctionList.Any(func => func.Equals(newfunction)) ;
    }

    public static DeclaredFunctionExpression CheckIfExist(string name , int counter)
    {
        return FunctionList.FirstOrDefault(func => func.Name == name && func.Parameters.Length == counter );
    }
}