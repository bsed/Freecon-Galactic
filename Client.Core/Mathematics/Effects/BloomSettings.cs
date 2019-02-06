#region File Description

//-----------------------------------------------------------------------------
// BloomSettings.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

namespace Freecon.Client.Mathematics.Effects
{
    /// <summary>
    /// Class holds all the settings used to tweak the bloom effect.
    /// </summary>
    public class BloomSettings
    {
        #region Fields

        // Name of a preset bloom setting, for display to the user.
        public readonly float BaseIntensity;


        // Independently control the color saturation of the bloom and
        // base images. Zero is totally desaturated, 1.0 leaves saturation
        // unchanged, while higher values increase the saturation level.
        public readonly float BaseSaturation;
        public readonly float BloomIntensity;
        public readonly float BloomSaturation;
        public readonly float BloomThreshold;


        // Controls how much blurring is applied to the bloom image.
        // The typical range is from 1 up to 10 or so.
        public readonly float BlurAmount;
        public readonly string Name;

        #endregion

        /// <summary>
        /// Table of preset bloom settings, used by the sample program.
        /// </summary>
        public static BloomSettings[] PresetSettings =
            {
                //                Name           Thresh  Blur Bloom  Base  BloomSat BaseSat
                new BloomSettings("Default", 0.02f, 8, 5, 3, 1, 1.5f),
                new BloomSettings("Soft", .25f, 3, 2, 1, 1, 1),
                new BloomSettings("Desaturated", 0.25f, 4, 2, 1, 2, 0),
                new BloomSettings("Saturated", 0.25f, 4, 2, 1, 2, 0),
                new BloomSettings("Blurry", 0, 2, 1, 0.1f, 1, 1),
                new BloomSettings("Subtle", 0.5f, 2, 1, 1, 1, 1),
                new BloomSettings("Custom", 0.35f, 3.5f, 1.8f, 1f, 1f, 1f),
            };

        /// <summary>
        /// Constructs a new bloom settings descriptor.
        /// </summary>
        public BloomSettings(string name, float bloomThreshold, float blurAmount,
                             float bloomIntensity, float baseIntensity,
                             float bloomSaturation, float baseSaturation)
        {
            Name = name;
            BloomThreshold = bloomThreshold;
            BlurAmount = blurAmount;
            BloomIntensity = bloomIntensity;
            BaseIntensity = baseIntensity;
            BloomSaturation = bloomSaturation;
            BaseSaturation = baseSaturation;
        }
    }
}