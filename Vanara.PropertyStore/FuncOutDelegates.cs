namespace System
{
	/// <summary>
	/// Encapsulates a method that has two parameters, the last being an <c>out</c> parameter, and returns a value of the type specified by the TResult parameter.
	/// </summary>
	/// <typeparam name="T1">The type of the first parameter of the method that this delegate encapsulates. This type parameter is contravariant. That is, you can use either the type you specified or any type that is less derived.</typeparam>
	/// <typeparam name="TOut">The type of the out parameter.</typeparam>
	/// <typeparam name="TRet">The type of the return value of the method that this delegate encapsulates. This type parameter is covariant. That is, you can use either the type you specified or any type that is more derived.</typeparam>
	/// <param name="arg1">The first parameter of the method that this delegate encapsulates.</param>
	/// <param name="argOut">The out argument of the method that this delagate encapsulates.</param>
	/// <returns>The return value of the method that this delegate encapsulates.</returns>
	public delegate TRet FuncOut<in T1, TOut, out TRet>(T1 arg1, out TOut argOut);

	/// <summary>
	/// Encapsulates a method that has three parameters, the last being an <c>out</c> parameter, and returns a value of the type specified by the TResult parameter.
	/// </summary>
	/// <typeparam name="T1">The type of the first parameter of the method that this delegate encapsulates. This type parameter is contravariant. That is, you can use either the type you specified or any type that is less derived.</typeparam>
	/// <typeparam name="T2">The type of the second parameter of the method that this delegate encapsulates. This type parameter is contravariant. That is, you can use either the type you specified or any type that is less derived.</typeparam>
	/// <typeparam name="TOut">The type of the out parameter.</typeparam>
	/// <typeparam name="TRet">The type of the return value of the method that this delegate encapsulates. This type parameter is covariant. That is, you can use either the type you specified or any type that is more derived.</typeparam>
	/// <param name="arg1">The first parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg2">The second parameter of the method that this delegate encapsulates.</param>
	/// <param name="argOut">The out argument of the method that this delagate encapsulates.</param>
	/// <returns>The return value of the method that this delegate encapsulates.</returns>
	public delegate TRet FuncOut<in T1, in T2, TOut, out TRet>(T1 arg1, T2 arg2, out TOut argOut);

	/// <summary>
	/// Encapsulates a method that has four parameters, the last being an <c>out</c> parameter, and returns a value of the type specified by the TResult parameter.
	/// </summary>
	/// <typeparam name="T1">The type of the first parameter of the method that this delegate encapsulates. This type parameter is contravariant. That is, you can use either the type you specified or any type that is less derived.</typeparam>
	/// <typeparam name="T2">The type of the second parameter of the method that this delegate encapsulates. This type parameter is contravariant. That is, you can use either the type you specified or any type that is less derived.</typeparam>
	/// <typeparam name="T3">The type of the third parameter of the method that this delegate encapsulates. This type parameter is contravariant. That is, you can use either the type you specified or any type that is less derived.</typeparam>
	/// <typeparam name="TOut">The type of the out parameter.</typeparam>
	/// <typeparam name="TRet">The type of the return value of the method that this delegate encapsulates. This type parameter is covariant. That is, you can use either the type you specified or any type that is more derived.</typeparam>
	/// <param name="arg1">The first parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg2">The second parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg3">The third parameter of the method that this delegate encapsulates.</param>
	/// <param name="argOut">The out argument of the method that this delagate encapsulates.</param>
	/// <returns>The return value of the method that this delegate encapsulates.</returns>
	public delegate TRet FuncOut<in T1, in T2, in T3, TOut, out TRet>(T1 arg1, T2 arg2, T3 arg3, out TOut argOut);

	/// <summary>
	/// Encapsulates a method that has five parameters, the last being an <c>out</c> parameter, and returns a value of the type specified by the TResult parameter.
	/// </summary>
	/// <typeparam name="T1">The type of the first parameter of the method that this delegate encapsulates. This type parameter is contravariant. That is, you can use either the type you specified or any type that is less derived.</typeparam>
	/// <typeparam name="T2">The type of the second parameter of the method that this delegate encapsulates. This type parameter is contravariant. That is, you can use either the type you specified or any type that is less derived.</typeparam>
	/// <typeparam name="T3">The type of the third parameter of the method that this delegate encapsulates. This type parameter is contravariant. That is, you can use either the type you specified or any type that is less derived.</typeparam>
	/// <typeparam name="T4">The type of the fourth parameter of the method that this delegate encapsulates. This type parameter is contravariant. That is, you can use either the type you specified or any type that is less derived.</typeparam>
	/// <typeparam name="TOut">The type of the out parameter.</typeparam>
	/// <typeparam name="TRet">The type of the return value of the method that this delegate encapsulates. This type parameter is covariant. That is, you can use either the type you specified or any type that is more derived.</typeparam>
	/// <param name="arg1">The first parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg2">The second parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg3">The third parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg4">The fourth parameter of the method that this delegate encapsulates.</param>
	/// <param name="argOut">The out argument of the method that this delagate encapsulates.</param>
	/// <returns>The return value of the method that this delegate encapsulates.</returns>
	public delegate TRet FuncOut<in T1, in T2, in T3, in T4, TOut, out TRet>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, out TOut argOut);

	/// <summary>
	/// Encapsulates a method that has six parameters, the last being an <c>out</c> parameter, and returns a value of the type specified by the TResult parameter.
	/// </summary>
	/// <typeparam name="T1">The type of the first parameter of the method that this delegate encapsulates. This type parameter is contravariant. That is, you can use either the type you specified or any type that is less derived.</typeparam>
	/// <typeparam name="T2">The type of the second parameter of the method that this delegate encapsulates. This type parameter is contravariant. That is, you can use either the type you specified or any type that is less derived.</typeparam>
	/// <typeparam name="T3">The type of the third parameter of the method that this delegate encapsulates. This type parameter is contravariant. That is, you can use either the type you specified or any type that is less derived.</typeparam>
	/// <typeparam name="T4">The type of the fourth parameter of the method that this delegate encapsulates. This type parameter is contravariant. That is, you can use either the type you specified or any type that is less derived.</typeparam>
	/// <typeparam name="T5">The type of the fifth parameter of the method that this delegate encapsulates. This type parameter is contravariant. That is, you can use either the type you specified or any type that is less derived.</typeparam>
	/// <typeparam name="TOut">The type of the out parameter.</typeparam>
	/// <typeparam name="TRet">The type of the return value of the method that this delegate encapsulates. This type parameter is covariant. That is, you can use either the type you specified or any type that is more derived.</typeparam>
	/// <param name="arg1">The first parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg2">The second parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg3">The third parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg4">The fourth parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg5">The fifth parameter of the method that this delegate encapsulates.</param>
	/// <param name="argOut">The out argument of the method that this delagate encapsulates.</param>
	/// <returns>The return value of the method that this delegate encapsulates.</returns>
	public delegate TRet FuncOut<in T1, in T2, in T3, in T4, in T5, TOut, out TRet>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, out TOut argOut);
}