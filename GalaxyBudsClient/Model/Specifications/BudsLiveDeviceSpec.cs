using System;
using System.Collections.Generic;

using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Touchpad;

namespace GalaxyBudsClient.Model.Specifications
{
    public class BudsLiveDeviceSpec : IDeviceSpec
    {
        public Dictionary<Features, FeatureRule?> Rules => new()
            {
                { Features.SeamlessConnection, new FeatureRule(3, "R180XXU0ATF2")  },
                { Features.AmbientPassthrough, null },
                { Features.Anc, null },
                { Features.GamingMode, null },
                { Features.CaseBattery, null },
                { Features.FragmentedMessages, null },
                { Features.StereoPan, new FeatureRule(7, "R180XXU0AUB5") },
                { Features.BixbyWakeup, null },
                { Features.FirmwareUpdates, null },
                { Features.LegacyNoiseControlMode, null },
                { Features.BuildInfo, null },
                { Features.Voltage, null },
                { Features.DebugSku, null }
            };
        
        public Models Device => Models.BudsLive;
        public string DeviceBaseName => "Buds Live";
        public ITouchOption TouchMap => new BudsLiveTouchOption();
        public Guid ServiceUuid => Uuids.BudsLive;

        public IReadOnlyCollection<ItemType> TrayShortcuts => Array.AsReadOnly(
            [
                ItemType.ToggleAnc,
                ItemType.ToggleEqualizer,
                ItemType.LockTouchpad
            ]
        );
        
        public string IconResourceKey => "Bean";
        public int MaximumAmbientVolume => 0; /* ambient sound unsupported */
    }
}