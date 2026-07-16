using System;
using System.IO;
using System.Security.Cryptography;

namespace Lidgren.Network
{
	public class NetAESEncryption : NetCryptoProviderBase
	{
		public NetAESEncryption(NetPeer peer)
#if UNITY
			: base(peer, new RijndaelManaged())
#else
			: base(peer, Aes.Create())
#endif
		{
		}

		public NetAESEncryption(NetPeer peer, string key)
#if UNITY
			: base(peer, new RijndaelManaged())
#else
			: base(peer, Aes.Create())
#endif
		{
			SetKey(key);
		}

		public NetAESEncryption(NetPeer peer, byte[] data, int offset, int count)
#if UNITY
			: base(peer, new RijndaelManaged())
#else
			: base(peer, Aes.Create())
#endif
		{
			SetKey(data, offset, count);
		}
	}
}
