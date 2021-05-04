// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Performance;

namespace osu.Game.Rulesets.Objects.Pooling
{
    /// <summary>
    /// When an <typeparamref name="TEntry"/> became alive, a <typeparamref name="TDrawable"/> is added to this container.
    /// When an <typeparamref name="TEntry"/> became dead, a <typeparamref name="TDrawable"/> is removed.
    /// </summary>
    public abstract class PoolableDrawableLifetimeContainer<TEntry, TDrawable> : CompositeDrawable
        where TEntry : LifetimeEntry
        where TDrawable : PoolableDrawableWithLifetime<TEntry>
    {
        /// <summary>
        /// All entries in this container including dead entries.
        /// </summary>
        public IEnumerable<TEntry> Entries => allEntries;

        /// <summary>
        /// All entries and drawables currently alive.
        /// </summary>
        public IEnumerable<(TEntry Entry, TDrawable Drawable)> AliveEntries => aliveDrawableMap.Select(x => (x.Key, x.Value));

        /// <summary>
        /// The amount of time prior to the current time within which <see cref="LifetimeEntry"/>s should be considered alive.
        /// </summary>
        internal double PastLifetimeExtension { get; set; }

        /// <summary>
        /// The amount of time after the current time within which <see cref="LifetimeEntry"/>s should be considered alive.
        /// </summary>
        internal double FutureLifetimeExtension { get; set; }

        private readonly Dictionary<TEntry, TDrawable> aliveDrawableMap = new Dictionary<TEntry, TDrawable>();

        private readonly LifetimeEntryManager lifetimeManager = new LifetimeEntryManager();
        private readonly HashSet<TEntry> allEntries = new HashSet<TEntry>();

        protected PoolableDrawableLifetimeContainer()
        {
            lifetimeManager.EntryBecameAlive += entryBecameAlive;
            lifetimeManager.EntryBecameDead += entryBecameDead;
            lifetimeManager.EntryCrossedBoundary += entryCrossedBoundary;
        }

        /// <summary>
        /// Add a pooled drawable entry.
        /// </summary>
        public virtual void Add(TEntry entry)
        {
            allEntries.Add(entry);
            lifetimeManager.AddEntry(entry);
        }

        /// <summary>
        /// Remove a pooled drawable entry.
        /// </summary>
        public virtual bool Remove(TEntry entry)
        {
            if (!lifetimeManager.RemoveEntry(entry)) return false;

            allEntries.Remove(entry);
            return true;
        }

        public virtual void Clear()
        {
            lifetimeManager.ClearEntries();
            Debug.Assert(aliveDrawableMap.Count == 0, "All entries should have been removed");
        }

        /// <summary>
        /// Get a drawable from entry.
        /// <see cref="PoolableDrawableWithLifetime{TEntry}.Apply"/> should be called with <paramref name="entry"/> for the returning drawable.
        /// </summary>
        protected abstract TDrawable GetDrawable(TEntry entry);

        /// <summary>
        /// Add the corresponding drawable from this container when an entry became alive.
        /// </summary>
        protected virtual void AddDrawable(TEntry entry, TDrawable drawable) => AddInternal(drawable);

        /// <summary>
        /// Remove the corresponding drawable from this container when an entry became dead and its drawable.
        /// </summary>
        protected virtual void RemoveDrawable(TEntry entry, TDrawable drawable) => RemoveInternal(drawable);

        private void entryBecameAlive(LifetimeEntry entry) => addDrawable((TEntry)entry);
        private void entryBecameDead(LifetimeEntry entry) => removeDrawable((TEntry)entry);

        private void addDrawable(TEntry entry)
        {
            Debug.Assert(!aliveDrawableMap.ContainsKey(entry));

            var drawable = GetDrawable(entry);
            aliveDrawableMap[entry] = drawable;

            AddDrawable(entry, drawable);
        }

        private void removeDrawable(TEntry entry)
        {
            Debug.Assert(aliveDrawableMap.ContainsKey(entry));

            var drawable = aliveDrawableMap[entry];
            RemoveDrawable(entry, drawable);

            aliveDrawableMap.Remove(entry);
        }

        private void entryCrossedBoundary(LifetimeEntry entry, LifetimeBoundaryKind kind, LifetimeBoundaryCrossingDirection direction) =>
            OnEntryCrossedBoundary((TEntry)entry, kind, direction);

        protected virtual void OnEntryCrossedBoundary(TEntry entry, LifetimeBoundaryKind kind, LifetimeBoundaryCrossingDirection direction)
        {
        }

        protected override bool CheckChildrenLife()
        {
            bool aliveChanged = base.CheckChildrenLife();
            aliveChanged |= lifetimeManager.Update(Time.Current - PastLifetimeExtension, Time.Current + FutureLifetimeExtension);
            return aliveChanged;
        }
    }
}
