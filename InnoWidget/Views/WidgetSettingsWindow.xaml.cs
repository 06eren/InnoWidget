using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using InnoWidget.Core.Services;
using InnoWidget.Widgets.Hardware;
using InnoWidget.Widgets.Network;
using InnoWidget.Widgets.Volcano;
using InnoWidget.Widgets.Ice;
using InnoWidget.Widgets.Crystal;

namespace InnoWidget.Views;

public partial class WidgetSettingsWindow : Window
{
    private readonly Dictionary<string, object> _widgetViewModels = new();
    private readonly WidgetSettingsService _settingsService;
    
    public WidgetSettingsWindow()
    {
        InitializeComponent();
        _settingsService = new WidgetSettingsService();
        InitializeWidgetViewModels();
        LoadSettings();
    }
    
    private void InitializeWidgetViewModels()
    {
        // Her widget için ViewModel oluştur (parametresiz constructor'lar için)
        // Not: Gerçek widget'lar service parametresi ister, settings için mock kullanıyoruz
        _widgetViewModels["Hardware"] = null!; // Gerçek implementation gerekli
        _widgetViewModels["Network"] = null!; // Gerçek implementation gerekli
        _widgetViewModels["Volcano"] = new VolcanoWidgetViewModel();
        _widgetViewModels["Ice"] = new IceWidgetViewModel();
        _widgetViewModels["Crystal"] = new CrystalWidgetViewModel();
        
        // DataContext'leri ayarla
        if (FindName("HardwareSettings") is FrameworkElement hardwareElement)
            hardwareElement.DataContext = _widgetViewModels["Hardware"];
            
        if (FindName("NetworkSettings") is FrameworkElement networkElement)
            networkElement.DataContext = _widgetViewModels["Network"];
            
        if (FindName("VolcanoSettings") is FrameworkElement volcanoElement)
            volcanoElement.DataContext = _widgetViewModels["Volcano"];
            
        if (FindName("IceSettings") is FrameworkElement iceElement)
            iceElement.DataContext = _widgetViewModels["Ice"];
            
        if (FindName("CrystalSettings") is FrameworkElement crystalElement)
            crystalElement.DataContext = _widgetViewModels["Crystal"];
    }
    
    private void LoadSettings()
    {
        // Kaydedilmiş ayarları ViewModel'lere uygula
        foreach (var kvp in _widgetViewModels)
        {
            if (kvp.Value != null) // Null olanları atla
            {
                _settingsService.ApplySettingsToViewModel(kvp.Key, kvp.Value);
            }
        }
    }
    
    private void ResetAll_Click(object sender, RoutedEventArgs e)
    {
        // Tüm widget'ların ayarlarını sıfırla
        foreach (var viewModel in _widgetViewModels.Values)
        {
            if (viewModel is VolcanoWidgetViewModel volcanoVm)
                volcanoVm.ResetToDefaultCommand?.Execute(null);
            else if (viewModel is IceWidgetViewModel iceVm)
                iceVm.ResetToDefaultCommand?.Execute(null);
            else if (viewModel is CrystalWidgetViewModel crystalVm)
                crystalVm.ResetToDefaultCommand?.Execute(null);
            // Hardware ve Network null olduğu için atlanıyor
        }
        
        MessageBox.Show("Tüm widget ayarları sıfırlandı.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        // Ayarları uygula ve kaydet
        try
        {
            // Tüm ViewModel'lerden ayarları al ve kaydet
            foreach (var kvp in _widgetViewModels)
            {
                if (kvp.Value != null) // Null olanları atla
                {
                    _settingsService.SaveSettingsFromViewModel(kvp.Key, kvp.Value);
                }
            }
            
            // Dosyaya kaydet
            _settingsService.SaveSettings();
            
            MessageBox.Show("Ayarlar başarıyla uygulandı ve kaydedildi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ayarlar uygulanırken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void Close_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
    
    protected override void OnClosed(EventArgs e)
    {
        // ViewModel'leri temizle
        foreach (var viewModel in _widgetViewModels.Values)
        {
            if (viewModel is IDisposable disposable)
                disposable.Dispose();
        }
        
        base.OnClosed(e);
    }
}
