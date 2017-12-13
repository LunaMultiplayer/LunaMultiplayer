using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace LunaCommon.Message.Base
{
    public sealed class RecyclableMemoryStreamManager
    {
        public const int DefaultBlockSize = 131072;

        public const int DefaultLargeBufferMultiple = 1048576;

        public const int DefaultMaximumBufferSize = 134217728;

        private readonly int blockSize;

        private readonly long[] largeBufferFreeSize;

        private readonly long[] largeBufferInUseSize;

        private readonly int largeBufferMultiple;

        private readonly ConcurrentStack<byte[]>[] largePools;

        private readonly int maximumBufferSize;

        private readonly ConcurrentStack<byte[]> smallPool;

        private long smallPoolFreeSize;

        private long smallPoolInUseSize;

        public bool AggressiveBufferReturn
        {
            get;
            set;
        }

        public int BlockSize
        {
            get
            {
                return this.blockSize;
            }
        }

        public bool GenerateCallStacks
        {
            get;
            set;
        }

        public int LargeBufferMultiple
        {
            get
            {
                return this.largeBufferMultiple;
            }
        }

        public long LargeBuffersFree
        {
            get
            {
                long count = (long)0;
                ConcurrentStack<byte[]>[] concurrentStackArray = this.largePools;
                for (int i = 0; i < (int)concurrentStackArray.Length; i++)
                {
                    count += (long)concurrentStackArray[i].Count;
                }
                return count;
            }
        }

        public long LargePoolFreeSize
        {
            get
            {
                return this.largeBufferFreeSize.Sum();
            }
        }

        public long LargePoolInUseSize
        {
            get
            {
                return this.largeBufferInUseSize.Sum();
            }
        }

        public int MaximumBufferSize
        {
            get
            {
                return this.maximumBufferSize;
            }
        }

        public long MaximumFreeLargePoolBytes
        {
            get;
            set;
        }

        public long MaximumFreeSmallPoolBytes
        {
            get;
            set;
        }

        public long MaximumStreamCapacity
        {
            get;
            set;
        }

        public long SmallBlocksFree
        {
            get
            {
                return (long)this.smallPool.Count();
            }
        }

        public long SmallPoolFreeSize
        {
            get
            {
                return this.smallPoolFreeSize;
            }
        }

        public long SmallPoolInUseSize
        {
            get
            {
                return this.smallPoolInUseSize;
            }
        }

        public RecyclableMemoryStreamManager() : this(131072, 1048576, 134217728)
        {
        }

        public RecyclableMemoryStreamManager(int blockSize, int largeBufferMultiple, int maximumBufferSize)
        {
            if (blockSize <= 0)
            {
                throw new ArgumentOutOfRangeException("blockSize", (object)blockSize, "blockSize must be a positive number");
            }
            if (largeBufferMultiple <= 0)
            {
                throw new ArgumentOutOfRangeException("largeBufferMultiple", "largeBufferMultiple must be a positive number");
            }
            if (maximumBufferSize < blockSize)
            {
                throw new ArgumentOutOfRangeException("maximumBufferSize", "maximumBufferSize must be at least blockSize");
            }
            this.blockSize = blockSize;
            this.largeBufferMultiple = largeBufferMultiple;
            this.maximumBufferSize = maximumBufferSize;
            if (!this.IsLargeBufferMultiple(maximumBufferSize))
            {
                throw new ArgumentException("maximumBufferSize is not a multiple of largeBufferMultiple", "maximumBufferSize");
            }
            this.smallPool = new ConcurrentStack<byte[]>();
            int num = maximumBufferSize / largeBufferMultiple;
            this.largeBufferInUseSize = new long[num + 1];
            this.largeBufferFreeSize = new long[num];
            this.largePools = new ConcurrentStack<byte[]>[num];
            for (int i = 0; i < (int)this.largePools.Length; i++)
            {
                this.largePools[i] = new ConcurrentStack<byte[]>();
            }
            RecyclableMemoryStreamManager.Events.Write.MemoryStreamManagerInitialized(blockSize, largeBufferMultiple, maximumBufferSize);
        }

        internal byte[] GetBlock()
        {
            byte[] numArray = null;
            if (this.smallPool.TryPop(out numArray))
            {
                Interlocked.Add(ref this.smallPoolFreeSize, (long)(-this.BlockSize));
            }
            else
            {
                numArray = new byte[this.BlockSize];
                RecyclableMemoryStreamManager.Events.Write.MemoryStreamNewBlockCreated(this.smallPoolInUseSize);
                if (this.BlockCreated != null)
                {
                    this.BlockCreated();
                }
            }
            Interlocked.Add(ref this.smallPoolInUseSize, (long)this.BlockSize);
            return numArray;
        }

        internal byte[] GetLargeBuffer(int requiredSize, string tag)
        {
            byte[] numArray = null;
            requiredSize = this.RoundToLargeBufferMultiple(requiredSize);
            int length = requiredSize / this.largeBufferMultiple - 1;
            if (length >= (int)this.largePools.Length)
            {
                length = (int)this.largeBufferInUseSize.Length - 1;
                numArray = new byte[requiredSize];
                string stackTrace = null;
                if (this.GenerateCallStacks)
                {
                    stackTrace = Environment.StackTrace;
                }
                RecyclableMemoryStreamManager.Events.Write.MemoryStreamNonPooledLargeBufferCreated(requiredSize, tag, stackTrace);
                if (this.LargeBufferCreated != null)
                {
                    this.LargeBufferCreated();
                }
            }
            else if (this.largePools[length].TryPop(out numArray))
            {
                Interlocked.Add(ref this.largeBufferFreeSize[length], (long)(-(int)numArray.Length));
            }
            else
            {
                numArray = new byte[requiredSize];
                RecyclableMemoryStreamManager.Events.Write.MemoryStreamNewLargeBufferCreated(requiredSize, this.LargePoolInUseSize);
                if (this.LargeBufferCreated != null)
                {
                    this.LargeBufferCreated();
                }
            }
            Interlocked.Add(ref this.largeBufferInUseSize[length], (long)((int)numArray.Length));
            return numArray;
        }

        public MemoryStream GetStream()
        {
            return new RecyclableMemoryStream(this);
        }

        public MemoryStream GetStream(string tag)
        {
            return new RecyclableMemoryStream(this, tag);
        }

        public MemoryStream GetStream(string tag, int requiredSize)
        {
            return new RecyclableMemoryStream(this, tag, requiredSize);
        }

        public MemoryStream GetStream(string tag, int requiredSize, bool asContiguousBuffer)
        {
            if (!asContiguousBuffer || requiredSize <= this.BlockSize)
            {
                return this.GetStream(tag, requiredSize);
            }
            return new RecyclableMemoryStream(this, tag, requiredSize, this.GetLargeBuffer(requiredSize, tag));
        }

        public MemoryStream GetStream(string tag, byte[] buffer, int offset, int count)
        {
            RecyclableMemoryStream recyclableMemoryStream = new RecyclableMemoryStream(this, tag, count);
            recyclableMemoryStream.Write(buffer, offset, count);
            recyclableMemoryStream.Position = (long)0;
            return recyclableMemoryStream;
        }

        private bool IsLargeBufferMultiple(int value)
        {
            if (value == 0)
            {
                return false;
            }
            return value % this.LargeBufferMultiple == 0;
        }

        internal void ReportStreamCreated()
        {
            if (this.StreamCreated != null)
            {
                this.StreamCreated();
            }
        }

        internal void ReportStreamDisposed()
        {
            if (this.StreamDisposed != null)
            {
                this.StreamDisposed();
            }
        }

        internal void ReportStreamFinalized()
        {
            if (this.StreamFinalized != null)
            {
                this.StreamFinalized();
            }
        }

        internal void ReportStreamLength(long bytes)
        {
            if (this.StreamLength != null)
            {
                this.StreamLength(bytes);
            }
        }

        internal void ReportStreamToArray()
        {
            if (this.StreamConvertedToArray != null)
            {
                this.StreamConvertedToArray();
            }
        }

        internal void ReturnBlocks(ICollection<byte[]> blocks, string tag)
        {
            if (blocks == null)
            {
                throw new ArgumentNullException("blocks");
            }
            int count = blocks.Count * this.BlockSize;
            Interlocked.Add(ref this.smallPoolInUseSize, (long)(-count));
            foreach (byte[] block in blocks)
            {
                if (block != null && (int)block.Length == this.BlockSize)
                {
                    continue;
                }
                throw new ArgumentException("blocks contains buffers that are not BlockSize in length");
            }
            foreach (byte[] numArray in blocks)
            {
                if (this.MaximumFreeSmallPoolBytes == (long)0 || this.SmallPoolFreeSize < this.MaximumFreeSmallPoolBytes)
                {
                    Interlocked.Add(ref this.smallPoolFreeSize, (long)this.BlockSize);
                    this.smallPool.Push(numArray);
                }
                else
                {
                    RecyclableMemoryStreamManager.Events.Write.MemoryStreamDiscardBuffer(RecyclableMemoryStreamManager.Events.MemoryStreamBufferType.Small, tag, RecyclableMemoryStreamManager.Events.MemoryStreamDiscardReason.EnoughFree);
                    if (this.BlockDiscarded == null)
                    {
                        break;
                    }
                    this.BlockDiscarded();
                    break;
                }
            }
            if (this.UsageReport != null)
            {
                this.UsageReport(this.smallPoolInUseSize, this.smallPoolFreeSize, this.LargePoolInUseSize, this.LargePoolFreeSize);
            }
        }

        internal void ReturnLargeBuffer(byte[] buffer, string tag)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (!this.IsLargeBufferMultiple((int)buffer.Length))
            {
                throw new ArgumentException(string.Concat("buffer did not originate from this memory manager. The size is not a multiple of ", this.LargeBufferMultiple));
            }
            int length = (int)buffer.Length / this.largeBufferMultiple - 1;
            if (length >= (int)this.largePools.Length)
            {
                length = (int)this.largeBufferInUseSize.Length - 1;
                RecyclableMemoryStreamManager.Events.Write.MemoryStreamDiscardBuffer(RecyclableMemoryStreamManager.Events.MemoryStreamBufferType.Large, tag, RecyclableMemoryStreamManager.Events.MemoryStreamDiscardReason.TooLarge);
                if (this.LargeBufferDiscarded != null)
                {
                    this.LargeBufferDiscarded(0);
                }
            }
            else if ((long)((this.largePools[length].Count + 1) * (int)buffer.Length) <= this.MaximumFreeLargePoolBytes || this.MaximumFreeLargePoolBytes == (long)0)
            {
                this.largePools[length].Push(buffer);
                Interlocked.Add(ref this.largeBufferFreeSize[length], (long)((int)buffer.Length));
            }
            else
            {
                RecyclableMemoryStreamManager.Events.Write.MemoryStreamDiscardBuffer(RecyclableMemoryStreamManager.Events.MemoryStreamBufferType.Large, tag, RecyclableMemoryStreamManager.Events.MemoryStreamDiscardReason.EnoughFree);
                if (this.LargeBufferDiscarded != null)
                {
                    this.LargeBufferDiscarded(Events.MemoryStreamDiscardReason.EnoughFree);
                }
            }
            Interlocked.Add(ref this.largeBufferInUseSize[length], (long)(-(int)buffer.Length));
            if (this.UsageReport != null)
            {
                this.UsageReport(this.smallPoolInUseSize, this.smallPoolFreeSize, this.LargePoolInUseSize, this.LargePoolFreeSize);
            }
        }

        private int RoundToLargeBufferMultiple(int requiredSize)
        {
            return (requiredSize + this.LargeBufferMultiple - 1) / this.LargeBufferMultiple * this.LargeBufferMultiple;
        }

        public event RecyclableMemoryStreamManager.EventHandler BlockCreated;

        public event RecyclableMemoryStreamManager.EventHandler BlockDiscarded;

        public event RecyclableMemoryStreamManager.EventHandler LargeBufferCreated;

        public event RecyclableMemoryStreamManager.LargeBufferDiscardedEventHandler LargeBufferDiscarded;

        public event RecyclableMemoryStreamManager.EventHandler StreamConvertedToArray;

        public event RecyclableMemoryStreamManager.EventHandler StreamCreated;

        public event RecyclableMemoryStreamManager.EventHandler StreamDisposed;

        public event RecyclableMemoryStreamManager.EventHandler StreamFinalized;

        public event RecyclableMemoryStreamManager.StreamLengthReportHandler StreamLength;

        public event RecyclableMemoryStreamManager.UsageReportEventHandler UsageReport;

        public delegate void EventHandler();

        public sealed class Events
        {
            public static RecyclableMemoryStreamManager.Events Write;

            static Events()
            {
                RecyclableMemoryStreamManager.Events.Write = new RecyclableMemoryStreamManager.Events();
            }

            public Events()
            {
            }

            public void MemoryStreamCreated(Guid guid, string tag, int requestedSize)
            {
            }

            public void MemoryStreamDiscardBuffer(RecyclableMemoryStreamManager.Events.MemoryStreamBufferType bufferType, string tag, RecyclableMemoryStreamManager.Events.MemoryStreamDiscardReason reason)
            {
            }

            public void MemoryStreamDisposed(Guid guid, string tag)
            {
            }

            public void MemoryStreamDoubleDispose(Guid guid, string tag, string allocationStack, string disposeStack1, string disposeStack2)
            {
            }

            public void MemoryStreamFinalized(Guid guid, string tag, string allocationStack)
            {
            }

            public void MemoryStreamManagerInitialized(int blockSize, int largeBufferMultiple, int maximumBufferSize)
            {
            }

            public void MemoryStreamNewBlockCreated(long smallPoolInUseBytes)
            {
            }

            public void MemoryStreamNewLargeBufferCreated(int requiredSize, long largePoolInUseBytes)
            {
            }

            public void MemoryStreamNonPooledLargeBufferCreated(int requiredSize, string tag, string allocationStack)
            {
            }

            public void MemoryStreamOverCapacity(int requestedCapacity, long maxCapacity, string tag, string allocationStack)
            {
            }

            public void MemoryStreamToArray(Guid guid, string tag, string stack, int size)
            {
            }

            public enum MemoryStreamBufferType
            {
                Small,
                Large
            }

            public enum MemoryStreamDiscardReason
            {
                TooLarge,
                EnoughFree
            }
        }

        public delegate void LargeBufferDiscardedEventHandler(RecyclableMemoryStreamManager.Events.MemoryStreamDiscardReason reason);

        public delegate void StreamLengthReportHandler(long bytes);

        public delegate void UsageReportEventHandler(long smallPoolInUseBytes, long smallPoolFreeBytes, long largePoolInUseBytes, long largePoolFreeBytes);
    }
}
