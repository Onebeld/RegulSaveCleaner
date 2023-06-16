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
using System.Reflection;

namespace Regul.S3PI.Interfaces;

/// <summary>
/// API Objects should all descend from this Abstract class.
/// It will provide versioning support -- when implemented.
/// It provides ContentFields support
/// </summary>
public abstract class AApiVersionedFields : IContentFields
{
    #region IContentFields Members
    /// <summary>
    /// The list of available field names on this API object
    /// </summary>
    public abstract List<string> ContentFields { get; } // This should be implemented with a call to GetContentFields(requestedApiVersion, this.GetType())
    /// <summary>
    /// A typed value on this object
    /// </summary>
    /// <param name="index">The name of the field (i.e. one of the values from ContentFields)</param>
    /// <returns>The typed value of the named field</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an unknown index name is requested</exception>
    public virtual TypedValue this[string index]
    {
        get
        {
            string[] fields = index.Split('.');
            object result = this;
            Type t = GetType();
            for (int i = 0; i < fields.Length; i++)
            {
                PropertyInfo p = t.GetProperty(fields[i]);
                if (p == null)
                    throw new ArgumentOutOfRangeException(nameof(index),
                        "Unexpected value received in index: " + index);
                t = p.PropertyType;
                result = p.GetValue(result, null);
            }

            return new TypedValue(t, result, "X");
        }
        set
        {
            string[] fields = index.Split('.');
            object result = this;
            Type t = GetType();
            PropertyInfo p = null;
            for (int i = 0; i < fields.Length; i++)
            {
                p = t.GetProperty(fields[i]);
                if (p == null)
                    throw new ArgumentOutOfRangeException(nameof(index), "Unexpected value received in index: " + index);
                if (i < fields.Length - 1)
                {
                    t = p.PropertyType;
                    result = p.GetValue(result, null);
                }
            }
            p.SetValue(result, value.Value, null);
        }
    }

    #endregion

    static readonly List<string> Banlist;
    static AApiVersionedFields()
    {
        Banlist = new List<string>();
        PropertyInfo[] array = typeof(AApiVersionedFields).GetProperties();
        for (int i = 0; i < array.Length; i++)
            Banlist.Add(array[i].Name);
    }

    /// <summary>
    /// Versioning is not currently implemented
    /// Return the list of fields for a given API Class
    /// </summary>
    /// <param name="t">The class type for which to get the fields</param>
    /// <returns>List of field names for the given API version</returns>
    public static List<string> GetContentFields(Type t)
    {
        List<string> fields = new();

        PropertyInfo[] ap = t.GetProperties();
        for (int i = 0; i < ap.Length; i++)
        {
            PropertyInfo m = ap[i];
            if (Banlist.Contains(m.Name)) continue;

            fields.Add(m.Name);
        }
        fields.Sort(new PriorityComparer(t));

        return fields;
    }

    /// <summary>
    /// Get the TGIBlock list for a Content Field.
    /// </summary>
    /// <param name="o">The object to query.</param>
    /// <param name="f">The property name under inspection.</param>
    /// <returns>The TGIBlock list for a Content Field, if present; otherwise <c>null</c>.</returns>
    public static DependentList<TGIBlock> GetTgiBlocks(AApiVersionedFields o, string f)
    {
        string tgiBlockListCf = TGIBlockListContentFieldAttribute.GetTgiBlockListContentField(o.GetType(), f);
        if (tgiBlockListCf != null)
            try
            {
                return o[tgiBlockListCf].Value as DependentList<TGIBlock>;
            }
            catch { }
        return null;
    }

    /// <summary>
    /// Get the TGIBlock list for a Content Field.
    /// </summary>
    /// <param name="f">The property name under inspection.</param>
    /// <returns>The TGIBlock list for a Content Field, if present; otherwise <c>null</c>.</returns>
    public DependentList<TGIBlock> GetTgiBlocks(string f) { return GetTgiBlocks(this, f); }

    private class PriorityComparer : IComparer<string>
    {
        readonly Type _t;
        public PriorityComparer(Type t) { _t = t; }
        public int Compare(string x, string y)
        {
            int res = ElementPriorityAttribute.GetPriority(_t, x).CompareTo(ElementPriorityAttribute.GetPriority(_t, y));
            if (res == 0) res = string.Compare(x, y, StringComparison.Ordinal);
            return res;
        }
    }

    static readonly List<string> ValueBuilderBanlist = new(new[] {
        "Value", "Stream", "AsBytes",
    });
    static readonly List<string> IDictionaryBanlist = new(new[] {
        "Keys", "Values", "Count", "IsReadOnly", "IsFixedSize", "IsSynchronized", "SyncRoot",
    });

    /// <summary>
    /// The fields ValueBuilder will return; used to eliminate those that should not be used.
    /// </summary>
    protected List<string> ValueBuilderFields
    {
        get
        {
            List<string> fields = ContentFields;
            fields.RemoveAll(Banlist.Contains);
            fields.RemoveAll(ValueBuilderBanlist.Contains);
            if (typeof(System.Collections.IDictionary).IsAssignableFrom(GetType())) fields.RemoveAll(IDictionaryBanlist.Contains);
            return fields;
        }
    }

    /// <summary>
    /// Returns a string representing the value of the field (and any contained sub-fields)
    /// </summary>
    protected string ValueBuilder
    {
        get
        {
            System.Text.StringBuilder sb = new();

            List<string> fields = ValueBuilderFields;

            string headerFmt = "\n--- {0}: {1} (0x{2:X}) ---";

            for (int i1 = 0; i1 < fields.Count; i1++)
            {
                string f = fields[i1];
                TypedValue tv = this[f];

                if (typeof(AApiVersionedFields).IsAssignableFrom(tv.Type))
                {
                    AApiVersionedFields apiObj = (AApiVersionedFields)tv.Value;
                    if (apiObj.ContentFields.Contains("Value") &&
                        typeof(string).IsAssignableFrom(GetContentFieldTypes(tv.Type)["Value"]))
                    {
                        string elem = (string)apiObj["Value"].Value;
                        if (elem.Contains("\n"))
                            sb.Append("\n--- " + tv.Type.Name + ": " + f + " ---\n   " + elem.Replace("\n", "\n   ").TrimEnd() + "\n---");
                        else
                            sb.Append("\n" + f + ": " + elem);
                    }
                }
                else if (tv.Type.BaseType != null && tv.Type.BaseType.Name.Contains("IndexList`"))
                {
                    System.Collections.IList l = (System.Collections.IList)tv.Value;
                    string fmt = "\n   [{0:X" + l.Count.ToString("X").Length + "}]: {1}";
                    int i = 0;

                    sb.Append(string.Format(headerFmt, tv.Type.Name, f, l.Count));
                    for (int i2 = 0; i2 < l.Count; i2++)
                        sb.Append(string.Format(fmt, i++, (string)((AHandlerElement)l[i2])["Value"].Value));

                    sb.Append("\n---");
                }
                else if (tv.Type.BaseType != null && tv.Type.BaseType.Name.Contains("SimpleList`"))
                {
                    System.Collections.IList l = (System.Collections.IList)tv.Value;
                    string fmt = "\n   [{0:X" + l.Count.ToString("X").Length + "}]: {1}";
                    int i = 0;

                    sb.Append(string.Format(headerFmt, tv.Type.Name, f, l.Count));
                    for (int i2 = 0; i2 < l.Count; i2++)
                        sb.Append(string.Format(fmt, i++, ((AHandlerElement)l[i2])["Val"]));

                    sb.Append("\n---");
                }
                else if (typeof(DependentList<TGIBlock>).IsAssignableFrom(tv.Type))
                {
                    DependentList<TGIBlock> l = (DependentList<TGIBlock>)tv.Value;
                    string fmt = "\n   [{0:X" + l.Count.ToString("X").Length + "}]: {1}";
                    int i = 0;

                    sb.Append(string.Format(headerFmt, tv.Type.Name, f, l.Count));
                    for (int i2 = 0; i2 < l.Count; i2++)
                        sb.Append(string.Format(fmt, i++, l[i2]));

                    sb.Append("\n---");
                }
                else if (tv.Type.BaseType != null && tv.Type.BaseType.Name.Contains("DependentList`"))
                {
                    System.Collections.IList l = (System.Collections.IList)tv.Value;
                    string fmtLong = "\n--- {0}[{1:X" + l.Count.ToString("X").Length + "}] ---\n   ";
                    string fmtShort = "\n   [{0:X" + l.Count.ToString("X").Length + "}]: {1}";
                    int i = 0;

                    sb.Append(string.Format(headerFmt, tv.Type.Name, f, l.Count));
                    for (int i2 = 0; i2 < l.Count; i2++)
                    {
                        AHandlerElement v = (AHandlerElement)l[i2];
                        if (v.ContentFields.Contains("Value") &&
                            typeof(string).IsAssignableFrom(GetContentFieldTypes(v.GetType())["Value"]))
                        {
                            string elem = (string)v["Value"].Value;
                            if (elem.Contains("\n"))
                                sb.Append(string.Format(fmtLong, f, i++) + elem.Replace("\n", "\n   ").TrimEnd());
                            else
                                sb.Append(string.Format(fmtShort, i++, elem));
                        }
                    }
                    sb.Append("\n---");
                }
                else if (tv.Type.HasElementType && typeof(AApiVersionedFields).IsAssignableFrom(tv.Type.GetElementType())) // it's an AApiVersionedFields array, slightly glossy...
                {
                    sb.Append(string.Format(headerFmt, tv.Type.Name, f, ((Array)tv.Value).Length));
                    sb.Append("\n   " + tv.ToString().Replace("\n", "\n   ").TrimEnd() + "\n---");
                }
                else
                {
                    string suffix = "";
                    DependentList<TGIBlock> tgis = GetTgiBlocks(f);
                    if (tgis != null && tgis.Count > 0)
                        try
                        {
                            int i = Convert.ToInt32(tv.Value);
                            if (i >= 0 && i < tgis.Count)
                                suffix = " (" + tgis[i] + ")";
                        }
                        catch (Exception e)
                        {
                            sb.Append(" (Exception: " + e.Message + ")");
                        }
                    sb.Append("\n" + f + ": " + tv + suffix);
                }
            }

            if (typeof(System.Collections.IDictionary).IsAssignableFrom(GetType()))
            {
                System.Collections.IDictionary l = this as System.Collections.IDictionary;
                string fmt = "\n   [{0:X" + l.Count.ToString("X").Length + "}] {1}: {2}";
                int i = 0;
                sb.Append("\n--- (0x" + l.Count.ToString("X") + ") ---");
                foreach (object key in l.Keys)
                    sb.Append(string.Format(fmt, i++,
                        new TypedValue(key.GetType(), key, "X"),
                        new TypedValue(l[key].GetType(), l[key], "X")));
                sb.Append("\n---");
            }

            return sb.ToString().Trim('\n');
        }
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="AApiVersionedFields"/> object.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="AApiVersionedFields"/> object.</returns>
    public override string ToString() => ValueBuilder;

    /// <summary>
    /// Gets a lookup table from fieldname to type.
    /// </summary>
    /// <param name="t">API data type to query</param>
    /// <returns></returns>
    public static Dictionary<string, Type> GetContentFieldTypes(Type t)
    {
        Dictionary<string, Type> types = new();

        PropertyInfo[] ap = t.GetProperties();
        for (int i = 0; i < ap.Length; i++)
        {
            PropertyInfo m = ap[i];
            if (Banlist.Contains(m.Name)) continue;

            types.Add(m.Name, m.PropertyType);
        }

        return types;
    }

#if UNDEF
        protected static List<string> getMethods(Int32 APIversion, Type t)
        {
            List<string> methods = null;

            Int32 recommendedApiVersion = getRecommendedApiVersion(t);//Could be zero if no "recommendedApiVersion" const field
            methods = new List<string>();
            MethodInfo[] am = t.GetMethods();
            foreach (MethodInfo m in am)
            {
                if (!m.IsPublic || banlist.Contains(m.Name)) continue;
                if ((m.Name.StartsWith("get_") && m.GetParameters().Length == 0)) continue;
                if (!checkVersion(t, m.Name, APIversion == 0 ? recommendedApiVersion : APIversion)) continue;

                methods.Add(m.Name);
            }

            return methods;
        }

        public List<string> Methods { get; }
        
        public TypedValue Invoke(string method, params TypedValue[] parms)
        {
            Type[] at = new Type[parms.Length];
            object[] ao = new object[parms.Length];
            for (int i = 0; i < parms.Length; i++) { at[i] = parms[i].Type; ao[i] = parms[i].Value; }//Array.Copy, please...

            MethodInfo m = this.GetType().GetMethod(method, at);
            if (m == null)
                throw new ArgumentOutOfRangeException("Unexpected method received: " + method + "(...)");

            return new TypedValue(m.ReturnType, m.Invoke(this, ao), "X");
        }
#endif

    // Random helper functions that should live somewhere...
}

/// <summary>
/// A useful extension to <see cref="AApiVersionedFields"/> where a change handler is required
/// </summary>
public abstract class AHandlerElement : AApiVersionedFields
{
    /// <summary>
    /// Holds the <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.
    /// </summary>
    protected EventHandler Handler;

    /// <summary>
    /// Indicates if the <see cref="AHandlerElement"/> has been changed by OnElementChanged()
    /// </summary>
    protected bool dirty;

    /// <summary>
    /// Initialize a new instance
    /// </summary>
    /// <param name="handler">The <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerElement"/> changes.</param>
    public AHandlerElement(EventHandler handler) { this.Handler = handler; }
    //public abstract AHandlerElement Clone(EventHandler handler);

    /// <summary>
    /// Flag the <see cref="AHandlerElement"/> as dirty and invoke the <see cref="EventHandler"/> delegate.
    /// </summary>
    protected void OnElementChanged()
    {
        dirty = true;
        //Console.WriteLine(this.GetType().Name + " dirtied.");
        Handler?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Change the element change handler to <paramref name="handler"/>.
    /// </summary>
    /// <param name="handler">The new element change handler.</param>
    internal void SetHandler(EventHandler handler) { if (!Equals(this.Handler, handler)) this.Handler = handler; }
}