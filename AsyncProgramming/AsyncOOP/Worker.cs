using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AsyncOOP
{
	public class Worker : BackgroundService
	{
		readonly ILogger<Worker> logger;
		readonly IMyFundamentalType asyncClass2;
		readonly IMyComposedType asyncClass3;

		public Worker(IMyFundamentalType asyncClass2, IMyComposedType asyncClass3, ILogger<Worker> logger)
		{
			this.asyncClass2 = asyncClass2;
			this.asyncClass3 = asyncClass3;
			this.logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			//while (!stoppingToken.IsCancellationRequested)
			//{
				logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
				await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);

				var instance = await AsyncClass1.CreateAsync();

				if (asyncClass2 is IAsyncInitialization instanceAsyncInit)
					await instanceAsyncInit.Initialization;
			//}
		}

		public override async Task StopAsync(CancellationToken stoppingToken)
		{
			logger.LogInformation("Consume Scoped Service Hosted Service is stopping.");
			await Task.CompletedTask;
		}
	}
}
