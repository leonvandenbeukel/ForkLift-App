using System;
using System.Collections.Generic;
using BTRemote.Model;
using BTRemote.Touch;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BTRemote
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        private readonly BluetoothDevice _btDevice;
        private readonly IBluetoothDeviceHelper _bluetoothDeviceHelper;
        private readonly Dictionary<long, TouchInfo> _idDictionary = new Dictionary<long, TouchInfo>();
        private float _canvasWidth;
        private float _canvasHeight;
        private readonly int _timerDelay = 250;
        private int _liftVerticalPosition = 0;
        private bool _lightOn = false;

        public MainPage(BluetoothDevice btDevice)
        {
            InitializeComponent();

            if (btDevice != null)
            {
                MessageLabel.Text = $"Trying to connect to Bluetooth device {btDevice.Name}...";
                _btDevice = btDevice;
                _bluetoothDeviceHelper = DependencyService.Get<IBluetoothDeviceHelper>();
                Connect2BluetoothDevice();
            }
            else
            {
                MessageLabel.Text = "No Bluetooth device found.";
            }

            Device.StartTimer(TimeSpan.FromMilliseconds(_timerDelay), OnTimerTick);
        }

        async void Connect2BluetoothDevice()
        {
            var connected = await _bluetoothDeviceHelper.Connect(_btDevice.Address);
            MessageLabel.Text = connected ? $"Connected to {_btDevice.Name}" : $"Cannot connect to {_btDevice.Name}!";
        }

        private bool OnTimerTick()
        {
            if (_idDictionary.Count > 0)
            {
                var idPosInfo = _idDictionary[0];

                if (_bluetoothDeviceHelper != null && _bluetoothDeviceHelper.Connected && _canvasHeight > 0 && _canvasWidth > 0)
                {
                    var percW = (100 * idPosInfo.Location.X) / _canvasWidth;
                    var percH = (100 * idPosInfo.Location.Y) / _canvasHeight;
                    var msg = $"{(int)percW},{(int)percH},{UpDownLift.Value},{_liftVerticalPosition},{(_lightOn ? 1:0)}|";

                    _bluetoothDeviceHelper.SendMessageAsync(msg);
                }
            }
            else if (_bluetoothDeviceHelper != null && _bluetoothDeviceHelper.Connected)
            {
                _bluetoothDeviceHelper.SendMessageAsync($"50,50,{UpDownLift.Value},{_liftVerticalPosition},{(_lightOn ? 1 : 0)}|");
            }

            return true;
        }

        private void CanvasView_OnTouch(object sender, SKTouchEventArgs args)
        {
            switch (args.ActionType)
            {
                case SKTouchAction.Entered:
                    break;
                case SKTouchAction.Pressed:
                    if (args.InContact)
                    {
                        _idDictionary.Add(args.Id, new TouchInfo { Location = args.Location });
                    }

                    break;
                case SKTouchAction.Moved:
                    if (_idDictionary.ContainsKey(args.Id))
                    {
                        _idDictionary[args.Id].Location = args.Location;
                    }
                    break;
                case SKTouchAction.Released:
                case SKTouchAction.Cancelled:
                    if (_idDictionary.ContainsKey(args.Id))
                    {
                        _idDictionary.Remove(args.Id);
                    }
                    break;
                case SKTouchAction.Exited:
                    break;
            }

            args.Handled = true;
            CanvasViewMove.InvalidateSurface();
        }

        private bool Connected => _bluetoothDeviceHelper != null && _bluetoothDeviceHelper.Connected;

        private void CanvasView_OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(BackgroundColor.ToSKColor());
            var w = canvas.LocalClipBounds.Width;
            var h = canvas.LocalClipBounds.Height;
            var joystickSize = 60;
            var joystickColor = SKColors.DarkSlateBlue;

            if ((int)_canvasHeight == 0)
            {
                _canvasHeight = h;
                _canvasWidth = w;
            }

            var strokeLineStyle = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Purple,
                StrokeWidth = 1,
                PathEffect = SKPathEffect.CreateDash(new float[] { 7, 7 }, 0)
            };

            canvas.DrawLine(w / 2, 0, w / 2, h, strokeLineStyle);
            canvas.DrawLine(0, h / 2, w, h / 2, strokeLineStyle);


            if (_idDictionary.Count == 0)
                canvas.DrawCircle(w / 2, h / 2, joystickSize, new SKPaint { Color = joystickColor, Style = SKPaintStyle.Fill });

            foreach (var key in _idDictionary.Keys)
            {
                var info = _idDictionary[key];

                canvas.DrawCircle(info.Location.X, info.Location.Y, joystickSize, new SKPaint
                {
                    Color = joystickColor,
                    Style = SKPaintStyle.Fill,
                });
            }
        }

        private void Slider_OnValueChanged(object sender, ValueChangedEventArgs e)
        {
            _liftVerticalPosition = (int)e.NewValue;
        }

        private void LightSwitch_OnToggled(object sender, ToggledEventArgs e)
        {
            _lightOn = e.Value;
        }
    }
}