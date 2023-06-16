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

using System.Collections.Generic;
using System.IO;

namespace RegulSaveCleaner.S3PI.Interfaces;

/// <summary>
/// Representation of a Sims 3 Package
/// </summary>
public interface IPackage : IContentFields
{
    #region Whole package
    /// <summary>
    /// Tell the package to save itself to wherever it believes it came from
    /// </summary>
    void SavePackage();

    #endregion

    #region Package header
    /// <summary>
    /// Package header: "DBPF" bytes
    /// </summary>
    [ElementPriority(1)]
    byte[] Magic { get; }
    /// <summary>
    /// Package header: unused
    /// </summary>
    [ElementPriority(2)]
    byte[] Unknown1 { get; }
    /// <summary>
    /// Package header: number of entries in the package index
    /// </summary>
    [ElementPriority(3)]
    int Indexcount { get; }
    /// <summary>
    /// Package header: unused
    /// </summary>
    [ElementPriority(4)]
    byte[] Unknown2 { get; }
    /// <summary>
    /// Package header: index size on disk in bytes
    /// </summary>
    [ElementPriority(5)]
    int Indexsize { get; }
    /// <summary>
    /// Package header: unused
    /// </summary>
    [ElementPriority(6)]
    byte[] Unknown3 { get; }
    /// <summary>
    /// Package header: always 3?
    /// </summary>
    [ElementPriority(7)]
    int Indexversion { get; }
    /// <summary>
    /// Package header: index position in file
    /// </summary>
    [ElementPriority(8)]
    int Indexposition { get; }
    /// <summary>
    /// Package header: unused
    /// </summary>
    [ElementPriority(9)]
    byte[] Unknown4 { get; }

    /// <summary>
    /// A <see cref="MemoryStream"/> covering the package header bytes
    /// </summary>
    [ElementPriority(10)]
    Stream HeaderStream { get; }
    #endregion

    #region Package index

    /// <summary>
    /// Package index: the index format in use
    /// </summary>
    [ElementPriority(11)]
    uint Indextype { get; }

    /// <summary>
    /// Package index: the index as a <see cref="IResourceIndexEntry"/> list
    /// </summary>
    [ElementPriority(12)]
    List<IResourceIndexEntry> GetResourceList { get; }

    #endregion
}