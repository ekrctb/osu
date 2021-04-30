// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Performance;
using osu.Framework.Graphics.Pooling;

namespace osu.Game.Rulesets.Objects.Pooling
{
    public class DrawablePoolWithLifetime<TEntry, TDrawable> : PoolableDrawableLifetimeContainer<TEntry, TDrawable>
        where TEntry : LifetimeEntry
        where TDrawable : PoolableDrawableWithLifetime<TEntry>, new()
    {
        private readonly DrawablePool<TDrawable> pool;

        public DrawablePoolWithLifetime(int initialSize, int? maximumSize = null)
        {
            base.AddInternal(pool = new DrawablePool<TDrawable>(initialSize, maximumSize));
        }

        protected override TDrawable GetDrawable(TEntry entry)
        {
            var drawable = pool.Get();
            drawable.Apply(entry);
            return drawable;
        }
    }
}
