using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TMR
{
	public class User
	{
		public Socket Socket
		{
			get;
			set;
		}

		public Server Server
		{
			get;
			set;
		}

		public string Guid
		{
			get;
			set;
		}

		public string IP
		{
			get;
			set;
		}

		public int Port
		{
			get;
			set;
		}

		internal ClientHandler Handler
		{
			get;
			set;
		}
	}
}
