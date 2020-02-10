using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TPL
{
	[TestClass]
	public class Tpl
	{
		#region Perform operation in parallel

		[TestMethod]
		public void TestParallel()
		{
			var numbers = Enumerable.Range(1, 100);
			
			MultiplyNumbers(numbers, 6);

			//MultiplyNumbers(numbers);
		}
		
		static void MultiplyNumbers(IEnumerable<int> numbers, int mul)
		{
			Parallel.ForEach(numbers, n =>
			{
				Debug.WriteLine(n * mul);
			});
		}

		//stop del loop ad una determinata condizione
		//il loop si stoppa dall'interno
		static void MultiplyNumbers(IEnumerable<int> numbers)
		{
			Parallel.ForEach(numbers, (n, state) =>
			{
				if (n % 66 == 0)
				{
					Debug.WriteLine(n);

					//utilizza ParallelLoopState.Stop per fermarlo
					//poichè l'esecuzione è parallela i numeri dopo il 66
					//che sono già in fase di processamento continuano l'esecuzione
					state.Stop();
				}
					
				else
					Debug.WriteLine(n * 2);
			});
		}

		//cancella il loop dall'esterno tramite cancellation token
		static void MultiplyNumbersCancellation(IEnumerable<int> numbers, int mul, CancellationToken token)
		{
			Parallel.ForEach(numbers, new ParallelOptions { CancellationToken = token}, n =>
			{
				Debug.WriteLine(n * mul);
			});
		}

		//attenzione che qualsiasi stato condiviso va protetto durante l'esecuzione di codice parallelo
		static void MultiplyNumbersLock(IEnumerable<int> numbers, int mul)
		{
			object mutex = new object();
			var sum = 0;
			
			Parallel.ForEach(numbers, n =>
			{
				//molto poco efficiente ma funziona
				lock (mutex)
				{
					sum += n;
					Debug.WriteLine(sum);
				}
				
				Debug.WriteLine(n * mul);
			});
		}

		#endregion

		#region Aggregation

		[TestMethod]
		public void TestParallelAggreagtion()
		{
			var numbers = Enumerable.Range(1, 100);

			var result = ParallelSum(numbers);
			Debug.WriteLine(result);
		}

		int ParallelSum(IEnumerable<int> values)
		{
			object mutex = new object();
			int result = 0;
			Parallel.ForEach(source: values,
				localInit: () => 0, //variabile locale al loop
				//il body può accedere al valore senza bisogno di sincronizzazione
				body: (item, state, localValue) => localValue + item,
				//questo delegato deve essere protetto
				localFinally: localValue =>
				{
					
					lock (mutex)
						result += localValue;
				});
			return result;
		}

		//oppure è decisamente più facile utilizzare PLINQ e tutti gli operatori che mette a disposizione
		//N.B. attenzione che PLINQ tende ad utilizzare tutte le risorse della macchina
		//mentre Parallel si adatta dinamicamente rispetto alle risorse già utilizzate

		//generalmente se il loop paralello ptoduce un output è più semplice utilizzare PLINQ

		int ParallelSum1(IEnumerable<int> values)
		{
			return values.AsParallel().Sum();
		}

		#endregion

		#region Parallel Invoke

		[TestMethod]
		public void TestParallelInvoke()
		{
			ProcessArray(new [] {1,2,3,4,5,6});
		}

		//supporta cancellazione esattamente come gli altri attraveros ParallelOptions

		void ProcessArray(int[] array)
		{
			Parallel.Invoke(
				() => ProcessPartialArray(array, 0, array.Length / 2),
				() => ProcessPartialArray(array, array.Length / 2, array.Length)
			);
		}

		void ProcessPartialArray(int[] array, int begin, int end)
		{
			//cpu bound task
			Debug.WriteLine(array.Length);
		}

		#endregion
	}
}
