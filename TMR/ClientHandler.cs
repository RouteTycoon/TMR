using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TMR
{
	class ClientHandler : IDisposable
	{
		private Socket _sock;

		private MessageEventHandler _onmsg;

		private bool isRun = false;

		public ClientHandler(Socket sock, MessageEventHandler msgevt)
		{
			_sock = sock;

			_onmsg = msgevt;
		}

		public void Start()
		{
			NetworkStream stream = null;
			StreamReader sr = null;

			stream = new NetworkStream(_sock);
			sr = new StreamReader(stream, Encoding.UTF8);

			byte[] buffer = new byte[2048];

			isRun = true;

			while (isRun)
			{
				try
				{
					int len = _sock.Receive(buffer);

					if (!isRun) return;

					if (_onmsg != null)
						_onmsg(new MessageEventArgs(new Message(Encoding.UTF8.GetString(buffer, 0, len))));
				}
				catch
				{
					break;
				}
			}
		}

		public void Stop()
		{
			isRun = false;
		}

		public void Dispose()
		{
			_sock.Close();
		}
	}
}
