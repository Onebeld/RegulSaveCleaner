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

namespace Regul.S3PI.Interfaces
{
    /// <summary>
    /// An abstract class, extending <see cref="AHandlerElement"/> and implementing <see cref="IResourceKey"/>.
    /// </summary>
    public abstract class AResourceKey : AHandlerElement, IResourceKey
    {
        #region Attributes
        /// <summary>
        /// The "type" of the resource
        /// </summary>
        protected uint resourceType;
        /// <summary>
        /// The "group" the resource is part of
        /// </summary>
        protected uint resourceGroup;
        /// <summary>
        /// The "instance" number of the resource
        /// </summary>
        protected ulong instance;
        #endregion

        /// <summary>
        /// Initialize a new instance
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AResourceKey"/> changes.</param>
        public AResourceKey(EventHandler handler) : base(handler) { }

        /// <summary>
        /// Initialize a new instance
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AResourceKey"/> changes.</param>
        /// <param name="resourceType">The type of the resource.</param>
        /// <param name="resourceGroup">The group of the resource.</param>
        /// <param name="instance">The instance of the resource.</param>
        public AResourceKey(EventHandler handler, uint resourceType, uint resourceGroup, ulong instance)
            : base(handler)
        {
            this.resourceType = resourceType;
            this.resourceGroup = resourceGroup;
            this.instance = instance;
        }

        #region IResourceKey Members
        /// <summary>
        /// The "type" of the resource
        /// </summary>
        [ElementPriority(1)]
        public virtual uint ResourceType { get => resourceType;
            set { if (resourceType != value) { resourceType = value; OnElementChanged(); } } }
        /// <summary>
        /// The "group" the resource is part of
        /// </summary>
        [ElementPriority(2)]
        public virtual uint ResourceGroup { get => resourceGroup;
            set { if (resourceGroup != value) { resourceGroup = value; OnElementChanged(); } } }
        /// <summary>
        /// The "instance" number of the resource
        /// </summary>
        [ElementPriority(3)]
        public virtual ulong Instance { get => instance;
            set { if (instance != value) { instance = value; OnElementChanged(); } } }
        #endregion

        #region IEqualityComparer<IResourceKey> Members

        /// <summary>
        /// Determines whether the specified <see cref="IResourceKey"/> instances are equal.
        /// </summary>
        /// <param name="x">The first <see cref="IResourceKey"/> to compare.</param>
        /// <param name="y">The second <see cref="IResourceKey"/> to compare.</param>
        /// <returns>true if the specified <see cref="IResourceKey"/> instances are equal; otherwise, false.</returns>
        public bool Equals(IResourceKey x, IResourceKey y) { return x.Equals(y); }

        /// <summary>
        /// Returns a hash code for the specified <see cref="IResourceKey"/>.
        /// </summary>
        /// <param name="obj">The <see cref="IResourceKey"/> for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified object.</returns>
        /// <exception cref="ArgumentNullException">The type of <paramref name="obj"/> is a reference type and
        /// <paramref name="obj"/> is null.</exception>
        /// <seealso cref="GetHashCode()"/>
        public int GetHashCode(IResourceKey obj) => obj.GetHashCode();

        /// <summary>
        /// Serves as a hash function for an <see cref="AResourceKey"/>.
        /// </summary>
        /// <returns>A hash code for the current <see cref="AResourceKey"/>.</returns>
        public override int GetHashCode() { return ResourceType.GetHashCode() ^ ResourceGroup.GetHashCode() ^ Instance.GetHashCode(); }

        #endregion

        #region IEquatable<IResourceKey> Members

        /// <summary>
        /// Indicates whether the current <see cref="AResourceKey"/> instance is equal to another <see cref="IResourceKey"/> instance.
        /// </summary>
        /// <param name="other">An <see cref="IResourceKey"/> instance to compare with this instance.</param>
        /// <returns>true if the current instance is equal to the <paramref name="other"/> parameter; otherwise, false.</returns>
        public bool Equals(IResourceKey other) { return CompareTo(other) == 0; }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="AResourceKey"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="AResourceKey"/>.</param>
        /// <returns>true if the specified <see cref="object"/> is equal to the current <see cref="AResourceKey"/>; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj is AResourceKey key && Equals(key);
        }

        #endregion

        #region IComparable<IResourceKey> Members

        /// <summary>
        /// Compare this <see cref="AResourceKey"/> to another <see cref="IResourceKey"/> for sort order purposes
        /// </summary>
        /// <param name="other">Target <see cref="IResourceKey"/></param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.  The return value has these meanings:
        /// <table>
        /// <thead><tr><td><strong>Value</strong></td><td><strong>Meaning</strong></td></tr></thead>
        /// <tbody>
        /// <tr><td>Less than zero</td><td>This instance is less than <paramref name="other"/>.</td></tr>
        /// <tr><td>Zero</td><td>This instance is equal to <paramref name="other"/>.</td></tr>
        /// <tr><td>Greater than zero</td><td>This instance is greater than <paramref name="other"/>.</td></tr>
        /// </tbody>
        /// </table>
        /// </returns>
        /// <exception cref="NotImplementedException">Either this object's Type or the target's is not comparable</exception>
        /// <exception cref="ArgumentException">The target is not comparable with this object</exception>
        public int CompareTo(IResourceKey other)
        {
            int res = ResourceType.CompareTo(other.ResourceType); if (res != 0) return res;
            res = ResourceGroup.CompareTo(other.ResourceGroup); if (res != 0) return res;
            return Instance.CompareTo(other.Instance);
        }

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
        public override string ToString() { return this; }
    }
}
