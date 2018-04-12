using System;
using System.Threading;
using System.Threading.Tasks;

namespace 侦听多个取消请求
{
 	class LinkedTokenSourceDemo
	{
		static void Main(string[] args)
		{
			WorkerWithTimer worker = new WorkerWithTimer();
			CancellationTokenSource cts = new CancellationTokenSource();

			Task.Run(() =>
			{
				Console.WriteLine("Press 'c' to cancel within 3 seconds after work begins.");
				Console.WriteLine("Or let the task time out by doing nothing.");
				if (Console.ReadKey(true).KeyChar == 'c')
					cts.Cancel();
			});

			Thread.Sleep(1000);

			Task task = Task.Run(() => worker.DoWork(cts.Token),cts.Token);

			try
			{
				task.Wait(cts.Token);
			}
			catch (OperationCanceledException e)
			{
				if (e.CancellationToken == cts.Token)
					Console.WriteLine("Canceled from UI thread throwing OCE.");
			}
			catch(AggregateException ae)
			{
				Console.WriteLine("AggregateException caught: " + ae.InnerException);
				foreach (var inner in ae.InnerExceptions)
				{
					Console.WriteLine(inner.Message + inner.Source);
				}
			}

			Console.WriteLine("Press any key to exit.");
			Console.ReadKey();
			cts.Dispose();
		}
	}

	class WorkerWithTimer
	{
		CancellationTokenSource _internalTokenSource = new CancellationTokenSource();
		CancellationToken _internalToken;
		CancellationToken _externalToken;
		Timer _timer;

		public WorkerWithTimer()
		{
			_internalTokenSource = new CancellationTokenSource();
			_internalToken = _internalTokenSource.Token;

			_timer = new Timer(new TimerCallback(CancelAfterTimeout), null, 30000, 30000);
		}

		public void DoWork(CancellationToken externalToken)
		{
			_internalToken = _internalTokenSource.Token;
			_externalToken = externalToken;

			using (CancellationTokenSource linkedCts =
				CancellationTokenSource.CreateLinkedTokenSource(_internalToken, externalToken))
			{
				try
				{
					DoWrokinternal(linkedCts.Token);
				}
				catch (OperationCanceledException)
				{
					if (_internalToken.IsCancellationRequested)
					{
						Console.WriteLine("操作超时。");
					}
					else if (_externalToken.IsCancellationRequested)
					{
						Console.WriteLine("取消每个用户的请求。");
						_externalToken.ThrowIfCancellationRequested();
					}
				}
			}
		}

		private void DoWrokinternal(CancellationToken token)
		{
			for (int i = 0; i < 1000; i++)
			{
				if (token.IsCancellationRequested)
				{
					_timer.Dispose();

					token.ThrowIfCancellationRequested();
				}

				Thread.SpinWait(7500000);
				Console.Write("working... ");
			}
		}

		private void CancelAfterTimeout(object state)
		{
			Console.WriteLine("\r\n定时器启动。");
			_internalTokenSource.Cancel();
			_timer.Dispose();
		}
	}
}
