using System;
using System.IO;

namespace ZipStream
{
    public class LimitStream : Stream
	{
        private Stream _baseStream;
        private int _position;
        private int _limit;

		public LimitStream (Stream baseStream, int limit)
		{
            _baseStream = baseStream;
            _limit = limit;
		}

        protected override void Dispose(bool disposing)
        {
            byte[] buf = new byte[4096];
            while (_position < _limit) {
                int numRead = _baseStream.Read(buf, 0, Math.Min(buf.Length, _limit - _position));
                if (numRead == 0)
                    break;
                _position += numRead;
            }

            base.Dispose(disposing);
        }

        #region implemented abstract members of Stream
        public override void Flush()
        {
            _baseStream.Flush();
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            int left = _limit - _position;
            if (left == 0)
                return 0;

            int bytesRead = Math.Min(count, left);
            bytesRead = _baseStream.Read(buffer, offset, bytesRead);
            _position += bytesRead;
            return bytesRead;
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
        public override bool CanRead
        {
            get
            {
                return true;
            }
        }
        public override bool CanSeek
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
                return false;
            }
        }
        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }
        public override long Position
        {
            get
            {
                return _position;
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        #endregion
	}
}
