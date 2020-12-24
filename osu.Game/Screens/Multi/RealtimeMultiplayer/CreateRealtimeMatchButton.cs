// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.RealtimeMultiplayer;
using osu.Game.Screens.Multi.Match.Components;

namespace osu.Game.Screens.Multi.RealtimeMultiplayer
{
    public class CreateRealtimeMatchButton : PurpleTriangleButton
    {
        [BackgroundDependencyLoader]
        private void load(StatefulMultiplayerClient multiplayerClient)
        {
            Triangles.TriangleScale = 1.5f;

            Text = "Create match";

            ((IBindable<bool>)Enabled).BindTo(multiplayerClient.IsConnected);
        }
    }
}
