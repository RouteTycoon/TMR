using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TMR
{
	public static class Utility
	{
		public static string ToBase64(Image img, ImageFormat format)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				img.Save(ms, format);
				byte[] b = ms.ToArray();

				string str = Convert.ToBase64String(b);
				return str;
			}
		}

		public static Image ToImage(string str)
		{
			byte[] b = Convert.FromBase64String(str);
			MemoryStream ms = new MemoryStream(b, 0, b.Length);

			ms.Write(b, 0, b.Length);
			Image img = Image.FromStream(ms, true);
			return img;
		}

		public static byte[] Receive(Socket sock)
		{
			int size = Receive_int(sock);
			MemoryStream ms = new MemoryStream();

			while (size > 0)
			{
				byte[] buffer;
				if (size < sock.ReceiveBufferSize)
					buffer = new byte[size];
				else
					buffer = new byte[sock.ReceiveBufferSize];

				int rec = sock.Receive(buffer, 0, buffer.Length, 0);
				size -= rec;

				List<byte> b = new List<byte>();

				for (int i = 0; i < rec; i++)
				{
					b.Add(buffer[i]);
				}

				byte[] buf = b.ToArray();

				ms.Write(buf, 0, buf.Length);
			}

			ms.Close();

			byte[] data = ms.ToArray();

			ms.Dispose();

			return data;
		}

		private static int Receive_int(Socket sock)
		{
			MemoryStream ms = new MemoryStream();
			int s = 4;
			while (s > 0)
			{
				byte[] buffer = new byte[s];
				int rec = sock.Receive(buffer, 0, s, 0);
				s -= rec;

				List<byte> b = new List<byte>();
				for (int i = 0; i < rec; i++)
				{
					b.Add(buffer[i]);
				}
				ms.Write(b.ToArray(), 0, b.Count);
			}

			byte[] sizeBuf = ms.ToArray();

			ms.Close();
			ms.Dispose();

			int size = BitConverter.ToInt32(sizeBuf, 0);

			return size;
		}

		public static void Send(Socket sock, byte[] data)
		{
			sock.Send(BitConverter.GetBytes(data.Length), 0, 4, 0);
			sock.Send(data);
		}

		public static byte[] FileToBytes(string path)
		{
			return File.ReadAllBytes(path);
		}

		public static Image TransparentImage
		{
			get
			{
				return Properties.Resources.DefaultImage;
			}
		}
	}
}
