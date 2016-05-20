using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TMR
{
	public class ServerSock
	{
		private const string NEW_LINE = "\n\n";
		private const int BUFFER_SIZE = 1000;

		private TcpClient m_sockClient;
		private byte[] m_Buffer = new byte[BUFFER_SIZE];
		private string m_strName;
		private StreamWriter m_Writer;
		private Encoding m_encKorean;

		public event RecvProcessEventHandler RecvProcess;
		public delegate void RecvProcessEventHandler(ServerSock sender, string strRecv);

		public ServerSock(TcpClient client)
		{
			m_sockClient = client;

			m_encKorean = Encoding.GetEncoding(949);
			m_Writer = new StreamWriter(m_sockClient.GetStream(), m_encKorean);

			m_sockClient.GetStream().BeginRead(m_Buffer, 0, BUFFER_SIZE, RecvData, null);
		}

		public string Name
		{
			get
			{
				return m_strName;
			}
			set
			{
				m_strName = value;
			}
		}

		public void SendData(string data)
		{
			lock (m_sockClient.GetStream())
			{
				m_Writer.Write(data + NEW_LINE);

				m_Writer.Flush();
			}
		}

		private void RecvData(IAsyncResult ar)
		{
			int nRecv = 0;
			string strRecv = null;

			try
			{
				lock (m_sockClient.GetStream())
				{
					nRecv = m_sockClient.GetStream().EndRead(ar);
				}

				strRecv = m_encKorean.GetString(m_Buffer, 0, nRecv - 2);

				if (RecvProcess != null)
				{
					RecvProcess(this, strRecv);
				}

				lock (m_sockClient.GetStream())
				{
					m_sockClient.GetStream().BeginRead(m_Buffer, 0, BUFFER_SIZE, RecvData, null);
				}
			}
			catch (Exception)
			{
			}
		}
	}
}
