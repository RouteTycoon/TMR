using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TMR
{
	public class Client
	{
		public event MessageEventHandler SendMessage;
		public event MessageEventHandler ReceiveMessage;
		public event UserEventHandler Joined;
		public event UserEventHandler Left;
		public event KickEventHandler Kicked;
		public event ClientErrorEventHandler ClientError;

		private class AsyncObject
		{
			public byte[] Buffer;
			public Socket WorkingSocket;

			public AsyncObject(int bufferSize)
			{
				Buffer = new byte[bufferSize];
			}
		}

		public ServerData Server
		{
			get;
			private set;
		} = new ServerData();

		public string Guid
		{
			get;
			set;
		}

		private bool _Connected;
		private Socket _ClientSocket = null;
		private AsyncCallback _fnReceiveHandler;
		private AsyncCallback _fnSendHandler;

		public Client()
		{
			_fnReceiveHandler = new AsyncCallback(receive);
			_fnSendHandler = new AsyncCallback(send);

			ReceiveMessage += Client_ReceiveMessage;
		}

		public bool isConnected
		{
			get
			{
				return _Connected;
			}
		}

		public void Start(string host_address, ushort host_port)
		{
			_ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

			bool join = false;
			try
			{
				_ClientSocket.Connect(host_address, host_port);

				join = true;
			}
			catch (Exception ex)
			{
				if (ClientError != null)
					ClientError(new ClientErrorEventArgs(ex, DateTime.Now, this));

				join = false;
			}

			_Connected = join;

			if (join)
			{
				AsyncObject ao = new AsyncObject(4096);
				ao.WorkingSocket = _ClientSocket;
				_ClientSocket.BeginReceive(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, _fnReceiveHandler, ao);
			}
		}

		public void Stop()
		{
			_Connected = false;

			SendToServer(new Message() { Type = MessageType.Left, Text = Guid });

			if (Left != null)
				Left(new UserEventArgs(Guid, ((IPEndPoint)_ClientSocket.RemoteEndPoint).Address.ToString(), ((IPEndPoint)_ClientSocket.RemoteEndPoint).Port));

			_ClientSocket.Close();
		}

		public void SendToServer(Message message)
		{
			AsyncObject ao = new AsyncObject(1);
			ao.Buffer = message;
			ao.WorkingSocket = _ClientSocket;

			try
			{
				_ClientSocket.BeginSend(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, _fnSendHandler, ao);
			}
			catch (Exception ex)
			{
				if (ClientError != null)
					ClientError(new ClientErrorEventArgs(ex, DateTime.Now, this));

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
				if (ClientError != null)
					ClientError(new ClientErrorEventArgs(ex, DateTime.Now, this));

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
				if (ClientError != null)
					ClientError(new ClientErrorEventArgs(ex, DateTime.Now, this));

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
				if (ClientError != null)
					ClientError(new ClientErrorEventArgs(ex, DateTime.Now, this));

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

		private void Client_ReceiveMessage(MessageEventArgs e)
		{
			if (Server.ServerName == string.Empty)
			{
				SendToServer(new Message() { Type = MessageType.Info, Text = Guid });
				Server.ServerName = e.Message.Text;
				return;
			}

			if (Server.MaxUserCount == null)
			{
				Server.MaxUserCount = Convert.ToInt32(e.Message.Text);
				return;
			}

			//if (Server.ServerImage == null)
			//{
			//	Server.ServerImage = Utility.ToImage(e.Message.Text);
			//	return;
			//}

			if (e.Message.Type == MessageType.Kick)
			{
				_Connected = false;
				
				if (Kicked != null)
					Kicked(new KickEventArgs(Guid, ((IPEndPoint)_ClientSocket.RemoteEndPoint).Address.ToString(), ((IPEndPoint)_ClientSocket.RemoteEndPoint).Port, e.Message.Text));

				_ClientSocket.Close();
			}
		}
	}
}
