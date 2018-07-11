﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.EventArgs;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public class MouseSettings : SettingsSubsection
    {
        protected override string Header => "Mouse";

        private readonly BindableBool rawInputToggle = new BindableBool();
        private Bindable<string> ignoredInputHandler;
        private SensitivitySetting sensitivity;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager osuConfig, FrameworkConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "Raw Input",
                    Bindable = rawInputToggle
                },
                sensitivity = new SensitivitySetting
                {
                    LabelText = "Cursor Sensitivity",
                    Bindable = config.GetBindable<double>(FrameworkSetting.CursorSensitivity)
                },
                new SettingsCheckbox
                {
                    LabelText = "Map absolute input to window",
                    Bindable = config.GetBindable<bool>(FrameworkSetting.MapAbsoluteInputToWindow)
                },
                new SettingsEnumDropdown<ConfineMouseMode>
                {
                    LabelText = "Confine mouse cursor to window",
                    Bindable = config.GetBindable<ConfineMouseMode>(FrameworkSetting.ConfineMouseMode),
                },
                new SettingsCheckbox
                {
                    LabelText = "Disable mouse wheel during gameplay",
                    Bindable = osuConfig.GetBindable<bool>(OsuSetting.MouseDisableWheel)
                },
                new SettingsCheckbox
                {
                    LabelText = "Disable mouse buttons during gameplay",
                    Bindable = osuConfig.GetBindable<bool>(OsuSetting.MouseDisableButtons)
                },
            };

            rawInputToggle.ValueChanged += enabled =>
            {
                // this is temporary until we support per-handler settings.
                const string raw_mouse_handler = @"OpenTKRawMouseHandler";
                const string standard_mouse_handler = @"OpenTKMouseHandler";

                ignoredInputHandler.Value = enabled ? standard_mouse_handler : raw_mouse_handler;
            };

            ignoredInputHandler = config.GetBindable<string>(FrameworkSetting.IgnoredInputHandlers);
            ignoredInputHandler.ValueChanged += handler =>
            {
                bool raw = !handler.Contains("Raw");
                rawInputToggle.Value = raw;
                sensitivity.Bindable.Disabled = !raw;
            };

            ignoredInputHandler.TriggerChange();
        }

        private class SensitivitySetting : SettingsSlider<double, SensitivitySlider>
        {
            public override Bindable<double> Bindable
            {
                get { return ((SensitivitySlider)Control).Sensitivity; }

                set
                {
                    BindableDouble doubleValue = (BindableDouble)value;

                    // create a second layer of bindable so we can only handle state changes when not being dragged.
                    ((SensitivitySlider)Control).Sensitivity = doubleValue;

                    // this bindable will still act as the "interactive" bindable displayed during a drag.
                    base.Bindable = new BindableDouble(doubleValue.Value)
                    {
                        Default = doubleValue.Default,
                        MinValue = doubleValue.MinValue,
                        MaxValue = doubleValue.MaxValue
                    };

                    // one-way binding to update the sliderbar with changes from external actions.
                    doubleValue.DisabledChanged += disabled => base.Bindable.Disabled = disabled;
                    doubleValue.ValueChanged += newValue => base.Bindable.Value = newValue;
                }
            }

            public SensitivitySetting()
            {
                KeyboardStep = 0.01f;
            }
        }

        private class SensitivitySlider : OsuSliderBar<double>
        {
            public Bindable<double> Sensitivity;

            public SensitivitySlider()
            {
                Current.ValueChanged += newValue =>
                {
                    if (!isDragging && Sensitivity != null)
                        Sensitivity.Value = newValue;
                };
            }

            private bool isDragging;

            protected override bool OnDragStart(DragStartEventArgs args)
            {
                isDragging = true;
                return base.OnDragStart(args);
            }

            protected override bool OnDragEnd(DragEndEventArgs args)
            {
                isDragging = false;
                Current.TriggerChange();

                return base.OnDragEnd(args);
            }

            public override string TooltipText => Current.Disabled ? "Enable raw input to adjust sensitivity" : Current.Value.ToString(@"0.##x");
        }
    }
}
