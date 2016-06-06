using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMR
{
	public delegate void MessageEventHandler(MessageEventArgs e);
	public delegate void UserEventHandler(UserEventArgs e);
	public delegate void KickEventHandler(KickEventArgs e);
	public delegate void ServerErrorEventHandler(ServerErrorEventArgs e);
	public delegate void ClientErrorEventHandler(ClientErrorEventArgs e);
}
