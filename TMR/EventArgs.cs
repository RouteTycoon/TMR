using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMR
{
	public class UserEventArgs : EventArgs
	{
		public UserEventArgs(string guid, string ip, int port)
		{
			GUID = guid;
			IP = ip;
			Port = port;
		}

		public string GUID
		{
			get;
			internal set;
		}

		public string IP
		{
			get;
			internal set;
		}

		public int Port
		{
			get;
			internal set;
		}
	}

	public class KickEventArgs : UserEventArgs
	{
		public KickEventArgs(string nickname, string ip, int port, string reason) : base(nickname, ip, port)
		{
			Reason = reason;
		}

		public string Reason
		{
			get;
			internal set;
		}
	}

	public class ErrorEventArgs : EventArgs
	{
		public ErrorEventArgs(Exception ex, DateTime time)
		{
			Exception = ex;
			Time = time;
		}

		public Exception Exception
		{
			get;
			internal set;
		}

		public DateTime Time
		{
			get;
			internal set;
		}
	}

	public class ServerErrorEventArgs : ErrorEventArgs
	{
		public ServerErrorEventArgs(Exception ex, DateTime time, Server server) : base(ex, time)
		{
			Server = server;
		}

		public Server Server
		{
			get;
			internal set;
		}
	}

	public class MessageEventArgs : EventArgs
	{
		public MessageEventArgs(Message msg)
		{
			Message = msg;
		}

		public Message Message
		{
			get;
			internal set;
		}
	}
}
