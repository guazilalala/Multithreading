using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace 侦听具有等待句柄的取消请求_ManualResetEvent_
{
	class CancelOldStyleEvents
	{
		static ManualResetEvent mre = new ManualResetEvent(false);

		static void Main(string[] args)
		{
			var cts = new CancellationTokenSource();
			Task.Run(()=> DoWork(cts.Token),cts.Token);
			Console.WriteLine("Press s to start/restart, p to pause, or c to cancel.");
			Console.WriteLine("Or any other key to exit.");

			bool goAgain = true;
			while (goAgain)
			{
				char ch = Console.ReadKey(true).KeyChar;
				switch (ch)
				{
					case 'c':
						cts.Cancel();
						break;
					case 'p':
						mre.Reset();
						break;
					case 's':
						mre.Set();
						break;
					default:
						goAgain = false;
						break;
				}

				Thread.Sleep(100);
			}
			cts.Dispose();
		}

		private static void DoWork(CancellationToken token)
		{
			while (true)
			{
				//如果事件没有发出信号，请等待事件。
				int eventThatSignaledIndex =
					WaitHandle.WaitAny(new WaitHandle[] { mre, token.WaitHandle }, new TimeSpan(0, 0, 20));

				//我们在等待时取消了吗？
				if (eventThatSignaledIndex == 1)
				{
					Console.WriteLine("The wait operation was canceled.");
					throw new OperationCanceledException(token);
				}
				//我们在运行时取消了吗？
				else if (token.IsCancellationRequested)
				{
					Console.WriteLine("I was canceled while running.");
					token.ThrowIfCancellationRequested();
				}
				//我们超时了吗？
				else if (eventThatSignaledIndex == WaitHandle.WaitTimeout)
				{
					Console.WriteLine("I timed out.");
					break;
				}
				else
				{
					Console.WriteLine(eventThatSignaledIndex);
					//Console.Write("Working... ");
					// Simulating work.
					//Thread.SpinWait(100000);
					//Thread.Sleep(1);
					SpinWait.SpinUntil(() => false, 1);
				}
			}
		}
	}
}
