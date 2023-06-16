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
using Regul.S3PI.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using Regul.S3PI.Resources;

namespace Regul.S3PI;

/// <summary>
/// Responsible for associating ResourceType in the IResourceIndexEntry with a particular class (a "wrapper") that understands it
/// or the default wrapper.
/// </summary>
public static class WrapperDealer
{
    private static readonly Type[] ResourceTypes =
    {
        typeof(DefaultResourceHandler)
    };

    /// <summary>
    /// Create a new Resource of the requested type, allowing the wrapper to initialise it appropriately
    /// </summary>
    /// <param name="resourceType">Type of resource (currently a string like "0xDEADBEEF")</param>
    /// <returns></returns>
    public static IResource CreateNewResource(string resourceType)
    {
        if (_typeMap == null)
        {
            _typeMap = new List<KeyValuePair<string, Type>>();
            try
            {
                for (int index = 0; index < ResourceTypes.Length; index++)
                {
                    Type t = ResourceTypes[index];

                    AResourceHandler arh =
                        (AResourceHandler)t.GetConstructor(Type.EmptyTypes)?.Invoke(Array.Empty<object>());

                    if (arh == null) continue;

                    foreach (Type k in arh.Keys)
                    {
                        for (int i = 0; i < arh[k].Count; i++)
                        {
                            string s = arh[k][i];
                            _typeMap.Add(new KeyValuePair<string, Type>(s, k));
                        }
                    }
                }
            }
            catch { }

            _typeMap.Sort((x, y) => string.Compare(x.Key, y.Key, StringComparison.Ordinal));
        }

        return WrapperForType(resourceType, null);
    }


    /// <summary>
    /// Retrieve a resource from a package, readying the appropriate wrapper
    /// </summary>
    /// <param name="pkg">Package containing <paramref name="rie"/></param>
    /// <param name="rie">Identifies resource to be returned</param>
    /// <returns>A resource from the package</returns>
    public static IResource GetResource(IPackage pkg, IResourceIndexEntry rie) => GetResource(pkg, rie, false);


    /// <summary>
    /// Retrieve a resource from a package, readying the appropriate wrapper or the default wrapper
    /// </summary>
    /// <param name="pkg">Package containing <paramref name="rie"/></param>
    /// <param name="rie">Identifies resource to be returned</param>
    /// <param name="alwaysDefault">When true, indicates WrapperDealer should always use the DefaultResource wrapper</param>
    /// <returns>A resource from the package</returns>
    public static IResource GetResource(IPackage pkg, IResourceIndexEntry rie, bool alwaysDefault)
    {
        if (_typeMap == null)
        {
            _typeMap = new List<KeyValuePair<string, Type>>();
            try
            {
                for (int index = 0; index < ResourceTypes.Length; index++)
                {
                    Type t = ResourceTypes[index];

                    AResourceHandler arh =
                        (AResourceHandler)t.GetConstructor(Type.EmptyTypes)?.Invoke(Array.Empty<object>());

                    if (arh == null) continue;

                    foreach (Type k in arh.Keys)
                    {
                        for (int i = 0; i < arh[k].Count; i++)
                        {
                            string s = arh[k][i];
                            _typeMap.Add(new KeyValuePair<string, Type>(s, k));
                        }
                    }
                }
            }
            catch { }

            _typeMap.Sort((x, y) => string.Compare(x.Key, y.Key, StringComparison.Ordinal));
        }

        return WrapperForType(alwaysDefault ? "*" : rie["ResourceType"], (pkg as APackage)?.GetResource(rie));
    }

    /// <summary>
    /// Retrieve the resource wrappers known to WrapperDealer.
    /// </summary>
    public static ICollection<KeyValuePair<string, Type>> TypeMap => new List<KeyValuePair<string, Type>>(_typeMap);

    /// <summary>
    /// Access the collection of wrappers on the &quot;disabled&quot; list.
    /// </summary>
    /// <remarks>Updates to entries in the collection will be used next time a wrapper is requested.
    /// Existing instances of a disabled wrapper will not be invalidated and it will remain possible to
    /// bypass WrapperDealer and instantiate instances of the wrapper class directly.</remarks>
    public static ICollection<KeyValuePair<string, Type>> Disabled => _disabled;

    #region Implementation
    static List<KeyValuePair<string, Type>> _typeMap;

    static readonly List<KeyValuePair<string, Type>> _disabled = new();

    static IResource WrapperForType(string type, Stream s)
    {
        Type t = _typeMap.Find(x => !_disabled.Contains(x) && x.Key == type).Value;

        if (t == null)
            t = _typeMap.Find(x => !_disabled.Contains(x) && x.Key == "*").Value;

        return (IResource)t.GetConstructor(new[] { typeof(Stream) })?.Invoke(new object[] { s });
    }
    #endregion
}