/***************************************************************************
 *  Copyright (C) 2009 by Peter L Jones                                    *
 *  pljones@users.sf.net                                                   *
 *                                                                         *
 *  This file is part of the Sims 3 Package Interface (s3pi)               *
 *                                                                         *
 *  s3pi is free software: you can redistribute it and/or modify           *
 *  it under the terms of the GNU General Public License as published by   *
 *  the Free Software Foundation, either version 3 of the License, or      *
 *  (at your option) any later version.                                    *
 *                                                                         *
 *  s3pi is distributed in the hope that it will be useful,                *
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of         *
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
 *  GNU General Public License for more details.                           *
 *                                                                         *
 *  You should have received a copy of the GNU General Public License      *
 *  along with s3pi.  If not, see <http://www.gnu.org/licenses/>.          *
 ***************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;

namespace RegulSaveCleaner.S3PI.Package
{
    /// <summary>
    /// Internal -- used by Package to handle compression routines
    /// </summary>
    internal static class Compression
    {
        public static byte[] UncompressStream(Stream stream, int filesize, int memsize)
        {
            BinaryReader r = new(stream);
            long end = stream.Position + filesize;

            byte[] uncdata = new byte[memsize];
            BinaryWriter bw = new(new MemoryStream(uncdata));

            byte[] data = r.ReadBytes(2);

            int datalen = (((data[0] & 0x80) != 0) ? 4 : 3) * (((data[0] & 0x01) != 0) ? 2 : 1);
            data = r.ReadBytes(datalen);

            long realsize = 0;
            for (int i = 0; i < data.Length; i++) realsize = (realsize << 8) + data[i];

            while (stream.Position < end) { Dechunk(stream, bw); }

            bw.Close();

            return uncdata;
        }

        public static void Dechunk(Stream stream, BinaryWriter bw)
        {
            BinaryReader r = new(stream);
            int copysize = 0;
            int copyoffset = 0;
            int datalen;
            byte[] data;

            byte packing = r.ReadByte();

            #region Compressed
            if (packing < 0x80) // 0.......; new data 3; copy data 10 (min 3); offset 1024
            {
                data = r.ReadBytes(1);
                datalen = packing & 0x03;
                copysize = ((packing >> 2) & 0x07) + 3;
                copyoffset = (((packing << 3) & 0x300) | data[0]) + 1;
            }
            else if (packing < 0xC0) // 10......; new data 3; copy data 67 (min 4); offset 16384
            {
                data = r.ReadBytes(2);
                datalen = (data[0] >> 6) & 0x03;
                copysize = (packing & 0x3F) + 4;
                copyoffset = (((data[0] << 8) & 0x3F00) | data[1]) + 1;
            }
            else if (packing < 0xE0) // 110.....; new data 3; copy data 1028 (min 5); offset 131072
            {
                data = r.ReadBytes(3);
                datalen = packing & 0x03;
                copysize = (((packing << 6) & 0x300) | data[2]) + 5;
                copyoffset = (((packing << 12) & 0x10000) | data[0] << 8 | data[1]) + 1;
            }
            #endregion
            #region Uncompressed
            else if (packing < 0xFC) // 1110000 - 11101111; new data 4-128
                datalen = (((packing & 0x1F) + 1) << 2);
            else // 111111..; new data 3
                datalen = packing & 0x03;
            #endregion

            if (datalen > 0)
            {
                data = r.ReadBytes(datalen);
                bw.Write(data);
            }

            if (copysize < copyoffset && copyoffset > 8) CopyA(bw.BaseStream, copyoffset, copysize); else CopyB(bw.BaseStream, copyoffset, copysize);
        }

        static void CopyA(Stream s, int offset, int len)
        {
            while (len > 0)
            {
                long dst = s.Position;
                byte[] b = new byte[Math.Min(offset, len)];
                len -= b.Length;

                s.Position -= offset;
                _ = s.Read(b, 0, b.Length);

                s.Position = dst;
                s.Write(b, 0, b.Length);
            }
        }

        static void CopyB(Stream s, int offset, int len)
        {
            while (len > 0)
            {
                long dst = s.Position;
                len--;

                s.Position -= offset;
                byte b = (byte)s.ReadByte();

                s.Position = dst;
                s.WriteByte(b);
            }
        }

        public static byte[] CompressStream(byte[] data) => Tiger.DbpfCompression.Compress(data, out byte[] res) ? res : data;
    }
}

/*
 * The following code was provided by Tiger
**/

namespace Tiger
{
    class DbpfCompression
    {
        public DbpfCompression(int level)
        {
            _mTracker = CreateTracker(level, out _mBruteForceLength);
        }

        private int _mBruteForceLength;
        private IMatchtracker _mTracker;

        private byte[] _mData;

        private int _mSequenceSource;
        private int _mSequenceLength;
        private int _mSequenceDest;
        private bool _mSequenceFound;

        public static bool Compress(byte[] data, out byte[] compressed)
        {
            compressed = new DbpfCompression(5).Compress(data);
            return compressed != null;
        }

        public byte[] Compress(byte[] data)
        {
            bool endisvalid = false;
            List<byte[]> compressedchunks = new();
            int compressedidx = 0;
            int compressedlen = 0;

            if (data.Length < 16 || data.LongLength > uint.MaxValue)
                return null;

            _mData = data;

            try
            {
                int lastbytestored = 0;

                while (compressedidx < data.Length)
                {
                    if (data.Length - compressedidx < 4)
                    {
                        // Just copy the rest
                        byte[] chunk = new byte[data.Length - compressedidx + 1];
                        chunk[0] = (byte)(0xFC | (data.Length - compressedidx));
                        Array.Copy(data, compressedidx, chunk, 1, data.Length - compressedidx);
                        compressedchunks.Add(chunk);
                        compressedidx += chunk.Length - 1;
                        compressedlen += chunk.Length;

                        endisvalid = true;
                        continue;
                    }

                    while (compressedidx > lastbytestored - 3)
                        _mTracker.Addvalue(data[lastbytestored++]);

                    // Search ahead in blocks of 4 bytes for a match until one is found
                    // Record the best match if multiple are found
                    _mSequenceSource = _mSequenceLength = 0;
                    _mSequenceDest = int.MaxValue;
                    _mSequenceFound = false;
                    do
                    {
                        for (int loop = 0; loop < 4; loop++)
                        {
                            if (lastbytestored < data.Length)
                                _mTracker.Addvalue(data[lastbytestored++]);
                            FindSequence(lastbytestored - 4);
                        }
                    }
                    while (!_mSequenceFound && lastbytestored + 4 <= data.Length);

                    if (!_mSequenceFound)
                        _mSequenceDest = _mData.Length;

                    // If the next match is more than four bytes away, put in codes to read uncompressed data
                    while (_mSequenceDest - compressedidx >= 4)
                    {
                        int tocopy = (_mSequenceDest - compressedidx) & ~3;
                        if (tocopy > 112) tocopy = 112;

                        byte[] chunk = new byte[tocopy + 1];
                        chunk[0] = (byte)(0xE0 | ((tocopy >> 2) - 1));
                        Array.Copy(data, compressedidx, chunk, 1, tocopy);
                        compressedchunks.Add(chunk);
                        compressedidx += tocopy;
                        compressedlen += chunk.Length;
                    }

                    if (_mSequenceFound)
                    {
                        byte[] chunk;
                        /*
                         * 00-7F  0oocccpp oooooooo
                         *   Read 0-3
                         *   Copy 3-10
                         *   Offset 0-1023
                         *   
                         * 80-BF  10cccccc ppoooooo oooooooo
                         *   Read 0-3
                         *   Copy 4-67
                         *   Offset 0-16383
                         *   
                         * C0-DF  110cccpp oooooooo oooooooo cccccccc
                         *   Read 0-3
                         *   Copy 5-1028
                         *   Offset 0-131071
                         *   
                         * E0-FC  111ppppp
                         *   Read 4-128 (Multiples of 4)
                         *   
                         * FD-FF  111111pp
                         *   Read 0-3
                         */
                        //if (FindRunLength(data, seqstart, compressedidx + seqidx) < seqlength)
                        //{
                        //    break;
                        //}
                        while (_mSequenceLength > 0)
                        {
                            int thislength = _mSequenceLength;
                            if (thislength > 1028)
                                thislength = 1028;
                            _mSequenceLength -= thislength;

                            int offset = _mSequenceDest - _mSequenceSource - 1;
                            int readbytes = _mSequenceDest - compressedidx;

                            _mSequenceSource += thislength;
                            _mSequenceDest += thislength;

                            if (thislength > 67 || offset > 16383)
                            {
                                chunk = new byte[readbytes + 4];
                                chunk[0] = (byte)(0xC0 | readbytes | (((thislength - 5) >> 6) & 0x0C) | ((offset >> 12) & 0x10));
                                chunk[1] = (byte)((offset >> 8) & 0xFF);
                                chunk[2] = (byte)(offset & 0xFF);
                                chunk[3] = (byte)((thislength - 5) & 0xFF);
                            }
                            else if (thislength > 10 || offset > 1023)
                            {
                                chunk = new byte[readbytes + 3];
                                chunk[0] = (byte)(0x80 | ((thislength - 4) & 0x3F));
                                chunk[1] = (byte)(((readbytes << 6) & 0xC0) | ((offset >> 8) & 0x3F));
                                chunk[2] = (byte)(offset & 0xFF);
                            }
                            else
                            {
                                chunk = new byte[readbytes + 2];
                                chunk[0] = (byte)((readbytes & 0x3) | (((thislength - 3) << 2) & 0x1C) | ((offset >> 3) & 0x60));
                                chunk[1] = (byte)(offset & 0xFF);
                            }

                            if (readbytes > 0)
                                Array.Copy(data, compressedidx, chunk, chunk.Length - readbytes, readbytes);

                            compressedchunks.Add(chunk);
                            compressedidx += thislength + readbytes;
                            compressedlen += chunk.Length;
                        }
                    }
                }

                if (compressedlen + 6 < data.Length)
                {
                    int chunkpos;
                    byte[] compressed;

                    if (data.Length > 0xFFFFFF)
                    {
                        // Activate the large data bit for > 16mb uncompressed data
                        compressed = new byte[compressedlen + 6 + (endisvalid ? 0 : 1)];
                        compressed[0] = 0x10 | 0x80; // 0x80 = length is 4 bytes
                        compressed[1] = 0xFB;
                        compressed[2] = (byte)(data.Length >> 24);
                        compressed[3] = (byte)(data.Length >> 16);
                        compressed[4] = (byte)(data.Length >> 8);
                        compressed[5] = (byte)(data.Length);
                        chunkpos = 6;
                    }
                    else
                    {
                        compressed = new byte[compressedlen + 5 + (endisvalid ? 0 : 1)];
                        compressed[0] = 0x10;
                        compressed[1] = 0xFB;
                        compressed[2] = (byte)(data.Length >> 16);
                        compressed[3] = (byte)(data.Length >> 8);
                        compressed[4] = (byte)(data.Length);
                        chunkpos = 5;
                    }

                    for (int loop = 0; loop < compressedchunks.Count; loop++)
                    {
                        Array.Copy(compressedchunks[loop], 0, compressed, chunkpos, compressedchunks[loop].Length);
                        chunkpos += compressedchunks[loop].Length;
                    }
                    if (!endisvalid)
                        compressed[compressed.Length - 1] = 0xfc;
                    return compressed;
                }

                return null;
            }
            finally
            {
                _mData = null;
                _mTracker.Reset();
            }
        }

        private void FindSequence(int startindex)
        {
            // Try a straight forward brute force first
            int end = -_mBruteForceLength;
            if (startindex < _mBruteForceLength)
                end = -startindex;

            byte searchforbyte = _mData[startindex];

            for (int loop = -1; loop >= end && _mSequenceLength < 1028; loop--)
            {
                byte curbyte = _mData[loop + startindex];
                if (curbyte != searchforbyte)
                    continue;

                int len = FindRunLength(startindex + loop, startindex);

                if (len <= _mSequenceLength
                    || len < 3
                    || len < 4 && loop <= -1024
                    || len < 5 && loop <= -16384)
                    continue;

                _mSequenceFound = true;
                _mSequenceSource = startindex + loop;
                _mSequenceLength = len;
                _mSequenceDest = startindex;
            }

            // Use the look-up table next
            int matchloc;
            if (_mSequenceLength < 1028 && _mTracker.FindMatch(out matchloc))
            {
                do
                {
                    int len = FindRunLength(matchloc, startindex);
                    if (len < 5)
                        continue;

                    _mSequenceFound = true;
                    _mSequenceSource = matchloc;
                    _mSequenceLength = len;
                    _mSequenceDest = startindex;
                }
                while (_mSequenceLength < 1028 && _mTracker.Nextmatch(out matchloc));
            }
        }

        private int FindRunLength(int src, int dst)
        {
            int endsrc = src + 1;
            int enddst = dst + 1;
            while (enddst < _mData.Length && _mData[endsrc] == _mData[enddst] && enddst - dst < 1028)
            {
                endsrc++;
                enddst++;
            }

            return enddst - dst;
        }

        private interface IMatchtracker
        {
            bool FindMatch(out int where);
            bool Nextmatch(out int where);
            void Addvalue(byte val);
            void Reset();
        }

        static IMatchtracker CreateTracker(int blockinterval, int lookupstart, int windowlength, int bucketdepth)
        {
            if (bucketdepth <= 1)
                return new SingledepthMatchTracker(blockinterval, lookupstart, windowlength);
            return new DeepMatchTracker(blockinterval, lookupstart, windowlength, bucketdepth);
        }

        static IMatchtracker CreateTracker(int level, out int bruteforcelength)
        {
            switch (level)
            {
                case 0:
                    bruteforcelength = 0;
                    return CreateTracker(4, 0, 16384, 1);
                case 1:
                    bruteforcelength = 0;
                    return CreateTracker(2, 0, 32768, 1);
                case 2:
                    bruteforcelength = 0;
                    return CreateTracker(1, 0, 65536, 1);
                case 3:
                    bruteforcelength = 0;
                    return CreateTracker(1, 0, 131000, 2);
                case 4:
                    bruteforcelength = 16;
                    return CreateTracker(1, 16, 131000, 2);
                case 5:
                    bruteforcelength = 16;
                    return CreateTracker(1, 16, 131000, 5);
                case 6:
                    bruteforcelength = 32;
                    return CreateTracker(1, 32, 131000, 5);
                case 7:
                    bruteforcelength = 32;
                    return CreateTracker(1, 32, 131000, 10);
                case 8:
                    bruteforcelength = 64;
                    return CreateTracker(1, 64, 131000, 10);
                case 9:
                    bruteforcelength = 128;
                    return CreateTracker(1, 128, 131000, 20);
                default:
                    return CreateTracker(5, out bruteforcelength);
            }
        }

        private class SingledepthMatchTracker : IMatchtracker
        {
            public SingledepthMatchTracker(int blockinterval, int lookupstart, int windowlength)
            {
                _mInterval = blockinterval;
                int res = lookupstart / blockinterval;
                if (lookupstart > 0)
                {
                    _mPendingValues = new uint[res];
                    _mQueueLength = _mPendingValues.Length * blockinterval;
                }
                else
                    _mQueueLength = 0;
                _mInsertedValues = new uint[windowlength / blockinterval - res];
                _mWindowStart = -(_mInsertedValues.Length + res) * blockinterval - 4;
            }

            public void Reset()
            {
                _mLookupTable.Clear();
                _mRunningValue = 0;

                _mRollingInterval = 0;
                _mWindowStart = -(_mInsertedValues.Length + (_mPendingValues?.Length ?? 0)) * _mInterval - 4;
                _mDataLength = 0;

                _mInitialized = false;
                _mInsertLocation = 0;
                _mPendingOffset = 0;

                // No need to clear the arrays, the values will be ignored by the next time around
            }

            // Current 32 bit value of the last 4 bytes
            private uint _mRunningValue;

            // How often to insert into the table
            private int _mInterval;
            // Avoid division by using a rolling count instead
            private int _mRollingInterval;

            // How many bytes to queue up before adding it to the lookup table
            private int _mQueueLength;

            // Queued up values for future matches
            private uint[] _mPendingValues;
            private int _mPendingOffset;

            // Bytes processed so far
            private int _mDataLength;
            private int _mWindowStart;

            // Four or more bytes read
            private bool _mInitialized;

            // Values values pending removal
            private uint[] _mInsertedValues;
            private int _mInsertLocation;

            // Hash of seen values
            private Dictionary<uint, int> _mLookupTable = new();

            #region IMatchtracker Members

            // Never more than one match with a depth of 1
            public bool Nextmatch(out int where)
            {
                where = 0;
                return false;
            }

            public void Addvalue(byte val)
            {
                if (_mInitialized)
                {
                    _mRollingInterval++;
                    // Time to add a value to the table
                    if (_mRollingInterval == _mInterval)
                    {
                        _mRollingInterval = 0;
                        // Remove a value from the table if the window just rolled past it
                        if (_mWindowStart >= 0)
                        {
                            int idx;
                            if (_mInsertLocation == _mInsertedValues.Length)
                                _mInsertLocation = 0;
                            uint oldval = _mInsertedValues[_mInsertLocation];
                            if (_mLookupTable.TryGetValue(oldval, out idx) && idx == _mWindowStart)
                                _mLookupTable.Remove(oldval);
                        }
                        if (_mPendingValues != null)
                        {
                            // Pop the top of the queue and put it in the table
                            if (_mDataLength > _mQueueLength + 4)
                            {
                                uint poppedval = _mPendingValues[_mPendingOffset];
                                _mInsertedValues[_mInsertLocation] = poppedval;
                                _mInsertLocation++;
                                if (_mInsertLocation > _mInsertedValues.Length)
                                    _mInsertLocation = 0;

                                // Put it into the table
                                _mLookupTable[poppedval] = _mDataLength - _mQueueLength - 4;
                            }
                            // Push the next value onto the queue
                            _mPendingValues[_mPendingOffset] = _mRunningValue;
                            _mPendingOffset++;
                            if (_mPendingOffset == _mPendingValues.Length)
                                _mPendingOffset = 0;
                        }
                        else
                        {
                            // No queue, straight to the dictionary
                            _mInsertedValues[_mInsertLocation] = _mRunningValue;
                            _mInsertLocation++;
                            if (_mInsertLocation > _mInsertedValues.Length)
                                _mInsertLocation = 0;

                            _mLookupTable[_mRunningValue] = _mDataLength - 4;
                        }
                    }
                }
                else
                {
                    _mRollingInterval++;
                    if (_mRollingInterval == _mInterval)
                        _mRollingInterval = 0;
                    _mInitialized = (_mDataLength == 3);
                }

                _mRunningValue = (_mRunningValue << 8) | val;
                _mDataLength++;
                _mWindowStart++;
            }

            public bool FindMatch(out int where)
            {
                return _mLookupTable.TryGetValue(_mRunningValue, out where);
            }

            #endregion
        }

        private class DeepMatchTracker : IMatchtracker
        {
            public DeepMatchTracker(int blockinterval, int lookupstart, int windowlength, int bucketdepth)
            {
                _mInterval = blockinterval;
                int res = lookupstart / blockinterval;
                if (lookupstart > 0)
                {
                    _mPendingValues = new uint[res];
                    _mQueueLength = _mPendingValues.Length * blockinterval;
                }
                else
                    _mQueueLength = 0;
                _mInsertedValues = new uint[windowlength / blockinterval - res];
                _mWindowStart = -(_mInsertedValues.Length + res) * blockinterval - 4;
                _mBucketDepth = bucketdepth;
            }

            public void Reset()
            {
                _mLookupTable.Clear();
                _mRunningValue = 0;

                _mRollingInterval = 0;
                _mWindowStart = -(_mInsertedValues.Length + (_mPendingValues?.Length ?? 0)) * _mInterval - 4;
                _mDataLength = 0;

                _mInitialized = false;
                _mInsertLocation = 0;
                _mPendingOffset = 0;

                _mCurrentMatch = null;

                // No need to clear the arrays, the values will be ignored by the next time around
            }

            private int _mBucketDepth;

            // Current 32 bit value of the last 4 bytes
            private uint _mRunningValue;

            // How often to insert into the table
            private int _mInterval;
            // Avoid division by using a rolling count instead
            private int _mRollingInterval;

            // How many bytes to queue up before adding it to the lookup table
            private int _mQueueLength;

            // Queued up values for future matches
            private uint[] _mPendingValues;
            private int _mPendingOffset;

            // Bytes processed so far
            private int _mDataLength;
            private int _mWindowStart;

            // Four or more bytes read
            private bool _mInitialized;

            // Values values pending removal
            private uint[] _mInsertedValues;
            private int _mInsertLocation;

            // Hash of seen values
            private Dictionary<uint, List<int>> _mLookupTable = new();

            // Save allocating items unnecessarily
            private Stack<List<int>> _mUnusedLists = new();

            private List<int> _mCurrentMatch;
            private int _mCurrentMatchIndex;

            #region IMatchtracker Members

            public void Addvalue(byte val)
            {
                if (_mInitialized)
                {
                    _mRollingInterval++;
                    // Time to add a value to the table
                    if (_mRollingInterval == _mInterval)
                    {
                        _mRollingInterval = 0;
                        // Remove a value from the table if the window just rolled past it
                        if (_mWindowStart > 0)
                        {
                            List<int> locations;
                            if (_mInsertLocation == _mInsertedValues.Length)
                                _mInsertLocation = 0;
                            uint oldval = _mInsertedValues[_mInsertLocation];
                            if (_mLookupTable.TryGetValue(oldval, out locations) && locations[0] == _mWindowStart)
                            {
                                locations.RemoveAt(0);
                                if (locations.Count == 0)
                                {
                                    _mLookupTable.Remove(oldval);
                                    _mUnusedLists.Push(locations);
                                }
                            }
                        }
                        if (_mPendingValues != null)
                        {
                            // Pop the top of the queue and put it in the table
                            if (_mDataLength > _mQueueLength + 4)
                            {
                                uint poppedval = _mPendingValues[_mPendingOffset];
                                _mInsertedValues[_mInsertLocation] = poppedval;
                                _mInsertLocation++;
                                if (_mInsertLocation > _mInsertedValues.Length)
                                    _mInsertLocation = 0;

                                // Put it into the table
                                List<int> locations;
                                if (_mLookupTable.TryGetValue(poppedval, out locations))
                                {
                                    // Check if the bucket runneth over
                                    if (locations.Count == _mBucketDepth)
                                        locations.RemoveAt(0);
                                }
                                else
                                {
                                    // Allocate a new bucket
                                    locations = _mUnusedLists.Count > 0 ? _mUnusedLists.Pop() : new List<int>(1);
                                    _mLookupTable[poppedval] = locations;
                                }
                                locations.Add(_mDataLength - _mQueueLength - 4);
                            }
                            // Push the next value onto the queue
                            _mPendingValues[_mPendingOffset] = _mRunningValue;
                            _mPendingOffset++;
                            if (_mPendingOffset == _mPendingValues.Length)
                                _mPendingOffset = 0;
                        }
                        else
                        {
                            _mInsertedValues[_mInsertLocation] = _mRunningValue;
                            _mInsertLocation++;
                            if (_mInsertLocation > _mInsertedValues.Length)
                                _mInsertLocation = 0;

                            // Put it into the table
                            List<int> locations;
                            if (_mLookupTable.TryGetValue(_mRunningValue, out locations))
                            {
                                // Check if the bucket runneth over
                                if (locations.Count == _mBucketDepth)
                                    locations.RemoveAt(0);
                            }
                            else
                            {
                                // Allocate a new bucket
                                locations = _mUnusedLists.Count > 0 ? _mUnusedLists.Pop() : new List<int>();
                                _mLookupTable[_mRunningValue] = locations;
                            }
                            locations.Add(_mDataLength - 4);
                        }
                    }
                }
                else
                {
                    _mRollingInterval++;
                    if (_mRollingInterval == _mInterval)
                        _mRollingInterval = 0;
                    _mInitialized = (_mDataLength == 3);
                }
                _mRunningValue = (_mRunningValue << 8) | val;
                _mDataLength++;
                _mWindowStart++;
            }

            public bool Nextmatch(out int where)
            {
                if (_mCurrentMatch != null && _mCurrentMatchIndex < _mCurrentMatch.Count)
                {
                    where = _mCurrentMatch[_mCurrentMatchIndex];
                    _mCurrentMatchIndex++;
                    return true;
                }
                where = -1;
                return false;
            }

            public bool FindMatch(out int where)
            {
                if (_mLookupTable.TryGetValue(_mRunningValue, out _mCurrentMatch))
                {
                    _mCurrentMatchIndex = 1;
                    where = _mCurrentMatch[0];
                    return true;
                }
                _mCurrentMatch = null;
                where = -1;
                return false;
            }

            #endregion
        }
    }
}