// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.EventArgs;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;

namespace osu.Game.Graphics.Containers
{
    public class OsuHoverContainer : OsuClickableContainer
    {
        protected Color4 HoverColour;

        protected Color4 IdleColour = Color4.White;

        protected virtual IEnumerable<Drawable> EffectTargets => new[] { Content };

        protected override bool OnHover(HoverEventArgs args)
        {
            EffectTargets.ForEach(d => d.FadeColour(HoverColour, 500, Easing.OutQuint));
            return base.OnHover(args);
        }

        protected override void OnHoverLost(HoverLostEventArgs args)
        {
            EffectTargets.ForEach(d => d.FadeColour(IdleColour, 500, Easing.OutQuint));
            base.OnHoverLost(args);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            HoverColour = colours.Yellow;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            EffectTargets.ForEach(d => d.FadeColour(IdleColour));
        }
    }
}
