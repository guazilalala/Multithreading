using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace 操作取消与对象取消_CancellationToken.Register_
{
	class Example
	{
		static void Main(string[] args)
		{
			var cts = new CancellationTokenSource();
			var token = cts.Token;

			var obj1 = new CancelableObject("1");
			var obj2 = new CancelableObject("2");
			var obj3 = new CancelableObject("3");

			token.Register(() => { obj1.Cancel(); });
			token.Register(() => { obj2.Cancel(); });
			token.Register(() => { obj3.Cancel(); });

			// Request cancellation on the token.
			cts.Cancel();
			// Call Dispose when we're done with the CancellationTokenSource.
			cts.Dispose();
			Console.Read();
		}
	}

	class CancelableObject
	{
		public string _id;

		public CancelableObject(string id)
		{
			_id = id;
		}

		public void Cancel()
		{
			Console.WriteLine("Object {0} Cancel callback", _id);
		}
	}
}
