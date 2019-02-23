using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BTRemote.Model;

namespace BTRemote
{
    public interface IBluetoothDeviceHelper
    {
        Task<bool> Connect(string deviceName);
        List<BluetoothDevice> GetBondedDevices();
        bool Connected { get; set; }
        Task SendMessageAsync(string data);
    }
}
