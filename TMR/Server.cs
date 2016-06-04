using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
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
			if (e.Message.Type == MessageType.Left)
			{
				User u = Users.Find((ev) => { return ev.GUID == e.Message.Text; });
				Users.Remove(u);
				if (LeftUser != null)
					LeftUser(new UserEventArgs(u.GUID, u.IP, u.Port));
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
						Utility.Send(sock, new Message() { Type = MessageType.Kick, Text = "Server Closed." });
						return;
					}
					else if (Data.MaxUserCount <= Users.Count)
					{
						Utility.Send(sock, new Message() { Type = MessageType.Kick, Text = "Server Full." });
						return;
					}

					ClientHandler handler = new ClientHandler(sock, ReceiveMessage);

					byte[] info = new Message() { Type = MessageType.Info, Text = Data.ServerName };
					Utility.Send(sock, info);

					info = new Message() { Type = MessageType.Info, Text = Data.MaxUserCount.ToString() };
					Utility.Send(sock, info);

					info = new Message() { Type = MessageType.Info, Text = Utility.ToBase64(Data.ServerImage, ImageFormat.Png) };
					Utility.Send(sock, info);

					Thread sockThread = new Thread(new ThreadStart(handler.Start));
					sockThread.Start();

					byte[] buffer = Utility.Receive(sock);
					int len = buffer.Length;

					if (JoinedUser != null)
						JoinedUser(new UserEventArgs(Encoding.UTF8.GetString(buffer, 0, len), ((IPEndPoint)sock.RemoteEndPoint).Address.ToString(), ((IPEndPoint)sock.RemoteEndPoint).Port));

					Users.Add(new User() { GUID = Encoding.UTF8.GetString(buffer, 0, len), IP = ((IPEndPoint)sock.RemoteEndPoint).Address.ToString(), Port = ((IPEndPoint)sock.RemoteEndPoint).Port, Handler = handler, Socket = sock });
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
				Utility.Send(it.Socket, new Message() { Text = "Server Closed.", Type = MessageType.Kick });
			}
			Users.Clear();
			isRun = false;
		}

		public void SendToAll(Message message)
		{
			foreach (var it in Users)
			{
				Utility.Send(it.Socket, message);
			}

			if (SendMessage != null)
				SendMessage(new MessageEventArgs(message));
		}

		public void SendToUser(User u, Message message)
		{
			Utility.Send(u.Socket, message);
			if (SendMessage != null)
				SendMessage(new MessageEventArgs(message));
		}

		public void Kick(User u, string Reason)
		{
			u.Handler.Stop();
			Utility.Send(u.Socket, new Message() { Text = Reason, Type = MessageType.Kick });
			Users.Remove(u);

			if (KickedUser != null)
				KickedUser(new KickEventArgs(u.GUID, u.IP, u.Port, Reason));
		}
	}
}
