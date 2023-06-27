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
using RegulSaveCleaner.S3PI.Interfaces;

namespace RegulSaveCleaner.S3PI.Package;

/// <summary>
/// Implementation of a package
/// </summary>
public sealed class Package : APackage
{
    #region APackage
    #region Whole package
    /// <summary>
    /// Tell the package to save itself to wherever it believes it came from
    /// </summary>
    public override void SavePackage()
    {
        if (!_packageStream.CanWrite)
            throw new InvalidOperationException("Package is read-only");

        // Lock the header while we save to prevent other processes saving concurrently
        // if it's not a file, it's probably safe not to lock it...
        using FileStream fs = _packageStream as FileStream;
        string tmpfile = Path.GetTempFileName();
        try
        {
            SaveAs(tmpfile);

            BinaryReader r = new(new FileStream(tmpfile, FileMode.Open));
            BinaryWriter w = new(_packageStream);
            _packageStream.Position = 0;
            w.Write(r.ReadBytes((int)r.BaseStream.Length));
            _packageStream.SetLength(_packageStream.Position);
            w.Flush();
            r.Dispose();
        }
        finally
        {
            File.Delete(tmpfile);
        }

        _packageStream.Position = 0;
        _header = new BinaryReader(_packageStream).ReadBytes(_header.Length);

        _index = null;
    }

    /// <summary>
    /// Save the package to a given stream
    /// </summary>
    /// <param name="s">Stream to save to</param>
    public void SaveAs(Stream s)
    {
        BinaryWriter w = new(s);
        w.Write(_header);

        List<uint> lT = new();
        List<uint> lG = new();
        List<uint> lIh = new();
        
        PackageIndex newIndex = new();
        for (int index = 0; index < Index.Count; index++)
        {
            IResourceIndexEntry ie = Index[index];
            
            if (!lT.Contains(ie.ResourceType)) lT.Add(ie.ResourceType);
            if (!lG.Contains(ie.ResourceGroup)) lG.Add(ie.ResourceGroup);
            if (!lIh.Contains((uint)(ie.Instance >> 32))) lIh.Add((uint)(ie.Instance >> 32));
            
            if (ie.IsDeleted) continue;

            ResourceIndexEntry newIe = (ie as ResourceIndexEntry)?.Clone();
            newIndex.Add(newIe);

            byte[] value = PackedChunk(ie as ResourceIndexEntry);

            if (newIe != null)
                newIe.Chunkoffset = (uint)s.Position;

            w.Write(value);
            w.Flush();
        }
        
        uint indexType = (uint)(lIh.Count <= 1 ? 0x04 : 0x00) | (uint)(lG.Count <= 1 ? 0x02 : 0x00) | (uint)(lT.Count <= 1 ? 0x01 : 0x00);
        newIndex.Indextype = indexType;

        long indexpos = s.Position;
        newIndex.Save(w);
        SetIndexcount(w, newIndex.Count);
        SetIndexsize(w, newIndex.Size);
        SetIndexposition(w, (int)indexpos);
        s.Flush();
    }

    /// <summary>
    /// Save the package to a given file
    /// </summary>
    /// <param name="path">File to save to - will be overwritten or created</param>
    public void SaveAs(string path)
    {
        using FileStream fs = new(path, FileMode.Create);
        SaveAs(fs);
        fs.Close();
    }

    // Static so cannot be defined on the interface

    /// <summary>
    /// Open an existing package by filename, read only
    /// </summary>
    /// <param name="packagePath">Fully qualified filename of the package</param>
    /// <returns>IPackage reference to an existing package on disk</returns>
    /// <exception cref="ArgumentNullException"><paramref name="packagePath"/> is null.</exception>
    /// <exception cref="FileNotFoundException">The file cannot be found.</exception>
    /// <exception cref="DirectoryNotFoundException"><paramref name="packagePath"/> is invalid, such as being on an unmapped drive.</exception>
    /// <exception cref="PathTooLongException">
    /// <paramref name="packagePath"/>, or a component of the file name, exceeds the system-defined maximum length.
    /// For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="packagePath"/> is an empty string (""), contains only white space, or contains one or more invalid characters.
    /// <br/>-or-<br/>
    /// <paramref name="packagePath"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// <paramref name="packagePath"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS environment.
    /// </exception>
    /// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
    /// <exception cref="InvalidDataException">Thrown if the package header is malformed.</exception>
    public static IPackage OpenPackage(string packagePath) { return OpenPackage(packagePath, false); }
    /// <summary>
    /// Open an existing package by filename, optionally readwrite
    /// </summary>
    /// <param name="packagePath">Fully qualified filename of the package</param>
    /// <param name="readwrite">True to indicate read/write access required</param>
    /// <returns>IPackage reference to an existing package on disk</returns>
    /// <exception cref="ArgumentNullException"><paramref name="packagePath"/> is null.</exception>
    /// <exception cref="FileNotFoundException">The file cannot be found.</exception>
    /// <exception cref="DirectoryNotFoundException"><paramref name="packagePath"/> is invalid, such as being on an unmapped drive.</exception>
    /// <exception cref="PathTooLongException">
    /// <paramref name="packagePath"/>, or a component of the file name, exceeds the system-defined maximum length.
    /// For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="packagePath"/> is an empty string (""), contains only white space, or contains one or more invalid characters.
    /// <br/>-or-<br/>
    /// <paramref name="packagePath"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// <paramref name="packagePath"/> refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS environment.
    /// </exception>
    /// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
    /// <exception cref="UnauthorizedAccessException">
    /// The access requested is not permitted by the operating system for <paramref name="packagePath"/>,
    /// such as when access is ReadWrite and the file or directory is set for read-only access.
    /// </exception>
    /// <exception cref="InvalidDataException">Thrown if the package header is malformed.</exception>
    public static IPackage OpenPackage(string packagePath, bool readwrite) => new Package(new FileStream(packagePath, FileMode.Open, readwrite ? FileAccess.ReadWrite : FileAccess.Read, FileShare.ReadWrite));

    /// <summary>
    /// Releases any internal references associated with the given package
    /// </summary>
    /// <param name="pkg">IPackage reference to close</param>
    public static void ClosePackage(IPackage pkg)
    {
        if (pkg is Package p)
        {
            if (p._packageStream != null) { try { p._packageStream.Close(); }
                catch
                {
                    // ignored
                }

                p._packageStream = null; }
            p._header = null;
            p._index = null;
        }
    }
    #endregion

    #region Package header

    /// <summary>
    /// Package header: number of entries in the package index
    /// </summary>
    public int Indexcount => BitConverter.ToInt32(_header, 36);

    /// <summary>
    /// Package header: index position in file
    /// </summary>
    public int Indexposition { get { int i = BitConverter.ToInt32(_header, 64); return i != 0 ? i : BitConverter.ToInt32(_header, 40); } }

    #endregion

    #region Package index

    /// <summary>
    /// Package index: the index
    /// </summary>
    public override List<IResourceIndexEntry> GetResourceList => Index;

    /// <summary>
    /// Searches the entire <see cref="IPackage"/>
    /// for the first <see cref="IResourceIndexEntry"/> that matches the conditions defined by
    /// the <c>Predicate&lt;IResourceIndexEntry&gt;</c> <paramref name="match"/>.
    /// </summary>
    /// <param name="match"><c>Predicate&lt;IResourceIndexEntry&gt;</c> defining matching conditions.</param>
    /// <returns>The first matching <see cref="IResourceIndexEntry"/>, if any; otherwise null.</returns>
    /// <remarks>Note that entries marked as deleted will not be returned.</remarks>
    public IResourceIndexEntry Find(Predicate<IResourceIndexEntry> match) { return Index.Find(x => !x.IsDeleted && match(x)); }

    #endregion
    
    #endregion


    #region Package implementation

    private Stream _packageStream;

    private Package(Stream s)
    {
        _packageStream = s;
        s.Position = 0;
        _header = new FastBinaryReader(s).ReadBytes(_header.Length);
    }

    private byte[] PackedChunk(ResourceIndexEntry ie)
    {
        byte[] chunk;
        if (ie.IsDirty)
        {
            Stream res = GetResource(ie);
            FastBinaryReader r = new(res);

            res.Position = 0;
            chunk = r.ReadBytes((int)ie.Memsize);

            byte[] comp = ie.Compressed != 0 ? Compression.CompressStream(chunk) : chunk;
            if (comp.Length < chunk.Length)
                chunk = comp;
        }
        else
        {
            _packageStream.Position = ie.Chunkoffset;
            chunk = new FastBinaryReader(_packageStream).ReadBytes((int)ie.Filesize);
        }
        return chunk;
    }
    #endregion

    #region Header implementation

    byte[] _header = new byte[96];

    void SetIndexcount(BinaryWriter w, int c) { w.BaseStream.Position = 36; w.Write(c); }
    void SetIndexsize(BinaryWriter w, int c) { w.BaseStream.Position = 44; w.Write(c); }
    void SetIndexposition(BinaryWriter w, int c) { w.BaseStream.Position = 40; w.Write(0); w.BaseStream.Position = 64; w.Write(c); }
    #endregion

    #region Index implementation

    private PackageIndex _index;
    private PackageIndex Index
    {
        get
        {
            if (_index != null) return _index;
                
            _index = new PackageIndex(_packageStream, Indexposition, Indexcount);
            return _index;
        }
    }
    
    #endregion


    // Required by API, not user tools

    /// <summary>
    /// Required internally by s3pi - <b>not</b> for use in user tools.
    /// Please use <c>WrapperDealer.GetResource(int, IPackage, IResourceIndexEntry)</c> instead.
    /// </summary>
    /// <param name="rc">IResourceIndexEntry of resource</param>
    /// <returns>The resource data (uncompressed, if necessary)</returns>
    /// <remarks>Used by WrapperDealer to get the data for a resource.</remarks>
    public override Stream GetResource(IResourceIndexEntry rc)
    {
        if (rc is ResourceIndexEntry rie)
        {
            if (rie.ResourceStream != null) return rie.ResourceStream;

            if (rc.Chunkoffset == 0xffffffff) return null;
            _packageStream.Position = rc.Chunkoffset;

            byte[] data;
            if (rc.Filesize == 1 && rc.Memsize == 0xFFFFFFFF) return null;//{ data = new byte[0]; }
            data = rc.Filesize == rc.Memsize ? new BinaryReader(_packageStream).ReadBytes((int)rc.Filesize) : Compression.UncompressStream(_packageStream, (int)rc.Filesize, (int)rc.Memsize);

            MemoryStream ms = new();
            ms.Write(data, 0, data.Length);
            ms.Position = 0;
            return ms;
        }

        return null;
    }
}