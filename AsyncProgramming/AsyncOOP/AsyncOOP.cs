using System;
using System.Threading.Tasks;

namespace AsyncOOP
{
	#region Async Init

	//utilizzo una factory per evitare di dimenticare di chiamare im metodo InitializeAsync

	internal class AsyncClass1
	{
		AsyncClass1()
		{
			//non fare questo!!
			//InitializeAsyncVoid();
		}

		static async void InitializeAsyncVoid()
		{
			await Task.Delay(TimeSpan.FromSeconds(3));
		}

		async Task<AsyncClass1> InitializeAsync()
		{
			await Task.Delay(TimeSpan.FromSeconds(3));
			return this;
		}

		public static Task<AsyncClass1> CreateAsync()
		{
			var instance = new AsyncClass1();
			return instance.InitializeAsync();
		}
	}

	#endregion

	#region Async Init Dependecy Injection

	//il pattern precedente non funziona con DI, più in generale non funziona con reflection
	//quindi nemmeno se l'instanza è costruita tramite data binding o Activator.CreateInstance

	//l'unica cosa che si può fare in questo caso è ritornare un istanza non completamente inizializzata
	//ma è possibile mitigare utilizzando Async Initialization Pattern

	//interfaccia marker
	public interface IAsyncInitialization
	{
		//il tipo che necessita di una inizializzazione asincrona deve definire una proprietà come questa
		Task Initialization { get; }
	}

	public interface IMyFundamentalType
	{
    }

    public class AsyncClass2 : IMyFundamentalType, IAsyncInitialization
    {
	    public Task Initialization { get; private set; }

	    public AsyncClass2()
	    {
		    //la proprietà viene assegnata nel normale costruttore della classe
			//il risultato dell'inizializzazione sarà quindi disponibile attraverso questo prop (eventuali eccezioni comprese)
		    Initialization = InitializeAsync();
	    }

	    async Task InitializeAsync()
	    {
		    await Task.Delay(TimeSpan.FromSeconds(3));
	    }
    }

	//posso anche fare composition utilizzando questo pattern

    public interface IMyComposedType
    {
    }

	public class AsyncClass3 : IMyComposedType, IAsyncInitialization
	{
		readonly IMyFundamentalType fundamental;
	    
	    public Task Initialization { get; private set; }

	    public AsyncClass3(IMyFundamentalType fundamental)
	    {
		    this.fundamental = fundamental;
		    Initialization = InitializeAsync();
	    }

		async Task InitializeAsync()
		{
			//eventualmente posso prima attendere l'inizializzazione dell'istanza base di cui ho bisogno
			if (fundamental is IAsyncInitialization fundamentalAsyncInit)
			    await fundamentalAsyncInit.Initialization;

			//e poi procedere con l'inizializzazione di questa istanza
		    await Task.Delay(TimeSpan.FromSeconds(3));
	    }
    }

	#endregion
}
