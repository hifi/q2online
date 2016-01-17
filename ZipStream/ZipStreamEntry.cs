using System;

namespace ZipStream
{
	public class ZipStreamEntry
    {
        static string[] _compressionMethods = new string[]{
            "store",
            "shrunk",
            "reduce1",
            "reduce2",
            "reduce3",
            "reduce4",
            "implode",
            "token",
            "deflate",
            "deflate64",
            "pkware",
            "reserved",
            "bzip2"
        };

        // only supported compression types
        public const int COMPRESS_STORE    = 0;
        public const int COMPRESS_DEFLATE  = 8;

        internal int _signature;
        internal short _version;
        internal short _flags;
        internal short _compression;
        internal short _time;
        internal short _date;
        internal int _crc32;
        internal int _compressedLength;
        internal int _uncompressedLength;
        internal short _nameLength;
        internal short _extraLength;
        internal byte[] _fileName;
        internal byte[] _extraField;

        public int Signature {
            get {
                return _signature;
            }
        }

        public short Version {
            get {
                return _version;
            }
        }

        public short Flags {
            get {
                return _flags;
            }
        }

        public short CompressionMethod {
            get {
                return _compression;
            }
        }

        public string CompressionMethodName {
            get {
                return _compressionMethods[_compression];
            }
        }

        public short ModificationTime {
            get {
                return _time;
            }
        }

        public short ModificationDate {
            get {
                return _date;
            }
        }

        public int CRC32 {
            get {
                return _crc32;
            }
        }

        public int CompressedLength {
            get {
                return _compressedLength;
            }
        }

        public int UncompressedLength {
            get {
                return _uncompressedLength;
            }
        }

        public short NameLength {
            get {
                return _nameLength;
            }
        }

        public short ExtraLength {
            get {
                return _extraLength;
            }
        }

        public String FileName {
            get {
                return System.Text.Encoding.UTF8.GetString(_fileName);
            }
        }

        internal ZipStreamEntry()
        {

        }

        public override string ToString()
        {
            string dump = "";
            dump += String.Format("Signature   : {0,8:X8}\n", Signature);
            dump += String.Format("Version     : {0,8:X8}\n", Version);
            dump += String.Format("Flags       : {0,8:X8}\n", Flags);
            dump += String.Format("Compression : {0,8:X8} ({1})\n", CompressionMethod, CompressionMethodName);
            dump += String.Format("Time        : {0,8:X8}\n", ModificationTime);
            dump += String.Format("Date        : {0,8:X8}\n", ModificationDate);
            dump += String.Format("CRC32       : {0,8:X8}\n", CRC32);
            dump += String.Format("CompSize    : {0}\n", CompressedLength);
            dump += String.Format("Size        : {0}\n", UncompressedLength);
            dump += String.Format("NameLength  : {0}\n", NameLength);
            dump += String.Format("ExtraLength : {0}\n", ExtraLength);
            dump += String.Format("Name:       : {0}\n", FileName);
            return dump;
        }
    }
}

