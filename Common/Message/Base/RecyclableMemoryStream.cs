using System;
using System.Collections.Generic;
using System.IO;

namespace LunaCommon.Message.Base
{
    public sealed class RecyclableMemoryStream : MemoryStream
    {
        private const long MaxStreamLength = 2147483647L;

        private readonly List<byte[]> blocks = new List<byte[]>(1);

        private byte[] largeBuffer;

        private List<byte[]> dirtyBuffers;

        private readonly Guid id;

        private readonly string tag;

        private readonly RecyclableMemoryStreamManager memoryManager;

        private bool disposed;

        private readonly string allocationStack;

        private string disposeStack;

        private readonly byte[] byteBuffer = new byte[1];

        private int length;

        private int position;

        internal string AllocationStack
        {
            get
            {
                return this.allocationStack;
            }
        }

        public override bool CanRead
        {
            get
            {
                return !this.disposed;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return !this.disposed;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return !this.disposed;
            }
        }

        public override int Capacity
        {
            get
            {
                this.CheckDisposed();
                if (this.largeBuffer != null)
                {
                    return (int)this.largeBuffer.Length;
                }
                if (this.blocks.Count <= 0)
                {
                    return 0;
                }
                return this.blocks.Count * this.memoryManager.BlockSize;
            }
            set
            {
                this.EnsureCapacity(value);
            }
        }

        internal string DisposeStack
        {
            get
            {
                return this.disposeStack;
            }
        }

        internal Guid Id
        {
            get
            {
                this.CheckDisposed();
                return this.id;
            }
        }

        public override long Length
        {
            get
            {
                this.CheckDisposed();
                return (long)this.length;
            }
        }

        internal RecyclableMemoryStreamManager MemoryManager
        {
            get
            {
                this.CheckDisposed();
                return this.memoryManager;
            }
        }

        public override long Position
        {
            get
            {
                this.CheckDisposed();
                return (long)this.position;
            }
            set
            {
                if (value < (long)0)
                {
                    throw new ArgumentOutOfRangeException("value", "value must be non-negative");
                }
                if (value > (long)2147483647)
                {
                    throw new ArgumentOutOfRangeException("value", string.Concat("value cannot be more than ", (long)2147483647));
                }
                this.position = (int)value;
            }
        }

        internal string Tag
        {
            get
            {
                this.CheckDisposed();
                return this.tag;
            }
        }

        public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager) : this(memoryManager, null)
        {
        }

        public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, string tag) : this(memoryManager, tag, 0)
        {
        }

        public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, string tag, int requestedSize) : this(memoryManager, tag, requestedSize, null)
        {
        }

        internal RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, string tag, int requestedSize, byte[] initialLargeBuffer)
        {
            this.memoryManager = memoryManager;
            this.id = Guid.NewGuid();
            this.tag = tag;
            if (requestedSize < memoryManager.BlockSize)
            {
                requestedSize = memoryManager.BlockSize;
            }
            if (initialLargeBuffer != null)
            {
                this.largeBuffer = initialLargeBuffer;
            }
            else
            {
                this.EnsureCapacity(requestedSize);
            }
            this.disposed = false;
            if (this.memoryManager.GenerateCallStacks)
            {
                this.allocationStack = Environment.StackTrace;
            }
            RecyclableMemoryStreamManager.Events.Write.MemoryStreamCreated(this.id, this.tag, requestedSize);
            this.memoryManager.ReportStreamCreated();
        }

        private void CheckDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(string.Format("The stream with Id {0} and Tag {1} is disposed.", this.id, this.tag));
            }
        }

        public override void Close()
        {
            this.Dispose(true);
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                string stackTrace = null;
                if (this.memoryManager.GenerateCallStacks)
                {
                    stackTrace = Environment.StackTrace;
                }
                RecyclableMemoryStreamManager.Events.Write.MemoryStreamDoubleDispose(this.id, this.tag, this.allocationStack, this.disposeStack, stackTrace);
                throw new InvalidOperationException("Cannot dispose of RecyclableMemoryStream twice");
            }
            RecyclableMemoryStreamManager.Events.Write.MemoryStreamDisposed(this.id, this.tag);
            if (this.memoryManager.GenerateCallStacks)
            {
                this.disposeStack = Environment.StackTrace;
            }
            if (!disposing)
            {
                RecyclableMemoryStreamManager.Events.Write.MemoryStreamFinalized(this.id, this.tag, this.allocationStack);
                if (AppDomain.CurrentDomain.IsFinalizingForUnload())
                {
                    base.Dispose(disposing);
                    return;
                }
                this.memoryManager.ReportStreamFinalized();
            }
            else
            {
                this.disposed = true;
                this.memoryManager.ReportStreamDisposed();
                GC.SuppressFinalize(this);
            }
            this.memoryManager.ReportStreamLength((long)this.length);
            if (this.largeBuffer != null)
            {
                this.memoryManager.ReturnLargeBuffer(this.largeBuffer, this.tag);
            }
            if (this.dirtyBuffers != null)
            {
                foreach (byte[] dirtyBuffer in this.dirtyBuffers)
                {
                    this.memoryManager.ReturnLargeBuffer(dirtyBuffer, this.tag);
                }
            }
            this.memoryManager.ReturnBlocks(this.blocks, this.tag);
            base.Dispose(disposing);
        }

        private void EnsureCapacity(int newCapacity)
        {
            if ((long)newCapacity > this.memoryManager.MaximumStreamCapacity && this.memoryManager.MaximumStreamCapacity > (long)0)
            {
                RecyclableMemoryStreamManager.Events.Write.MemoryStreamOverCapacity(newCapacity, this.memoryManager.MaximumStreamCapacity, this.tag, this.allocationStack);
                object[] objArray = new object[] { "Requested capacity is too large: ", newCapacity, ". Limit is ", this.memoryManager.MaximumStreamCapacity };
                throw new InvalidOperationException(string.Concat(objArray));
            }
            if (this.largeBuffer == null)
            {
                while (this.Capacity < newCapacity)
                {
                    this.blocks.Add(this.MemoryManager.GetBlock());
                }
            }
            else if (newCapacity > (int)this.largeBuffer.Length)
            {
                byte[] largeBuffer = this.memoryManager.GetLargeBuffer(newCapacity, this.tag);
                this.InternalRead(largeBuffer, 0, this.length, 0);
                this.ReleaseLargeBuffer();
                this.largeBuffer = largeBuffer;
                return;
            }
        }
        

        public override byte[] GetBuffer()
        {
            this.CheckDisposed();
            if (this.largeBuffer != null)
            {
                return this.largeBuffer;
            }
            if (this.blocks.Count == 1)
            {
                return this.blocks[0];
            }
            byte[] largeBuffer = this.MemoryManager.GetLargeBuffer(this.Capacity, this.tag);
            this.InternalRead(largeBuffer, 0, this.length, 0);
            this.largeBuffer = largeBuffer;
            if (this.blocks.Count > 0 && this.memoryManager.AggressiveBufferReturn)
            {
                this.memoryManager.ReturnBlocks(this.blocks, this.tag);
                this.blocks.Clear();
            }
            return this.largeBuffer;
        }

        private int InternalRead(byte[] buffer, int offset, int count, int fromPosition)
        {
            if (this.length - fromPosition <= 0)
            {
                return 0;
            }
            if (this.largeBuffer != null)
            {
                int num = Math.Min(count, this.length - fromPosition);
                Buffer.BlockCopy(this.largeBuffer, fromPosition, buffer, offset, num);
                return num;
            }
            int blockIndex = this.OffsetToBlockIndex(fromPosition);
            int num1 = 0;
            int num2 = Math.Min(count, this.length - fromPosition);
            int blockOffset = this.OffsetToBlockOffset(fromPosition);
            while (num2 > 0)
            {
                int num3 = Math.Min((int)this.blocks[blockIndex].Length - blockOffset, num2);
                Buffer.BlockCopy(this.blocks[blockIndex], blockOffset, buffer, num1 + offset, num3);
                num1 += num3;
                num2 -= num3;
                blockIndex++;
                blockOffset = 0;
            }
            return num1;
        }

        private int OffsetToBlockIndex(int offset)
        {
            return offset / this.memoryManager.BlockSize;
        }

        private int OffsetToBlockOffset(int offset)
        {
            return offset % this.memoryManager.BlockSize;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            this.CheckDisposed();
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "offset cannot be negative");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "count cannot be negative");
            }
            if (offset + count > (int)buffer.Length)
            {
                throw new ArgumentException("buffer length must be at least offset + count");
            }
            int num = this.InternalRead(buffer, offset, count, this.position);
            this.position += num;
            return num;
        }

        public override int ReadByte()
        {
            this.CheckDisposed();
            if (this.position == this.length)
            {
                return -1;
            }
            byte item = 0;
            if (this.largeBuffer != null)
            {
                item = this.largeBuffer[this.position];
            }
            else
            {
                int blockIndex = this.OffsetToBlockIndex(this.position);
                int blockOffset = this.OffsetToBlockOffset(this.position);
                item = this.blocks[blockIndex][blockOffset];
            }
            this.position++;
            return item;
        }

        private void ReleaseLargeBuffer()
        {
            if (!this.memoryManager.AggressiveBufferReturn)
            {
                if (this.dirtyBuffers == null)
                {
                    this.dirtyBuffers = new List<byte[]>(1);
                }
                this.dirtyBuffers.Add(this.largeBuffer);
            }
            else
            {
                this.memoryManager.ReturnLargeBuffer(this.largeBuffer, this.tag);
            }
            this.largeBuffer = null;
        }

        public override long Seek(long offset, SeekOrigin loc)
        {
            int num;
            this.CheckDisposed();
            if (offset > (long)2147483647)
            {
                throw new ArgumentOutOfRangeException("offset", string.Concat("offset cannot be larger than ", (long)2147483647));
            }
            switch (loc)
            {
                case SeekOrigin.Begin:
                    {
                        num = (int)offset;
                        break;
                    }
                case SeekOrigin.Current:
                    {
                        num = (int)offset + this.position;
                        break;
                    }
                case SeekOrigin.End:
                    {
                        num = (int)offset + this.length;
                        break;
                    }
                default:
                    {
                        throw new ArgumentException("Invalid seek origin", "loc");
                    }
            }
            if (num < 0)
            {
                throw new IOException("Seek before beginning");
            }
            this.position = num;
            return (long)this.position;
        }

        public override void SetLength(long value)
        {
            this.CheckDisposed();
            if (value < (long)0 || value > (long)2147483647)
            {
                throw new ArgumentOutOfRangeException("value", string.Concat("value must be non-negative and at most ", (long)2147483647));
            }
            this.EnsureCapacity((int)value);
            this.length = (int)value;
            if ((long)this.position > value)
            {
                this.position = (int)value;
            }
        }

        [Obsolete("This method has degraded performance vs. GetBuffer and should be avoided.")]
        public override byte[] ToArray()
        {
            string stackTrace;
            this.CheckDisposed();
            byte[] numArray = new byte[checked(this.Length)];
            this.InternalRead(numArray, 0, this.length, 0);
            if (this.memoryManager.GenerateCallStacks)
            {
                stackTrace = Environment.StackTrace;
            }
            else
            {
                stackTrace = null;
            }
            RecyclableMemoryStreamManager.Events.Write.MemoryStreamToArray(this.id, this.tag, stackTrace, 0);
            this.memoryManager.ReportStreamToArray();
            return numArray;
        }

        public override string ToString()
        {
            return string.Format("Id = {0}, Tag = {1}, Length = {2:N0} bytes", this.Id, this.Tag, this.Length);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.CheckDisposed();
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", (object)offset, "Offset must be in the range of 0 - buffer.Length-1");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", (object)count, "count must be non-negative");
            }
            if (count + offset > (int)buffer.Length)
            {
                throw new ArgumentException("count must be greater than buffer.Length - offset");
            }
            if (this.Position + (long)count > (long)2147483647)
            {
                throw new IOException("Maximum capacity exceeded");
            }
            int position = (int)this.Position + count;
            int blockSize = this.memoryManager.BlockSize;
            if ((long)((position + blockSize - 1) / blockSize * blockSize) > (long)2147483647)
            {
                throw new IOException("Maximum capacity exceeded");
            }
            this.EnsureCapacity(position);
            if (this.largeBuffer != null)
            {
                Buffer.BlockCopy(buffer, offset, this.largeBuffer, this.position, count);
            }
            else
            {
                int num = count;
                int num1 = 0;
                int blockIndex = this.OffsetToBlockIndex(this.position);
                int blockOffset = this.OffsetToBlockOffset(this.position);
                while (num > 0)
                {
                    byte[] item = this.blocks[blockIndex];
                    int num2 = Math.Min(blockSize - blockOffset, num);
                    Buffer.BlockCopy(buffer, offset + num1, item, blockOffset, num2);
                    num -= num2;
                    num1 += num2;
                    blockIndex++;
                    blockOffset = 0;
                }
            }
            this.Position = (long)position;
            this.length = Math.Max(this.position, this.length);
        }

        public override void WriteByte(byte value)
        {
            this.CheckDisposed();
            this.byteBuffer[0] = value;
            this.Write(this.byteBuffer, 0, 1);
        }

        public override void WriteTo(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (this.largeBuffer != null)
            {
                stream.Write(this.largeBuffer, 0, this.length);
                return;
            }
            int num = 0;
            int num1 = this.length;
            while (num1 > 0)
            {
                int num2 = Math.Min((int)this.blocks[num].Length, num1);
                stream.Write(this.blocks[num], 0, num2);
                num1 -= num2;
                num++;
            }
        }
    }
}
