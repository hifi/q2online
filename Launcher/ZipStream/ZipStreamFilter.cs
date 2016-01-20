using System;
using System.IO;

namespace ZipStream
{
    public class ZipStreamFilter : Stream
    {
        static sbyte[] ZIP_LOCAL = { 0x50, 0x4B, 0x03, 0x04, -1, 0x00 };
        static byte[] ibuf;
        private Stream s;
        private int ip;
        private int op;

        public ZipStreamFilter(Stream s)
        {
            this.s = s;
            ibuf = new byte[ZIP_LOCAL.Length];
        }

        override public int ReadByte()
        {
            byte[] b = new byte[1];
            if (Read(b, 0, 1) == 1)
                return b[0];
            else
                return -1;
        }

        override public int Read(byte[] b, int off, int len)
        {
            // first seek for the zip header if not found yet, keep read bytes
            while (ip < ZIP_LOCAL.Length) {
                int c = s.ReadByte();
                if (c == ZIP_LOCAL[ip] || ZIP_LOCAL[ip] == -1) {
                    ibuf[ip++] = (byte)c;
                } else {
                    ip = 0;
                }
            }

            // satisfy as much of this read as we can from our buffer
            int pos = 0;
            while (op < ibuf.Length && pos < len) {
                b[off + pos++] = ibuf[op++];
            }

            if (pos == len)
                return pos;

            return pos + s.Read(b, off + pos, len - pos);
        }

        #region implemented abstract members of Stream

        public override void Flush ()
        {
            s.Flush();
        }

        public override long Seek (long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength (long value)
        {
            throw new NotSupportedException();
        }

        public override void Write (byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead {
            get {
                return s.CanRead;
            }
        }

        public override bool CanSeek {
            get {
                return false;
            }
        }

        public override bool CanWrite {
            get {
                return false;
            }
        }

        public override long Length {
            get {
                return s.Length;
            }
        }

        public override long Position {
            get {
                return s.Position;
            }
            set {
                throw new NotSupportedException();
            }
        }

        #endregion
    }
}

