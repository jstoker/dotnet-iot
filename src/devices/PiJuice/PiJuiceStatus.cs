﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers.Binary;
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
        /// <returns>PiJuice status</returns>
        public Status GetStatus()
        {
            Status status = new Status();

            var response = _piJuice.ReadCommand(PiJuiceCommand.Status, 1);

            status.IsFault = (response[0] & 0x01) == 0x01;
            status.IsButton = (response[0] & 0x02) == 0x02;
            status.Battery = (BatteryState)(response[0] >> 2 & 0x03);
            status.PowerInput = (PowerInState)(response[0] >> 4 & 0x03);
            status.PowerInput5VoltInput = (PowerInState)(response[0] >> 6 & 0x03);

            return status;
        }

        /// <summary>
        /// Get current fault status of PiJuice
        /// </summary>
        /// <returns>PiJuice fault status</returns>
        public FaultStatus GetFaultStatus()
        {
            FaultStatus faultStatus = new FaultStatus();

            var response = _piJuice.ReadCommand(PiJuiceCommand.FaultEvent, 1);

            faultStatus.ButtonPowerOff = (response[0] & 0x01) == 0x01;
            faultStatus.ForcedPowerOff = (response[0] & 0x02) == 0x02;
            faultStatus.ForcedSystemPowerOff = (response[0] & 0x04) == 0x04;
            faultStatus.WatchdogReset = (response[0] & 0x08) == 0x08;
            faultStatus.BatteryProfileInvalid = (response[0] & 0x20) == 0x20;
            faultStatus.BatteryChargingTemperatureFault = (BatteryChargingTemperatureFault)(response[0] >> 6 & 0x03);

            return faultStatus;
        }

        /// <summary>
        /// Gets event generated by PiJuice buttons presses
        /// </summary>
        /// <returns>List of button event types</returns>
        public List<ButtonEventType> GetButtonEvents()
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.ButtonEvent, 2);

            var buttonEvents = new List<ButtonEventType>(3)
            {
                Enum.IsDefined(typeof(ButtonEventType), response[0] & 0x0F) ? (ButtonEventType)(response[0] & 0x0F) : ButtonEventType.Unknown,
                Enum.IsDefined(typeof(ButtonEventType), (response[0] >> 4) & 0x0F) ? (ButtonEventType)((response[0] >> 4) & 0x0F) : ButtonEventType.Unknown,
                Enum.IsDefined(typeof(ButtonEventType), response[1] & 0x0F) ? (ButtonEventType)(response[1] & 0x0F) : ButtonEventType.Unknown,
            };

            return buttonEvents;
        }

        /// <summary>
        /// Clears generated button event
        /// </summary>
        /// <param name="button">Button to clear button event for</param>
        public void ClearButtonEvent(Button button)
        {
            var array = button switch
            {
                Button.Switch1 => new byte[] { 0xF0, 0xFF },
                Button.Switch2 => new byte[] { 0x0F, 0xFF },
                Button.Switch3 => new byte[] { 0xFF, 0xF0 },
                _ => throw new NotImplementedException()
            };

            _piJuice.WriteCommand(PiJuiceCommand.ButtonEvent, array);
        }

        /// <summary>
        /// Get battery charge level between 0 and 100 percent
        /// </summary>
        /// <returns>Battery charge level percentage</returns>
        public byte GetChargeLevel()
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.ChargeLevel, 1);

            return response[0];
        }

        /// <summary>
        /// Get battery temperature
        /// </summary>
        /// <returns>Battery temperature in celsius</returns>
        public Temperature GetBatteryTemperature()
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.BatteryTemperature, 2);

            return new Temperature(BinaryPrimitives.ReadInt16LittleEndian(response), TemperatureUnit.DegreeCelsius);
        }

        /// <summary>
        /// Get battery voltage
        /// </summary>
        /// <returns>Battery voltage in millivolts</returns>
        public ElectricPotential GetBatteryVoltage()
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.BatteryVoltage, 2);

            return new ElectricPotential(BinaryPrimitives.ReadInt16LittleEndian(response), ElectricPotentialUnit.Millivolt);
        }

        /// <summary>
        /// Get battery current
        /// </summary>
        /// <returns>Battery current in milliamps</returns>
        public ElectricCurrent GetBatteryCurrent()
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.BatteryCurrent, 2);

            int i = BinaryPrimitives.ReadInt16LittleEndian(response);

            return new ElectricCurrent(i, ElectricCurrentUnit.Milliampere);
        }

        /// <summary>
        /// Get supplied voltage
        /// </summary>
        /// <returns>Voltage supplied from the GPIO power output from the PiJuice or when charging, voltage supplied in millivolts</returns>
        public ElectricPotential GetIOVoltage()
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.IOVoltage, 2);

            return new ElectricPotential(BinaryPrimitives.ReadInt16LittleEndian(response), ElectricPotentialUnit.Millivolt);
        }

        /// <summary>
        /// Get supplied current in milliamps
        /// </summary>
        /// <returns>Current supplied from the GPIO power output from the PiJuice or when charging, current supplied in milliamps</returns>
        public ElectricCurrent GetIOCurrent()
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.IOCurrent, 2);

            int i = BinaryPrimitives.ReadInt16LittleEndian(response);

            return new ElectricCurrent(i, ElectricCurrentUnit.Milliampere);
        }

        /// <summary>
        /// Get the color for a specific Led
        /// </summary>
        /// <param name="led">Led to get color for</param>
        /// <returns>Color of Led</returns>
        public Color GetLedState(Led led)
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.LedState + (byte)led, 3);

            return Color.FromArgb(0, response[0], response[1], response[2]);
        }

        /// <summary>
        /// Set the color for a specific Led
        /// </summary>
        /// <param name="led">Led to which color is to be applied</param>
        /// <param name="color">Color for the Led</param>
        public void SetLedState(Led led, Color color)
        {
            _piJuice.WriteCommand(PiJuiceCommand.LedState + (byte)led, new byte[] { color.R, color.G, color.B });
        }

        /// <summary>
        /// Get blinking pattern for a specific Led
        /// </summary>
        /// <param name="led">Led to get blinking pattern for</param>
        /// <returns>Led blinking pattern</returns>
        public LedBlink GetLedBlink(Led led)
        {
            var response = _piJuice.ReadCommand(PiJuiceCommand.LedBlink + (byte)led, 9);

            return new LedBlink
            {
                Led = led,
                BlinkIndefinite = response[0] == 255,
                Count = response[0],
                ColorFirstPeriod = Color.FromArgb(0, response[1], response[2], response[3]),
                FirstPeriod = new TimeSpan(0, 0, 0, 0, response[4] * 10),
                ColorSecondPeriod = Color.FromArgb(0, response[5], response[6], response[7]),
                SecondPeriod = new TimeSpan(0, 0, 0, 0, response[8] * 10)
            };
        }

        /// <summary>
        /// Set blinking pattern for a specific Led
        /// </summary>
        /// <param name="ledBlink">Led blinking pattern</param>
        public void SetLedBlink(LedBlink ledBlink)
        {
            if (ledBlink.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(ledBlink.Count));
            }

            if (ledBlink.FirstPeriod.TotalMilliseconds < 10 || ledBlink.FirstPeriod.TotalMilliseconds > 2550)
            {
                throw new ArgumentOutOfRangeException(nameof(ledBlink.FirstPeriod));
            }

            if (ledBlink.SecondPeriod.TotalMilliseconds < 10 || ledBlink.SecondPeriod.TotalMilliseconds > 2550)
            {
                throw new ArgumentOutOfRangeException(nameof(ledBlink.SecondPeriod));
            }

            var data = new byte[9];

            data[0] = (byte)(ledBlink.Count & 0xFF);
            data[1] = ledBlink.ColorFirstPeriod.R;
            data[2] = ledBlink.ColorFirstPeriod.G;
            data[3] = ledBlink.ColorFirstPeriod.B;
            data[4] = (byte)((int)(ledBlink.FirstPeriod.TotalMilliseconds / 10) & 0xFF);
            data[5] = ledBlink.ColorSecondPeriod.R;
            data[6] = ledBlink.ColorSecondPeriod.G;
            data[7] = ledBlink.ColorSecondPeriod.B;
            data[8] = (byte)((int)(ledBlink.SecondPeriod.TotalMilliseconds / 10) & 0xFF);

            _piJuice.WriteCommand(PiJuiceCommand.LedBlink + (byte)ledBlink.Led, data);
        }
    }
}
