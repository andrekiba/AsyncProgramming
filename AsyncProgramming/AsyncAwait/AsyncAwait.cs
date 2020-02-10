using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;

namespace AsyncAwait
{
    [TestClass]
    public class AsyncAwait
    {
	    const string Google = "http://www.google.it";

        #region Synchronization Context

        [TestMethod]
        public void TestWithSyncContext()
        {
	        AsyncContext.Run(Run);
        }

        [TestMethod]
        public async Task TestWithoutSyncContext()
        {
	        await Run();
        }

        static async Task Run()
        {
	        Debug.WriteLine($"SynchronizationContext {SynchronizationContext.Current?.ToString() ?? "null"}");
	        
	        Debug.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} start");

	        Task<int> t = DoSomethingAsync();
	        
	        Debug.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} libero di fare altro nel frattempo!");
	        
	        var result = await t;
	        
	        Debug.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} il risultato è {result}");

	        Debug.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} end");
        }

        static async Task<int> DoSomethingAsync()
        {
	        Debug.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} start DoSomethingAsync");
	        
	        var result = 6;

	        await Task.Delay(TimeSpan.FromSeconds(3));
	        
	        Debug.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} incremento risultato di 10");
	        result += 10;

	        Debug.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} end DoSomethingAsync");
	        
	        return result;
        }

        #endregion

        #region Pausing - Retry

        [TestMethod]
        public async Task TestPausingRetry()
        {
	        (await DelayResult("ciao!", TimeSpan.FromSeconds(1))).Output();

	        (await DownloadStringWithRetries(new HttpClient(), Google)).Output();
	        (await DownloadStringWithTimeout(new HttpClient(), Google)).Output();
        }

        static async Task<T> DelayResult<T>(T result, TimeSpan delay)
        {
	        await Task.Delay(delay);
	        return result;
        }

        static async Task<string> DownloadStringWithRetries(HttpClient client, string uri)
        {
	        // riprova dopo 1, 2, 4 secondi
	        var nextDelay = TimeSpan.FromSeconds(1);
	        for (var i = 0; i != 3; ++i)
	        {
		        try
		        {
			        return await client.GetStringAsync(uri);
		        }
		        catch
		        {
			        // se va in eccezione attende e riproverà
		        }

		        await Task.Delay(nextDelay);
		        nextDelay += nextDelay;
	        }

	        // riprova un'ultima volta, se va in eccezione viene salvata nel task e propagata
	        return await client.GetStringAsync(uri);
        }

        //utile nel caso ci fosse un metodo che non supporta cancellation
        static async Task<string> DownloadStringWithTimeout(HttpClient client, string uri)
        {
	        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
	        var downloadTask = client.GetStringAsync(uri);
	        
            //timespan ifinito e cancellation token per creare un task
            //che verrà cancellato (set canceled) dopo il tempo desiderato
	        var timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);

	        var completedTask = await Task.WhenAny(downloadTask, timeoutTask);
	        
	        if (completedTask == timeoutTask)
		        return null;
	        
	        return await downloadTask;
        }

        #region DelayImplementation

        static Task Delay(int milliseconds)
        {
	        var tcs = new TaskCompletionSource<object>();

	        var timer = new Timer(x => tcs.SetResult(null), null, milliseconds, Timeout.Infinite);

	        tcs.Task.ContinueWith(x => timer.Dispose());

	        return tcs.Task;
        }

        #endregion

        #endregion 

        #region CPU-bound

        [TestMethod]
        public async Task TestCpuBound()
        {
            //esegue il metodo in parallelo su più thread
            Parallel.ForEach(Enumerable.Range(1, 100), CpuBoundMethod);

            //è corretto utilizzare Task.Run solo per operazioni CPU-bound poichè utilizza un thread
            await Task.Run(() => CpuBoundMethod(100));
            
            await Task.Factory.StartNew(() => CpuBoundMethod(101), 
	            CancellationToken.None, 
	            TaskCreationOptions.DenyChildAttach,
	            TaskScheduler.Default);
        }

        static void CpuBoundMethod(int n)
        {
            Debug.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} scrivo {n}");
        }

        #endregion

        #region IO-bound

        [TestMethod]
        public void TestDoSomethingAsync()
        {
	        AsyncContext.Run(async () =>
	        {
		        var t = DoSomethingAsync();

		        Debug.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} libero di fare altro nel frattempo!");

		        var result = await t;

		        Debug.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} risultato finale: {result}");
	        });
        }

        [TestMethod]
        public async Task TestIoBound()
        {
	        Debug.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} start IoBoundMethod");

	        await IoBoundMethod();

	        Debug.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} end IoBoundMethod");
        }

        static async Task IoBoundMethod()
        {
	        await using var stream = new FileStream(".\\IoBound.txt", FileMode.OpenOrCreate);
	        await using var writer = new StreamWriter(stream);
	        await writer.WriteLineAsync("Scrivo 6 in asincrono!");
	        writer.Close();
	        stream.Close();
        }

        #endregion

        #region Sequential - Concurrent

        [TestMethod]
        public async Task TestSequential()
        {
            var sequential = Enumerable.Range(1, 4).Select(t => Task.Delay(TimeSpan.FromSeconds(1)));

            foreach (var task in sequential)
            {
                await task;
            }
        }

        [TestMethod]
        public async Task TestConcurrent()
        {
            var concurrent = Enumerable.Range(1, 4).Select(t => Task.Delay(TimeSpan.FromSeconds(1)));
            
            await Task.WhenAll(concurrent);
            
            //await Task.WhenAny(concurrent);
        }

        #endregion

        #region Async Exception

        //le eccezioni sollevate in un metodo marcato come async Task vengono catturate
        //e messe nel task stesso e vengono sollevate solo quando il task in questione verrà awaitato
        //quando questo accade la prima eccezione contenuta nel task viene ri-sollevata e il suo stack trace
        //preservato completamente (non c'è il problema del re-throw a cui siamo abituati)

        //N.B. in caso di Task.WhenAll solo la prima eventuale eccezione viene sollevata

        //N.B. attenzione che non è così se il metodo ritorna Task ma non è marcato con async!

        //N.B. attenzione che non è così se il metodo è "async void" perchè non esiste alcun task!!

        [TestMethod]
        public async Task TestTrySomethingAsync()
        {
            await TrySomethingAsync();

            //è possibile utilizzare Assert.ThrowsExceptionAsync per testare che un metodo effettivamente sollevi eccezione
            //questo approcio è decisamente meglio di ExpectedException perchè so quel'è l'azione che deve sollevare eccezione
            //N.B. attenzione che Assert va awaitato perchè propaga eventuali errori sull'assert
            await Assert.ThrowsExceptionAsync<NotImplementedException>(async () => await ThrowExceptionAsync(), "Eccezione!");
        }

        static async Task TrySomethingAsync()
        {
            // Il metodo inizia e l'eccezione viene salvata nel task
            var task = ThrowExceptionAsync();
            try
            {
                // l'eccezione viene sollevata qui quando si attende il task
                await task;
            }
            catch (NotImplementedException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        static async Task ThrowExceptionAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            throw new NotImplementedException();
        }

        #endregion

        #region Async Void

        //se vogliamo essere sicuri di catchare un'eccezione in un metodo async void non c'è una buona soluzione!
        //se possibile la cosa giusta da fare è cambiare la firma in async Task

        //questo non è sempre possibile tipo negli ICommand o negli event handler 
        //in questo cosa la via migliore è usare qualcosa tipo SafeFireAndForget
        
        //se occorre testare un metodo async void è meglio scrivere tutto il codice in un metodo async Task e testare quello
        //poi qeusto metodo costruito ad-hoc verrà richiamato dal metodo async void

        [TestMethod]
        public async Task TestAsyncVoid()
        {
	        var catched = false;
	        try
	        {
		        AvoidAsyncVoid();
		        
		        //await UseAsyncTask();
                
		        //UseAsyncTask().SafeFireAndForget(e =>
                //{
	                //poichè è fire and forget il test termina prima di vedere questo messaggio!
                    //ma in questo modo l'eccezione non è persa per sempre in un codice realistico
	            //    e.Message.Output();
                //});
	        }
	        catch (InvalidOperationException e)
	        {
		        Debug.WriteLine(e.Message);
		        catched = true;
	        }

            if(!catched)
				Debug.WriteLine("dove è finita l'eccezione?");
        }

        static async void AvoidAsyncVoid()
        {
            //anche mettendo la try-catch dentro al metodo le cose non si risolvono!

	        //try
            //{
                Debug.WriteLine("Sono dentro AvoidAsyncVoid");
		        await Task.Delay(TimeSpan.FromSeconds(1));

		        Debug.WriteLine("Sto per sollevare eccezione");
		        throw new InvalidOperationException("Eccezione non catturata!");
	        //}
	        //catch (Exception e)
	        //{
	        //    e.Output();
	        //}
        }

        static async Task UseAsyncTask()
        {
	        Debug.WriteLine("Sono dentro UseAsyncTask");
	        await Task.Delay(TimeSpan.FromSeconds(1));

	        Debug.WriteLine("Sto per sollevare eccezione");
	        throw new InvalidOperationException("Eccezione catturata!");
        }

        //quando un metodo async void solleva eccezione questa viene propagata al SynchronizationContext
        //attivo nel momento in cui il metodo async void ha iniziato la sua esecuzione
        //solitamente è quindi possibile gestire queste eccezioni ad un livello più alto
        //ad esempio Application.DisptacherUnhandledException per WPF o il middleware UseExceptionHandler per ASP.NET

        #endregion

        #region Deadlock

        [TestMethod]
        [Ignore]
        public void TestDeadlock()
        {
            AsyncContext.Run(async () =>
            {
                await Deadlock();

                //var jsonTask = GetJsonAsync(Google);
                //var result = jsonTask.Result;
            });
        }

        static async Task Deadlock()
        {
            //richiedere Result significa bloccare in modo sincorono il chiamante in attesa del risultato
            //se SynchronizationContext ammette un singolo thread accade che il thread rimane bloccato in attesa
            //e non può essere richiamato quando il Task è completo 

            var result = DoSomethingAsync().Result;
            //var result = await DoSomethingAsync();

            //Task.Delay(TimeSpan.FromSeconds(2)).Wait();
            //await Task.Delay(TimeSpan.FromSeconds(2))

            //qui non ci arriva mai --> deadlock
            Debug.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} rusultato: {result}");
        }

        static async Task<JObject> GetJsonAsync(string url)
        {
	        using var client = new HttpClient();
	        var jsonString = await client.GetStringAsync(new Uri(url));
	        return JObject.Parse(jsonString);
        }

        #endregion

        #region ConfigureAwait

        //è importante valutare se il contesto è importante quando viene eseguita la continuazione oppure no
        //questo perchè la macchina a stati ha un costo di gestione (circa 100 byte per ogni await)
        //inoltre una stima plausibile può essere che circa 100 continuazione al secondo possono essere ok per UI thread
        //di più comincia ad essere problematico a livello di performance (ad esempio skipped frames)

        //se si ha un metodo asincrono che in parte necessita del contesto e in parte no
        //è meglio fare refactoring e splittarlo così da non usare il contesto dove non necessario

        [TestMethod]
        public void TestConfigureAwait()
        {
            AsyncContext.Run(async () =>
            {
	            Debug.WriteLine($"SynchronizationContext {SynchronizationContext.Current?.ToString() ?? "null"}");

                await DoSomethingAsync();

                Debug.WriteLine($"SynchronizationContext {SynchronizationContext.Current?.ToString() ?? "null"}");

                await DoSomethingAsync().ConfigureAwait(false);

                Debug.WriteLine($"SynchronizationContext {SynchronizationContext.Current?.ToString() ?? "null"}");

                Debug.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId}");
            });
        }

        //WPF
        async void DownloadButton_Click(object sender, EventArgs e)
        {
            // Attende in modo asincrono UI thread non è bloccato
            await DownloadFileAsync("file.txt");

            // viene recuperato il contesto e quindi possiamo aggiornare direttamente la UI
            //resultTextBox.Text = "File downloaded!";
        }

        async Task DownloadFileAsync(string fileName)
        {

            // utilizziamo HttpClient o simili per fare il download.
            //var fileContent = await DownloadFileContentsAsync(fileName).ConfigureAwait(false);

            // poichè abbiamo usato ConfigureAwait(false), qui non siamo più nel contesto della UI.
            // Invece stiamo eseguendo su un thread del thread pool

            // scrive il file su disco in asincrono
            //await WriteToDiskAsync(fileName, fileContent).ConfigureAwait(false);

            // il secondo ConfigureAwait non è necessario ma è buona pratica metterlo
        }

        #endregion

        #region Completed Task

        [TestMethod]
        public async Task TestAlreadyCompleted()
        {
            var mySyncImpl = new MySynchronousImplementation();
            
            (await mySyncImpl.GetValueAsync()).Output();
            
            await mySyncImpl.DoAsync();

            try
            {
	            await mySyncImpl.NotImplementedAsync<NotImplementedException>();
            }
            catch (NotImplementedException e)
            {
	            e.Output();
            }

            try
            {
	            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
	            cts.Cancel();
	            (await mySyncImpl.GetValueCanceledAsync(cts.Token)).Output();
            }
            catch (TaskCanceledException e)
            {
	            e.Output();
            }
        }

        interface IMyAsyncInterface
        {
	        Task<int> GetValueAsync();
	        Task DoAsync();
	        Task<T> NotImplementedAsync<T>();
	        Task<int> GetValueCanceledAsync(CancellationToken ct = default);
        }

        class MySynchronousImplementation : IMyAsyncInterface
        {
	        public Task<int> GetValueAsync()
	        {
		        return Task.FromResult(42);
	        }

	        public Task DoAsync()
	        {
		        return Task.CompletedTask;
	        }

	        public Task<T> NotImplementedAsync<T>()
	        {
		        return Task.FromException<T>(new NotImplementedException());
	        }

	        public Task<int> GetValueCanceledAsync(CancellationToken ct)
	        {
		        return ct.IsCancellationRequested ? Task.FromCanceled<int>(ct) : Task.FromResult(16);
	        }
        }


        #endregion

        #region Caching

        static readonly Task<int> zeroTask = Task.FromResult(0);

        public Task<int> GetValueAsync()
        {
	        return zeroTask;
        }

        [TestMethod]
        public Task TestCaching()
        {
	        return GetValueAsync();
        }

        #endregion 

        #region Composition

        [TestMethod]
        public async Task TestComposition()
        {
	        //await DoOperationsConcurrentlyAsync();
	        //await DownloadAllAsync(new HttpClient(), new List<string>{ Google, "www.facebook.com"});
	        //await GetFirstToRespondAsync();
	        await GetFirstToRespondMaybeFaultedAsync();
            //await ObserveAllExceptionsAsync();
        }

        #region All

        public async Task DoOperationsConcurrentlyAsync()
        {
            var tasks = new Task<int>[3];
            tasks[0] = DoSomethingAsync();
            tasks[1] = DoSomethingAsync();
            tasks[2] = DoSomethingAsync();

            // a questo punto tutti e 3 i task sono in running

            // WhenAll reswtituisce un task che diventa completo quando tutti i task sottesi sono completi
            // se i task ritornano tutti lo stesso tipo e nessuno fallisce il risultato sarà un array
            // se anche uno solo fallisce task conterrà l'eccezione
            var result = await Task.WhenAll(tasks);
            result.Output();
        }

        static async Task<string> DownloadAllAsync(HttpClient client, IEnumerable<string> urls)
        {
	        var downloads = urls.Select(client.GetStringAsync);
	        // nessun task è ancora iniziato perchè la sequenza non è ancora stata valutata (lazy)

	        // tutti i download partono contemporaneamente
	        Task<string>[] downloadTasks = downloads.ToArray();
	        // adesso tutti i task sono partiti

	        // attesa asincrona di tutti quanti
	        string[] htmlPages = await Task.WhenAll(downloadTasks);

	        return string.Concat(htmlPages);
        }

        #endregion 

        #region Any

        public async Task<int> GetFirstToRespondAsync()
        {
            // ad esempio chiama due web service e vede chi risponde prima
            Task<int>[] tasks = { DoSomethingAsync(), DoSomethingAsync() };

            // attende il primo che risponde
            var firstTask = await Task.WhenAny(tasks);

            // Return the result.
            return await firstTask;
        }

        //il task ritornato da Task.WhenAny non viene mai completato in stato faulted o canceled
        //completa sempre con successo e il risultato è il task interno che completa per primo
        //se il task interno non completa con successo l'eccezione non viene propagata al task esterno
        //se si vuole osservare l'eccezione è necessario awaitare il task interno dopo il suo completamento

        //inoltre occorre considerare che anche quando il primo task completa
        //gli altri vanno cmq avanti nell'esecuzione e sarebbe quindi buona norma cancellarli
        //altriementi a loro volta verranno completati e abbandonati
        public async Task GetFirstToRespondMaybeFaultedAsync()
        {
	        var maybeFoultedTasks = new Task<int>[3];
	        maybeFoultedTasks[0] = DoSomethingAsync();
	        maybeFoultedTasks[1] = ThrowNotImplementedExceptionAsync();
	        maybeFoultedTasks[2] = ThrowInvalidOperationExceptionAsync();

	        // attende il primo che risponde
	        var firstTask = await Task.WhenAny(maybeFoultedTasks);

	        try
	        {
		        await firstTask;
            }
	        catch (Exception e)
	        {
		        e.Output();
	        }
        }

        #endregion 

        #region Exception

        //mettere l'async in questo caso è importante perchè in questo modo l'eccezione verrà
        //messa all'interno del task (che completerà quindi in stato faulted) e non sollevata direttamente
        //verrà valutata quando il taks verrà awaitato
        static async Task<int> ThrowNotImplementedExceptionAsync()
        {
	        throw new NotImplementedException();
        }
        static async Task<int> ThrowInvalidOperationExceptionAsync()
        {
	        throw new InvalidOperationException();
        }

        async Task ObserveOneExceptionAsync()
        {
	        var task1 = ThrowNotImplementedExceptionAsync();
	        var task2 = ThrowInvalidOperationExceptionAsync();

	        try
	        {
		        await Task.WhenAll(task1, task2);
	        }
	        catch (Exception ex)
	        {
		        // "ex" può essere NotImplementedException oppure InvalidOperationException
                ex.GetType().Output();
	        }
        }

        async Task ObserveAllExceptionsAsync()
        {
	        var task1 = ThrowNotImplementedExceptionAsync();
	        var task2 = ThrowInvalidOperationExceptionAsync();

	        Task allTasks = Task.WhenAll(task1, task2);
	        try
	        {
		        await allTasks;
	        }
	        catch
	        {
		        AggregateException allExceptions = allTasks.Exception;
                allExceptions.InnerExceptions.ToList().ForEach(e => e.Output());
	        }
        }

        #endregion

        #region Process as they complete

        async Task<int> DelayAndReturnAsync(int value)
        {
	        await Task.Delay(TimeSpan.FromSeconds(value));
	        return value;
        }

        //processati in ordine indipendentemente dall'ordine di completamento
        async Task ProcessTasksInOrderAsync()
        {
	        var taskA = DelayAndReturnAsync(2);
	        var taskB = DelayAndReturnAsync(3);
	        var taskC = DelayAndReturnAsync(1);
	        Task<int>[] tasks = { taskA, taskB, taskC };

	        // Await di ogni task in ordine
	        foreach (var task in tasks)
	        {
		        var result = await task;
		        result.Output();
	        }
        }

        //processati in ordine di completamento
        //rispetto alla soluzione precedente è molto differente perchè in questo caso
        //i task vengono processati in modo concorrente e non uno alla volta
        async Task ProcessTasksByCompletionAsync()
        {
            // Create a sequence of tasks.
            var taskA = DelayAndReturnAsync(2);
            var taskB = DelayAndReturnAsync(3);
            var taskC = DelayAndReturnAsync(1);
            Task<int>[] tasks = {taskA, taskB, taskC};

	        var taskQuery = from t in tasks select AwaitAndProcessAsync(t);
	        var processingTasks = taskQuery.ToArray();

			//Task[] processingTasks = tasks.Select(async t =>
			//{
			//    var result = await t;
			//    result.Output();
			//}).ToArray();

	        // Await all processing to complete
	        await Task.WhenAll(processingTasks);
        }

        //mi serve un metodo che possa awaitare il singolo task e processarlo
        static async Task AwaitAndProcessAsync(Task<int> task)
        {
	        var result = await task;
	        result.Output();
        }

        //se si vuole evitare di processare i task in modo concorrente (perchè ad esempio agiscono su di un oggetto condiviso)
        //è possibile utilizzare un lock asincrono implementato tramite SemaphoreSlim
        //oppure in Nito.AsyncEx esiste un extension method che ordina per completamento (utilizza TaskCompletionSource)
        async Task UseOrderByCompletionAsync()
        {
            // Create a sequence of tasks.
            var taskA = DelayAndReturnAsync(2);
            var taskB = DelayAndReturnAsync(3);
            var taskC = DelayAndReturnAsync(1);
            var tasks = new[] {taskA, taskB, taskC};

	        // Await each one as they complete.
	        foreach (var task in tasks.OrderByCompletion())
	        {
		        var result = await task;
		        result.Output();
	        }
        }

        #endregion 

        #endregion

        #region Report Progress

        [TestMethod]
        public void TestReportProgressAsync()
        {
            AsyncContext.Run(async () =>
            {
                await ReportProgressAsync();
            });
        }

        static async Task ReportProgressAsync()
        {
            //attenzione che il report può avvenire in asincrono (mentre il metodo principale continua l'esecuzione)
            //quindi è meglio utilizzare un tipo immutabile o almeno value type
            //come parametro T per evitare che il valore venga modificato dalla continuazione del metodo in asincrono  
            var progress = new Progress<int>(); //implementa IProgress<T>
            progress.ProgressChanged += (sender, p) =>
            {
                //N.B.: la callback cattura il contesto
                //sappiamo che quando viene costrutita in questo caso il contesto è quello
                //del Main thread quindi è possibile aggiornare l'interfaccia senza incorrere in problemi
                //anche se il nostro metodo asincrono invoca la callback di report da un thread di background
                Debug.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} report progress: {p}");
            };

            var result = await DoSomethingWithProgressAsync(progress);
        }

        static async Task<int> DoSomethingWithProgressAsync(IProgress<int> progress = null)
        {
            var val = 6;
            await Task.Delay(TimeSpan.FromSeconds(1));
            Debug.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} incremento risultato di 10");
            val += 10;
            progress?.Report(val);
            await Task.Delay(TimeSpan.FromSeconds(1));
            Debug.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} incremento risultato di 10");
            val += 10;
            progress?.Report(val);
            await Task.Delay(TimeSpan.FromSeconds(1));
            Debug.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} incremento risultato di 10");
            val += 10;
            progress?.Report(val);
            return val;
        }

        #endregion

        #region Cancellation
        //la cancellazione è cooperativa quindi significa che un metodo può essere cancellato
        //solo se la sua implementazione lo supporta tramite passaggio di un cancellation token
        
        //esiste una fonte che triggera la cancellazione: CancellationTokenSource
        //esiste il destinatario che viene cancellato: CancellationToken
        
        //il codice cancellato per convenzione solleva OperationCanceledException o una sua derivata
        //in questo modo il codice che ha richiesto la cancellazione può conoscere che è effettivamente stata eseguita

        [TestMethod]
        public async Task TestCancellation()
        {
            await Cancellation();
        }

        static async Task Cancellation()
        {
            var source = new CancellationTokenSource(TimeSpan.FromSeconds(2));
           //source.CancelAfter(TimeSpan.FromSeconds(2));
            var task = Task.Run(() => SlowMethod(source.Token), source.Token);

            try
            {
                await task;
            }
            catch (OperationCanceledException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        static void SlowMethod(CancellationToken cancellationToken)
        {
            for (var i = 0; i < 200000; i++)
            {
                if (i % 1000 == 0)
                    cancellationToken.ThrowIfCancellationRequested();
            }
        }

        #endregion

        #region Directly return a Task

        [TestMethod]
        public async Task TestDirectlyReturnATask()
        {
	        var r1 = await GetStringAsync(Google);

	        try
	        {
		        var r2 = await GetStringWithAsyncAwait1(Google);

		        //var r3 = await GetStringWithoutAsyncAwait1(google);

		        //var task1 = GetStringWithAsyncAwait2();
		        //var r4 = await task1; //exception thrown here

		        var task2 = GetStringWithoutAsyncAwait2(); //excetpion thrown here
		        var r5 = await task2;
            }
	        catch (Exception e)
	        {
		        Debug.WriteLine(e);
	        }
        }

        //OK perchè di fatto è soltanto passthrough
        static Task<string> GetStringAsync(string url)
        {
            var client = new HttpClient();
            return client.GetStringAsync(url);
        }
        
        //se invece ho altro codice devo stare attento
        //ad esempio nel caso in cui utilizzo uno "using"
        static async Task<string> GetStringWithAsyncAwait1(string url)
        {
	        using var client = new HttpClient();
	        return await client.GetStringAsync(url);
        }

        static Task<string> GetStringWithoutAsyncAwait1(string url)
        {
	        using var client = new HttpClient();
	        return client.GetStringAsync(url);
        }

        //oppure se c'è codice che può generare eccezioni
        public async Task<string> GetStringWithAsyncAwait2()
        {
	        var client = new HttpClient();
            const string url = Google;
	        //codice che potrebbe causare eccezione
	        if(url.EndsWith("it"))
				throw new InvalidOperationException("ciao!");
	        
	        return await client.GetStringAsync(url);
        }

        public Task<string> GetStringWithoutAsyncAwait2()
        {
	        var client = new HttpClient();
	        const string url = Google;
	        //codice che potrebbe causare eccezione
	        if (url.EndsWith("it"))
		        throw new InvalidOperationException("ciao!");
            
	        return client.GetStringAsync(url);
        }

        #endregion

        #region Guidelines

        /*

        Old                     New                                 Description
        
        task.Wait	            await task	                        Wait/await for a task to complete
        
        task.Result	            await task	                        Get the result of a completed task
        
        Task.WaitAny	        await Task.WhenAny	                Wait/await for one of a collection of tasks to complete
        
        Task.WaitAll	        await Task.WhenAll	                Wait/await for every one of a collection of tasks to complete
        
        Thread.Sleep	        await Task.Delay	                Wait/await for a period of time
        
        Task constructor	    Task.Run or TaskFactory.StartNew	Create a code-based task

        */

        #endregion

        #region Mapping

        /*
            
        Type                                    Lambda                                                  Parameters	    Return Value
            
        Action	                                () => { }	                                            None	        None
        Func<Task>	                            async () => { await Task.Yield(); }	                    None	        None
            
        Func<TResult>	                        () => { return 6; }	                                    None	        TResult
        Func<Task<TResult>>	                    async () => { await Task.Yield(); return 6; }	        None	        TResult
            
        Action<TArg1>	                        x => { }	                                            TArg1	        None
        Func<TArg1, Task>	                    async x => { await Task.Yield(); }	                    TArg1	        None
            
        Func<TArg1, TResult>	                x => { return 6; }	                                    TArg1	        TResult
        Func<TArg1, Task<TResult>>	            async x => { await Task.Yield(); return 6; }	        TArg1	        TResult
            
        Action<TArg1, TArg2>	                (x, y) => { }	                                        TArg1, TArg2	None
        Func<TArg1, TArg2, Task>	            async (x, y) => { await Task.Yield(); }	                TArg1, TArg2	None
            
        Func<TArg1, TArg2, TResult>	            (x, y) => { return 6; }	                                TArg1, TArg2	TResult
        Func<TArg1, TArg2, Task<TResult>>	    async (x, y) => { await Task.Yield(); return 6; }	    TArg1, TArg2	TResult

        */

        #endregion

        #region ValueTask

        //i ValueTask sono utilizzati in scenari dove un metodo asincrono viene eseguito la maggior parte delle volte (hot path)
        //in modo sincrono e il comportamento asincrono è più raro
        //dal nome si capisce che è un value type (struct) al contrario di Task che è una classe
        //per quanto riguarda un'app l'utilizzo di Task rimane cmq consigliato, ValueTask può essere una soluzione per ottimazzare
        //oppure per chi scrive librerie

		[TestMethod]
		[DataRow(1)]
		[DataRow(6)]
        public async Task TestValueTask(int value)
        {
	        //N.B. attenzione che i value task si awaitano una sola volta!
	        var result = await CalcualteNumberAsync(value);
	        result.Output();

            //se occorre fare qualcosa di più complesso la prima cosa da fare è trasformare il ValueTask in un Task
            var task = CalcualteNumberAsync(value).AsTask();
            //... altro lavoro concorrente
            var n1 = await task;
            var n2 = await task;

            //una volta trasformati si possono anche awaitare in modo concorrente
            var task1 = CalcualteNumberAsync(value).AsTask();
            var task2 = CalcualteNumberAsync(value).AsTask();
	        int[] results = await Task.WhenAll(task1, task2);
        }

        //è possibile implementare un metodo che ritorna un ValueTask in modo normale con async await
        public ValueTask<int> CalcualteNumberAsync(int value)
        {
	        //si può costruire un ValueTask passando un Task come parametro nel suo costruttore
            return value > 2 ? new ValueTask<int>(value * 2) : new ValueTask<int>(DoSomethingAsync());
        }

        //N.B. quindi se un metodo ritorna ValueTask la prima cosa che dobbiamo fare è chiamare await
        //oppure trasformarlo subito in un Task
        //attenzione inoltre che ValueTask<T>.Result o ValueTask<T>.GetAwaiter().GetResult() non sono come quelli di Task
        //non devono essere mai chiamati prima di essere certi che il value task sia completo
        //su Task bloccano il thread, con ValueTask non è detto e quindi il comportamento atteso può stupire

        #endregion 

        #region TaskCompletionSource

        [TestMethod]
        public async Task TestTaskCompletionSource()
        {
            var service = new MyAsyncHttpService();
            var result = await service.DownloadStringAsync(new Uri(Google));
            result.Output();
        }

        public interface IMyAsyncHttpService
        {
	        void DownloadString(Uri address, Action<string, Exception> callback);
        }

        public class MyAsyncHttpService : IMyAsyncHttpService
        {
            public void DownloadString(Uri address, Action<string, Exception> callback)
            {
	            using var webClient = new WebClient();
	            try
	            {
		            var result = webClient.DownloadString(address);
		            callback(result, null);
	            }
	            catch (Exception e)
	            {
		            callback(null, e);
	            }
            }
        }

        #endregion
    }

    internal static class MyAsyncHttpServiceExtensions
    {
	    public static Task<string> DownloadStringAsync(this AsyncAwait.IMyAsyncHttpService httpService, Uri address)
	    {
		    var tcs = new TaskCompletionSource<string>();

		    httpService.DownloadString(address, (result, exception) =>
		    {
			    if (exception != null)
				    tcs.TrySetException(exception);
			    else
				    tcs.TrySetResult(result);
		    });

		    return tcs.Task;
	    }
    }
}
