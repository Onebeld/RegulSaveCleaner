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
using System.IO;
using Regul.S3PI.Interfaces;

namespace Regul.S3PI.Resources
{
    /// <summary>
    /// A resource wrapper that "does nothing", providing the minimal support required for any resource in a Package.
    /// </summary>
    public sealed class DefaultResource : AResource
    {
        /// <summary>
        /// Create a new instance of the resource.
        /// </summary>
        /// <param name="s">Data stream to use, or null to create from scratch</param>
        public DefaultResource(Stream s) : base(s) { if (stream == null) { stream = new MemoryStream(); dirty = true; } }

        /// <summary>
        /// <see cref="DefaultResource"/> does not know how to parse anything, so this method is unimplemented.
        /// </summary>
        /// <returns>Throws <see cref="NotImplementedException"/>.</returns>
        /// <exception cref="NotImplementedException">Thrown if called.</exception>
        protected override Stream UnParse() { throw new NotImplementedException(); }

        /// <summary>
        /// The resource content as a Stream, with the stream position set to the begining.
        /// </summary>
        public override Stream Stream { get { stream.Position = 0; return stream; } }
    }

    /// <summary>
    /// ResourceHandler for DefaultResource wrapper
    /// </summary>
    public class DefaultResourceHandler : AResourceHandler
    {
        /// <summary>
        /// Populate the <see cref="AResourceHandler"/> Dictionary.
        /// <para>&quot;*&quot; is used to inform WrapperDealer this will happily wrap any resource.</para>
        /// </summary>
        public DefaultResourceHandler()
        {
            Add(typeof(DefaultResource), new List<string>(new[] { "*" }));
        }
    }
}
