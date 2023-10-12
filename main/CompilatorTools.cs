using System;
using System.Collections.Generic;
namespace HULK ;

public class CompilatorTools
{
    // Las palabras claves del HULK
    private static Dictionary<string , SyntaxKind> KWords = new Dictionary<string , SyntaxKind>()
    {
        {"let" , SyntaxKind.LetKwToken},
        {"if" , SyntaxKind.IfKwToken} ,
        {"in" , SyntaxKind.InKwToken} ,
        {"then" , SyntaxKind.ThenKwToken},
        {"else" , SyntaxKind.ElseKwToken} , 
        {"true" , SyntaxKind.TrueToken} ,
        {"false" , SyntaxKind.FalseToken} ,
        {"function" , SyntaxKind.FunctionKwToken},
        {"PI" , SyntaxKind.PIKwToken}
    };

    public static Dictionary<string , int> FunctionsDepth = new Dictionary<string, int>();

    public static List<string> Bugs;


    public static SyntaxKind GetKwKind(string text )
    {
        // Si el texto es una palabra clave devuelve su tipo de token, en otro caso devuelve un IdentifierToken.
        if(KWords.ContainsKey(text))
        {
            return KWords[text];             // usar operador '?'        
        }
        return SyntaxKind.IdentifierToken ;
    }

    public static object GetKwValue(SyntaxKind Kind)
    {
        switch(Kind)
        {
            case(SyntaxKind.TrueToken):
            case(SyntaxKind.FalseToken):
                return Kind == SyntaxKind.TrueToken ;
            case(SyntaxKind.PIKwToken) :
                return Math.PI;
            default :
                return null ;
        }        
    }
    public static int GetBinaryOpPrecedence(SyntaxKind kind)
    {
        // Se usa para darle una precedencia a cada operador, mientras mayor sea, primero se ejecutaran.
        switch (kind)
        {
            case(SyntaxKind.ExponentToken):
                return 5 ;
            
            case(SyntaxKind.StarToken):
            case(SyntaxKind.SlashToken):
                return 4 ;
            
            case(SyntaxKind.PlusSignToken):
            case(SyntaxKind.MinusToken):
            case(SyntaxKind.ArrobaToken):
            case(SyntaxKind.PercentageToken):
                return 3 ;

            case(SyntaxKind.NotEqualToken):
            case(SyntaxKind.EqualEqualToken):
            case(SyntaxKind.MoreOrEqualToken):
            case(SyntaxKind.LessOrEqualToken):
            case(SyntaxKind.LessToken):
            case(SyntaxKind.MoreToken):
                return 2 ;
            
            case(SyntaxKind.AndToken):
            case(SyntaxKind.OrToken):
            case(SyntaxKind.DobleAndToken):
            case(SyntaxKind.DobleOrToken):
                return 1 ;

            default:
                return 0 ;
        }
    }
    public static int GetUnaryOpPrecedence(SyntaxKind kind)
    {
        // La precedencia entre operadores unarios y binarios se intercala para la correcta interaccion entre ellos
        switch (kind)
        {
            case(SyntaxKind.PlusSignToken):
            case(SyntaxKind.MinusToken):
            case(SyntaxKind.NotToken):
                return 6 ;

            default:
                return 0 ;
        }
    }

    public static void AddFunctionCallCount(string name)
    {
        if(FunctionsDepth.ContainsKey(name))
        {
            FunctionsDepth[name] ++ ;
        }

        else
        {
            FunctionsDepth.Add(name , 1);
        }
    }

    public static void DecreaseFunctionCallCount(string name)
    {
        if(FunctionsDepth.ContainsKey(name))
        {
            FunctionsDepth[name] -- ;
        }
    }

    public static int GetFunctionDepth(string name)
    {   
        if(FunctionsDepth.ContainsKey(name))
            return FunctionsDepth[name] ;
        return 0 ;
    }
}

