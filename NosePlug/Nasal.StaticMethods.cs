﻿using HarmonyLib;
using NosePlug.Plugs;
using System.Linq.Expressions;
using System.Reflection;

namespace NosePlug;

partial class Nasal
{
    /// <summary>
    /// Create a method plug for a static void returning method
    /// </summary>
    /// <param name="method">The <see cref="MethodInfo"/> to create a plug for</param>
    /// <returns>A new method plug</returns>
    /// <exception cref="ArgumentNullException">When the passed in <see cref="MethodInfo"/> is <c>null</c></exception>
    public static IMethodPlug Method(MethodInfo method)
    {
        if (method is null)
        {
            throw new ArgumentNullException(nameof(method));
        }

        return new MethodPlug(method);
    }

    /// <summary>
    /// Create a method plug for a method with a return value
    /// </summary>
    /// <typeparam name="TReturn">The return type of the method</typeparam>
    /// <param name="methodInfo">The <see cref="MethodInfo"/> to create a plug for</param>
    /// <returns>A new method plug</returns>
    /// <exception cref="ArgumentNullException">When the passed in <see cref="MethodInfo"/> is <c>null</c></exception>
    public static IMethodPlug<TReturn> Method<TReturn>(MethodInfo methodInfo)
    {
        if (methodInfo is null)
        {
            throw new ArgumentNullException(nameof(methodInfo));
        }

        return new MethodPlug<TReturn>(methodInfo);
    }

    /// <summary>
    /// Create a method plug for a void returning method
    /// </summary>
    /// <param name="methodExpression">An expression referencing a method. The parameter values passed in the expression, are ignored.</param>
    /// <returns>A new method plug.</returns>
    /// <exception cref="ArgumentNullException">When the passed in <see cref="Expression&lt;Action&gt;"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">When the passed in <see cref="Expression&lt;Action&gt;"/> is not a <see cref="MethodCallExpression"/>.</exception>
    public static IMethodPlug Method(Expression<Action> methodExpression)
    {
        if (methodExpression is null)
        {
            throw new ArgumentNullException(nameof(methodExpression));
        }

        if (methodExpression.Body is MethodCallExpression methodCallExpression)
        {
            MethodInfo original = methodCallExpression.Method;
            return Method(original);
        }
        throw new ArgumentException("Expresion is not a method call expression");
    }

    /// <summary>
    /// Create a method plug for a method with a return value
    /// </summary>
    /// <typeparam name="TReturn">The return type of the method</typeparam>
    /// <param name="methodExpression">An expression referencing a method with a return value. The paramters passed in the expression, are ignored.</param>
    /// <returns>A new method plug.</returns>
    /// <exception cref="ArgumentNullException">When the passed in expression is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">When the passed in expression is not a <see cref="MethodCallExpression"/>.</exception>
    public static IMethodPlug<TReturn> Method<TReturn>(Expression<Func<TReturn>> methodExpression)
    {
        if (methodExpression is null)
        {
            throw new ArgumentNullException(nameof(methodExpression));
        }

        if (methodExpression.Body is MethodCallExpression methodCallExpression)
        {
            MethodInfo original = methodCallExpression.Method;
            return Method<TReturn>(original);
        }
        throw new ArgumentException("Expresion is not a method call expression", nameof(methodExpression));
    }

    /// <summary>
    /// Create a method plug for a static method given its name and parameters.
    /// </summary>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="parameterTypes">A collection of types matching the parameters for the method.</param>
    /// <returns>A new method plug.</returns>
    /// <exception cref="ArgumentException">When the method name is null or whitespace.</exception>
    public static IMethodPlug Method<TContainingType>(string methodName, params Type[] parameterTypes)
        => Method<TContainingType>(methodName, null, parameterTypes);

    /// <summary>
    /// Create a method plug for a method with a return value given its name and parameters.
    /// </summary>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="parameterTypes">A collection of types matching the parameters for the method.</param>
    /// <returns>A new method plug.</returns>
    /// <exception cref="ArgumentException">When the method name is null or whitespace.</exception>
    public static IMethodPlug<TReturn> Method<TContainingType, TReturn>(string methodName, params Type[] parameterTypes)
        => Method<TContainingType, TReturn>(methodName, null, parameterTypes);

    /// <summary>
    /// Create a method plug for a method given its name and parameters.
    /// </summary>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="genericTypeParameters">The generic type parameters for the method.</param>
    /// <param name="parameterTypes">A collection of types matching the parameters for the method.</param>
    /// <returns>A new method plug.</returns>
    /// <exception cref="ArgumentException">When the method name is null or whitespace.</exception>
    public static IMethodPlug Method<TContainingType>(string methodName, Type[]? genericTypeParameters, Type[] parameterTypes)
    {
        if (string.IsNullOrWhiteSpace(methodName))
        {
            throw new ArgumentException($"'{nameof(methodName)}' cannot be null or whitespace.", nameof(methodName));
        }

        var methods = AccessTools.GetDeclaredMethods(typeof(TContainingType))
            .Where(x => string.Equals(x.Name, methodName))
            .ToList();

        MethodInfo method = methods.Count switch
        {
            0 => throw new MissingMethodException(typeof(TContainingType).FullName, methodName),
            1 => methods[0],
            _ => methods.FirstOrDefault(x => x.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes))
                    ?? throw new MissingMethodException($"Could not find method '{methodName}' on '{typeof(TContainingType).FullName}' with parameter type(s) {string.Join(", ", parameterTypes.Select(x => x.FullName))}")
        };

        if (genericTypeParameters is not null)
        {
            method = method.MakeGenericMethod(genericTypeParameters);
        }

        return Method(method);
    }

    /// <summary>
    /// Create a method plug for a method with a return value given its name and parameters.
    /// </summary>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="genericTypeParameters">The generic type parameters for the method.</param>
    /// <param name="parameterTypes">A collection of types matching the parameters for the method.</param>
    /// <returns>A new method plug.</returns>
    /// <exception cref="ArgumentException">When the method name is null or whitespace.</exception>
    public static IMethodPlug<TReturn> Method<TContainingType, TReturn>(string methodName, Type[]? genericTypeParameters, Type[] parameterTypes)
    {
        if (string.IsNullOrWhiteSpace(methodName))
        {
            throw new ArgumentException($"'{nameof(methodName)}' cannot be null or whitespace.", nameof(methodName));
        }

        var methods = AccessTools.GetDeclaredMethods(typeof(TContainingType))
            .Where(x => string.Equals(x.Name, methodName))
            .ToList();

        MethodInfo method = methods.Count switch
        {
            0 => throw new MissingMethodException(typeof(TContainingType).FullName, methodName),
            1 => methods[0],
            _ => methods.FirstOrDefault(x => x.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes))
                    ?? throw new MissingMethodException($"Could not find method '{methodName}' on '{typeof(TContainingType).FullName}' with parameter type(s) {string.Join(", ", parameterTypes.Select(x => x.FullName))}")
        };

        if (genericTypeParameters is not null)
        {
            method = method.MakeGenericMethod(genericTypeParameters);
        }

        return Method<TReturn>(method);
    }

}
