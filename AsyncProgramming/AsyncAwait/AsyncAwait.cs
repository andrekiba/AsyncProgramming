using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.AsyncEx;

namespace AsyncAwait
{
    [TestClass]
    public class AsyncAwait
    {
        #region Synchronization Context

        [TestMethod]
        public void WithSyncContext()
        {
	        AsyncContext.Run(Run);
        }

        [TestMethod]
        public async Task WithoutSyncContext()
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

        #region CPU-bound

        [TestMethod]
        public async Task TestCpuBound()
        {
            //esegue il metodo in parallelo su più thread
            Parallel.ForEach(Enumerable.Range(1, 100), CpuBoundMethod);

            //è corretto utilizzare Task.Run solo per operazioni CPU-bound poichè utilizza un thread
            await Task.Run(() => CpuBoundMethod(100));
            await Task.Factory.StartNew(() => CpuBoundMethod(101));
        }

        static void CpuBoundMethod(int n)
        {
            Debug.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} scrivo {n}");
        }

        #endregion

        #region Sequential - Concurrent

        [TestMethod]
        public async Task Sequential()
        {
            var sequential = Enumerable.Range(1, 4).Select(t => Task.Delay(TimeSpan.FromSeconds(1)));

            foreach (var task in sequential)
            {
                await task;
            }
        }

        [TestMethod]
        public async Task Concurrent()
        {
            var concurrent = Enumerable.Range(1, 4).Select(t => Task.Delay(TimeSpan.FromSeconds(1)));
            await Task.WhenAll(concurrent);
            //await Task.WhenAny(concurrent);
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

        #region Async Exception

        [TestMethod]
        public async Task TestTrySomethingAsync()
        {
            await TrySomethingAsync();
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

        [TestMethod]
        public async Task TestAsyncVoid()
        {
	        try
	        {
		        AvoidAsyncVoid();
		        //await UseAsyncTask();
	        }
	        catch (InvalidOperationException e)
	        {
		        Debug.WriteLine(e.Message);
	        }

	        Debug.WriteLine("dove è finita l'eccezione?");
        }

        static async void AvoidAsyncVoid()
        {
            Debug.WriteLine("Sono dentro AvoidAsyncVoid");
            await Task.Delay(TimeSpan.FromSeconds(1));

            Debug.WriteLine("Sto per sollevare eccezione");
            throw new InvalidOperationException("Eccezione non catturata!");
        }

        static async Task UseAsyncTask()
        {
	        Debug.WriteLine("Sono dentro UseAsyncTask");
	        await Task.Delay(TimeSpan.FromSeconds(1));

	        Debug.WriteLine("Sto per sollevare eccezione");
	        throw new InvalidOperationException("Eccezione catturata!");
        }

        #endregion

        #region Deadlock

        [TestMethod]
        [Ignore]
        public void TestDeadlock()
        {
            AsyncContext.Run(async () =>
            {
                await Deadlock();
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

        #endregion

        #region ConfigureAwait

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

        #region Composition

        public async Task DoOperationsConcurrentlyAsync()
        {
            var tasks = new Task[3];
            tasks[0] = DoSomethingAsync();
            tasks[1] = DoSomethingAsync();
            tasks[2] = DoSomethingAsync();

            // a questo punto tutti e 3 i task sono in running

            // WhenAll reswtituisce un task che diventa completo quando tutti i task sottesi sono completi
            await Task.WhenAll(tasks);
        }

        public async Task<int> GetFirstToRespondAsync()
        {
            // chiama due web service e vede chi risponde prima
            Task<int>[] tasks = { DoSomethingAsync(), DoSomethingAsync() };

            // attende il primo che risponde
            var firstTask = await Task.WhenAny(tasks);

            // Return the result.
            return await firstTask;
        }

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
            //attenzione che il report può avvenire in asincrono quindi è meglio utilizzare un value type o un tipo immutabile
            //come parametro T per evitare che il valore venga modificato dalla continuazione del metodo in asincrono  
            var progress = new Progress<int>();
            progress.ProgressChanged += (sender, p) =>
            {
                //N.B.: la callback cattura il contesto, sappiamo che quando viene costrutita in questo caso il contesto è quello
                //del Main thread quindi è possibile aggiornare l'interfaccia senza incorrere in problemi
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

        #region DelayImplementation

        static Task Delay(int milliseconds)
        {
            var tcs = new TaskCompletionSource<object>();

            var timer = new Timer(x => tcs.SetResult(null), null, milliseconds, Timeout.Infinite);

            tcs.Task.ContinueWith(x => timer.Dispose());

            return tcs.Task;
        }

        #endregion
    }
}
