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

namespace RegulSaveCleaner.S3PI.Interfaces;

/// <summary>
/// Element priority is used when displaying elements
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false)]
public class ElementPriorityAttribute : Attribute
{
    int _priority;
    /// <summary>
    /// Element priority is used when displaying elements
    /// </summary>
    /// <param name="priority">Element priority, lower values are higher priority</param>
    public ElementPriorityAttribute(int priority) { _priority = priority; }
    /// <summary>
    /// Element priority, lower values are higher priority
    /// </summary>
    public int Priority { get => _priority;
        set => _priority = value;
    }

    /// <summary>
    /// Return the ElementPriority value for a Content Field.
    /// </summary>
    /// <param name="t">Type on which Content Field exists.</param>
    /// <param name="index">Content Field name.</param>
    /// <returns>The value of the ElementPriorityAttribute Priority field, if found;
    /// otherwise Int32.MaxValue.</returns>
    public static int GetPriority(Type t, string index)
    {
        System.Reflection.PropertyInfo pi = t.GetProperty(index);

        if (pi == null) return int.MaxValue;
            
        foreach (object attr in pi.GetCustomAttributes(typeof(ElementPriorityAttribute), true))
            return ((ElementPriorityAttribute)attr).Priority;

        return int.MaxValue;
    }
}