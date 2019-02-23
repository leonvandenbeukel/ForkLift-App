using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Android.Bluetooth;
using BTRemote.Droid;
using Java.Util;
using Xamarin.Forms;
using Application = Android.App.Application;
using BluetoothDevice = BTRemote.Model.BluetoothDevice;
using Log = Android.Util.Log;

[assembly: Dependency(typeof(BluetoothDeviceHelper))]
namespace BTRemote.Droid
{
    public class BluetoothDeviceHelper : Java.Lang.Object, IBluetoothDeviceHelper
    {
        private readonly string _tag = $"{Application.Context.PackageName} {nameof(MainActivity)}";
        private static readonly UUID MyUuid = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
        private readonly BluetoothAdapter _bluetoothAdapter = null;
        private BluetoothSocket _btSocket = null;
        private Stream _outStream = null;
        private Stream _inStream = null;

        public BluetoothDeviceHelper()
        {
            _bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
        }

        public bool Connected { get; set; }

        public async Task<bool> Connect(string deviceName)
        {
            if (!_bluetoothAdapter.Enable())
                return false;

            try
            {
                var device = _bluetoothAdapter.GetRemoteDevice(deviceName);
                _bluetoothAdapter.CancelDiscovery();
                _btSocket = device.CreateRfcommSocketToServiceRecord(MyUuid);

                await _btSocket.ConnectAsync();

                if (_btSocket.IsConnected)
                    Connected = true;
            }
            catch (Exception ex)
            {
                Connected = false;
                var error = $"Cannot connect to bluetooth device ({deviceName}):, {ex.Message}, {ex.StackTrace}.";
                Log.Info(_tag, error);
            }

            return Connected;
        }

        public List<BluetoothDevice> GetBondedDevices()
        {
            var devices = new List<BluetoothDevice>();

            if (_bluetoothAdapter != null && _bluetoothAdapter.Enable())
            {
                foreach (var bluetoothAdapterBondedDevice in _bluetoothAdapter.BondedDevices)
                {
                    devices.Add(
                        new BluetoothDevice
                        {
                            Address = bluetoothAdapterBondedDevice.Address,
                            Name = bluetoothAdapterBondedDevice.Name
                        });
                }
            }

            return devices;
        }

        public async Task SendMessageAsync(string data)
        {
            if (!Connected)
                return;

            try
            {
                _outStream = _btSocket.OutputStream;
            }
            catch (Exception ex)
            {
                Log.Error(_tag, $"Cannot get OutputStream from BT device: {ex.Message}, {ex.StackTrace}.");
            }

            byte[] buffer = Encoding.UTF8.GetBytes(data);

            try
            {
                await _outStream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                Log.Error(_tag, $"Cannot write data to BT device: {ex.Message}, {ex.StackTrace}.");
            }
        }
    }
}