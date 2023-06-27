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
using System.Text;
using System.Runtime.Serialization;

namespace RegulSaveCleaner.S3PI.Interfaces
{
    /// <summary>
    /// A tuple associating a data type (or class) with a value object (of the given type)
    /// </summary>
    [Serializable]
    public class TypedValue : IConvertible, ICloneable, ISerializable
    {
        /// <summary>
        /// The data type
        /// </summary>
        public readonly Type Type;
        /// <summary>
        /// The value
        /// </summary>
        public readonly object Value;

        string _format;

        /// <summary>
        /// Create a new <see cref="TypedValue"/>
        /// </summary>
        /// <param name="t">The data type</param>
        /// <param name="v">The value</param>
        public TypedValue(Type t, object v) : this(t, v, "") { }
        /// <summary>
        /// Create a new <see cref="TypedValue"/>
        /// </summary>
        /// <param name="t">The data type</param>
        /// <param name="v">The value</param>
        /// <param name="f">The default format for <see cref="ToString()"/></param>
        public TypedValue(Type t, object v, string f) { Type = t; Value = v; _format = f; }

        /// <summary>
        /// Return a string representing the <see cref="TypedValue"/>
        /// <paramref name="tv"/>.
        /// </summary>
        /// <param name="tv">The value to convert.</param>
        /// <returns>A string representing <paramref name="tv"/>.</returns>
        public static implicit operator string(TypedValue tv) { return tv.ToString(tv._format); }

        /// <summary>
        /// Return the Value as a string using the default format
        /// </summary>
        /// <returns>String representation of Value in default format</returns>
        public override string ToString() { return ToString(_format); }
        /// <summary>
        /// Return the Value as a string using the given format
        /// </summary>
        /// <param name="format">Format to use for result</param>
        /// <returns>String representation of Value in given format</returns>
        public string ToString(string format)
        {
            if (format == "X")
            {
                if (Type == typeof(long)) return "0x" + ((long)Value).ToString("X16");
                if (Type == typeof(ulong)) return "0x" + ((ulong)Value).ToString("X16");
                if (Type == typeof(int)) return "0x" + ((int)Value).ToString("X8");
                if (Type == typeof(uint)) return "0x" + ((uint)Value).ToString("X8");
                if (Type == typeof(short)) return "0x" + ((short)Value).ToString("X4");
                if (Type == typeof(ushort)) return "0x" + ((ushort)Value).ToString("X4");
                if (Type == typeof(sbyte)) return "0x" + ((sbyte)Value).ToString("X2");
                if (Type == typeof(byte)) return "0x" + ((byte)Value).ToString("X2");

                //well, no... but...
                if (Type == typeof(double)) return ((double)Value).ToString("F4");
                if (Type == typeof(float)) return ((float)Value).ToString("F4");
                
                if (typeof(Enum).IsAssignableFrom(Type))
                {
                    TypedValue etv = new(Enum.GetUnderlyingType(Type), Value, "X");
                    return $"{"" + etv} ({"" + Value})";
                }
            }

            if (typeof(string).IsAssignableFrom(Type) || typeof(char[]).IsAssignableFrom(Type))
            {
                string s = typeof(string).IsAssignableFrom(Type) ? (string)Value : new string((char[])Value);
                // -- It is not necessarily correct that a zero byte indicates a unicode string; these should have been
                // correctly read in already so no translation should be needed... so the ToANSIString is currently commented out
                return s.IndexOf((char)0) != -1 /*s.Length % 2 == 0 ? ToANSIString(s) :/**/ ? ToDisplayString(s.ToCharArray()) : s.Normalize();
            }

            if (Type.HasElementType) // it's an array
                return FromSimpleArray((Array)Value);

            return Value.ToString();
        }

        #region ToString helpers
        static string ToAnsiString(string unicode)
        {
            StringBuilder t = new();
            for (int i = 0; i < unicode.Length; i += 2) t.Append((char)((unicode[i] << 8) + unicode[i + 1]));
            return t.ToString().Normalize();
        }

        static string FromSimpleArray(Array ary)
        {
            StringBuilder sb = new();
            for (int i = 0; i < ary.Length; i++)
            {
                object v = ary.GetValue(i);
                TypedValue tv = new(v.GetType(), v, "X");
                sb.Append($" [{i:X}:'{"" + tv}']");
                if (i % 16 == 15) sb.Append("\n");
            }
            return sb.ToString().TrimStart().TrimEnd('\n');
        }

        private static readonly string[] LowNames = {
                                                "NUL", "SOH", "STX", "ETX", "EOT", "ENQ", "ACK", "BEL",
                                                "BS", "HT", /*"LF",**/ "VT", "FF", "CR", "SO", "SI",
                                                "DLE", "DC1", "DC2", "DC3", "DC4", "NAK", "SYN", "ETB",
                                                "CAN", "EM", "SUB", "ESC", "FS", "GS", "RS", "US",
                                            };
        static string ToDisplayString(char[] text)
        {
            StringBuilder t = new();
            foreach (char c in text)
            {
                if (c < 32 && c != '\n')
                    t.Append($"<{LowNames[c]}>");
                else if (c == 127)
                    t.Append("<DEL>");
                else if (c > 127)
                    t.Append($"<U+{(int) c:X4}>");
                else
                    t.Append(c);
            }
            return t.ToString().Normalize();
        }
        #endregion
        
        #region IConvertible Members

        /// <summary>
        /// Returns the <see cref="TypeCode"/> for this instance.
        /// </summary>
        /// <returns>The enumerated constant that is the <see cref="TypeCode"/> of the <see cref="TypedValue"/> class.</returns>
        public TypeCode GetTypeCode() => TypeCode.String;

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="Boolean"/> value
        /// (ignoring the specified culture-specific formatting information).
        /// </summary>
        /// <param name="provider">(unused, may be null) An <see cref="IFormatProvider"/> interface implementation
        /// that supplies culture-specific formatting information.</param>
        /// <returns>A <see cref="Boolean"/> value equivalent to the value of this instance.</returns>
        /// <exception cref="NotImplementedException">Thrown if the <see cref="TypedValue"/> value
        /// cannot be assigned to a <see cref="Boolean"/>.</exception>
        public bool ToBoolean(IFormatProvider provider)
        {
            if (typeof(bool).IsAssignableFrom(Type)) return (bool)Value;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="Byte"/> value
        /// (ignoring the specified culture-specific formatting information).
        /// </summary>
        /// <param name="provider">(unused, may be null) An <see cref="IFormatProvider"/> interface implementation
        /// that supplies culture-specific formatting information.</param>
        /// <returns>A <see cref="Byte"/> value equivalent to the value of this instance.</returns>
        /// <exception cref="NotImplementedException">Thrown if the <see cref="TypedValue"/> value
        /// cannot be assigned to a <see cref="Byte"/>.</exception>
        public byte ToByte(IFormatProvider provider)
        {
            if (typeof(byte).IsAssignableFrom(Type)) return (byte)Value;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="Char"/> value
        /// (ignoring the specified culture-specific formatting information).
        /// </summary>
        /// <param name="provider">(unused, may be null) An <see cref="IFormatProvider"/> interface implementation
        /// that supplies culture-specific formatting information.</param>
        /// <returns>A <see cref="Char"/> value equivalent to the value of this instance.</returns>
        /// <exception cref="NotImplementedException">Thrown if the <see cref="TypedValue"/> value
        /// cannot be assigned to a <see cref="Char"/>.</exception>
        public char ToChar(IFormatProvider provider)
        {
            if (typeof(char).IsAssignableFrom(Type)) return (char)Value;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="DateTime"/> value
        /// (ignoring the specified culture-specific formatting information).
        /// </summary>
        /// <param name="provider">(unused, may be null) An <see cref="IFormatProvider"/> interface implementation
        /// that supplies culture-specific formatting information.</param>
        /// <returns>A <see cref="DateTime"/> value equivalent to the value of this instance.</returns>
        /// <exception cref="NotImplementedException">Thrown if the <see cref="TypedValue"/> value
        /// cannot be assigned to a <see cref="DateTime"/>.</exception>
        public DateTime ToDateTime(IFormatProvider provider)
        {
            if (typeof(DateTime).IsAssignableFrom(Type)) return (DateTime)Value;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="Decimal"/> value
        /// (ignoring the specified culture-specific formatting information).
        /// </summary>
        /// <param name="provider">(unused, may be null) An <see cref="IFormatProvider"/> interface implementation
        /// that supplies culture-specific formatting information.</param>
        /// <returns>A <see cref="Decimal"/> value equivalent to the value of this instance.</returns>
        /// <exception cref="NotImplementedException">Thrown if the <see cref="TypedValue"/> value
        /// cannot be assigned to a <see cref="Decimal"/>.</exception>
        public decimal ToDecimal(IFormatProvider provider)
        {
            if (typeof(decimal).IsAssignableFrom(Type)) return (decimal)Value;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="Double"/> value
        /// (ignoring the specified culture-specific formatting information).
        /// </summary>
        /// <param name="provider">(unused, may be null) An <see cref="IFormatProvider"/> interface implementation
        /// that supplies culture-specific formatting information.</param>
        /// <returns>A <see cref="Double"/> value equivalent to the value of this instance.</returns>
        /// <exception cref="NotImplementedException">Thrown if the <see cref="TypedValue"/> value
        /// cannot be assigned to a <see cref="Double"/>.</exception>
        public double ToDouble(IFormatProvider provider)
        {
            if (typeof(double).IsAssignableFrom(Type)) return (double)Value;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="Int16"/> value
        /// (ignoring the specified culture-specific formatting information).
        /// </summary>
        /// <param name="provider">(unused, may be null) An <see cref="IFormatProvider"/> interface implementation
        /// that supplies culture-specific formatting information.</param>
        /// <returns>A <see cref="Int16"/> value equivalent to the value of this instance.</returns>
        /// <exception cref="NotImplementedException">Thrown if the <see cref="TypedValue"/> value
        /// cannot be assigned to a <see cref="Int16"/>.</exception>
        public short ToInt16(IFormatProvider provider)
        {
            if (typeof(short).IsAssignableFrom(Type)) return (short)Value;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="Int32"/> value
        /// (ignoring the specified culture-specific formatting information).
        /// </summary>
        /// <param name="provider">(unused, may be null) An <see cref="IFormatProvider"/> interface implementation
        /// that supplies culture-specific formatting information.</param>
        /// <returns>A <see cref="Int32"/> value equivalent to the value of this instance.</returns>
        /// <exception cref="NotImplementedException">Thrown if the <see cref="TypedValue"/> value
        /// cannot be assigned to a <see cref="Int32"/>.</exception>
        public int ToInt32(IFormatProvider provider)
        {
            if (typeof(int).IsAssignableFrom(Type)) return (int)Value;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="Int64"/> value
        /// (ignoring the specified culture-specific formatting information).
        /// </summary>
        /// <param name="provider">(unused, may be null) An <see cref="IFormatProvider"/> interface implementation
        /// that supplies culture-specific formatting information.</param>
        /// <returns>A <see cref="Int64"/> value equivalent to the value of this instance.</returns>
        /// <exception cref="NotImplementedException">Thrown if the <see cref="TypedValue"/> value
        /// cannot be assigned to a <see cref="Int64"/>.</exception>
        public long ToInt64(IFormatProvider provider)
        {
            if (typeof(long).IsAssignableFrom(Type)) return (long)Value;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="SByte"/> value
        /// (ignoring the specified culture-specific formatting information).
        /// </summary>
        /// <param name="provider">(unused, may be null) An <see cref="IFormatProvider"/> interface implementation
        /// that supplies culture-specific formatting information.</param>
        /// <returns>A <see cref="SByte"/> value equivalent to the value of this instance.</returns>
        /// <exception cref="NotImplementedException">Thrown if the <see cref="TypedValue"/> value
        /// cannot be assigned to a <see cref="SByte"/>.</exception>
        public sbyte ToSByte(IFormatProvider provider)
        {
            if (typeof(sbyte).IsAssignableFrom(Type)) return (sbyte)Value;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="Single"/> value
        /// (ignoring the specified culture-specific formatting information).
        /// </summary>
        /// <param name="provider">(unused, may be null) An <see cref="IFormatProvider"/> interface implementation
        /// that supplies culture-specific formatting information.</param>
        /// <returns>A <see cref="Single"/> value equivalent to the value of this instance.</returns>
        /// <exception cref="NotImplementedException">Thrown if the <see cref="TypedValue"/> value
        /// cannot be assigned to a <see cref="Single"/>.</exception>
        public float ToSingle(IFormatProvider provider)
        {
            if (typeof(float).IsAssignableFrom(Type)) return (float)Value;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="String"/> value
        /// (ignoring the specified culture-specific formatting information).
        /// </summary>
        /// <param name="provider">(unused, may be null) An <see cref="IFormatProvider"/> interface implementation
        /// that supplies culture-specific formatting information.</param>
        /// <returns>A <see cref="String"/> value equivalent to the value of this instance.</returns>
        /// <exception cref="NotImplementedException">Thrown if the <see cref="TypedValue"/> value
        /// cannot be assigned to a <see cref="String"/>.</exception>
        public string ToString(IFormatProvider provider)
        {
            if (typeof(string).IsAssignableFrom(Type)) return (string)Value;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the value of this instance to an <see cref="Object"/> of the specified <see cref="Type"/>
        /// that has an equivalent value, using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="conversionType">The <see cref="Type"/> to which the value of this instance is converted.</param>
        /// <param name="provider">An <see cref="IFormatProvider"/> interface implementation that supplies culture-specific formatting information.</param>
        /// <returns>An <see cref="Object"/> instance of type <paramref name="conversionType"/> whose value is equivalent to the value of this instance.</returns>
        /// <exception cref="NotImplementedException">Thrown if the <see cref="TypedValue"/> value
        /// cannot be assigned to a <paramref name="conversionType"/> object.</exception>
        public object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType.IsAssignableFrom(Type)) return Convert.ChangeType(Value, conversionType, provider);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="UInt16"/> value
        /// (ignoring the specified culture-specific formatting information).
        /// </summary>
        /// <param name="provider">(unused, may be null) An <see cref="IFormatProvider"/> interface implementation
        /// that supplies culture-specific formatting information.</param>
        /// <returns>A <see cref="UInt16"/> value equivalent to the value of this instance.</returns>
        /// <exception cref="NotImplementedException">Thrown if the <see cref="TypedValue"/> value
        /// cannot be assigned to a <see cref="UInt16"/>.</exception>
        public ushort ToUInt16(IFormatProvider provider)
        {
            if (typeof(ushort).IsAssignableFrom(Type)) return (ushort)Value;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="UInt32"/> value
        /// (ignoring the specified culture-specific formatting information).
        /// </summary>
        /// <param name="provider">(unused, may be null) An <see cref="IFormatProvider"/> interface implementation
        /// that supplies culture-specific formatting information.</param>
        /// <returns>A <see cref="UInt32"/> value equivalent to the value of this instance.</returns>
        /// <exception cref="NotImplementedException">Thrown if the <see cref="TypedValue"/> value
        /// cannot be assigned to a <see cref="UInt32"/>.</exception>
        public uint ToUInt32(IFormatProvider provider)
        {
            if (typeof(uint).IsAssignableFrom(Type)) return (uint)Value;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the value of this instance to an equivalent <see cref="UInt64"/> value
        /// (ignoring the specified culture-specific formatting information).
        /// </summary>
        /// <param name="provider">(unused, may be null) An <see cref="IFormatProvider"/> interface implementation
        /// that supplies culture-specific formatting information.</param>
        /// <returns>A <see cref="UInt64"/> value equivalent to the value of this instance.</returns>
        /// <exception cref="NotImplementedException">Thrown if the <see cref="TypedValue"/> value
        /// cannot be assigned to a <see cref="UInt64"/>.</exception>
        public ulong ToUInt64(IFormatProvider provider)
        {
            if (typeof(ulong).IsAssignableFrom(Type)) return (ulong)Value;
            throw new NotImplementedException();
        }

        #endregion

        #region ICloneable Members

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        /// <exception cref="NotImplementedException">Thrown if the value cannot be cloned.</exception>
        public object Clone()
        {
            if (typeof(ICloneable).IsAssignableFrom(Type)) return new TypedValue(Type, ((ICloneable)Value).Clone(), _format);
            throw new NotImplementedException();
        }

        #endregion

        #region ISerializable Members

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo to populate with data.</param>
        /// <param name="context">The destination (see System.Runtime.Serialization.StreamingContext) for this serialization.</param>
        /// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Type", Type, typeof(Type));
            info.AddValue("Value", Value, Type);
            info.AddValue("format", _format);
        }

        #endregion
    }
}
