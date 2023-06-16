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

namespace Regul.S3PI.Interfaces;

/// <summary>
/// Abstract definition of a package
/// </summary>
public abstract class APackage : AApiVersionedFields, IPackage
{
    #region AApiVersionedFields
    /// <summary>
    /// The list of available field names on this API object
    /// </summary>
    public override List<string> ContentFields => GetContentFields(GetType());

    #endregion

    #region IPackage Members

    #region Whole package
    /// <summary>
    /// Tell the package to save itself to wherever it believes it came from
    /// </summary>
    public abstract void SavePackage();

    #endregion

    #region Package header
    /// <summary>
    /// Package header: "DBPF" bytes
    /// </summary>
    [ElementPriority(1)]
    public abstract byte[] Magic { get; }
    /// <summary>
    /// Package header: unused
    /// </summary>
    [ElementPriority(2)]
    public abstract byte[] Unknown1 { get; }
    /// <summary>
    /// Package header: number of entries in the package index
    /// </summary>
    [ElementPriority(3)]
    public abstract int Indexcount { get; }
    /// <summary>
    /// Package header: unused
    /// </summary>
    [ElementPriority(4)]
    public abstract byte[] Unknown2 { get; }
    /// <summary>
    /// Package header: index size on disk in bytes
    /// </summary>
    [ElementPriority(5)]
    public abstract int Indexsize { get; }
    /// <summary>
    /// Package header: unused
    /// </summary>
    [ElementPriority(6)]
    public abstract byte[] Unknown3 { get; }
    /// <summary>
    /// Package header: always 3?
    /// </summary>
    [ElementPriority(7)]
    public abstract int Indexversion { get; }
    /// <summary>
    /// Package header: index position in file
    /// </summary>
    [ElementPriority(8)]
    public abstract int Indexposition { get; }
    /// <summary>
    /// Package header: unused
    /// </summary>
    [ElementPriority(9)]
    public abstract byte[] Unknown4 { get; }

    /// <summary>
    /// A MemoryStream covering the package header bytes
    /// </summary>
    [ElementPriority(10)]
    public abstract Stream HeaderStream { get; }
    #endregion

    #region Package index

    /// <summary>
    /// Package index: the index format in use
    /// </summary>
    [ElementPriority(11)]
    public abstract uint Indextype { get; }

    /// <summary>
    /// Package index: the index
    /// </summary>
    [ElementPriority(12)]
    public abstract List<IResourceIndexEntry> GetResourceList { get; }

    #endregion

    #region Package content

    /// <summary>
    /// Tell the package to delete the resource indexed by <paramref name="rc"/>
    /// </summary>
    /// <param name="rc">Target resource index</param>
    public abstract void DeleteResource(IResourceIndexEntry rc);
    #endregion

    #endregion

    // Static so cannot be defined on the interface

    // Required by API, not user tools

    /// <summary>
    /// Required internally by s3pi - <b>not</b> for use in user tools.
    /// Please use <c>WrapperDealer.GetResource(int, IPackage, IResourceIndexEntry)</c> instead.
    /// </summary>
    /// <param name="rie">IResourceIndexEntry of resource</param>
    /// <returns>The resource data (uncompressed, if necessary)</returns>
    /// <remarks>Used by WrapperDealer to get the data for a resource.</remarks>
    public abstract Stream GetResource(IResourceIndexEntry rie);
}