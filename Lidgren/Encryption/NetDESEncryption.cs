using System;
using System.IO;
using System.Security.Cryptography;

namespace Lidgren.Network
{
	public class NetDESEncryption : NetCryptoProviderBase
	{
		public NetDESEncryption(NetPeer peer)
			: base(peer, DES.Create())
		{
		}

		public NetDESEncryption(NetPeer peer, string key)
			: base(peer, DES.Create())
		{
			SetKey(key);
		}

		public NetDESEncryption(NetPeer peer, byte[] data, int offset, int count)
			: base(peer, DES.Create())
		{
			SetKey(data, offset, count);
		}
	}
}
