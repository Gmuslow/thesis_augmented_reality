using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using UnityEngine.WSA;
using static UnityEngine.Windows.WebCam.VideoCapture;
using static UnityEngine.XR.Interaction.Toolkit.Inputs.XRInputTrackingAggregator;
//#if ENABLE_WINMD_SUPPORT
#if WINDOWS_UWP
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.UI.Popups;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;
using Windows.Devices.Radios;
using Windows.ApplicationModel.Store.Preview;
using Windows.Storage.Streams;
#endif

public class ConnectBluetooth : MonoBehaviour
{
    // Start is called before the first frame update

#if WINDOWS_UWP
    private GattSession gattSession;
    private DeviceWatcher deviceWatcher;
    BluetoothLEDevice bluetoothLeDevice;
    private GattCharacteristic writeCharacteristic;
    private GattCharacteristic target_address_characteristic;
    //private ;
#endif


    public float pollTime = 1f;
    float counter;
    public static string rssiValue = "FF";
    public static bool changed = true;
    public bool printRSSIResponses = false;
    async void Start()
    {
        counter = pollTime;
        await Connect();
    }

    // Update is called once per frame
    void Update()
    {
        //Polls microcontroller for newest rssi value
        counter -= Time.deltaTime;
        if (counter < 0)
        {
            counter = pollTime;
            ReadRSSIValue();
        }
    }

    async Task Connect()
    {
#if WINDOWS_UWP
        
        
        StartDeviceWatcher();
        string deviceId = "BluetoothLE#BluetoothLEc8:96:65:eb:03:99-0c:43:14:f4:69:c7";
        
        bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(deviceId);
        bluetoothLeDevice.ConnectionStatusChanged += BLEConnectionChanged;
        if (bluetoothLeDevice != null)
        {
            Debug.Log("microcontroller connected");
            // Get the GATT services
            gattSession = await GattSession.FromDeviceIdAsync(BluetoothDeviceId.FromId(deviceId));

            if (gattSession != null)
            {
                Debug.Log("Gatt session initialized");
                // Set MaintainConnection to true
                gattSession.MaintainConnection = true;
                gattSession.SessionStatusChanged += GattSession_SessionChanged;
            }
            else
            {
                Debug.Log("Failed to create GATT session.");
            }
            
            Guid serviceID = new Guid("1e32dbec-aab2-4db9-96de-6a88eeeeba34");
            Guid writeCharacteristicID = new Guid("0da1f612-0837-4052-b55c-5dccd990e098");
            target_address_characteristic = await GetCharacteristicByUuidAsync(bluetoothLeDevice, serviceID, writeCharacteristicID);
            byte[] target_address = { 0xAC, 0x23, 0x3F, 0xAB, 0x46, 0x06 };
            await WriteToTargetAddress(target_address);

            Guid rssiGUID = new Guid("2b0134b1-701e-4028-8921-7fa032b0b48d");
            writeCharacteristic = await GetCharacteristicByUuidAsync(bluetoothLeDevice, serviceID, rssiGUID);
            if (writeCharacteristic == null)
            {
                Debug.Log($"Characteristic with UUID {rssiGUID} not found.");
                return;
            }

            // Enable notifications for the characteristic
            GattCommunicationStatus status = await writeCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
            if (status != GattCommunicationStatus.Success)
            {
                Debug.Log("Failed to enable notifications. " + status);
                return;
            }
            else {
                Debug.Log("Enabled Notifications.");
                writeCharacteristic.ValueChanged += Characteristic_ValueChanged;
            }

        }
        else
        {
            Debug.Log("Failed to connect to microcontroller.");
            
        }
        bluetoothLeDevice.Dispose();
#else
        Debug.Log("UWP APIs not supported!");

#endif
    }
    string oldSample = "";
    
    async void ReadRSSIValue()
    {

#if WINDOWS_UWP
        if (writeCharacteristic != null)
        {
        GattCharacteristicProperties properties = writeCharacteristic.CharacteristicProperties;

        if (properties.HasFlag(GattCharacteristicProperties.Read))
        {
            GattReadResult result = await writeCharacteristic.ReadValueAsync();
            if (result.Status == GattCommunicationStatus.Success)
            {
                var reader = DataReader.FromBuffer(result.Value);
                byte[] input = new byte[reader.UnconsumedBufferLength];
                reader.ReadBytes(input);

                string dataString = BitConverter.ToString(input);

                // Print the data to the debug output
                if (printRSSIResponses)
                    Debug.Log("Characteristic Value: " + dataString);

                if (oldSample == dataString.Split("-")[7])
                {
                    changed = false;
                }
                else{
                rssiValue = dataString.Split("-")[6];
                oldSample = dataString.Split("-")[7];
                changed = true;
                }
            }
        }
        else {
            Debug.Log("Characteristic does not have read permissions");
        }
        }
#endif

    }



#if WINDOWS_UWP
    void StartDeviceWatcher()
    {
        string deviceSelector = BluetoothDevice.GetDeviceSelector();
        deviceWatcher = DeviceInformation.CreateWatcher(deviceSelector);
    
        // Add event handlers
        deviceWatcher.Added += DeviceAdded;
        deviceWatcher.Removed += DeviceRemoved;
    
        // Start the watcher
        deviceWatcher.Start();
    }
    private void DeviceAdded(DeviceWatcher sender, DeviceInformation deviceInfo)
    {
        // A new device was added (connected)
        Debug.Log($"Device added: {deviceInfo.Name}, Id: {deviceInfo.Id}");
    }

    private void DeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
    {
        // A device was removed (disconnected)
        Debug.Log($"Device removed: {deviceInfoUpdate.Id}");
    }
    public async Task<GattCharacteristic> GetCharacteristicByUuidAsync(BluetoothLEDevice bluetoothDevice, Guid serviceUuid, Guid characteristicUuid)
    {
        if (bluetoothDevice == null)
        {
            Debug.Log("Bluetooth device is not connected.");
            return null;
        }

        // Get the service with the specified UUID
        var service = await GetServiceByUuidAsync(bluetoothDevice, serviceUuid);
        if (service == null)
        {
            Debug.Log($"Service with UUID {serviceUuid} not found.");
            return null;
        }

        // Get the characteristic with the specified UUID
        var characteristics = await service.GetCharacteristicsForUuidAsync(characteristicUuid);
        if (characteristics.Status != GattCommunicationStatus.Success || characteristics.Characteristics.Count == 0)
        {
            Debug.Log($"Characteristic with UUID {characteristicUuid} not found.");
            return null;
        }

        // Return the first characteristic found (assuming no duplicates)
        return characteristics.Characteristics[0];
    }

    private async Task<GattDeviceService> GetServiceByUuidAsync(BluetoothLEDevice bluetoothDevice,  Guid serviceUuid)
    {
        // Get all services of the connected device
        var servicesResult = await bluetoothDevice.GetGattServicesAsync();
        if (servicesResult.Status != GattCommunicationStatus.Success)
        {
            Debug.Log("Failed to get GATT services.");
            return null;
        }

        // Find the service with the specified UUID
        var services = servicesResult.Services;
        for (int i = 0; i < services.Count; i++)
        {
            if (services[i].Uuid == serviceUuid)
            {
                Debug.Log("Found Service UUID" + serviceUuid);
                return services[i];
            }
        }
        Debug.Log("Couldnt get service of uuid " + serviceUuid);
        return null;
    }
#endif


    //writes a data buffer to microcontroller target_address characteristic
    public async Task WriteToTargetAddress(byte[] data)
    {
#if WINDOWS_UWP
        Debug.Log("Writing to target address");
        // Store the characteristic reference for later use
        GattCharacteristic characteristic = target_address_characteristic;

        // Check if the characteristic supports write operation
        if (characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Write))
        {
            // Create a data writer and write the data to it
            using (var dataWriter = new DataWriter())
            {
                dataWriter.WriteBytes(data);

                // Write the data to the characteristic
                try
                {
                    await characteristic.WriteValueAsync(dataWriter.DetachBuffer());
                    string dataString = BitConverter.ToString(data);
                    Debug.Log("Write request sent successfully. Sent: " + dataString);
                }
                catch (Exception ex)
                {
                    Debug.Log($"Failed to send write request: {ex.Message}");
                    // Handle the exception accordingly
                }
            }
        }
        else
        {
            Debug.Log("Characteristic does not support write operation.");
            // Handle the case where the characteristic does not support write operation
        }
#endif
    }
#if WINDOWS_UWP

    private void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
    {
        // Handle the incoming notification
        var reader = DataReader.FromBuffer(args.CharacteristicValue);
        byte[] data = new byte[args.CharacteristicValue.Length];
        reader.ReadBytes(data);
        Debug.Log($"Received notification: {BitConverter.ToString(data)}");
    }

    private void GattSession_SessionChanged(GattSession sender, GattSessionStatusChangedEventArgs args)
    {
        Debug.Log("Gatt session changed:\t" + args.Status);
    }
    private void BLEConnectionChanged(BluetoothLEDevice sender, object o)
    {
        Debug.Log("Bluetooth Connection Status Changed:\t" + sender.ConnectionStatus);
    }

#endif

    public async void DiscoverBLEDevices()
    {
        Debug.Log("Starting Discovery...");
#if WINDOWS_UWP
        string selector = BluetoothLEDevice.GetDeviceSelector();
        var devices = await DeviceInformation.FindAllAsync(selector);

        foreach (var device in devices)
        {
            Debug.Log($"Name: {device.Name}, Id: {device.Id}");
            // Process the discovered BLE device here
        }
        if (devices == null)
        {
            Debug.Log("DEVICES ARE NULL");
        }
#endif
    }




    void Scan()
    {
#if WINDOWS_UWP
        string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };

        string selector = RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort);
        DeviceWatcher deviceWatcher = DeviceInformation.CreateWatcher(selector);

        // Register event handlers before starting the watcher.
        // Added, Updated and Removed are required to get all nearby devices
        deviceWatcher.Added += DeviceWatcher_Added;
        deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
        deviceWatcher.Stopped += DeviceWatcher_Stopped;
        deviceWatcher.Updated += DeviceWatcher_updated;

        // Start the watcher.
        
        try
        {
            Debug.Log("Starting Scan");
            
            deviceWatcher.Start();
            
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            // Handle the error accordingly
            
        }
#endif
    }
#if WINDOWS_UWP
    void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation args)
    {
        Debug.Log("found device");
        
    }

    void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object args)
    {
        Debug.Log("Enumeration Completed");
    }

    void DeviceWatcher_Stopped(DeviceWatcher sender, object args)
    {

        Debug.Log("DeviceWatcher Stopped");
        // You may handle any cleanup tasks here
    }

    void DeviceWatcher_updated(DeviceWatcher sender, DeviceInformationUpdate args)
    {
        Debug.Log("Device Updated" + args.Id);
    }
#endif


    /*async void GetPermissions()
    {
        Debug.Log("Getting Bluetooth Permissions...");
#if WINDOWS_UWP
        var installState = await InstallControl.GetForCurrentView().GetAppInstallStateAsync();
        if (installState.InstallState == AppInstallState.Installed)
        {
            // App is installed
            if (installState.IsUserActive)
            {
                // App is currently active
                Debug.Log("App is active.");
            }
            else
            {
                // App is not active
                Debug.Log("App is not active.");
            }

            // Check Bluetooth permissions
            if (installState.IsUserPresent)
            {
                // App has Bluetooth permissions
                Debug.Log("App has Bluetooth permissions.");
            }
            else
            {
                // App does not have Bluetooth permissions
                Debug.Log("App does not have Bluetooth permissions.");
            }
        }
#endif
    }*/

    async void BluetoothEnabled()
    {
        Debug.Log("Seeing if bluetooth is enabled...");
#if WINDOWS_UWP
        var radios = await Radio.GetRadiosAsync();
        foreach (var radio in radios)
        {
            if (radio.Kind == RadioKind.Bluetooth)
            {
                if (radio.State == RadioState.On)
                {
                    // Bluetooth is enabled
                    Debug.Log("Bluetooth is enabled.");
                }
                else
                {
                    // Bluetooth is disabled
                    Debug.Log("Bluetooth is disabled.");
                }
                break; // No need to check further
            }
        }
#endif
    }

    async void TraceAsyncMethod()
    {
        Debug.Log("Start of async method");

        // Call an async method
        await Task.Delay(7000); // Simulate asynchronous operation (e.g., waiting for 1 second)

        Debug.Log("End of async method");
    }
    public static sbyte HexStringToSignedByte(string hexString)
    {
        // Convert the hexadecimal string to a signed byte
        sbyte signedByte = Convert.ToSByte(hexString, 16);

        return signedByte;
    }
}
