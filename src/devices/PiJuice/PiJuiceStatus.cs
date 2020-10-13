﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Drawing;
using Iot.Device.PiJuiceDevice.Models;
using UnitsNet;
using UnitsNet.Units;

namespace Iot.Device.PiJuiceDevice
{
    /// <summary>
    /// PiJuiceStatus class to support status of the PiJuice
    /// </summary>
    public class PiJuiceStatus
    {
        private readonly PiJuice _piJuice;

        /// <summary>
        /// PiJuiceStatus constructor
        /// </summary>
        /// <param name="piJuice">The PiJuice class</param>
        public PiJuiceStatus(PiJuice piJuice)
        {
            _piJuice = piJuice;
        }

        /// <summary>
        /// Get basic PiJuice status information
        /// </summary>
        public Status GetStatus()
        {
            Status status = new Status();

            var response = _piJuice.ReadCommand(PiJuiceCommand.Status, 1);

            status.IsFault = (response[0] & 0x01) == 0x01;
            status.IsButton = (response[0] & 0x02) == 0x02;
            status.Battery = (BatteryState)(response[0] >> 2 & 0x03);
            status.PowerInput = (PowerInState)(response[0] >> 4 & 0x03);
            status.PowerInput5vIo = (PowerInState)(response[0] >> 6 & 0x03);

            return status;
        }

        /// <summary>
        /// Get current fault status of PiJuice
        /// </summary>
        public FaultStatus GetFaultStatus()
        {
            FaultStatus faultStatus = new FaultStatus();

            var response = _piJuice.ReadCommand(PiJuiceCommand.FaultEvent, 1);

            faultStatus.ButtonPowerOff = (response[0] & 0x01) == 0x01;
            faultStatus.ForcedPowerOff = (response[0] & 0x02) == 0x02;
            faultStatus.ForcedSystemPowerOff = (response[0] & 0x04) == 0x04;
            faultStatus.WatchdogReset = (response[0] & 0x08) == 0x08;
            faultStatus.BatteryProfileInvalid = (response[0] & 0x20) == 0x20;
            faultStatus.BatteryChargingTempFault = (BatteryChargingTempFault)(response[0] >> 6 & 0x03);

            return faultStatus;
        }

        /// <summary>
        /// TODO: Fill In
        /// </summary>
        public List<ButtonEventType> GetButtonEvents()
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.ButtonEvent, 2);

            var buttonEvents = new List<ButtonEventType>(3)
            {
                // TODO: Handle Unknown
                (ButtonEventType)(response[0] & 0x0F),
                (ButtonEventType)((response[0] >> 4) & 0x0F),
                (ButtonEventType)(response[1] & 0x0F)
            };

            return buttonEvents;
        }

        /// <summary>
        /// TODO: Fill In
        /// </summary>
        public void ClearButtonEvent(Button button)
        {
            var array = button switch
            {
                Button.SW1 => new byte[] { 0xF0, 0xFF, 0 },
                Button.SW2 => new byte[] { 0x0F, 0xFF, 0 },
                Button.SW3 => new byte[] { 0xFF, 0xF0, 0 },
                _ => throw new NotImplementedException()
            };

            _piJuice.WriteCommand(PiJuiceCommand.ButtonEvent, array);
        }

        /// <summary>
        /// Get battery charge level between 0 and 100 percent
        /// </summary>
        public byte GetChargeLevel()
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.ChargeLevel, 1);

            return response[0];
        }

        /// <summary>
        /// Get battery temperature in celsius
        /// </summary>
        public Temperature GetBatteryTemperature()
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.BatteryTemperature, 2);

            return new Temperature((response[1] << 8) | response[0], TemperatureUnit.DegreeCelsius);
        }

        /// <summary>
        /// Get battery voltage in millivolts
        /// </summary>
        public ElectricPotential GetBatteryVoltage()
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.BatteryVoltage, 2);

            return new ElectricPotential((response[1] << 8) | response[0], ElectricPotentialUnit.Millivolt);
        }

        /// <summary>
        /// Get battery current in milliamps
        /// </summary>
        public ElectricCurrent GetBatteryCurrent()
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.BatteryCurrent, 2);

            var i = (response[1] << 8) | response[0];
            if ((i & (1 << 15)) == (1 << 15))
            {
                i -= (1 << 16);
            }

            return new ElectricCurrent(i, ElectricCurrentUnit.Milliampere);
        }

        /// <summary>
        /// Get supplied voltage in millivolts
        /// </summary>
        public ElectricPotential GetIOVoltage()
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.IOVoltage, 2);

            return new ElectricPotential((response[1] << 8) | response[0], ElectricPotentialUnit.Millivolt);
        }

        /// <summary>
        /// Get supplied current in milliamps
        /// </summary>
        public ElectricCurrent GetIOCurrent()
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.IOCurrent, 2);

            var i = (response[1] << 8) | response[0];
            if ((i & (1 << 15)) == (1 << 15))
            {
                i -= (1 << 16);
            }

            return new ElectricCurrent(i, ElectricCurrentUnit.Milliampere);
        }

        /// <summary>
        /// Get the color for a specific LED
        /// </summary>
        public Color GetLEDState(LED led)
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.LEDState + (byte)led, 3);

            return Color.FromArgb(0, response[0], response[1], response[2]);
        }

        /// <summary>
        /// Set the color for a specific LED
        /// </summary>
        public void SetLedState(LED led, Color color)
        {
            _piJuice.WriteCommand(PiJuiceCommand.LEDState + (byte)led, new byte[] { color.R, color.G, color.B });
        }

        /// <summary>
        /// Get blinking pattern for a specific LED
        /// </summary>
        public LedBlink GetLedBlink(LED led)
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.LEDBlink + (byte)led, 9);

            return new LedBlink
            {
                Led = led,
                BlinkIndefinite = response[0] == 255,
                Count = response[0],
                RGB1 = Color.FromArgb(0, response[1], response[2], response[3]),
                Period1 = response[4] * 10,
                RGB2 = Color.FromArgb(0, response[5], response[6], response[7]),
                Period2 = response[8] * 10
            };
        }

        /*/// <summary>
        /// Set blinking pattern for a specific LED
        /// </summary>
        public void SetLedBlink(LedBlink ledBlink)
        {
            if (ledBlink.Count < 1 || ledBlink.Count > 254)
            {
                throw new ArgumentOutOfRangeException("dfd");
            }

            if (ledBlink.Period1 < 10 || ledBlink.Period1 > 2550)
            {
                throw new ArgumentOutOfRangeException("dfd");
            }

            if (ledBlink.Period2 < 10 || ledBlink.Period2 > 2550)
            {
                throw new ArgumentOutOfRangeException("dfd");
            }

            _piJuice.WriteCommand(PiJuiceCommand.LEDBlink + (byte)ledBlink.Led, new byte[] { color.R, color.G, color.B });
        }*/
    }
}
