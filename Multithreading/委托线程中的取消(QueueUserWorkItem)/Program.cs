using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace 委托线程中的取消_QueueUserWorkItem_
{
	class Example
	{
		static void Main(string[] args)
		{
			//创建一个token source.
			var cts = new CancellationTokenSource();

			//将token传递到可取消的操作
			ThreadPool.QueueUserWorkItem(new WaitCallback(DoSomeWork), cts.Token);
			Thread.Sleep(2500);

			// 请求取消
			//cts.Cancel();
			Console.WriteLine("Cancellation set in token source...");
			Thread.Sleep(2500);

			cts.Dispose();

			Console.Read();

		}

		private static void DoSomeWork(object obj)
		{
			CancellationToken token = (CancellationToken)obj;

			for (int i = 0; i < 10000000; i++)
			{
				if (token.IsCancellationRequested)
				{
					Console.WriteLine("In iteration {0}, cancellation has been requested...",
										 i + 1);

					break;
				}
				Console.WriteLine(i);
				Thread.Sleep(1);
				//Thread.SpinWait(500)
				SpinWait.SpinUntil(() => false, 1);
			}
		}
	}
}
