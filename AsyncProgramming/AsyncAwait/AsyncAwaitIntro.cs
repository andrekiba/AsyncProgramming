using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AsyncAwait
{
	public class AsyncAwaitIntro
	{
		#region Intro

		async Task ReadDataFromUrl(string url)
		{
			var httpclient = new HttpClient();
			var bytes = await httpclient.GetByteArrayAsync(url);
			var data = Encoding.ASCII.GetString(bytes);
		}

		#endregion
    }
}
