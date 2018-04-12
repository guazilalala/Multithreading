using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace 托管线程中的取消
{
	class Example
	{
		static void Main(string[] args)
		{
			var tokenSource = new CancellationTokenSource();
			var token = tokenSource.Token;

			//存储对任务的引用，这样我们就可以等待它们
			//在取消后观察他们的状态。
			Task t;
			var tasks = new ConcurrentBag<Task>();

			Console.WriteLine("Press any key to begin tasks...");
			Console.ReadKey(true);
			Console.WriteLine("To terminate the example, press 'c' to cancel and exit...");
			Console.WriteLine();

			//当令牌源被取消时，请求取消单个任务。
			//将令牌传递给用户委托，以及任务，这样它就可以
			//正确处理异常。
			t = Task.Factory.StartNew(()=> DoSomeWork(1,token),token);
			Console.WriteLine("Task {0} 执行", t.Id);
			tasks.Add(t);

			t = Task.Factory.StartNew(() =>
			{
				Task tc;
				for (int i = 3; i <= 10; i++)
				{
					tc = Task.Factory.StartNew(iteration => DoSomeWork((int)iteration, token), i, token);
					Console.WriteLine("Task {0} 执行", tc.Id);
					tasks.Add(tc);
					// Pass the same token again to do work on the parent task.  
					// All will be signaled by the call to tokenSource.Cancel below.
					DoSomeWork(2, token);
				}
			},token);

			Console.WriteLine("Task {0} 执行", t.Id);
			tasks.Add(t);

			// Request cancellation from the UI thread. 
			char ch = Console.ReadKey().KeyChar;
			if (ch == 'c' || ch == 'C')
			{
				tokenSource.Cancel();
				Console.WriteLine("\nTask 请求取消.");

				// Optional: Observe the change in the Status property on the task. 
				// It is not necessary to wait on tasks that have canceled. However, 
				// if you do wait, you must enclose the call in a try-catch block to 
				// catch the TaskCanceledExceptions that are thrown. If you do  
				// not wait, no exception is thrown if the token that was passed to the  
				// StartNew method is the same token that requested the cancellation. 
			}

			try
			{
				Task.WaitAll(tasks.ToArray());
			}
			catch (AggregateException e)
			{
				Console.WriteLine("\nAggregateException 抛出以下内部异常:");

				foreach (var v in e.InnerExceptions)
				{
					if (v is TaskCanceledException)
						Console.WriteLine("   Task任务取消异常: Task {0}",
										  ((TaskCanceledException)v).Task.Id);
					else
						Console.WriteLine("   异常: {0}", v.GetType().Name);
				}
				Console.WriteLine();
			}
			finally
			{
				tokenSource.Dispose();
			}

			foreach (var task in tasks)
				Console.WriteLine("Task {0} 现在的状态 {1}", task.Id, task.Status);

			Console.Read();
		}

		private static void DoSomeWork(int taskNum, CancellationToken ct)
		{
			// 是否请求取消
			if (ct.IsCancellationRequested == true)
			{
				Console.WriteLine("Task {0} 在开始之前被取消了.",
								  taskNum);
				ct.ThrowIfCancellationRequested();
			}

			int maxIterations = 100;

			for (int i = 0; i <= maxIterations; i++)
			{
				var sw = new SpinWait();
				for (int j = 0; j <= 100; j++)
					sw.SpinOnce();

				if (ct.IsCancellationRequested)
				{
					Console.WriteLine("Task {0} 取消", taskNum);
					ct.ThrowIfCancellationRequested();
				}
			}
		}
	}
}
