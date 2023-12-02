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
        public static byte[] UncompressStream(Stream stream, int fileSize, int memSize)
        {
            BinaryReader r = new(stream);
            long end = stream.Position + fileSize;

            byte[] uncData = new byte[memSize];
            BinaryWriter bw = new(new MemoryStream(uncData));

            byte[] data = r.ReadBytes(2);

            int dataLen = (data[0] & 0x80) != 0 ? 4 : 3 * ((data[0] & 0x01) != 0 ? 2 : 1);
            data = r.ReadBytes(dataLen);

            long realSize = 0;
            for (int i = 0; i < data.Length; i++) realSize = (realSize << 8) + data[i];

            while (stream.Position < end) { Dechunk(stream, bw); }

            bw.Close();

            return uncData;
        }

        private static void Dechunk(Stream stream, BinaryWriter bw)
        {
            BinaryReader r = new(stream);
            int copySize = 0;
            int copyOffset = 0;
            int dataLen;
            byte[] data;

            byte packing = r.ReadByte();

            #region Compressed
            switch (packing)
            {
                // 0.......; new data 3; copy data 10 (min 3); offset 1024
                case < 0x80:
                    data = r.ReadBytes(1);
                    dataLen = packing & 0x03;
                    copySize = (packing >> 2 & 0x07) + 3;
                    copyOffset = (packing << 3 & 0x300 | data[0]) + 1;
                    break;
                // 10......; new data 3; copy data 67 (min 4); offset 16384
                case < 0xC0:
                    data = r.ReadBytes(2);
                    dataLen = data[0] >> 6 & 0x03;
                    copySize = (packing & 0x3F) + 4;
                    copyOffset = (data[0] << 8 & 0x3F00 | data[1]) + 1;
                    break;
                // 110.....; new data 3; copy data 1028 (min 5); offset 131072
                case < 0xE0:
                    data = r.ReadBytes(3);
                    dataLen = packing & 0x03;
                    copySize = (packing << 6 & 0x300 | data[2]) + 5;
                    copyOffset = (packing << 12 & 0x10000 | data[0] << 8 | data[1]) + 1;
                    break;
                // 1110000 - 11101111; new data 4-128
                case < 0xFC:
                    dataLen = (packing & 0x1F) + 1 << 2;
                    break;
                // 111111..; new data 3
                default:
                    dataLen = packing & 0x03;
                    break;
            }
            #endregion

            if (dataLen > 0)
            {
                data = r.ReadBytes(dataLen);
                bw.Write(data);
            }

            if (copySize < copyOffset && copyOffset > 8) CopyA(bw.BaseStream, copyOffset, copySize); else CopyB(bw.BaseStream, copyOffset, copySize);
        }

        private static void CopyA(Stream s, int offset, int len)
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

        private static void CopyB(Stream s, int offset, int len)
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
        private DbpfCompression(int level)
        {
            _mTracker = CreateTracker(level, out _mBruteForceLength);
        }

        private int _mBruteForceLength;
        private IMatchTracker _mTracker;

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

        private byte[] Compress(byte[] data)
        {
            bool endIsValid = false;
            List<byte[]> compressedChunks = new();
            int compressedIdx = 0;
            int compressedLen = 0;

            if (data.Length < 16 || data.LongLength > uint.MaxValue)
                return null;

            _mData = data;

            try
            {
                int lastByteStored = 0;

                while (compressedIdx < data.Length)
                {
                    if (data.Length - compressedIdx < 4)
                    {
                        // Just copy the rest
                        byte[] chunk = new byte[data.Length - compressedIdx + 1];
                        chunk[0] = (byte)(0xFC | data.Length - compressedIdx);
                        Array.Copy(data, compressedIdx, chunk, 1, data.Length - compressedIdx);
                        compressedChunks.Add(chunk);
                        compressedIdx += chunk.Length - 1;
                        compressedLen += chunk.Length;

                        endIsValid = true;
                        continue;
                    }

                    while (compressedIdx > lastByteStored - 3)
                        _mTracker.AddValue(data[lastByteStored++]);

                    // Search ahead in blocks of 4 bytes for a match until one is found
                    // Record the best match if multiple are found
                    _mSequenceSource = _mSequenceLength = 0;
                    _mSequenceDest = int.MaxValue;
                    _mSequenceFound = false;
                    do
                    {
                        for (int loop = 0; loop < 4; loop++)
                        {
                            if (lastByteStored < data.Length)
                                _mTracker.AddValue(data[lastByteStored++]);
                            FindSequence(lastByteStored - 4);
                        }
                    }
                    while (!_mSequenceFound && lastByteStored + 4 <= data.Length);

                    if (!_mSequenceFound)
                        _mSequenceDest = _mData.Length;

                    // If the next match is more than four bytes away, put in codes to read uncompressed data
                    while (_mSequenceDest - compressedIdx >= 4)
                    {
                        int toCopy = _mSequenceDest - compressedIdx & ~3;
                        if (toCopy > 112) toCopy = 112;

                        byte[] chunk = new byte[toCopy + 1];
                        chunk[0] = (byte)(0xE0 | (toCopy >> 2) - 1);
                        Array.Copy(data, compressedIdx, chunk, 1, toCopy);
                        compressedChunks.Add(chunk);
                        compressedIdx += toCopy;
                        compressedLen += chunk.Length;
                    }

                    if (_mSequenceFound)
                    {
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
                        while (_mSequenceLength > 0)
                        {
                            int thisLength = _mSequenceLength;
                            if (thisLength > 1028)
                                thisLength = 1028;
                            _mSequenceLength -= thisLength;

                            int offset = _mSequenceDest - _mSequenceSource - 1;
                            int readBytes = _mSequenceDest - compressedIdx;

                            _mSequenceSource += thisLength;
                            _mSequenceDest += thisLength;

                            byte[] chunk;

                            if (thisLength > 67 || offset > 16383)
                            {
                                chunk = new byte[readBytes + 4];
                                chunk[0] = (byte)(0xC0 | readBytes | thisLength - 5 >> 6 & 0x0C | offset >> 12 & 0x10);
                                chunk[1] = (byte)(offset >> 8 & 0xFF);
                                chunk[2] = (byte)(offset & 0xFF);
                                chunk[3] = (byte)(thisLength - 5 & 0xFF);
                            }
                            else if (thisLength > 10 || offset > 1023)
                            {
                                chunk = new byte[readBytes + 3];
                                chunk[0] = (byte)(0x80 | thisLength - 4 & 0x3F);
                                chunk[1] = (byte)(readBytes << 6 & 0xC0 | offset >> 8 & 0x3F);
                                chunk[2] = (byte)(offset & 0xFF);
                            }
                            else
                            {
                                chunk = new byte[readBytes + 2];
                                chunk[0] = (byte)(readBytes & 0x3 | thisLength - 3 << 2 & 0x1C | offset >> 3 & 0x60);
                                chunk[1] = (byte)(offset & 0xFF);
                            }

                            if (readBytes > 0)
                                Array.Copy(data, compressedIdx, chunk, chunk.Length - readBytes, readBytes);

                            compressedChunks.Add(chunk);
                            compressedIdx += thisLength + readBytes;
                            compressedLen += chunk.Length;
                        }
                    }
                }

                if (compressedLen + 6 < data.Length)
                {
                    int chunkPos;
                    byte[] compressed;

                    if (data.Length > 0xFFFFFF)
                    {
                        // Activate the large data bit for > 16mb uncompressed data
                        compressed = new byte[compressedLen + 6 + (endIsValid ? 0 : 1)];
                        compressed[0] = 0x10 | 0x80; // 0x80 = length is 4 bytes
                        compressed[1] = 0xFB;
                        compressed[2] = (byte)(data.Length >> 24);
                        compressed[3] = (byte)(data.Length >> 16);
                        compressed[4] = (byte)(data.Length >> 8);
                        compressed[5] = (byte)data.Length;
                        chunkPos = 6;
                    }
                    else
                    {
                        compressed = new byte[compressedLen + 5 + (endIsValid ? 0 : 1)];
                        compressed[0] = 0x10;
                        compressed[1] = 0xFB;
                        compressed[2] = (byte)(data.Length >> 16);
                        compressed[3] = (byte)(data.Length >> 8);
                        compressed[4] = (byte)data.Length;
                        chunkPos = 5;
                    }

                    for (int loop = 0; loop < compressedChunks.Count; loop++)
                    {
                        Array.Copy(compressedChunks[loop], 0, compressed, chunkPos, compressedChunks[loop].Length);
                        chunkPos += compressedChunks[loop].Length;
                    }
                    if (!endIsValid)
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

        private void FindSequence(int startIndex)
        {
            // Try a straight forward brute force first
            int end = -_mBruteForceLength;
            if (startIndex < _mBruteForceLength)
                end = -startIndex;

            byte searchForByte = _mData[startIndex];

            for (int loop = -1; loop >= end && _mSequenceLength < 1028; loop--)
            {
                byte curByte = _mData[loop + startIndex];
                if (curByte != searchForByte)
                    continue;

                int len = FindRunLength(startIndex + loop, startIndex);

                if (len <= _mSequenceLength
                    || len < 3
                    || len < 4 && loop <= -1024
                    || len < 5 && loop <= -16384)
                    continue;

                _mSequenceFound = true;
                _mSequenceSource = startIndex + loop;
                _mSequenceLength = len;
                _mSequenceDest = startIndex;
            }

            // Use the look-up table next
            if (_mSequenceLength < 1028 && _mTracker.FindMatch(out int matchLoc))
            {
                do
                {
                    int len = FindRunLength(matchLoc, startIndex);
                    if (len < 5)
                        continue;

                    _mSequenceFound = true;
                    _mSequenceSource = matchLoc;
                    _mSequenceLength = len;
                    _mSequenceDest = startIndex;
                }
                while (_mSequenceLength < 1028 && _mTracker.NextMatch(out matchLoc));
            }
        }

        private int FindRunLength(int src, int dst)
        {
            int endSrc = src + 1;
            int endDst = dst + 1;
            while (endDst < _mData.Length && _mData[endSrc] == _mData[endDst] && endDst - dst < 1028)
            {
                endSrc++;
                endDst++;
            }

            return endDst - dst;
        }

        private interface IMatchTracker
        {
            bool FindMatch(out int where);
            bool NextMatch(out int where);
            void AddValue(byte val);
            void Reset();
        }

        private static IMatchTracker CreateTracker(int blockInterval, int lookupStart, int windowLength, int bucketDepth)
        {
            if (bucketDepth <= 1)
                return new SingleDepthMatchTracker(blockInterval, lookupStart, windowLength);
            return new DeepMatchTracker(blockInterval, lookupStart, windowLength, bucketDepth);
        }

        private static IMatchTracker CreateTracker(int level, out int bruteforceLength)
        {
            while (true)
            {
                switch (level)
                {
                    case 0:
                        bruteforceLength = 0;
                        return CreateTracker(4, 0, 16384, 1);
                    case 1:
                        bruteforceLength = 0;
                        return CreateTracker(2, 0, 32768, 1);
                    case 2:
                        bruteforceLength = 0;
                        return CreateTracker(1, 0, 65536, 1);
                    case 3:
                        bruteforceLength = 0;
                        return CreateTracker(1, 0, 131000, 2);
                    case 4:
                        bruteforceLength = 16;
                        return CreateTracker(1, 16, 131000, 2);
                    case 5:
                        bruteforceLength = 16;
                        return CreateTracker(1, 16, 131000, 5);
                    case 6:
                        bruteforceLength = 32;
                        return CreateTracker(1, 32, 131000, 5);
                    case 7:
                        bruteforceLength = 32;
                        return CreateTracker(1, 32, 131000, 10);
                    case 8:
                        bruteforceLength = 64;
                        return CreateTracker(1, 64, 131000, 10);
                    case 9:
                        bruteforceLength = 128;
                        return CreateTracker(1, 128, 131000, 20);
                    default:
                        level = 5;
                        continue;
                }
            }
        }

        private class SingleDepthMatchTracker : IMatchTracker
        {
            public SingleDepthMatchTracker(int blockInterval, int lookupStart, int windowLength)
            {
                _mInterval = blockInterval;
                int res = lookupStart / blockInterval;
                if (lookupStart > 0)
                {
                    _mPendingValues = new uint[res];
                    _mQueueLength = _mPendingValues.Length * blockInterval;
                }
                else
                    _mQueueLength = 0;
                _mInsertedValues = new uint[windowLength / blockInterval - res];
                _mWindowStart = -(_mInsertedValues.Length + res) * blockInterval - 4;
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
            private readonly int _mInterval;
            // Avoid division by using a rolling count instead
            private int _mRollingInterval;

            // How many bytes to queue up before adding it to the lookup table
            private readonly int _mQueueLength;

            // Queued up values for future matches
            private readonly uint[] _mPendingValues;
            private int _mPendingOffset;

            // Bytes processed so far
            private int _mDataLength;
            private int _mWindowStart;

            // Four or more bytes read
            private bool _mInitialized;

            // Values values pending removal
            private readonly uint[] _mInsertedValues;
            private int _mInsertLocation;

            // Hash of seen values
            private readonly Dictionary<uint, int> _mLookupTable = new();

            #region IMatchtracker Members

            // Never more than one match with a depth of 1
            public bool NextMatch(out int where)
            {
                where = 0;
                return false;
            }

            public void AddValue(byte val)
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
                            uint oldVal = _mInsertedValues[_mInsertLocation];
                            if (_mLookupTable.TryGetValue(oldVal, out idx) && idx == _mWindowStart)
                                _mLookupTable.Remove(oldVal);
                        }
                        if (_mPendingValues != null)
                        {
                            // Pop the top of the queue and put it in the table
                            if (_mDataLength > _mQueueLength + 4)
                            {
                                uint poppedVal = _mPendingValues[_mPendingOffset];
                                _mInsertedValues[_mInsertLocation] = poppedVal;
                                _mInsertLocation++;
                                if (_mInsertLocation > _mInsertedValues.Length)
                                    _mInsertLocation = 0;

                                // Put it into the table
                                _mLookupTable[poppedVal] = _mDataLength - _mQueueLength - 4;
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
                    _mInitialized = _mDataLength == 3;
                }

                _mRunningValue = _mRunningValue << 8 | val;
                _mDataLength++;
                _mWindowStart++;
            }

            public bool FindMatch(out int where)
            {
                return _mLookupTable.TryGetValue(_mRunningValue, out where);
            }

            #endregion
        }

        private class DeepMatchTracker : IMatchTracker
        {
            public DeepMatchTracker(int blockInterval, int lookupStart, int windowLength, int bucketDepth)
            {
                _mInterval = blockInterval;
                int res = lookupStart / blockInterval;
                if (lookupStart > 0)
                {
                    _mPendingValues = new uint[res];
                    _mQueueLength = _mPendingValues.Length * blockInterval;
                }
                else
                    _mQueueLength = 0;
                _mInsertedValues = new uint[windowLength / blockInterval - res];
                _mWindowStart = -(_mInsertedValues.Length + res) * blockInterval - 4;
                _mBucketDepth = bucketDepth;
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

            public void AddValue(byte val)
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
                            if (_mInsertLocation == _mInsertedValues.Length)
                                _mInsertLocation = 0;
                            uint oldVal = _mInsertedValues[_mInsertLocation];
                            if (_mLookupTable.TryGetValue(oldVal, out List<int> locations) && locations[0] == _mWindowStart)
                            {
                                locations.RemoveAt(0);
                                if (locations.Count == 0)
                                {
                                    _mLookupTable.Remove(oldVal);
                                    _mUnusedLists.Push(locations);
                                }
                            }
                        }
                        if (_mPendingValues != null)
                        {
                            // Pop the top of the queue and put it in the table
                            if (_mDataLength > _mQueueLength + 4)
                            {
                                uint poppedVal = _mPendingValues[_mPendingOffset];
                                _mInsertedValues[_mInsertLocation] = poppedVal;
                                _mInsertLocation++;
                                if (_mInsertLocation > _mInsertedValues.Length)
                                    _mInsertLocation = 0;

                                // Put it into the table
                                if (_mLookupTable.TryGetValue(poppedVal, out List<int> locations))
                                {
                                    // Check if the bucket runneth over
                                    if (locations.Count == _mBucketDepth)
                                        locations.RemoveAt(0);
                                }
                                else
                                {
                                    // Allocate a new bucket
                                    locations = _mUnusedLists.Count > 0 ? _mUnusedLists.Pop() : new List<int>(1);
                                    _mLookupTable[poppedVal] = locations;
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
                            if (_mLookupTable.TryGetValue(_mRunningValue, out List<int> locations))
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
                    _mInitialized = _mDataLength == 3;
                }
                _mRunningValue = _mRunningValue << 8 | val;
                _mDataLength++;
                _mWindowStart++;
            }

            public bool NextMatch(out int where)
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