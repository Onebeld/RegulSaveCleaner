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

namespace RegulSaveCleaner.S3PI.Interfaces
{
    public abstract class AResourceKey
    {
        #region Attributes
        /// <summary>
        /// The "type" of the resource
        /// </summary>
        private readonly uint resourceType;
        /// <summary>
        /// The "group" the resource is part of
        /// </summary>
        private readonly uint resourceGroup;
        /// <summary>
        /// The "instance" number of the resource
        /// </summary>
        private readonly ulong instance;
        #endregion

        /// <summary>
        /// Initialize a new instance
        /// </summary>
        public AResourceKey() { }

        #region IResourceKey Members
        /// <summary>
        /// The "type" of the resource
        /// </summary>
        public virtual uint ResourceType { get => resourceType; }
        /// <summary>
        /// The "group" the resource is part of
        /// </summary>
        public virtual uint ResourceGroup { get => resourceGroup; }
        /// <summary>
        /// The "instance" number of the resource
        /// </summary>
        public virtual ulong Instance { get => instance; }
        #endregion

        /// <summary>
        /// Converts an <see cref="AResourceKey"/> to a string representation.
        /// </summary>
        /// <param name="value">The <see cref="AResourceKey"/> to convert</param>
        /// <returns>The 42 character string representation of this resource key,
        /// of the form 0xXXXXXXXX-0xXXXXXXXX-0xXXXXXXXXXXXXXXXX.</returns>
        public static implicit operator string(AResourceKey value) { return
            $"0x{value.ResourceType:X8}-0x{value.ResourceGroup:X8}-0x{value.Instance:X16}"; }
        /// <summary>
        /// Returns a string representation of this <see cref="AResourceKey"/>.
        /// </summary>
        /// <returns>The 42 character string representation of this resource key,
        /// of the form 0xXXXXXXXX-0xXXXXXXXX-0xXXXXXXXXXXXXXXXX.</returns>
        public override string ToString() => this;
    }
}
