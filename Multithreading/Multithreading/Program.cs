using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Multithreading
{
	class Program
	{
		static void Main(string[] args)
		{
			var tokenSource = new CancellationTokenSource();
			var token = tokenSource.Token;
			token.Register(() => { Console.WriteLine("这是一个Callback"); });


			tokenSource.Cancel();

			var task = Task.Factory.StartNew(()=> 
			{
				for (int i = 0; i < 1000000; i++)
				{
					if (token.IsCancellationRequested)
					{
						Console.WriteLine("任务被取消了。");

						Task.Factory.StartNew(()=> 
						{
							if (token.IsCancellationRequested)
							{
								Console.WriteLine("AAAA");
							}
							else
							{
								Console.WriteLine("BBBBB");
							}
						},token);
						break;
					}
					Console.WriteLine(i);
					Thread.Sleep(10000);
				}

			},token);



			char ch = Console.ReadKey().KeyChar;
			if (ch == 'c' || ch == 'C')
			{
				tokenSource.Cancel();
				Console.WriteLine("\nTask 请求取消.");
			}

			tokenSource.Dispose();

			tokenSource = new CancellationTokenSource();
			Console.WriteLine(tokenSource.Token.IsCancellationRequested);
			Console.WriteLine(token.IsCancellationRequested);
			Console.Read();
		}
	}
}
