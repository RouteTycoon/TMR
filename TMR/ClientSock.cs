using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace TMR
{
	public class ClientSock : TcpClient
	{
		private const string NEW_LINE = "\n\n";
		private const int BUFFER_SIZE = 1000;

		private byte[] m_Buffer = new byte[BUFFER_SIZE];
		private string m_strName;
		private StreamWriter m_Writer;
		private Encoding m_encKorean;

		public event RecvProcessEventHandler RecvProcess;
		public delegate void RecvProcessEventHandler(ClientSock sender, string strRecv);

		public void Delete()
		{
			if (Active)
				Dispose(true);
		}

		public void Init()
		{
			m_encKorean = Encoding.GetEncoding(949);
			m_Writer = new StreamWriter(GetStream(), m_encKorean);

			GetStream().BeginRead(m_Buffer, 0, BUFFER_SIZE, RecvData, null);
		}

		public bool ConnectTo(string ip, int port)
		{
			try
			{
				Connect(ip, port);
			}
			catch (Exception)
			{
				return false;
			}

			return true;
		}

		public void SendData(string str)
		{
			try
			{
				m_Writer.Write(str + NEW_LINE);
				m_Writer.Flush();
			}catch(Exception)
			{

			}
		}

		private void RecvData(IAsyncResult ar)
		{
			int nRecv = 0;
			string strRecv = "";

			try
			{
				lock (GetStream())
				{
					nRecv = GetStream().EndRead(ar);
				}

				strRecv = m_encKorean.GetString(m_Buffer, 0, nRecv - 2);

				if (RecvProcess != null)
				{
					RecvProcess(this, strRecv);
				}

				lock (GetStream())
				{
					GetStream().BeginRead(m_Buffer, 0, BUFFER_SIZE, RecvData, null);
				}
			}
			catch (Exception)
			{

			}
		}
	}
}
