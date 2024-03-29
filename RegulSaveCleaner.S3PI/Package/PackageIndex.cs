﻿/***************************************************************************
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
using System.Collections.Generic;
using System.IO;

namespace RegulSaveCleaner.S3PI.Package;

/// <summary>
/// Internal -- used by Package to manage the package index
/// </summary>
internal class PackageIndex : List<ResourceIndexEntry>
{
    private const int NumFields = 9;

    public uint Indextype { get; set; }

    int HdrSize
    {
        get
        {
            int hc = 1;
            for (int i = 0; i < sizeof(uint); i++) if ((Indextype & 1 << i) != 0) hc++;
            return hc;
        }
    }

    public PackageIndex() { }

    public PackageIndex(Stream s, int indexPosition, int indexCount)
    {
        if (s == null) return;
        if (indexPosition == 0) return;

        BinaryReader r = new(s);
        s.Position = indexPosition;
        Indextype = r.ReadUInt32();

        int[] hdr = new int[HdrSize];
        int[] entry = new int[NumFields - HdrSize];

        hdr[0] = (int)Indextype;
        for (int i = 1; i < hdr.Length; i++)
            hdr[i] = r.ReadInt32();

        for (int i = 0; i < indexCount; i++)
        {
            for (int j = 0; j < entry.Length; j++)
                entry[j] = r.ReadInt32();
            Add(new ResourceIndexEntry(hdr, entry));
        }
    }

    public int Size => (Count * (NumFields - HdrSize) + HdrSize) * 4;

    public void Save(BinaryWriter w)
    {
        BinaryReader r = Count == 0 ? new BinaryReader(new MemoryStream(new byte[NumFields * 4])) : new BinaryReader(this[0].Stream);
            
        r.BaseStream.Position = 4;
        w.Write(Indextype);
        if ((Indextype & 0x01) != 0) w.Write(r.ReadUInt32()); else r.BaseStream.Position += 4;
        if ((Indextype & 0x02) != 0) w.Write(r.ReadUInt32()); else r.BaseStream.Position += 4;
        if ((Indextype & 0x04) != 0) w.Write(r.ReadUInt32()); else r.BaseStream.Position += 4;

        for (int index = 0; index < Count; index++)
        {
            ResourceIndexEntry ie = this[index];
                
            r = new BinaryReader(ie.Stream);
            r.BaseStream.Position = 4;
            if ((Indextype & 0x01) == 0) w.Write(r.ReadUInt32());
            else r.BaseStream.Position += 4;
            if ((Indextype & 0x02) == 0) w.Write(r.ReadUInt32());
            else r.BaseStream.Position += 4;
            if ((Indextype & 0x04) == 0) w.Write(r.ReadUInt32());
            else r.BaseStream.Position += 4;
            w.Write(r.ReadBytes((int) (ie.Stream.Length - ie.Stream.Position)));
        }
    }
}