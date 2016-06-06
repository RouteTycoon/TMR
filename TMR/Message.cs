using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TMR
{
	public class Message
	{
		public Message()
		{

		}

		public Message(string text)
		{
			Type = (MessageType)Convert.ToInt32(text.Substring(0, 1));
			Text = text.Substring(1);
		}

		public Message(byte[] bytes)
		{
			string text = Encoding.Unicode.GetString(bytes);

			Type = (MessageType)Convert.ToInt32(text.Substring(0, 1));
			Text = text.Substring(1);
		}

		public MessageType Type
		{
			get;
			set;
		}

		public string Text
		{
			get;
			set;
		}

		public override string ToString()
		{
			return (int)Type + Text;
		}

		public byte[] ToBytes()
		{
			return Encoding.Unicode.GetBytes(ToString());
		}

		public static bool operator ==(Message a, Message b)
		{
			if (a.Text == b.Text && a.Type == b.Type) return true;
			else return false;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is Message)) return false;

			Message a = this;
			Message b = obj as Message;
			if (a.Text == b.Text && a.Type == b.Type) return true;
			else return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static bool operator !=(Message a, Message b)
		{
			if (a.Text == b.Text && a.Type == b.Type) return false;
			else return true;
		}

		public static implicit operator string(Message m)
		{
			return m.ToString();
		}

		public static implicit operator byte[](Message m)
		{
			return m.ToBytes();
		}
	}

	public enum MessageType
	{
		Joined,
		Left,
		Kick,
		Chat,
		Command,
		Info,
		None,
	}
}
