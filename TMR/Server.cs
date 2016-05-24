using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TMR
{
	public class Server
	{
		private TcpListener _listener;
		private bool isRun = false;

		#region events
		public event UserEventHandler JoinedUser;
		public event UserEventHandler LeftUser;
		public event KickEventHandler KickedUser;
		public event ServerErrorEventHandler ServerError;
		public event MessageEventHandler SendMessage;
		public event MessageEventHandler ReceiveMessage;
		#endregion

		public Server(IPAddress ip, int port = 31120)
		{
			_listener = new TcpListener(ip, port);
			_listener.Start();
			ReceiveMessage += Server_ReceiveMessage;
		}

		private void Server_ReceiveMessage(MessageEventArgs e)
		{
			if(e.Message.Type == MessageType.Left)
			{
				User u = Users.Find((ev) => { return ev.Nickname == e.Message.Text; });
                Users.Remove(u);
				if(LeftUser != null)
					LeftUser(new UserEventArgs(u.Nickname, u.IP));
				return;
			}
		}

		public ServerData Data
		{
			get;
			set;
		}

		public List<User> Users
		{
			get;
			set;
		} = new List<User>();

		public void Start()
		{
			try
			{
				isRun = true;
				while (isRun)
				{
					Socket sock = _listener.AcceptSocket();

					if (!isRun)
					{
						sock.Send(new Message() { Type = MessageType.Kick, Text = "Server Closed." });
						return;
					}

					ClientHandler handler = new ClientHandler(sock, ReceiveMessage);

					byte[] info = new Message() { Type = MessageType.Info, Text = Data.ServerName };
					sock.Send(info, info.Length, SocketFlags.None);

					Thread.Sleep(500);

					info = new Message() { Type = MessageType.Info, Text = Data.MaxUserCount.ToString() };
					sock.Send(info, info.Length, SocketFlags.None);

					Thread sockThread = new Thread(new ThreadStart(handler.Start));
					sockThread.Start();

					byte[] buffer = new byte[2048];
					int len = sock.Receive(buffer);

					if (JoinedUser != null)
						JoinedUser(new UserEventArgs(Encoding.UTF8.GetString(buffer, 0, len), ((IPEndPoint)sock.RemoteEndPoint).Address.ToString()));

					Users.Add(new User() { Nickname = Encoding.UTF8.GetString(buffer, 0, len), IP = ((IPEndPoint)sock.RemoteEndPoint).Address.ToString(), Handler = handler, Socket = sock });
				}
			}
			catch (Exception ex)
			{
				if (ServerError != null)
					ServerError(new ServerErrorEventArgs(ex, DateTime.Now, this));
			}
			finally
			{
				Stop();
			}
		}

		public void Stop()
		{
			_listener.Stop();
			foreach (var it in Users)
			{
				it.Socket.Send(new Message() { Text = "Server Closed", Type = MessageType.Kick }.ToBytes());
			}
			Users.Clear();
			isRun = false;
		}

		public void SendToAll(Message message)
		{
			foreach (var it in Users)
			{
				it.Socket.Send(message.ToBytes());
			}

			if (SendMessage != null)
				SendMessage(new MessageEventArgs(message));
		}

		public void SendToUser(User u, Message message)
		{
			u.Socket.Send(message.ToBytes());
			if (SendMessage != null)
				SendMessage(new MessageEventArgs(message));
		}

		public void Kick(User u, string Reason)
		{
			u.Handler.Stop();
			u.Socket.Send(new Message() { Text = Reason, Type = MessageType.Kick }.ToBytes());
			Users.Remove(u);

			if (KickedUser != null)
				KickedUser(new KickEventArgs(u.Nickname, u.IP, Reason));
		}
	}
}
