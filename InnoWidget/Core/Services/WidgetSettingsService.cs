using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using InnoWidget.Widgets.Hardware;
using InnoWidget.Widgets.Network;
using InnoWidget.Widgets.Volcano;
using InnoWidget.Widgets.Ice;
using InnoWidget.Widgets.Crystal;

namespace InnoWidget.Core.Services;

public class WidgetSettingsService
{
    private readonly string _settingsFilePath;
    private Dictionary<string, WidgetSettingsData> _settings = new();

    public WidgetSettingsService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "InnoWidget");
        Directory.CreateDirectory(appFolder);
        _settingsFilePath = Path.Combine(appFolder, "widgetSettings.json");
        
        LoadSettings();
    }

    public void LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = File.ReadAllText(_settingsFilePath);
                _settings = JsonSerializer.Deserialize<Dictionary<string, WidgetSettingsData>>(json) ?? new();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Settings load error: {ex.Message}");
            _settings = new();
        }
    }

    public void SaveSettings()
    {
        try
        {
            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            File.WriteAllText(_settingsFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Settings save error: {ex.Message}");
        }
    }

    public void ApplySettingsToViewModel(string widgetId, object viewModel)
    {
        if (!_settings.TryGetValue(widgetId, out var settingsData))
            return;

        switch (widgetId)
        {
            case "Hardware":
                if (viewModel is HardwareWidgetViewModel hardwareVm)
                {
                    hardwareVm.AnimationsEnabled = settingsData.AnimationsEnabled;
                    hardwareVm.AnimationSpeed = settingsData.AnimationSpeed;
                    hardwareVm.PulseEnabled = settingsData.PulseEnabled;
                    hardwareVm.RotateEnabled = settingsData.RotateEnabled;
                    hardwareVm.GlowEnabled = settingsData.GlowEnabled;
                    hardwareVm.SakuraEnabled = settingsData.SakuraEnabled;
                    hardwareVm.PetalDensity = settingsData.PetalDensity;
                    hardwareVm.PetalSpeed = settingsData.PetalSpeed;
                    hardwareVm.UpdateInterval = settingsData.UpdateInterval;
                    hardwareVm.HighPerformanceMode = settingsData.HighPerformanceMode;
                    hardwareVm.NativeOptimization = settingsData.NativeOptimization;
                    hardwareVm.SelectedTheme = settingsData.SelectedTheme;
                    hardwareVm.BorderStyle = settingsData.BorderStyle;
                }
                break;

            case "Network":
                if (viewModel is NetworkWidgetViewModel networkVm)
                {
                    networkVm.AnimationsEnabled = settingsData.AnimationsEnabled;
                    networkVm.AnimationSpeed = settingsData.AnimationSpeed;
                    networkVm.PulseEnabled = settingsData.PulseEnabled;
                    networkVm.RotateEnabled = settingsData.RotateEnabled;
                    networkVm.GlowEnabled = settingsData.GlowEnabled;
                    networkVm.NeonParticlesEnabled = settingsData.NeonParticlesEnabled;
                    networkVm.NeonIntensity = settingsData.NeonIntensity;
                    networkVm.GlowRadius = settingsData.GlowRadius;
                    networkVm.UpdateInterval = settingsData.UpdateInterval;
                    networkVm.HighSpeedMode = settingsData.HighSpeedMode;
                    networkVm.RealTimeUpdates = settingsData.RealTimeUpdates;
                    networkVm.SelectedNeonColor = settingsData.SelectedNeonColor;
                    networkVm.BackgroundStyle = settingsData.BackgroundStyle;
                }
                break;

            case "Volcano":
                if (viewModel is VolcanoWidgetViewModel volcanoVm)
                {
                    volcanoVm.AnimationsEnabled = settingsData.AnimationsEnabled;
                    volcanoVm.AnimationSpeed = settingsData.AnimationSpeed;
                    volcanoVm.PulseEnabled = settingsData.PulseEnabled;
                    volcanoVm.GlowEnabled = settingsData.GlowEnabled;
                    volcanoVm.LavaParticlesEnabled = settingsData.LavaParticlesEnabled;
                    volcanoVm.EruptionFrequency = settingsData.EruptionFrequency;
                    volcanoVm.LavaDensity = settingsData.LavaDensity;
                    // MaxTemperature ve AlertThreshold private setter'a sahip, atlanıyor
                    volcanoVm.SelectedTheme = settingsData.SelectedTheme;
                    volcanoVm.BorderStyle = settingsData.BorderStyle;
                }
                break;

            case "Ice":
                if (viewModel is IceWidgetViewModel iceVm)
                {
                    iceVm.AnimationsEnabled = settingsData.AnimationsEnabled;
                    iceVm.AnimationSpeed = settingsData.AnimationSpeed;
                    iceVm.PulseEnabled = settingsData.PulseEnabled;
                    iceVm.GlowEnabled = settingsData.GlowEnabled;
                    iceVm.FreezeEnabled = settingsData.FreezeEnabled;
                    iceVm.SnowParticlesEnabled = settingsData.SnowParticlesEnabled;
                    iceVm.SnowDensity = settingsData.SnowDensity;
                    iceVm.SnowSpeed = settingsData.SnowSpeed;
                    // MinTemperature ve FreezingPoint private setter'a sahip, atlanıyor
                    iceVm.SelectedTheme = settingsData.SelectedTheme;
                    iceVm.BorderStyle = settingsData.BorderStyle;
                }
                break;

            case "Crystal":
                if (viewModel is CrystalWidgetViewModel crystalVm)
                {
                    crystalVm.AnimationsEnabled = settingsData.AnimationsEnabled;
                    crystalVm.AnimationSpeed = settingsData.AnimationSpeed;
                    crystalVm.PulseEnabled = settingsData.PulseEnabled;
                    crystalVm.GlowEnabled = settingsData.GlowEnabled;
                    crystalVm.ShineEnabled = settingsData.ShineEnabled;
                    crystalVm.CrystalDensity = settingsData.CrystalDensity;
                    crystalVm.VibrationFrequency = settingsData.VibrationFrequency;
                    crystalVm.MaxEnergyLevel = settingsData.MaxEnergyLevel;
                    crystalVm.HealingThreshold = settingsData.HealingThreshold;
                    crystalVm.SelectedTheme = settingsData.SelectedTheme;
                    crystalVm.BorderStyle = settingsData.BorderStyle;
                    // RotateEnabled property'si Crystal'de yok, atlanıyor
                }
                break;
        }
    }

    public void SaveSettingsFromViewModel(string widgetId, object viewModel)
    {
        var settingsData = new WidgetSettingsData();

        switch (widgetId)
        {
            case "Hardware":
                if (viewModel is HardwareWidgetViewModel hardwareVm)
                {
                    settingsData.AnimationsEnabled = hardwareVm.AnimationsEnabled;
                    settingsData.AnimationSpeed = hardwareVm.AnimationSpeed;
                    settingsData.PulseEnabled = hardwareVm.PulseEnabled;
                    settingsData.RotateEnabled = hardwareVm.RotateEnabled;
                    settingsData.GlowEnabled = hardwareVm.GlowEnabled;
                    settingsData.SakuraEnabled = hardwareVm.SakuraEnabled;
                    settingsData.PetalDensity = hardwareVm.PetalDensity;
                    settingsData.PetalSpeed = hardwareVm.PetalSpeed;
                    settingsData.UpdateInterval = hardwareVm.UpdateInterval;
                    settingsData.HighPerformanceMode = hardwareVm.HighPerformanceMode;
                    settingsData.NativeOptimization = hardwareVm.NativeOptimization;
                    settingsData.SelectedTheme = hardwareVm.SelectedTheme;
                    settingsData.BorderStyle = hardwareVm.BorderStyle;
                }
                break;

            case "Network":
                if (viewModel is NetworkWidgetViewModel networkVm)
                {
                    settingsData.AnimationsEnabled = networkVm.AnimationsEnabled;
                    settingsData.AnimationSpeed = networkVm.AnimationSpeed;
                    settingsData.PulseEnabled = networkVm.PulseEnabled;
                    settingsData.RotateEnabled = networkVm.RotateEnabled;
                    settingsData.GlowEnabled = networkVm.GlowEnabled;
                    settingsData.NeonParticlesEnabled = networkVm.NeonParticlesEnabled;
                    settingsData.NeonIntensity = networkVm.NeonIntensity;
                    settingsData.GlowRadius = networkVm.GlowRadius;
                    settingsData.UpdateInterval = networkVm.UpdateInterval;
                    settingsData.HighSpeedMode = networkVm.HighSpeedMode;
                    settingsData.RealTimeUpdates = networkVm.RealTimeUpdates;
                    settingsData.SelectedNeonColor = networkVm.SelectedNeonColor;
                    settingsData.BackgroundStyle = networkVm.BackgroundStyle;
                }
                break;

            case "Volcano":
                if (viewModel is VolcanoWidgetViewModel volcanoVm)
                {
                    settingsData.AnimationsEnabled = volcanoVm.AnimationsEnabled;
                    settingsData.AnimationSpeed = volcanoVm.AnimationSpeed;
                    settingsData.PulseEnabled = volcanoVm.PulseEnabled;
                    settingsData.GlowEnabled = volcanoVm.GlowEnabled;
                    settingsData.LavaParticlesEnabled = volcanoVm.LavaParticlesEnabled;
                    settingsData.EruptionFrequency = volcanoVm.EruptionFrequency;
                    settingsData.LavaDensity = volcanoVm.LavaDensity;
                    // MaxTemperature ve AlertThreshold private getter'a sahip, atlanıyor
                    settingsData.SelectedTheme = volcanoVm.SelectedTheme;
                    settingsData.BorderStyle = volcanoVm.BorderStyle;
                }
                break;

            case "Ice":
                if (viewModel is IceWidgetViewModel iceVm)
                {
                    settingsData.AnimationsEnabled = iceVm.AnimationsEnabled;
                    settingsData.AnimationSpeed = iceVm.AnimationSpeed;
                    settingsData.PulseEnabled = iceVm.PulseEnabled;
                    settingsData.GlowEnabled = iceVm.GlowEnabled;
                    settingsData.FreezeEnabled = iceVm.FreezeEnabled;
                    settingsData.SnowParticlesEnabled = iceVm.SnowParticlesEnabled;
                    settingsData.SnowDensity = iceVm.SnowDensity;
                    settingsData.SnowSpeed = iceVm.SnowSpeed;
                    // MinTemperature ve FreezingPoint private getter'a sahip, atlanıyor
                    settingsData.SelectedTheme = iceVm.SelectedTheme;
                    settingsData.BorderStyle = iceVm.BorderStyle;
                }
                break;

            case "Crystal":
                if (viewModel is CrystalWidgetViewModel crystalVm)
                {
                    settingsData.AnimationsEnabled = crystalVm.AnimationsEnabled;
                    settingsData.AnimationSpeed = crystalVm.AnimationSpeed;
                    settingsData.PulseEnabled = crystalVm.PulseEnabled;
                    settingsData.GlowEnabled = crystalVm.GlowEnabled;
                    settingsData.ShineEnabled = crystalVm.ShineEnabled;
                    settingsData.CrystalDensity = crystalVm.CrystalDensity;
                    settingsData.VibrationFrequency = crystalVm.VibrationFrequency;
                    settingsData.MaxEnergyLevel = crystalVm.MaxEnergyLevel;
                    settingsData.HealingThreshold = crystalVm.HealingThreshold;
                    settingsData.SelectedTheme = crystalVm.SelectedTheme;
                    settingsData.BorderStyle = crystalVm.BorderStyle;
                    // RotateEnabled property'si Crystal'de yok, atlanıyor
                }
                break;
        }

        _settings[widgetId] = settingsData;
    }
}

public class WidgetSettingsData
{
    // Common settings
    public bool AnimationsEnabled { get; set; } = true;
    public double AnimationSpeed { get; set; } = 1.0;
    public bool PulseEnabled { get; set; } = true;
    public bool RotateEnabled { get; set; } = true;
    public bool GlowEnabled { get; set; } = true;
    public int SelectedTheme { get; set; } = 0;
    public int BorderStyle { get; set; } = 0;

    // Hardware specific
    public bool SakuraEnabled { get; set; } = true;
    public int PetalDensity { get; set; } = 5;
    public double PetalSpeed { get; set; } = 1.0;
    public int UpdateInterval { get; set; } = 2000;
    public bool HighPerformanceMode { get; set; } = true;
    public bool NativeOptimization { get; set; } = true;

    // Network specific
    public bool NeonParticlesEnabled { get; set; } = true;
    public int NeonIntensity { get; set; } = 5;
    public int GlowRadius { get; set; } = 15;
    public bool HighSpeedMode { get; set; } = true;
    public bool RealTimeUpdates { get; set; } = true;
    public int SelectedNeonColor { get; set; } = 0;
    public int BackgroundStyle { get; set; } = 0;

    // Volcano specific
    public bool LavaParticlesEnabled { get; set; } = true;
    public int EruptionFrequency { get; set; } = 5;
    public int LavaDensity { get; set; } = 5;
    public double MaxTemperature { get; set; } = 1200;
    public double AlertThreshold { get; set; } = 1000;

    // Ice specific
    public bool FreezeEnabled { get; set; } = true;
    public bool SnowParticlesEnabled { get; set; } = true;
    public int SnowDensity { get; set; } = 5;
    public double SnowSpeed { get; set; } = 1.0;
    public double MinTemperature { get; set; } = -25;
    public double FreezingPoint { get; set; } = 0;

    // Crystal specific
    public bool ShineEnabled { get; set; } = true;
    public int CrystalDensity { get; set; } = 5;
    public int VibrationFrequency { get; set; } = 440;
    public int MaxEnergyLevel { get; set; } = 100;
    public int HealingThreshold { get; set; } = 85;
}
