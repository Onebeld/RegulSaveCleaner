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

namespace RegulSaveCleaner.S3PI.Interfaces
{
    /// <summary>
    /// A resource contained in a package.
    /// </summary>
    public abstract class AResource : IResource
    {
        #region Attributes
        /// <summary>
        /// Resource data <see cref="System.IO.Stream"/>
        /// </summary>
        protected Stream stream;

        /// <summary>
        /// Indicates the resource stream may no longer reflect the resource content
        /// </summary>
        protected bool dirty;
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the resource
        /// </summary>
        /// <param name="s"><see cref="System.IO.Stream"/> to use, or null to create from scratch.</param>
        protected AResource(Stream s)
        {
            stream = s;
        }
        #endregion

        #region IResource Members
        /// <summary>
        /// The resource content as a <see cref="System.IO.Stream"/>.
        /// </summary>
        public virtual Stream Stream
        {
            get
            {
                if (dirty)
                {
                    stream = UnParse();
                    dirty = false;
                    //Console.WriteLine(this.GetType().Name + " flushed.");
                }
                stream.Position = 0;
                return stream;
            }
        }

        #endregion

        /// <summary>
        /// AResource classes must supply an <see cref="UnParse()"/> method that serializes the class to a <see cref="System.IO.Stream"/> that is returned.
        /// </summary>
        /// <returns><see cref="System.IO.Stream"/> containing serialized class data.</returns>
        protected abstract Stream UnParse();
    }
}
