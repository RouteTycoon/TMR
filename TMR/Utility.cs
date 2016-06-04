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
			byte[] size = new byte[4];
			sock.Receive(size, 0, size.Length, 0);

			int byte_size = BitConverter.ToInt32(size, 0);

			byte[] buffer = new byte[byte_size];
			sock.Receive(buffer, 0, buffer.Length, 0);

			return buffer;
		}

		public static void Send(Socket sock, byte[] data)
		{
			sock.Send(BitConverter.GetBytes(data.Length));
			sock.Send(data);
		}
	}
}
