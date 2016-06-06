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
		public event UserEventHandler JoinedUser;
		public event UserEventHandler LeftUser;
		public event KickEventHandler KickedUser;
		public event ServerErrorEventHandler ServerError;
		public event MessageEventHandler SendMessage;
		public event MessageEventHandler ReceiveMessage;

		private class AsyncObject
		{
			public byte[] Buffer;
			public Socket WorkingSocket;

			public AsyncObject(int bufferSize)
			{
				Buffer = new byte[bufferSize];
			}
		}

		public List<User> Users
		{
			get;
			private set;
		} = new List<User>();

		public ServerData Data
		{
			get;
			set;
		} = new ServerData();

		private Socket _ServerSocket = null;
		private AsyncCallback _fnReceiveHandler;
		private AsyncCallback _fnSendHandler;
		private AsyncCallback _fnAcceptHandler;

		public Server()
		{
			_fnReceiveHandler = new AsyncCallback(receive);
			_fnSendHandler = new AsyncCallback(send);
			_fnAcceptHandler = new AsyncCallback(connect);

			ReceiveMessage += Server_ReceiveMessage;
		}

		public void Start(ushort port)
		{
			_ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
			_ServerSocket.Bind(new IPEndPoint(IPAddress.Any, port));
			_ServerSocket.Listen(5);
			_ServerSocket.BeginAccept(_fnAcceptHandler, null);
		}

		public void Stop()
		{
			_ServerSocket.Close();
		}

		public User GetUserByGuid(string guid)
		{
			return Users.Find((e) => { return e.Guid == guid; });
		}

		public User GetUserByIP(string ip)
		{
			return Users.Find((e) => { return e.IP == ip; });
		}

		public User GetUserBySocket(Socket sock)
		{
			return Users.Find((e) => { return e.Socket == sock; });
		}

		public bool Kick(User user, string Reason)
		{
			try
			{
				SendToUser(user, new Message() { Type = MessageType.Kick, Text = Reason });
				Users.Remove(user);

				if (KickedUser != null)
					KickedUser(new KickEventArgs(user.Guid, user.IP, user.Port, Reason));

				return true;
			}
			catch (Exception ex)
			{
				if (ServerError != null)
					ServerError(new ServerErrorEventArgs(ex, DateTime.Now, this));

				return false;
			}
		}

		public bool SendToAll(Message message)
		{
			foreach (var it in Users)
			{
				AsyncObject ao = new AsyncObject(1);
				ao.Buffer = message;
				ao.WorkingSocket = it.Socket;

				try
				{
					it.Socket.BeginSend(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, _fnSendHandler, ao);
				}
				catch (Exception ex)
				{
					if (ServerError != null)
						ServerError(new ServerErrorEventArgs(ex, DateTime.Now, this));

					return false;
				}
			}

			return true;
		}

		public bool SendToUser(User user, Message message)
		{
			AsyncObject ao = new AsyncObject(1);
			ao.Buffer = message;
			ao.WorkingSocket = user.Socket;
			try
			{
				user.Socket.BeginSend(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, _fnSendHandler, ao);
			}
			catch (Exception ex)
			{
				if (ServerError != null)
					ServerError(new ServerErrorEventArgs(ex, DateTime.Now, this));

				return false;
			}

			return true;
		}

		public bool SendToOther(User other, Message message)
		{
			foreach (var it in Users)
			{
				if (it == other) continue;

				AsyncObject ao = new AsyncObject(1);
				ao.Buffer = message;
				ao.WorkingSocket = it.Socket;
				try
				{
					it.Socket.BeginSend(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, _fnSendHandler, ao);
				}
				catch (Exception ex)
				{
					if (ServerError != null)
						ServerError(new ServerErrorEventArgs(ex, DateTime.Now, this));

					return false;
				}
			}

			return true;
		}

		private void connect(IAsyncResult ar)
		{
			Socket client;
			try
			{
				client = _ServerSocket.EndAccept(ar);
			}
			catch (Exception ex)
			{
				if (ServerError != null)
					ServerError(new ServerErrorEventArgs(ex, DateTime.Now, this));

				return;
			}

			AsyncObject ao = new AsyncObject(4096);
			ao.WorkingSocket = client;

			User user = new User();
			user.Socket = client;
			user.IP = ((IPEndPoint)client.RemoteEndPoint).Address.ToString();
			user.Port = ((IPEndPoint)client.RemoteEndPoint).Port;
			user.Server = this;
			user.Guid = null;
			Users.Add(user);

			Message info = new Message() { Type = MessageType.Info, Text = Data.ServerName };
			SendToUser(user, info);

			info = new Message() { Type = MessageType.Info, Text = Data.MaxUserCount.GetValueOrDefault().ToString() };
			SendToUser(user, info);

			//info = new Message() { Type = MessageType.Info, Text = Utility.ToBase64(Data.ServerImage, ImageFormat.Png) };
			//SendToUser(user, info);

			try
			{
				client.BeginReceive(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, _fnReceiveHandler, ao);
			}
			catch (Exception ex)
			{
				if (ServerError != null)
					ServerError(new ServerErrorEventArgs(ex, DateTime.Now, this));

				return;
			}
		}

		private void receive(IAsyncResult ar)
		{
			AsyncObject ao = (AsyncObject)ar.AsyncState;

			int recvBytes;

			try
			{
				recvBytes = ao.WorkingSocket.EndReceive(ar);
			}
			catch (Exception ex)
			{
				if (ServerError != null)
					ServerError(new ServerErrorEventArgs(ex, DateTime.Now, this));

				return;
			}

			if (recvBytes > 0)
			{
				byte[] msgByte = new byte[recvBytes];
				Array.Copy(ao.Buffer, msgByte, recvBytes);
				Message msg = new Message(msgByte);

				if (ReceiveMessage != null)
					ReceiveMessage(new MessageEventArgs(msg, ao.WorkingSocket));
			}

			try
			{
				ao.WorkingSocket.BeginReceive(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, _fnReceiveHandler, ao);
			}
			catch (Exception ex)
			{
				if (ServerError != null)
					ServerError(new ServerErrorEventArgs(ex, DateTime.Now, this));

				return;
			}
		}

		private void send(IAsyncResult ar)
		{
			AsyncObject ao = (AsyncObject)ar.AsyncState;

			int sendBytes;

			try
			{
				sendBytes = ao.WorkingSocket.EndSend(ar);
			}
			catch (Exception ex)
			{
				if (ServerError != null)
					ServerError(new ServerErrorEventArgs(ex, DateTime.Now, this));

				return;
			}

			if (sendBytes > 0)
			{
				byte[] msgByte = new byte[sendBytes];
				Array.Copy(ao.Buffer, msgByte, sendBytes);

				Message msg = new Message(msgByte);

				if (SendMessage != null)
					SendMessage(new MessageEventArgs(msg, ao.WorkingSocket));
			}
		}

		private void Server_ReceiveMessage(MessageEventArgs e)
		{
			User sender = GetUserBySocket(e.Sender);
			if (sender != null)
			{
				if (sender.Socket == null)
				{
					sender.Guid = e.Message.Text;

					if (JoinedUser != null)
						JoinedUser(new UserEventArgs(sender.Guid, sender.IP, sender.Port));

					return;
				}
			}

			if (e.Message.Type == MessageType.Left)
			{
				User u = Users.Find((ev) => { return ev.Guid == e.Message.Text; });
				Users.Remove(u);
				if (LeftUser != null)
					LeftUser(new UserEventArgs(u.Guid, u.IP, u.Port));
				return;
			}
		}
	}
}
