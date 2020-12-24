/* Copyright (c) 2010 Michael Lidgren

Permission is hereby granted, free of charge, to any person obtaining a copy of this software
and associated documentation files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom
the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or
substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

#if !__NOIPENDPOINT__
using NetEndPoint = System.Net.IPEndPoint;
using NetAddress = System.Net.IPAddress;
#endif

using System;
using System.Net;

using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Lidgren.Network
{
	/// <summary>
	/// Utility methods
	/// </summary>
	public static partial class NetUtility
	{
		private static readonly bool IsMono = Type.GetType("Mono.Runtime") != null;

		private static IPAddress s_broadcastAddress;
		public static IPAddress GetCachedBroadcastAddress()
		{
			if (s_broadcastAddress == null)
				s_broadcastAddress = GetBroadcastAddress();
			return s_broadcastAddress;
		}

		/// <summary>
		/// Create a hex string from an Int64 value
		/// </summary>
		public static string ToHexString(long data)
		{
			return ToHexString(BitConverter.GetBytes(data));
		}

		/// <summary>
		/// Create a hex string from an array of bytes
		/// </summary>
		public static string ToHexString(byte[] data)
		{
			return ToHexString(data.AsSpan());
		}

		public static string ToHexString(byte[] data, int offset, int length)
		{
			return ToHexString(data.AsSpan(offset, length));
		}
		
		/// <summary>
		/// Create a hex string from an array of bytes
		/// </summary>
		public static string ToHexString(ReadOnlySpan<byte> data)
		{
			char[] c = new char[data.Length * 2];
			byte b;
			for (int i = 0; i < data.Length; ++i)
			{
				b = ((byte)(data[i] >> 4));
				c[i * 2] = (char)(b > 9 ? b + 0x37 : b + 0x30);
				b = ((byte)(data[i] & 0xF));
				c[i * 2 + 1] = (char)(b > 9 ? b + 0x37 : b + 0x30);
			}
			return new string(c);
		}

		/// <summary>
		/// Returns true if the endpoint supplied is on the same subnet as this host
		/// </summary>
		public static bool IsLocal(NetEndPoint endPoint)
		{
			if (endPoint == null)
				return false;
			return IsLocal(endPoint.Address);
		}

		/// <summary>
		/// Returns true if the IPAddress supplied is on the same subnet as this host
		/// </summary>
		public static bool IsLocal(NetAddress remote)
		{
			NetAddress mask;
			var local = GetMyAddress(out mask);

			if (mask == null)
				return false;

			uint maskBits = BitConverter.ToUInt32(mask.GetAddressBytes(), 0);
			uint remoteBits = BitConverter.ToUInt32(remote.GetAddressBytes(), 0);
			uint localBits = BitConverter.ToUInt32(local.GetAddressBytes(), 0);

			// compare network portions
			return ((remoteBits & maskBits) == (localBits & maskBits));
		}

		/// <summary>
		/// Returns how many bits are necessary to hold a certain number
		/// </summary>
		[CLSCompliant(false)]
		public static int BitsToHoldUInt(uint value)
		{
#if NETCOREAPP
			return Math.Max(32 - BitOperations.LeadingZeroCount(value), 1);
#else
			int bits = 1;
			while ((value >>= 1) != 0)
				bits++;
			return bits;
#endif
		}

		/// <summary>
		/// Returns how many bits are necessary to hold a certain number
		/// </summary>
		[CLSCompliant(false)]
		public static int BitsToHoldUInt64(ulong value)
		{
#if NETCOREAPP
			return Math.Max(64 - BitOperations.LeadingZeroCount(value), 1);
#else
			int bits = 1;
			while ((value >>= 1) != 0)
				bits++;
			return bits;
#endif
		}

		/// <summary>
		/// Returns how many bytes are required to hold a certain number of bits
		/// </summary>
		public static int BytesToHoldBits(int numBits)
		{
			return (numBits + 7) / 8;
		}

		internal static bool CompareElements(byte[] one, byte[] two)
		{
			if (one.Length != two.Length)
				return false;
			for (int i = 0; i < one.Length; i++)
				if (one[i] != two[i])
					return false;
			return true;
		}

		/// <summary>
		/// Convert a hexadecimal string to a byte array
		/// </summary>
		public static byte[] ToByteArray(String hexString)
		{
			byte[] retval = new byte[hexString.Length / 2];
			for (int i = 0; i < hexString.Length; i += 2)
				retval[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
			return retval;
		}

		/// <summary>
		/// Converts a number of bytes to a shorter, more readable string representation
		/// </summary>
		public static string ToHumanReadable(long bytes)
		{
			if (bytes < 4000) // 1-4 kb is printed in bytes
				return bytes + " bytes";
			if (bytes < 1000 * 1000) // 4-999 kb is printed in kb
				return Math.Round(((double)bytes / 1000.0), 2) + " kilobytes";
			return Math.Round(((double)bytes / (1000.0 * 1000.0)), 2) + " megabytes"; // else megabytes
		}

		internal static int RelativeSequenceNumber(int nr, int expected)
		{
			return (nr - expected + NetConstants.NumSequenceNumbers + (NetConstants.NumSequenceNumbers / 2)) % NetConstants.NumSequenceNumbers - (NetConstants.NumSequenceNumbers / 2);

			// old impl:
			//int retval = ((nr + NetConstants.NumSequenceNumbers) - expected) % NetConstants.NumSequenceNumbers;
			//if (retval > (NetConstants.NumSequenceNumbers / 2))
			//	retval -= NetConstants.NumSequenceNumbers;
			//return retval;
		}

		/// <summary>
		/// Gets the window size used internally in the library for a certain delivery method
		/// </summary>
		public static int GetWindowSize(NetDeliveryMethod method)
		{
			switch (method)
			{
				case NetDeliveryMethod.Unknown:
					return 0;

				case NetDeliveryMethod.Unreliable:
				case NetDeliveryMethod.UnreliableSequenced:
					return NetConstants.UnreliableWindowSize;

				case NetDeliveryMethod.ReliableOrdered:
					return NetConstants.ReliableOrderedWindowSize;

				case NetDeliveryMethod.ReliableSequenced:
				case NetDeliveryMethod.ReliableUnordered:
				default:
					return NetConstants.DefaultWindowSize;
			}
		}

		// shell sort
		internal static void SortMembersList(System.Reflection.MemberInfo[] list)
		{
			int h;
			int j;
			System.Reflection.MemberInfo tmp;

			h = 1;
			while (h * 3 + 1 <= list.Length)
				h = 3 * h + 1;

			while (h > 0)
			{
				for (int i = h - 1; i < list.Length; i++)
				{
					tmp = list[i];
					j = i;
					while (true)
					{
						if (j >= h)
						{
							if (string.Compare(list[j - h].Name, tmp.Name, StringComparison.InvariantCulture) > 0)
							{
								list[j] = list[j - h];
								j -= h;
							}
							else
								break;
						}
						else
							break;
					}

					list[j] = tmp;
				}
				h /= 3;
			}
		}

		internal static NetDeliveryMethod GetDeliveryMethod(NetMessageType mtp)
		{
			if (mtp >= NetMessageType.UserReliableOrdered1)
				return NetDeliveryMethod.ReliableOrdered;
			else if (mtp >= NetMessageType.UserReliableSequenced1)
				return NetDeliveryMethod.ReliableSequenced;
			else if (mtp >= NetMessageType.UserReliableUnordered)
				return NetDeliveryMethod.ReliableUnordered;
			else if (mtp >= NetMessageType.UserSequenced1)
				return NetDeliveryMethod.UnreliableSequenced;
			return NetDeliveryMethod.Unreliable;
		}

		/// <summary>
		/// Creates a comma delimited string from a lite of items
		/// </summary>
		public static string MakeCommaDelimitedList<T>(IList<T> list)
		{
			var cnt = list.Count;
			StringBuilder bdr = new StringBuilder(cnt * 5); // educated guess
			for(int i=0;i<cnt;i++)
			{
				bdr.Append(list[i].ToString());
				if (i != cnt - 1)
					bdr.Append(", ");
			}
			return bdr.ToString();
		}

		public static byte[] ComputeSHAHash(byte[] bytes)
		{
			// this is defined in the platform specific files
			return ComputeSHAHash(bytes, 0, bytes.Length);
		}

        /// <summary>
        /// Copies from <paramref name="src"/> to <paramref name="dst"/>. Maps to an IPv6 address
        /// </summary>
        /// <param name="src">Source.</param>
        /// <param name="dst">Destination.</param>
        internal static void CopyEndpoint(IPEndPoint src, IPEndPoint dst)
        {
            dst.Port = src.Port;
            if (src.AddressFamily == AddressFamily.InterNetwork)
                dst.Address = src.Address.MapToIPv6();
            else
                dst.Address = src.Address;
        }

        /// <summary>
        /// Maps the IPEndPoint object to an IPv6 address. Has allocation
        /// </summary>
        internal static IPEndPoint MapToIPv6(IPEndPoint endPoint)
        {
            if (endPoint.AddressFamily == AddressFamily.InterNetwork)
                return new IPEndPoint(endPoint.Address.MapToIPv6(), endPoint.Port);
            return endPoint;
        }

        // MemoryMarshal.Read and MemoryMarshal.Write are not GUARANTEED to allow unaligned reads/writes.
        // The current CoreCLR implementation does but that's an implementation detail.
        // These are basically MemoryMarshal.Read/Write but well, guaranteed to allow unaligned access.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static T ReadUnaligned<T>(ReadOnlySpan<byte> source) where T : unmanaged
        {
            if (Unsafe.SizeOf<T>() > source.Length)
            {
	            throw new ArgumentOutOfRangeException();
            }

            return Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(source));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void WriteUnaligned<T>(Span<byte> destination, ref T value) where T : unmanaged
        {
            if ((uint)Unsafe.SizeOf<T>() > (uint)destination.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(destination), value);
        }
    }
}
