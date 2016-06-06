using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TMR
{
	public class UserEventArgs : EventArgs
	{
		public UserEventArgs(string guid, string ip, int port)
		{
			Guid = guid;
			IP = ip;
			Port = port;
		}

		public string Guid
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
		public KickEventArgs(string guid, string ip, int port, string reason) : base(guid, ip, port)
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

	public class ClientErrorEventArgs : ErrorEventArgs
	{
		public ClientErrorEventArgs(Exception ex, DateTime time, Client client) : base(ex, time)
		{
			Client = client;
		}

		public Client Client
		{
			get;
			internal set;
		}
	}

	public class MessageEventArgs : EventArgs
	{
		public MessageEventArgs(Message msg, Socket sock)
		{
			Message = msg;
			Sender = sock;
		}

		public Message Message
		{
			get;
			internal set;
		}

		public Socket Sender
		{
			get;
			internal set;
		}
	}
}
