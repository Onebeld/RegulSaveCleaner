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
using System.IO;
using RegulSaveCleaner.S3PI.Interfaces;

namespace RegulSaveCleaner.S3PI.Package;

/// <summary>
/// Implementation of an index entry
/// </summary>
public class ResourceIndexEntry : AResourceIndexEntry
{
    #region AResourceIndexEntry
    /// <summary>
    /// The "type" of the resource
    /// </summary>
    public override uint ResourceType
    {
        get => BitConverter.ToUInt32(_indexEntry, 4);
    }
    /// <summary>
    /// The "group" the resource is part of
    /// </summary>
    public override uint ResourceGroup
    {
        get => BitConverter.ToUInt32(_indexEntry, 8);
    }
    /// <summary>
    /// The "instance" number of the resource
    /// </summary>
    public override ulong Instance
    {
        get => ((ulong)BitConverter.ToUInt32(_indexEntry, 12) << 32) | BitConverter.ToUInt32(_indexEntry, 16);
    }
    /// <summary>
    /// If the resource was read from a package, the location in the package the resource was read from
    /// </summary>
    public override uint Chunkoffset
    {
        get => BitConverter.ToUInt32(_indexEntry, 20);
        set { byte[] src = BitConverter.GetBytes(value); Array.Copy(src, 0, _indexEntry, 20, src.Length);
            IsDirty = true;
        }
    }
    /// <summary>
    /// The number of bytes the resource uses within the package
    /// </summary>
    public override uint Filesize
    {
        get => BitConverter.ToUInt32(_indexEntry, 24) & 0x7fffffff;
    }
    /// <summary>
    /// The number of bytes the resource uses in memory
    /// </summary>
    public override uint Memsize
    {
        get => BitConverter.ToUInt32(_indexEntry, 28);
    }
    /// <summary>
    /// 0xFFFF if Filesize != Memsize, else 0x0000
    /// </summary>
    public override ushort Compressed
    {
        get => BitConverter.ToUInt16(_indexEntry, 32);
        set { byte[] src = BitConverter.GetBytes(value); Array.Copy(src, 0, _indexEntry, 32, src.Length); IsDirty = true; }
    }

    /// <summary>
    /// A MemoryStream covering the index entry bytes
    /// </summary>
    public override Stream Stream => new MemoryStream(_indexEntry);

    /// <summary>
    /// True if the index entry has been deleted from the package index
    /// </summary>
    public override bool IsDeleted { get => _isDeleted;
        set { if (_isDeleted != value) { _isDeleted = value; IsDirty = true; } } }

    /// <summary>
    /// Get a copy of this element but with a new change event handler
    /// </summary>
    /// <returns>Return a copy of this element but with a new change event handler</returns>
    public virtual ResourceIndexEntry Clone() { return new ResourceIndexEntry(_indexEntry); }
    #endregion


    #region Implementation
    /// <summary>
    /// The index entry data
    /// </summary>
    readonly byte[] _indexEntry;

    /// <summary>
    /// True if the index entry should be treated as deleted
    /// </summary>
    bool _isDeleted;

    /// <summary>
    /// Create a new index entry as a byte-for-byte copy of <paramref name="indexEntry"/>
    /// </summary>
    /// <param name="indexEntry">The source index entry</param>
    private ResourceIndexEntry(byte[] indexEntry) { _indexEntry = (byte[])indexEntry.Clone(); }

    /// <summary>
    /// Create a new expanded index entry from the header and entry data passed
    /// </summary>
    /// <param name="header">header ints (same for each index entry); [0] is the index type</param>
    /// <param name="entry">entry ints (specific to this entry)</param>
    internal ResourceIndexEntry(int[] header, int[] entry)
    {
        _indexEntry = new byte[(header.Length + entry.Length) * 4];
        MemoryStream ms = new(_indexEntry);
        BinaryWriter w = new(ms);

        w.Write(header[0]);

        int hc = 1;// header[0] is indexType, already written!
        int ec = 0;
        uint ihGt = (uint)header[0];
        w.Write((ihGt & 0x01) != 0 ? header[hc++] : entry[ec++]);
        w.Write((ihGt & 0x02) != 0 ? header[hc++] : entry[ec++]);
        w.Write((ihGt & 0x04) != 0 ? header[hc++] : entry[ec++]);

        for (; hc < header.Length - 1; hc++)
            w.Write(header[hc]);

        for (; ec < entry.Length; ec++)
            w.Write(entry[ec]);
    }

    /// <summary>
    /// The uncompressed resource data associated with this index entry
    /// (used to save having to uncompress the same entry again if it's requested more than once)
    /// Setting the stream updates the Memsize
    /// </summary>
    /// <remarks>Use Package.ReplaceResource() from user code</remarks>
    internal Stream ResourceStream { get; }

    /// <summary>
    /// True if the index entry should be treated as dirty - e.g. the ResourceStream has been replaced
    /// </summary>
    internal bool IsDirty { get; set; }

    #endregion
}