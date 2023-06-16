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

namespace RegulSaveCleaner.S3PI.Extensions;

/// <summary>
/// Provides a look-up from resource type to resource &quot;tag&quot; and file extension.
/// The collection is read-only.
/// </summary>
/// <seealso cref="TGIN"/>
public class ExtList : Dictionary<string, List<string>>
{
    static readonly ExtList E;
    static ExtList() { E = new ExtList(); }
    /// <summary>
    /// A look-up from resource type to resource &quot;tag&quot; and file extension.
    /// The collection is read-only.
    /// </summary>
    public static ExtList Ext => E;

    ExtList()
    {
        using (StringReader sr = new(Properties.Resources.Extensions))
        {
            string s;
            while ((s = sr.ReadLine()) != null)
            {
                if (s.StartsWith(";")) continue;
                List<string> t = new(s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                if (t.Count < 2) continue;
                string t0 = t[0];
                t.RemoveAt(0);
                Add(t0, t);
            }
        }
    }

    /// <summary>
    /// Gets or sets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get or set.</param>
    /// <returns>The value associated with the specified key.
    /// If the specified key is not found, returns a default value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
    /// <exception cref="InvalidOperationException">An attempt was made to set a value.</exception>
    public new List<string> this[string key]
    {
        get => ContainsKey(key) ? base[key] : base["*"];
        set => throw new InvalidOperationException();
    }
}