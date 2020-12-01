// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.Objects.Drawables.Pieces;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CaughtFruit : PoolableCaughtObject
    {
        public readonly Bindable<FruitVisualRepresentation> VisualRepresentation = new Bindable<FruitVisualRepresentation>();

        public CaughtFruit()
        {
            AccentColour.BindValueChanged(_ => updatePiece());
            VisualRepresentation.BindValueChanged(_ => updatePiece(), true);
        }

        private void updatePiece()
        {
            InternalChild = new FruitPiece
            {
                AccentColour = { BindTarget = AccentColour },
                VisualRepresentation = { BindTarget = VisualRepresentation },
            };
        }
    }
}
