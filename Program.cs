using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

const string SERVICE_UUID = "a07498ca-ad5b-474e-940d-16f1fbe7e8cd";
const string CHAR_UUID    = "51ff12bb-3ed8-46e5-b4f9-d64e2fec021b";
const string DEVICE_NAME  = "BLE-Controller";

// --- Create GATT Service ---
var serviceResult = await GattServiceProvider.CreateAsync(Guid.Parse(SERVICE_UUID));
if (serviceResult.Error != BluetoothError.Success)
{
    Console.WriteLine($"Failed to create GATT service: {serviceResult.Error}");
    return;
}
var serviceProvider = serviceResult.ServiceProvider;

// --- Create Characteristic ---
var charParams = new GattLocalCharacteristicParameters
{
    CharacteristicProperties =
        GattCharacteristicProperties.Write |
        GattCharacteristicProperties.WriteWithoutResponse,
    ReadProtectionLevel  = GattProtectionLevel.Plain,
    WriteProtectionLevel = GattProtectionLevel.Plain,
};

var charResult = await serviceProvider.Service.CreateCharacteristicAsync(
    Guid.Parse(CHAR_UUID), charParams);

if (charResult.Error != BluetoothError.Success)
{
    Console.WriteLine($"Failed to create characteristic: {charResult.Error}");
    return;
}

var characteristic = charResult.Characteristic;

// --- Handle incoming writes ---
characteristic.WriteRequested += async (sender, args) =>
{
    using var deferral = args.GetDeferral();
    var request = await args.GetRequestAsync();
    var reader  = DataReader.FromBuffer(request.Value);
    reader.InputStreamOptions = InputStreamOptions.Partial;
    var data    = new byte[request.Value.Length];
    reader.ReadBytes(data);
    var msg     = System.Text.Encoding.UTF8.GetString(data).Trim();
    var ts      = DateTime.Now.ToString("HH:mm:ss.fff");
    Console.WriteLine($"[{ts}] Input: {msg}");
    request.Respond();
};

// --- Start Advertising ---
var advParams = new GattServiceProviderAdvertisingParameters
{
    IsDiscoverable = true,
    IsConnectable  = true,
};
serviceProvider.StartAdvertising(advParams);

Console.WriteLine($"Advertising as \"{DEVICE_NAME}\"");
Console.WriteLine("Open controller.html on your phone in Chrome and tap Connect.\n");
Console.WriteLine("Press Ctrl+C to stop.\n");

// Keep running until Ctrl+C
var tcs = new TaskCompletionSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; tcs.SetResult(); };
await tcs.Task;

serviceProvider.StopAdvertising();
Console.WriteLine("\nStopped.");
