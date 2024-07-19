using System.Diagnostics.CodeAnalysis;
using GalaxyBudsClient.Platform.Windows.Bluetooth;
using GalaxyBudsClient.Platform.Interfaces;
using GalaxyBudsClient.Platform.Windows.Impl;

namespace GalaxyBudsClient.Platform.Windows;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class WindowsPlatformImplCreator : IPlatformImplCreator
{
    public IDesktopServices CreateDesktopServices() => new DesktopServices();
    public virtual IBluetoothService? CreateBluetoothService() => new BluetoothService();
    public IHotkeyBroadcast CreateHotkeyBroadcast() => new HotkeyBroadcast();
    public IHotkeyReceiver CreateHotkeyReceiver() => new HotkeyReceiver();
    public IMediaKeyRemote CreateMediaKeyRemote() => new MediaKeyRemote();
    public INotificationListener? CreateNotificationListener() => null;
    public IOfficialAppDetector CreateOfficialAppDetector() => new OfficialAppDetector();
}