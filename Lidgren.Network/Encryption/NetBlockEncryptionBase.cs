using System;
using System.Collections.Generic;

namespace Lidgren.Network
{
	/// <summary>
	/// Base for a non-threadsafe encryption class
	/// </summary>
	public abstract class NetBlockEncryptionBase : NetEncryption
	{
		/// <summary>
		/// Block size in bytes for this cipher
		/// </summary>
		public abstract int BlockSize { get; }

		/// <summary>
		/// NetBlockEncryptionBase constructor
		/// </summary>
		public NetBlockEncryptionBase(NetPeer peer)
			: base(peer)
		{
		}

		/// <summary>
		/// Encrypt am outgoing message with this algorithm; no writing can be done to the message after encryption, or message will be corrupted
		/// </summary>
		public override bool Encrypt(NetOutgoingMessage msg)
		{
			int payloadBitLength = msg.LengthBits;
			int numBytes = msg.LengthBytes;
			int blockSize = BlockSize;
			int numBlocks = (int)Math.Ceiling((double)numBytes / (double)blockSize);
			int dstSize = numBlocks * blockSize;

			msg.EnsureBufferSize(dstSize * 8 + (4 * 8)); // add 4 bytes for payload length at end
			msg.LengthBits = dstSize * 8; // length will automatically adjust +4 bytes when payload length is written

			Span<byte> tmp = stackalloc byte[BlockSize];

			for(int i=0;i<numBlocks;i++)
			{
				var subSpan = msg.m_data.AsSpan(i * blockSize);
				EncryptBlock(subSpan, tmp);
				tmp.CopyTo(subSpan);
			}

			// add true payload length last
			msg.Write((UInt32)payloadBitLength);

			return true;
		}

		/// <summary>
		/// Decrypt an incoming message encrypted with corresponding Encrypt
		/// </summary>
		/// <param name="msg">message to decrypt</param>
		/// <returns>true if successful; false if failed</returns>
		public override bool Decrypt(NetIncomingMessage msg)
		{
			int numEncryptedBytes = msg.LengthBytes - 4; // last 4 bytes is true bit length
			int blockSize = BlockSize;
			int numBlocks = numEncryptedBytes / blockSize;
			if (numBlocks * blockSize != numEncryptedBytes)
				return false;

			Span<byte> tmp = stackalloc byte[BlockSize];

			for (int i = 0; i < numBlocks; i++)
			{
				var subSpan = msg.m_data.AsSpan(i * blockSize);
				DecryptBlock(subSpan, tmp);
				tmp.CopyTo(subSpan);
			}

			// read 32 bits of true payload length
			uint realSize = NetBitWriter.ReadUInt32(msg.m_data, 32, (numEncryptedBytes * 8));
			msg.m_bitLength = (int)realSize;
			return true;
		}

		/// <summary>
		/// Encrypt a block of bytes
		/// </summary>
		protected abstract void EncryptBlock(ReadOnlySpan<byte> source, Span<byte> destination);

		/// <summary>
		/// Decrypt a block of bytes
		/// </summary>
		protected abstract void DecryptBlock(ReadOnlySpan<byte> source, Span<byte> destination);
	}
}
