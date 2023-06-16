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

namespace RegulSaveCleaner.S3PI.Interfaces;

/// <summary>
/// An implementation of AResourceKey that supports storing the Type, Group and Instance in any order.
/// </summary>
/// <remarks>An explicit implementation of <see cref="IEquatable{TGIBlock}"/> is required by
/// <see cref="CountedTgiBlockList"/> and <see cref="TGIBlockList"/>.</remarks>
public class TGIBlock : AResourceKey, IEquatable<TGIBlock>
{
    #region Attributes
    string _order = "TGI";
    #endregion

    #region Constructors

    /// <summary>
    /// Initialize a new TGIBlock
    /// with the default order ("TGI") and specified values.
    /// </summary>
    /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
    /// <param name="resourceType">The resource type value.</param>
    /// <param name="resourceGroup">The resource group value.</param>
    /// <param name="instance">The resource instance value.</param>
    public TGIBlock(EventHandler handler, uint resourceType, uint resourceGroup, ulong instance)
        : base(handler, resourceType, resourceGroup, instance) { }

    #endregion

    #region Data I/O

    #endregion

    #region AHandlerElement
    /// <summary>
    /// The list of available field names on this API object
    /// </summary>
    public override List<string> ContentFields => GetContentFields(GetType());

    // /// <summary>
    // /// Get a copy of the <see cref="TGIBlock"/> but with a new change <see cref="EventHandler"/>.
    // /// </summary>
    // /// <param name="handler">The replacement <see cref="EventHandler"/> delegate.</param>
    // /// <returns>Return a copy of the <see cref="TGIBlock"/> but with a new change <see cref="EventHandler"/>.</returns>
    // public override AHandlerElement Clone(EventHandler handler) { return new TGIBlock(requestedApiVersion, handler, this); }
    #endregion

    #region IEquatable<TGIBlock> Members

    /// <summary>
    /// Indicates whether the current <see cref="TGIBlock"/> instance is equal to another <see cref="TGIBlock"/> instance.
    /// </summary>
    /// <param name="other">An <see cref="TGIBlock"/> instance to compare with this instance.</param>
    /// <returns>true if the current instance is equal to the <paramref name="other"/> parameter; otherwise, false.</returns>
    public bool Equals(TGIBlock other) { return base.Equals(other); }

    #endregion

    #region Content Fields

    #endregion
}