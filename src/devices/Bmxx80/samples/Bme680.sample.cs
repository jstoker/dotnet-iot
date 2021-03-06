﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.PowerMode;
using Iot.Device.Common;
using UnitsNet;

Console.WriteLine("Hello BME680!");

// The I2C bus ID on the Raspberry Pi 3.
const int busId = 1;
// set this to the current sea level pressure in the area for correct altitude readings
Pressure defaultSeaLevelPressure = WeatherHelper.MeanSeaLevel;

I2cConnectionSettings i2cSettings = new(busId, Bme680.DefaultI2cAddress);
I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);

using Bme680 bme680 = new Bme680(i2cDevice, Temperature.FromDegreesCelsius(20.0));

while (true)
{
    // get the time a measurement will take with the current settings
    var measurementDuration = bme680.GetMeasurementDuration(bme680.HeaterProfile);

    // 10 consecutive measurement with default settings
    for (var i = 0; i < 10; i++)
    {
        // This instructs the sensor to take a measurement.
        bme680.SetPowerMode(Bme680PowerMode.Forced);

        // wait while measurement is being taken
        Thread.Sleep(measurementDuration.ToTimeSpan());

        // Print out the measured data
        bme680.TryReadTemperature(out var tempValue);
        bme680.TryReadPressure(out var preValue);
        bme680.TryReadHumidity(out var humValue);
        bme680.TryReadGasResistance(out var gasResistance);
        var altValue = WeatherHelper.CalculateAltitude(preValue, defaultSeaLevelPressure, tempValue);

        Console.WriteLine($"Gas resistance: {gasResistance:0.##}Ohm");
        Console.WriteLine($"Temperature: {tempValue.DegreesCelsius:0.#}\u00B0C");
        Console.WriteLine($"Pressure: {preValue.Hectopascals:0.##}hPa");
        Console.WriteLine($"Altitude: {altValue:0.##}m");
        Console.WriteLine($"Relative humidity: {humValue:0.#}%");

        // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
        Console.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(tempValue, humValue).DegreesCelsius:0.#}\u00B0C");
        Console.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(tempValue, humValue).DegreesCelsius:0.#}\u00B0C");

        // when measuring the gas resistance on each cycle it is important to wait a certain interval
        // because a heating plate is activated which will heat up the sensor without sleep, this can
        // falsify all readings coming from the sensor
        Thread.Sleep(1000);
    }

    // change the settings
    bme680.TemperatureSampling = Sampling.HighResolution;
    bme680.HumiditySampling = Sampling.UltraHighResolution;
    bme680.PressureSampling = Sampling.Skipped;

    bme680.ConfigureHeatingProfile(Bme680HeaterProfile.Profile2, Temperature.FromDegreesCelsius(280), Duration.FromMilliseconds(80), Temperature.FromDegreesCelsius(24));
    bme680.HeaterProfile = Bme680HeaterProfile.Profile2;

    measurementDuration = bme680.GetMeasurementDuration(bme680.HeaterProfile);

    // 10 consecutive measurements with custom settings
    for (int i = 0; i < 10; i++)
    {
        // perform the measurement
        bme680.SetPowerMode(Bme680PowerMode.Forced);
        Thread.Sleep(measurementDuration.ToTimeSpan());

        // Print out the measured data
        bme680.TryReadTemperature(out var tempValue);
        bme680.TryReadPressure(out var preValue);
        bme680.TryReadHumidity(out var humValue);
        bme680.TryReadGasResistance(out var gasResistance);
        var altValue = WeatherHelper.CalculateAltitude(preValue, defaultSeaLevelPressure, tempValue);

        Console.WriteLine($"Gas resistance: {gasResistance:0.##}Ohm");
        Console.WriteLine($"Temperature: {tempValue.DegreesCelsius:0.#}\u00B0C");
        Console.WriteLine($"Pressure: {preValue.Hectopascals:0.##}hPa");
        Console.WriteLine($"Altitude: {altValue:0.##}m");
        Console.WriteLine($"Relative humidity: {humValue:0.#}%");

        // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
        Console.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(tempValue, humValue).DegreesCelsius:0.#}\u00B0C");
        Console.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(tempValue, humValue).DegreesCelsius:0.#}\u00B0C");

        Thread.Sleep(1000);
    }

    // reset will change settings back to default
    bme680.Reset();
}
