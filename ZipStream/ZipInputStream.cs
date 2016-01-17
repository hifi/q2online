using System;
using System.Collections;
using System.IO;
using System.IO.Compression;

namespace ZipStream
{
    public class ZipInputStream : Stream, IDisposable
    {
        public const int LocalFileHeaderSignature = 0x04034B50;

        private BinaryReader _reader; 
        private Stream _entryStream;

        public IEnumerable Entries
        {
            get {
                ZipStreamEntry entry;
                while ((entry = NextEntry()) != null)
                    yield return entry;
            }
        }

        public ZipInputStream (Stream inputStream)
        {
            _reader = new BinaryReader(inputStream);
        }

        protected ZipStreamEntry NextEntry()
        {
            if (_entryStream != null) {
                _entryStream.Dispose();
                _entryStream = null;
            }

            ZipStreamEntry entry = new ZipStreamEntry();

            entry._signature            = _reader.ReadInt32();

            if (entry.Signature != LocalFileHeaderSignature)
                return null;

            entry._version              = _reader.ReadInt16();
            entry._flags                = _reader.ReadInt16();
            entry._compression          = _reader.ReadInt16();
            entry._time                 = _reader.ReadInt16();
            entry._date                 = _reader.ReadInt16();
            entry._crc32                = _reader.ReadInt32();
            entry._compressedLength     = _reader.ReadInt32();
            entry._uncompressedLength   = _reader.ReadInt32();
            entry._nameLength           = _reader.ReadInt16();
            entry._extraLength          = _reader.ReadInt16();
            entry._fileName             = _reader.ReadBytes(entry.NameLength);
            entry._extraField           = _reader.ReadBytes(entry.ExtraLength);

            _entryStream = new LimitStream(_reader.BaseStream, entry.CompressedLength);

            if (entry.CompressionMethod == ZipStreamEntry.COMPRESS_DEFLATE) {
                _entryStream = new DeflateStream(_entryStream, CompressionMode.Decompress);
            }

            return entry;
        }

        #region implemented abstract members of Stream

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_entryStream == null)
                throw new EndOfStreamException();

            return _entryStream.Read(buffer, offset, count);
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
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            // FIXME?
            //_reader.Dispose();
            _reader = null;

            if (_entryStream != null) {
                _entryStream.Dispose();
                _entryStream = null;
            }

            base.Dispose(disposing);
        }
    }

}

