using BTRemote.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BTRemote
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SelectBtDevice : ContentPage
    {
        public SelectBtDevice()
        {
            InitializeComponent();

            var bluetoothDeviceHelper = DependencyService.Get<IBluetoothDeviceHelper>();
            var devices = bluetoothDeviceHelper.GetBondedDevices();
            BluetoothDevicesListView.ItemsSource = devices;

            if (devices.Count == 0)
                LabelInfo.Text = "No (bonded) Bluetooth devices found!";
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            await Navigation.PushModalAsync(new MainPage(e.Item as BluetoothDevice));
        }
    }
}
