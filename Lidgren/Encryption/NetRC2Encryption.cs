using System;
using System.IO;
using System.Security.Cryptography;

namespace Lidgren.Network
{
	public class NetRC2Encryption : NetCryptoProviderBase
	{
		public NetRC2Encryption(NetPeer peer)
			: base(peer, RC2.Create())
		{
		}

		public NetRC2Encryption(NetPeer peer, string key)
			: base(peer, RC2.Create())
		{
			SetKey(key);
		}

		public NetRC2Encryption(NetPeer peer, byte[] data, int offset, int count)
			: base(peer, RC2.Create())
		{
			SetKey(data, offset, count);
		}
	}
}
