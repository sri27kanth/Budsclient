using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.ViewModels.Dialogs;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Platform.Model;

namespace GalaxyBudsClient.Interface.Dialogs;

public partial class ManualPairDialog : UserControl
{
    public ManualPairDialog()
    {
        InitializeComponent();
    }
    
    public static async Task<(Models model, BluetoothDevice device)?> OpenDialogAsync()
    {
        IEnumerable<BluetoothDevice> devices;
        try
        {
            devices = await BluetoothImpl.Instance.GetDevicesAsync();
        }
        catch (BluetoothException ex)
        {
            _ = new MessageBox
            {
                Title = Strings.Error,
                Description = ex.Message
            }.ShowAsync();
            return null;
        }

        var dialog = new ContentDialog
        {
            Title = Strings.DevselManualPair,
            PrimaryButtonText = Strings.Okay,
            CloseButtonText = Strings.Cancel,
            DefaultButton = ContentDialogButton.Primary
        };

        var viewModel = new ManualPairDialogViewModel
        {
            Devices = devices
        };
        dialog.Content = new ManualPairDialog
        {
            DataContext = viewModel
        };

        dialog.PrimaryButtonClick += OnPrimaryButtonClick;
        var result = await dialog.ShowAsync(TopLevel.GetTopLevel(MainView.Instance));
        dialog.PrimaryButtonClick -= OnPrimaryButtonClick;
            
        return result == ContentDialogResult.None ||
               viewModel.SelectedDevice == null || 
               viewModel.SelectedModel == null ? null : (viewModel.SelectedModel.Value, viewModel.SelectedDevice);

        void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = viewModel.SelectedModel == null ||
                          viewModel.SelectedModel == Models.NULL || 
                          viewModel.SelectedDevice == null;
            if (args.Cancel)
            {
                _ = new MessageBox
                {
                    Title = Strings.Error,
                    Description = Strings.DevselInvalidSelection
                }.ShowAsync();
            }
        }
    }
}