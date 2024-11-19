using System;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using BenchmarkDotNet.Attributes;

namespace LanguageExt.Benchmarks;

[RPlotExporter, RankColumn]
public class IlBenchmarks
{
	private static readonly Type ArrayListType = typeof(ArrayList);
	private static readonly ConstructorInfo Ctor = ArrayListType.GetConstructor(System.Type.EmptyTypes);
	private readonly Func<object> _dynamicMethodActivator;
	private readonly Func<ArrayList> _expression;
	private readonly Func<ArrayList> _FuncT;
	private readonly Delegate _delegate;

	private readonly dynamic _dynDelegate;

	public IlBenchmarks()
	{
		DynamicMethod createHeadersMethod = new DynamicMethod(
				$"KafkaDynamicMethodHeaders",
				ArrayListType,
				null,
				typeof(IlBenchmarks).Module,
				false);

		ILGenerator il = createHeadersMethod.GetILGenerator();
		il.Emit(OpCodes.Newobj, Ctor);
		il.Emit(OpCodes.Ret);

		_dynamicMethodActivator = (Func<object>)createHeadersMethod.CreateDelegate(typeof(Func<object>));

		_expression = Expression.Lambda<Func<ArrayList>>(Expression.New(ArrayListType)).Compile();

		_FuncT = () => new ArrayList();
		_delegate = _FuncT;
		_dynDelegate = _FuncT;
	}


	[Benchmark(Baseline = true)]
	public object Direct() => new ArrayList();

	[Benchmark]
	public object Reflection() => Ctor.Invoke(null);

	[Benchmark]
	public object ActivatorCreateInstance() => Activator.CreateInstance(ArrayListType);

	[Benchmark]
	public object CompiledExpression() => _expression();

	[Benchmark]
	public object ReflectionEmit() => _dynamicMethodActivator();

	[Benchmark]
	public object FuncT() => _FuncT();

	[Benchmark]
	public object Delegate() => _delegate.DynamicInvoke();

	[Benchmark]
	public object DynamicDelegate() => _dynDelegate();
}