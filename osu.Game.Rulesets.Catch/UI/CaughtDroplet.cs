// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Catch.Objects.Drawables.Pieces;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CaughtDroplet : PoolableCaughtObject
    {
        public CaughtDroplet()
        {
            AccentColour.BindValueChanged(_ => updatePiece());
        }

        private void updatePiece()
        {
            InternalChild = new DropletPiece
            {
                AccentColour = { BindTarget = AccentColour },
            };
        }
    }
}
