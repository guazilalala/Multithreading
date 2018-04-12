using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace 通过轮询侦听取消请求
{
	public struct Rectangle
	{
		public int columns;
		public int rows;
	}
	class CancelByPolling
	{
		static void Main(string[] args)
		{
			var tokenSource = new CancellationTokenSource();
			Rectangle rect = new Rectangle() { columns = 1000, rows = 500 };

			Task.Run(()=> NestedLoops(rect,tokenSource.Token),tokenSource.Token);

			Console.WriteLine("Press 'c' to cancel");
			if (Console.ReadKey(true).KeyChar == 'c')
			{
				tokenSource.Cancel();
				Console.WriteLine("Press any key to exit.");
			}

			Console.ReadKey();
			tokenSource.Dispose();
		}

		private static void NestedLoops(Rectangle rect, CancellationToken token)
		{
			for (int x = 0; x < rect.columns && !token.IsCancellationRequested; x++)
			{
				for (int y = 0; y < rect.rows; y++)
				{
					Thread.SpinWait(5000);
					Console.WriteLine("{0},{1}",x,y);
				}

				if (token.IsCancellationRequested)
				{
					Console.WriteLine("\r\nCancelling after row {0}.", x);
					Console.WriteLine("Press any key to exit.");

					break;
					// ...or, if using Task:
					// token.ThrowIfCancellationRequested();
				}
			}
		}
	}
}
