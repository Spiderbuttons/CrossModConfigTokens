using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace CrossModCompatibilityTokens;

public class DynamicFactory
{
    private static Type[] CreateParameterTypes(ref MethodInfo method)
    {
        return method.GetParameters().Select(p => p.ParameterType).ToArray();
    }
    
    private static Type CreateReturnType(ref MethodInfo method)
    {
        return method.ReturnType;
    }

    public static DynamicMethod CreateDynamicMethod(MethodInfo method)
    {
        Type delegateType = Expression.GetDelegateType(CreateParameterTypes(ref method).Concat(new[] { CreateReturnType(ref method) }).ToArray());
        var dynamicMethod = new DynamicMethod(method.Name, CreateReturnType(ref method), CreateParameterTypes(ref method), typeof(DynamicFactory).Module);
        var il = dynamicMethod.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        for (int i = 1; i <= method.GetParameters().Length; i++)
            il.Emit(OpCodes.Ldarg_S, i);
        il.Emit(OpCodes.Call, method);
        il.Emit(OpCodes.Ret);
        return dynamicMethod;
    }
}