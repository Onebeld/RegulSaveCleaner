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
/// Abstract definition of a package
/// </summary>
public abstract class APackage : IPackage
{
    #region IPackage Members

    #region Whole package
    /// <summary>
    /// Tell the package to save itself to wherever it believes it came from
    /// </summary>
    public abstract void SavePackage();

    #endregion

    #region Package index

    /// <summary>
    /// Package index: the index
    /// </summary>
    public abstract List<IResourceIndexEntry> GetResourceList { get; }

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