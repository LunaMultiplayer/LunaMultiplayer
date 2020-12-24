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
			: this(peer)
		{
			SetKey(key);
		}

		public NetDESEncryption(NetPeer peer, byte[] data, int offset, int count)
			: this(peer, data.AsSpan(offset, count))
		{
		}

		public NetDESEncryption(NetPeer peer, ReadOnlySpan<byte> data)
			: this(peer)
		{
			SetKey(data);
		}
	}
}
