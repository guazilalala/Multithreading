using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace 善用SpinWait处理线程空转以利提升效能
{
	class Program
	{
		public static bool IsRunning { get; set; }
		public static int Interval { get; set; } = 1000000000;
		static void Main(string[] args)
		{
			Start(() =>
			{
				Console.WriteLine(1);
			});

			Console.Read();
		}

		public static void Start(Action action)
		{
			IsRunning = true;
			Task.Factory.StartNew(()=> 
			{
				Stopwatch watch = new Stopwatch();
				while (IsRunning)
				{
					watch.Restart();
					action.Invoke();

					SpinWait.SpinUntil(() => IsRunning, 1);
					Thread.Sleep(1);				
				}
			});
		}

	
	}
}
