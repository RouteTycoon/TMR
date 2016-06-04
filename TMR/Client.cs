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
	public class Client : IDisposable
	{
		private TcpClient _client;
		private NetworkStream _stream;
		private StreamReader _sr;

		#region events
		public event MessageEventHandler SendMessage;
		public event MessageEventHandler ReceiveMessage;
		public event UserEventHandler Joined;
		public event UserEventHandler Left;
		public event KickEventHandler Kicked;
		#endregion

		private bool isRun = false;

		public ServerData Server
		{
			get;
			set;
		}

		public string GUID
		{
			get;
			set;
		}

		public Client(IPAddress ip, int port = 31120)
		{
			_client = new TcpClient();
			_client.Connect(ip, port);

			_stream = _client.GetStream();
			_sr = new StreamReader(_stream, Encoding.UTF8);

			Server = new ServerData();

			ReceiveMessage += Client_ReceiveMessage;
		}

		private void Client_ReceiveMessage(MessageEventArgs e)
		{
			if (Server.ServerName == string.Empty)
			{
				Utility.Send(_client.Client, Encoding.UTF8.GetBytes(GUID));
				Server.ServerName = e.Message.Text;
				return;
			}

			if (Server.MaxUserCount == null)
			{
				Server.MaxUserCount = Convert.ToInt32(e.Message.Text);
				return;
			}

			if (Server.ServerImage == null)
			{
				Server.ServerImage = Utility.ToImage(e.Message.Text);
				return;
			}

			if (e.Message.Type == MessageType.Kick)
			{
				isRun = false;

				if (Kicked != null)
					Kicked(new KickEventArgs(GUID, ((IPEndPoint)_client.Client.RemoteEndPoint).Address.ToString(), ((IPEndPoint)_client.Client.RemoteEndPoint).Port, e.Message.Text));

				_client.Close();
			}
		}

		public void Start()
		{
			if (Joined != null)
				Joined(new UserEventArgs(GUID, ((IPEndPoint)_client.Client.RemoteEndPoint).Address.ToString(), ((IPEndPoint)_client.Client.RemoteEndPoint).Port));

			isRun = true;

			while (isRun)
			{
				byte[] buffer = Utility.Receive(_client.Client);
				int len = buffer.Length;

				if (!isRun) return;

				Message message = new Message(Encoding.UTF8.GetString(buffer, 0, len));

				if (ReceiveMessage != null)
					ReceiveMessage(new MessageEventArgs(message));
			}
		}

		public void Stop()
		{
			isRun = false;

			Utility.Send(_client.Client, new Message() { Type = MessageType.Left, Text = GUID });

			if (Left != null)
				Left(new UserEventArgs(GUID, ((IPEndPoint)_client.Client.RemoteEndPoint).Address.ToString(), ((IPEndPoint)_client.Client.RemoteEndPoint).Port));

			_client.Close();
		}

		public void Dispose()
		{
			_client.Close();
		}

		public void SendToServer(Message message)
		{
			Utility.Send(_client.Client, message);
			if (SendMessage != null)
				SendMessage(new MessageEventArgs(message));
		}
	}
}
