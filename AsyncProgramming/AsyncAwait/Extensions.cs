using System.Diagnostics;

namespace AsyncAwait
{
	public static class Extensions
	{
		public static void Output<T>(this T input)
		{
			Debug.WriteLine(input);
		}
	}
}
