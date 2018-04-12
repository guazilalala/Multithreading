using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace 通过注册回调进行侦听
{
	class Example
	{
		static void Main(string[] args)
		{
			CancellationTokenSource cts = new CancellationTokenSource();

			Task.Factory.StartNew(()=> 
			{
				StartWebRequest(cts.Token);

			},cts.Token);

			Thread.Sleep(1000);
			cts.Cancel();

			Console.Read();
		}

		private static void StartWebRequest(CancellationToken token)
		{
			WebClient wc = new WebClient();
			wc.DownloadDataCompleted += (s, e) => Console.WriteLine("Request completed.");

			token.Register(()=> 
			{
				wc.CancelAsync();
				Console.WriteLine("Request cancelled!");
			});

			Console.WriteLine("Starting request.");
			wc.DownloadStringAsync(new Uri("http://www.baidu.com"));
		}
	}
}
