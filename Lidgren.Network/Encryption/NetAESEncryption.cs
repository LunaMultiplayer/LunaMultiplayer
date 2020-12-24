using System;
using System.Security.Cryptography;

namespace Lidgren.Network
{
	public class NetAESEncryption : NetCryptoProviderBase
	{
		public NetAESEncryption(NetPeer peer)
			: base(peer, Aes.Create())
		{
		}

		public NetAESEncryption(NetPeer peer, string key)
			: this(peer)
		{
			SetKey(key);
		}

		public NetAESEncryption(NetPeer peer, byte[] data, int offset, int count)
			: this(peer, data.AsSpan(offset, count))
		{
		}

		public NetAESEncryption(NetPeer peer, ReadOnlySpan<byte> data)
			: this(peer)
		{
			SetKey(data);
		}
	}
}
