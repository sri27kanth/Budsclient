﻿using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using FluentIcons.Common;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Interface.ViewModels.Pages;

public class HomePageViewModel : MainPageViewModelBase
{
    public override Control CreateView() => new HomePage { DataContext = this };
    public override string TitleKey => BreadcrumbViewModel.DeviceNameKey;
    public override string? FallbackTitle => Strings.Home;
    public override Symbol IconKey => Symbol.Home;
    public override bool ShowsInFooter => false;
    
    private readonly DispatcherTimer _refreshTimer = new();
    
    public HomePageViewModel()
    {
        // Low refresh rate since we rely on status updates as cues
        _refreshTimer.Interval = new TimeSpan(0, 0, 12);
        _refreshTimer.Tick += async (_, _) =>
        {
            if (BluetoothImpl.Instance.IsConnected)
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.DEBUG_GET_ALL_DATA);
        };
        
        SppMessageReceiver.Instance.StatusUpdate += OnStatusUpdateReceived;
        BluetoothImpl.Instance.Connected += async (_, _) =>
        {
            await Task.Delay(1000);
            await BluetoothImpl.Instance.SendRequestAsync(MsgIds.DEBUG_GET_ALL_DATA);
        };
    }
    
    private void OnStatusUpdateReceived(object? sender, StatusUpdateDecoder e)
    {
        /* Status updates are only sent if something has changed.
           We use this knowledge to request updated debug data. */
        _ = BluetoothImpl.Instance.SendRequestAsync(MsgIds.DEBUG_GET_ALL_DATA);
    }

    public override void OnNavigatedTo() => _refreshTimer.Start();
    public override void OnNavigatedFrom() => _refreshTimer.Stop();
}


