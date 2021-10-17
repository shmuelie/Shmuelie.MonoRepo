// <copyright file="StrongKeyedCollection.cs" company="Shmueli Englard">
// Copyright (c) Shmueli Englard. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
#if NETSTANDARD2_0
using System.Diagnostics.CodeAnalysis;
#endif

namespace Shmuelie.Common
{
    /// <summary>
    ///     Implementation of <see cref="KeyedCollection{TKey, TItem}"/> that does not require constant sub-typing.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <seealso cref="KeyedCollection{TKey, TItem}" />
    public class StrongKeyedCollection<TKey, TItem> : KeyedCollection<TKey, TItem>
    {
        /// <summary>
        ///     Method to extract the key from the specified element.
        /// </summary>
        private readonly Func<TItem, TKey> _getKeyForItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="StrongKeyedCollection{TKey, TItem}"/> class.
        /// </summary>
        /// <param name="getKeyForItem">Method to extract the key from the specified element.</param>
        public StrongKeyedCollection(Func<TItem, TKey> getKeyForItem)
        {
            _getKeyForItem = getKeyForItem;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StrongKeyedCollection{TKey, TItem}"/> class.
        /// </summary>
        /// <param name="getKeyForItem">Method to extract the key from the specified element.</param>
        /// <param name="comparer">The implementation of the <see cref="IEqualityComparer{T}"/> generic interface to use when comparing keys, or <see langword="null"/> to use the default equality comparer for the type of the key, obtained from <see cref="EqualityComparer{T}.Default"/>.</param>
        public StrongKeyedCollection(Func<TItem, TKey> getKeyForItem, IEqualityComparer<TKey> comparer)
            : base(comparer)
        {
            _getKeyForItem = getKeyForItem;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StrongKeyedCollection{TKey, TItem}"/> class.
        /// </summary>
        /// <param name="getKeyForItem">Method to extract the key from the specified element.</param>
        /// <param name="comparer">The implementation of the <see cref="IEqualityComparer{T}"/> generic interface to use when comparing keys, or <see langword="null"/> to use the default equality comparer for the type of the key, obtained from <see cref="EqualityComparer{T}.Default"/>.</param>
        /// <param name="dictionaryCreationThreshold">The number of elements the collection can hold without creating a lookup dictionary (0 creates the lookup dictionary when the first item is added), or -1 to specify that a lookup dictionary is never created.</param>
        public StrongKeyedCollection(Func<TItem, TKey> getKeyForItem, IEqualityComparer<TKey> comparer, int dictionaryCreationThreshold)
            : base(comparer, dictionaryCreationThreshold)
        {
            _getKeyForItem = getKeyForItem;
        }

        /// <summary>
        ///     Gets the keys in the <see cref="StrongKeyedCollection{TKey, TItem}"/>.
        /// </summary>
        /// <value>
        ///     The keys in the <see cref="StrongKeyedCollection{TKey, TItem}"/>.
        /// </value>
        public IEnumerable<TKey> Keys => this.Select(GetKeyForItem);

        /// <summary>
        ///     Tries to add an object to the end of the <see cref="StrongKeyedCollection{TKey, TItem}"/>.
        /// </summary>
        /// <param name="item">The object to be added to the end of the <see cref="StrongKeyedCollection{TKey, TItem}"/>. Cannot be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if able to add <paramref name="item"/>; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="item"/> is <see langword="null"/>.</exception>
        public bool TryAdd(TItem item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (Contains(GetKeyForItem(item)))
            {
                return false;
            }

            Add(item);
            return true;
        }

#if NETSTANDARD2_0
        /// <summary>
        ///     Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="item">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <typeparamref name="TItem"/> parameter. This parameter is passed uninitialized.</param>
        /// <returns><see langword="true"/> if the <see cref="StrongKeyedCollection{TKey, TItem}"/> contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TItem item)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (Dictionary is object)
            {
                return Dictionary.TryGetValue(key, out item);
            }

            foreach (TItem itemInItems in Items)
            {
                TKey keyInItems = GetKeyForItem(itemInItems);
                if (keyInItems is object && Comparer.Equals(key, keyInItems))
                {
                    item = itemInItems;
                    return true;
                }
            }

            item = default;
            return false;
        }
#endif

        /// <summary>
        ///     Gets an item or adds the item if it is not in the <see cref="StrongKeyedCollection{TKey, TItem}"/>.
        /// </summary>
        /// <param name="item">The item to get or add.</param>
        /// <returns>If <paramref name="item"/> is already in the <see cref="StrongKeyedCollection{TKey, TItem}"/>, the <typeparamref name="TItem"/> in the <see cref="StrongKeyedCollection{TKey, TItem}"/>; otherwise, <paramref name="item"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="item"/> is <see langword="null"/>.</exception>
        public TItem GetOrAdd(TItem item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            TKey key = GetKeyForItem(item);
            if (Contains(key))
            {
                return this[key];
            }

            Add(item);
            return item;
        }

        /// <summary>
        ///     Creates an <see cref="IDictionary{TKey, TValue}"/> from this collection.
        /// </summary>
        /// <returns>An <see cref="IDictionary{TKey, TValue}"/> that contains keys and values from this collection.</returns>
        public IDictionary<TKey, TItem> ToDictionary()
        {
            if (Dictionary is null)
            {
                return new Dictionary<TKey, TItem>(Dictionary);
            }

            return this.ToDictionary(GetKeyForItem);
        }

        /// <summary>
        /// When implemented in a derived class, extracts the key from the specified element.
        /// </summary>
        /// <param name="item">The element from which to extract the key.</param>
        /// <returns>
        /// The key for the specified element.
        /// </returns>
        protected override TKey GetKeyForItem(TItem item) => _getKeyForItem(item);

        /// <summary>Inserts an element into the <see cref="KeyedCollection{TKey, TItem}" /> at the specified index.</summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <para><paramref name="index">index</paramref> is less than 0.</para>
        ///     <para>--or--.</para>
        ///     <para><paramref name="index">index</paramref> is greater than <see cref="Collection{T}.Count" />.</para>
        /// </exception>
        /// <exception cref="ArgumentNullException"><para><paramref name="item"/> is <see langword="null"/>.</para></exception>
        protected override void InsertItem(int index, TItem item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            base.InsertItem(index, item);
        }

        /// <summary>Replaces the item at the specified index with the specified item.</summary>
        /// <param name="index">The zero-based index of the item to be replaced.</param>
        /// <param name="item">The new item.</param>
        /// <exception cref="ArgumentNullException"><paramref name="item"/> is <see langword="null"/>.</exception>
        protected override void SetItem(int index, TItem item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            base.SetItem(index, item);
        }
    }
}
