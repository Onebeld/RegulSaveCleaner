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
/// Abstract extension to <see cref="AHandlerList{T}"/> adding support for <see cref="System.IO.Stream"/> IO
/// and partially implementing <see cref="IGenericAdd"/>.
/// </summary>
/// <typeparam name="T"><see cref="Type"/> of list element</typeparam>
/// <seealso cref="AHandlerList{T}"/>
/// <seealso cref="IGenericAdd"/>
public abstract class DependentList<T> : AHandlerList<T>, IGenericAdd
    where T : AHandlerElement, IEquatable<T>
{
    /// <summary>
    /// Holds the <see cref="EventHandler"/> delegate to invoke if an element in the <see cref="DependentList{T}"/> changes.
    /// </summary>
    /// <remarks>Work around for list event handler triggering during stream constructor and other places.</remarks>
    protected EventHandler elementHandler;

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="DependentList{T}"/> class
    /// that is empty.
    /// </summary>
    /// <param name="handler">The <see cref="EventHandler"/> to call on changes to the list or its elements.</param>
    /// <param name="maxSize">Optional; -1 for unlimited size, otherwise the maximum number of elements in the list.</param>
    protected DependentList(EventHandler handler, long maxSize = -1) : base(handler, maxSize) { }

    // Add stream-based constructors and support

    #endregion

    #region Data I/O

    #endregion

    #region IGenericAdd

#if OLDVERSION
        /// <summary>
        /// Adds an entry to an <see cref="DependentList{T}"/>.
        /// </summary>
        /// <param name="fields">
        /// The object to add: either an instance or, for abstract generic lists,
        /// a concrete type or (to be deprecated) the concrete type constructor arguments.  See 'remarks'.
        /// </param>
        /// <returns>True on success</returns>
        /// <exception cref="InvalidOperationException">Thrown when list size exceeded.</exception>
        /// <exception cref="NotSupportedException">The <see cref="DependentList{T}"/> is read-only.</exception>
        /// <remarks>
        /// <para>As at 27 April 2012, changes are afoot in how this works.
        /// Using <see cref="ConstructorParametersAttribute"/> will soon be deprecated.
        /// Concrete implementations of abstract types will be expected to have a default constructor taking only <c>APIversion</c> and <c>elementHandler</c>.</para>
        /// <para>Currently, this method supports the following invocations:</para>
        /// <list type="bullet">
        /// <item><term><c>Add(T newElement)</c></term><description>Create a new instance from the one supplied.</description></item>
        /// <item><term><c>Add(Type concreteOfT)</c></term>
        /// <description>Create a default instance of the given concrete implementation of an abstract class.
        /// Where <c>concreteOfT</c> has a <see cref="ConstructorParametersAttribute"/>, these will additionally be passed to the constructor (to be deprecated).</description>
        /// </item>
        /// <item><term><c>Add(param object[] parameters)</c></term>
        /// <description>(to be deprecated)
        /// For abstract types, the concrete type will be looked up from the supplied parameters.
        /// A new instance will be created, passing the supplied parameters.</description>
        /// </item>
        /// </list>
        /// <para>The new instance will be created passing a zero APIversion and the list's change handler.</para>
        /// </remarks>
        /// <seealso cref="Activator.CreateInstance(Type, object[])"/>
        public virtual bool Add(params object[] fields)
        {
            if (fields == null) return false;

            Type elementType = typeof(T);

            if (fields.Length == 1)
            {
                if (fields[0] is T) // Add(new ConcreteType(0, null, foo, bar))
                {
                    AHandlerElement element = fields[0] as AHandlerElement;
                    base.Add(element.Clone(elementHandler) as T);
                    return true;
                }
            }

            if (elementType.IsAbstract)
            {
                if (fields.Length == 1 && fields[0] is Type && elementType.IsAssignableFrom(fields[0] as Type)) // Add(typeof(ConcreteType))
                {
                    elementType = fields[0] as Type;
                    ConstructorParametersAttribute[] constructorParametersArray = elementType.GetCustomAttributes(typeof(ConstructorParametersAttribute), true) as ConstructorParametersAttribute[];
                    if (constructorParametersArray.Length == 1) // ConstructorParametersAttribute present -- deprecated
                        fields = constructorParametersArray[0].parameters;
                    else // ConstructorParametersAttribute absent: this will become the only way
                        fields = new object[] { };
                }
                else // Add(foo, bar) -- deprecated
                {
                    elementType = GetElementType(fields);
                }
            }

            try
            {
                object[] args = new object[fields.Length + 2];
                args[0] = (int)0;
                args[1] = elementHandler;
                Array.Copy(fields, 0, args, 2, fields.Length);
                T newElement = Activator.CreateInstance(elementType, args) as T;
                //T newElement = Activator.CreateInstance(elementType, new object[] { elementHandler, }) as T; // eventually...
                base.Add(newElement);
                return true;
            }
            catch (MissingMethodException) { } // That's allowed... for now
            return false;
        }
#endif

    /// <summary>
    /// Adds an entry to a <see cref="DependentList{T}"/>, setting the element change handler.
    /// </summary>
    /// <param name="newElement">An instance of type <c>{T}</c> to add to the list.</param>
    /// <exception cref="InvalidOperationException">Thrown when list size exceeded.</exception>
    /// <exception cref="NotSupportedException">The <see cref="DependentList{T}"/> is read-only.</exception>
    public override void Add(T newElement)
    {
        newElement.SetHandler(elementHandler);
        base.Add(newElement);
    }

    #endregion
}