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
namespace System.Collections.Generic;

/// <summary>
/// Abstract extension of <see cref="List{T}"/>
/// providing feedback on list updates through the supplied <see cref="EventHandler"/>.
/// </summary>
/// <typeparam name="T"><see cref="Type"/> of list element</typeparam>
public abstract class AHandlerList<T> : List<T>
    where T : IEquatable<T>
{
    /// <summary>
    /// Holds the <see cref="EventHandler"/> delegate to invoke if the <see cref="AHandlerList{T}"/> changes.
    /// </summary>
    protected EventHandler handler;
    /// <summary>
    /// The maximum size of the list, or -1 for no limit.
    /// </summary>
    protected long maxSize = -1;

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="AHandlerList{T}"/> class
    /// that is empty
    /// and with maximum size of <paramref name="maxSize"/> (default is unlimited).
    /// </summary>
    /// <param name="handler">The <see cref="EventHandler"/> to call on changes to the list.</param>
    /// <param name="maxSize">Optional; -1 for unlimited size, otherwise the maximum number of elements in the list.</param>
    protected AHandlerList(EventHandler handler, long maxSize = -1) { this.handler = handler; this.maxSize = maxSize; }

    #endregion

    #region List<T> Members

    #endregion

    #region IList<T> Members

    /// <summary>
    /// Removes the <see cref="AHandlerList{T}"/> item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the item to remove.</param>
    /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="AHandlerList{T}"/>.</exception>
    /// <exception cref="System.NotSupportedException">The <see cref="AHandlerList{T}"/> is read-only.</exception>
    public new virtual void RemoveAt(int index) { base.RemoveAt(index); OnListChanged(); }
    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <returns>The element at the specified index.</returns>
    /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="AHandlerList{T}"/>.</exception>
    /// <exception cref="System.NotSupportedException">The <see cref="AHandlerList{T}"/> is read-only.</exception>
    public new T this[int index] { get => base[index];
        set { if (!base[index].Equals(value)) { base[index] = value; OnListChanged(); } } }
    #endregion

    #region ICollection<T> Members
    /// <summary>
    /// Adds an object to the end of the <see cref="AHandlerList{T}"/>.
    /// </summary>
    /// <param name="item">The object to add to the <see cref="AHandlerList{T}"/>.</param>
    /// <exception cref="System.InvalidOperationException">Thrown when list size exceeded.</exception>
    /// <exception cref="System.NotSupportedException">The <see cref="AHandlerList{T}"/> is read-only.</exception>
    public new virtual void Add(T item) { if (maxSize >= 0 && Count == maxSize) throw new InvalidOperationException(); base.Add(item); OnListChanged(); }
    /// <summary>
    /// Removes all items from the <see cref="AHandlerList{T}"/>.
    /// </summary>
    /// <exception cref="System.NotSupportedException">The <see cref="AHandlerList{T}"/> is read-only.</exception>
    public new virtual void Clear() { base.Clear(); OnListChanged(); }

    #endregion

    /// <summary>
    /// The maximum size of the list, or -1 for no limit (read-only).
    /// </summary>
    public long MaxSize => maxSize;

    /// <summary>
    /// Invokes the list change event handler.
    /// </summary>
    protected void OnListChanged()
    {
        handler?.Invoke(this, EventArgs.Empty);
    }
}