﻿using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Platform;
using Symbol = FluentIcons.Common.Symbol;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class DevicesPageViewModel : MainPageViewModelBase
{
    public override Control CreateView() => new DevicesPage { DataContext = this };
    public override string TitleKey => Keys.DevicesHeader;
    public override Symbol IconKey => Symbol.BluetoothConnected;
    public override bool ShowsInFooter => true;
    
    public async void DoNewCommand()
    {
        if (!await Utils.Interface.Dialogs.RequireFullVersion("Multi-device support not available in demo"))
            return;
        
        await DeviceSelectionDialog.OpenDialogAsync();
    }

    public async void DoConnectCommand(object? param)
    {
        if (param is not Device device)
            return;
        
        if (device.MacAddress == Settings.Data.LastDeviceMac)
            return;
        
        var cd = new ContentDialog
        {
            Title = Strings.PleaseWait,
            Content = Strings.ConnlostConnecting,
            CloseButtonText = Strings.Cancel,
            CloseButtonCommand = new MiniCommand(p => _ = BluetoothImpl.Instance.DisconnectAsync())
        };
        _ = cd.ShowAsync(TopLevel.GetTopLevel(MainView.Instance));
        
        if(BluetoothImpl.Instance.IsConnected)
            await BluetoothImpl.Instance.DisconnectAsync();
        
        BluetoothImpl.Instance.Device.Current = device;
        await BluetoothImpl.Instance.ConnectAsync(device);

        cd.Hide();
    }
    
    public async void DoDeleteCommand(object? param)
    {
        if (param is not Device device)
            return;
        
        var result = await new QuestionBox
        {
            Title = Strings.DevicesDeleteLong,
            Description = Strings.DevicesDeleteConfirmation,
            ButtonText = Strings.ContinueButton,
            CloseButtonText = Strings.Cancel
        }.ShowAsync();

        if(result)
            BluetoothImpl.Instance.UnregisterDevice(device);
    }
}