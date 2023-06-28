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

using System.IO;
namespace RegulSaveCleaner.S3PI.Resources;

/// <summary>
/// A resource wrapper that "does nothing", providing the minimal support required for any resource in a Package.
/// </summary>
public sealed class DefaultResource
{
    #region Attributes
    /// <summary>
    /// Resource data <see cref="System.IO.Stream"/>
    /// </summary>
    private readonly Stream _stream;

    #endregion

    /// <summary>
    /// Create a new instance of the resource.
    /// </summary>
    /// <param name="s">Data _stream to use, or null to create from scratch</param>
    public DefaultResource(Stream s)
    {
        _stream = s ?? new MemoryStream();
    }

    /// <summary>
    /// The resource content as a Stream, with the _stream position set to the begining.
    /// </summary>
    public Stream Stream { get { _stream.Position = 0; return _stream; } }
}