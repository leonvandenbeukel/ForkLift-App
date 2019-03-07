using BTRemote.Interfaces;
using BTRemote.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BTRemote.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SelectBtDevice : ContentPage
    {
        public SelectBtDevice()
        {
            InitializeComponent();

            var bluetoothDeviceHelper = DependencyService.Get<IBluetoothDeviceHelper>();

            if (bluetoothDeviceHelper != null)
            {
                var devices = bluetoothDeviceHelper.GetBondedDevices();
                BluetoothDevicesListView.ItemsSource = devices;

                if (devices.Count == 0)
                    LabelInfo.Text = "No (bonded) Bluetooth devices found!";
            }
            else
            {
                LabelInfo.Text = "Bluetooth is not available!";
            }
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            await Navigation.PushModalAsync(new MainPage(e.Item as BluetoothDevice));
        }
    }
}
