using GalaxyBudsClient.Platform.Interfaces;

namespace GalaxyBudsClient.Platform.Stubs;

public class DummyPlatformImplCreator : IPlatformImplCreator
{
    public IDesktopServices CreateDesktopServices() => new DummyDesktopServices();
    public IBluetoothService CreateBluetoothService() => new DummyBluetoothService();
    public IHotkeyBroadcast CreateHotkeyBroadcast() => new DummyHotkeyBroadcast();
    public IHotkeyReceiver CreateHotkeyReceiver() => new DummyHotkeyReceiver();
    public IMediaKeyRemote CreateMediaKeyRemote() => new DummyMediaKeyRemote();
    public INotificationListener CreateNotificationListener() => new DummyNotificationListener();
    public IOfficialAppDetector CreateOfficialAppDetector() => new DummyOfficialAppDetector();
}