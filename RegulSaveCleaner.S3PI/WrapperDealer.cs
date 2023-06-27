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

using RegulSaveCleaner.S3PI.Interfaces;
using RegulSaveCleaner.S3PI.Resources;

namespace RegulSaveCleaner.S3PI;

/// <summary>
/// Responsible for associating ResourceType in the IResourceIndexEntry with a particular class (a "wrapper") that understands it
/// or the default wrapper.
/// </summary>
public static class WrapperDealer
{
    /// <summary>
    /// Retrieve a resource from a package, readying the appropriate wrapper or the default wrapper
    /// </summary>
    /// <param name="pkg">Package containing <paramref name="rie"/></param>
    /// <param name="rie">Identifies resource to be returned</param>
    /// <returns>A resource from the package</returns>
    public static IResource GetResource(IPackage pkg, IResourceIndexEntry rie) => new DefaultResource((pkg as APackage)?.GetResource(rie));
}