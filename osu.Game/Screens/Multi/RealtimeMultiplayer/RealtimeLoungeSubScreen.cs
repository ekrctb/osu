// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Logging;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.RealtimeMultiplayer;
using osu.Game.Screens.Multi.Lounge;
using osu.Game.Screens.Multi.Lounge.Components;
using osu.Game.Screens.Multi.Match;

namespace osu.Game.Screens.Multi.RealtimeMultiplayer
{
    public class RealtimeLoungeSubScreen : LoungeSubScreen
    {
        protected override FilterControl CreateFilterControl() => new RealtimeFilterControl();

        protected override RoomSubScreen CreateRoomSubScreen(Room room) => new RealtimeMatchSubScreen(room);

        [Resolved]
        private StatefulMultiplayerClient client { get; set; }

        public override void Open(Room room)
        {
            if (!client.IsConnected.Value)
            {
                Logger.Log("Not currently connected to the multiplayer server.", LoggingTarget.Runtime, LogLevel.Important);
                return;
            }

            base.Open(room);
        }
    }
}
