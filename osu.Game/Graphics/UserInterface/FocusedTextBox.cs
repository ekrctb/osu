// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using System;
using osu.Framework.EventArgs;
using osu.Game.Input.Bindings;
using OpenTK.Input;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A textbox which holds focus eagerly.
    /// </summary>
    public class FocusedTextBox : OsuTextBox
    {
        protected override Color4 BackgroundUnfocused => new Color4(10, 10, 10, 255);
        protected override Color4 BackgroundFocused => new Color4(10, 10, 10, 255);

        public Action Exit;

        private bool focus;

        public bool HoldFocus
        {
            get { return focus; }
            set
            {
                focus = value;
                if (!focus && HasFocus)
                    base.KillFocus();
            }
        }

        // We may not be focused yet, but we need to handle keyboard input to be able to request focus
        public override bool HandleKeyboardInput => HoldFocus || base.HandleKeyboardInput;

        protected override void OnFocus(FocusEventArgs args)
        {
            base.OnFocus(args);
            BorderThickness = 0;
        }

        protected override bool OnKeyDown(KeyDownEventArgs args)
        {
            if (!HasFocus) return false;

            if (args.Key == Key.Escape)
                return false; // disable the framework-level handling of escape key for confority (we use GlobalAction.Back).

            return base.OnKeyDown(args);
        }

        public override bool OnPressed(GlobalAction action)
        {
            if (action == GlobalAction.Back)
            {
                if (Text.Length > 0)
                {
                    Text = string.Empty;
                    return true;
                }
            }

            return base.OnPressed(action);
        }

        protected override void KillFocus()
        {
            base.KillFocus();
            Exit?.Invoke();
        }

        public override bool RequestsFocus => HoldFocus;
    }
}
