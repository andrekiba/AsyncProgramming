using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AsyncOOP
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureServices((hostContext, services) =>
				{
					services.AddHostedService<Worker>();
					services.AddTransient<IMyFundamentalType, AsyncClass2>();
					services.AddTransient<IMyComposedType, AsyncClass3>();
				});
	}
}
