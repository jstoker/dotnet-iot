﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.PiJuiceDevice.Models
{
    /// <summary>
    /// TODO: Fill In
    /// </summary>
    public class FaultStatus
    {
        /// <summary>
        /// TODO: Fill In
        /// </summary>
        public bool ButtonPowerOff { get; set; }

        /// <summary>
        /// TODO: Fill In
        /// </summary>
        public bool ForcedPowerOff { get; set; }

        /// <summary>
        /// TODO: Fill In
        /// </summary>
        public bool ForcedSystemPowerOff { get; set; }

        /// <summary>
        /// TODO: Fill In
        /// </summary>
        public bool WatchdogReset { get; set; }

        /// <summary>
        /// TODO: Fill In
        /// </summary>
        public bool BatteryProfileInvalid { get; set; }

        /// <summary>
        /// TODO: Fill In
        /// </summary>
        public BatteryChargingTempFault BatteryChargingTempFault { get; set; }
    }
}
