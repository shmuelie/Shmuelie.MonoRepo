// <copyright file="DisposingCollection.cs" company="Shmueli Englard">
// Copyright (c) Shmueli Englard. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Shmuelie.Common
{
    /// <summary>
    ///     Collection of types implementing <see cref="IDisposable"/>.
    /// </summary>
    /// <remarks>
    ///     <para>Calls <see cref="IDisposable.Dispose()"/> when an item is removed.</para>
    ///     <para>When <see cref="Dispose()"/> is called the collection is cleared, with each item's <see cref="IDisposable.Dispose()"/> called.</para>
    /// </remarks>
    /// <seealso cref="Collection{T}"/>
    /// <seealso cref="IDisposable"/>
    /// <threadsafety static="true" instance="false"/>
    public sealed class DisposingCollection : Collection<IDisposable>, IDisposable
    {
        /// <summary>
        ///     Clears the collection and calls <see cref="IDisposable.Dispose"/> on all items.
        /// </summary>
        /// <seealso cref="Collection{T}.Clear()"/>
        public void Dispose() => ClearItems();

        /// <summary>
        ///     Removes all elements from the <see cref="DisposingCollection"/>.
        /// </summary>
        /// <seealso cref="Collection{T}.Clear()"/>
        protected override void ClearItems()
        {
            List<Exception> exceptions = new List<Exception>(Count);
            foreach (IDisposable disposable in this)
            {
                try
                {
                    disposable?.Dispose();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            base.ClearItems();
            if (exceptions.Count == 1)
            {
                throw exceptions[0];
            }

            if (exceptions.Count > 1)
            {
                throw new AggregateException("Issue disposing collection items", exceptions);
            }
        }

        /// <summary>
        ///     Removes the element at the specified index of the <see cref="DisposingCollection"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <para><paramref name="index"/> is less than zero.</para>
        ///     <para>--or--.</para>
        ///     <para><paramref name="index"/> is equal to or greater than <see cref="Collection{T}.Count"/>.</para>
        /// </exception>
        /// <seealso cref="Collection{T}.RemoveAt(int)"/>
        protected override void RemoveItem(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "index must be greater than or equal to zero and less than Count.");
            }

            try
            {
                this[index]?.Dispose();
            }
            finally
            {
                base.RemoveItem(index);
            }
        }
    }
}
