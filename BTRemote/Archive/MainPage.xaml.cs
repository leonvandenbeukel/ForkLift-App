using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BTRemote.Model;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

using TouchTracking;

namespace BTRemote
{
    public partial class MainPage : ContentPage
    {
        private readonly BluetoothDevice _btDevice;
        private IBluetoothDeviceHelper _bluetoothDeviceHelper;
        //private SKCanvasView canvasView;
        bool pageIsActive;
        //float dashPhase;

        static readonly SKColor crossHairColor = SKColors.Red;

        View canvasView;
        Stopwatch stopwatch = new Stopwatch();
        //SKBitmap bitmap;
        //SKCanvas bitmapCanvas;
        // SKPath for clipping drawings to circle
        //SKPath clipPath = new SKPath();

        // SKPaint for crosshairs on bitmap
        SKPaint thinLinePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            Color = crossHairColor
        };

        private SKPaint currentPositionCircle = new SKPaint
        {
            Color = SKColors.BlueViolet,
            Style = SKPaintStyle.StrokeAndFill,
            StrokeWidth = 3
        };

        // Item to store in touch-tracking dictionary
        class FingerInfo
        {
            public Point ThisPosition;
            public SKPoint LastPosition;
        }

        // Touch-tracking dictionary for tracking multiple fingers
        Dictionary<long, FingerInfo> idDictionary = new Dictionary<long, FingerInfo>();


        public MainPage(BluetoothDevice btDevice)
        {
            InitializeComponent();

            if (btDevice != null)
            {
                _btDevice = btDevice;
                _bluetoothDeviceHelper = DependencyService.Get<IBluetoothDeviceHelper>();
            }

            //canvasView = new SKCanvasView();
            //canvasView.PaintSurface += OnCanvasViewPaintSurface;
            //canvasView.SizeChanged += CanvasView_SizeChanged;
            //Content = canvasView;


            SKGLView canvasView = new SKGLView();
            canvasView.PaintSurface += OnGLViewPaintSurface;
            this.canvasView = canvasView;

            CanvasViewLayout.Children.Add(canvasView);

            stopwatch.Start();
            Device.StartTimer(TimeSpan.FromMilliseconds(16), OnTimerTick);
        }

        private bool OnTimerTick()
        {
            //if (bitmap == null)
            //{
            //    return true;
            //}

            // Redraw the canvas.
            if (canvasView is SKCanvasView view)
            {
                view.InvalidateSurface();
            }
            else
            {
                (canvasView as SKGLView)?.InvalidateSurface();
            }

            return true;
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            OnPaintSurface(args.Surface, args.Info.Width, args.Info.Height);
        }

        void OnGLViewPaintSurface(object sender, SKPaintGLSurfaceEventArgs args)
        {
            OnPaintSurface(args.Surface, args.BackendRenderTarget.Width, args.BackendRenderTarget.Height);
        }

        //private void OnGLViewPaintSurface(object sender, SKPaintGLSurfaceEventArgs args)
        void OnPaintSurface(SKSurface surface, int width, int height)
        {
            SKCanvas canvas = surface.Canvas;

            //// If bitmap does not exist, create it
            //if (bitmap == null)
            //{
            //    // Set three fields
            //    bitmap = new SKBitmap(width, height);
            //    bitmapCanvas = new SKCanvas(bitmap);

            //    // Establishes circular clipping and colors background
            //    PrepBitmap(bitmapCanvas, width, height);
            //}

            // Clear the canvas
            canvas.Clear(SKColors.White);

            // Set the rotate transform
            float radius = width / 2;
            //canvas.RotateDegrees(angle, radius, radius);

            // Set a circular clipping area
            //clipPath.Reset();
            //clipPath.AddCircle(radius, radius, radius);
            //canvas.ClipPath(clipPath);

            // Draw the bitmap
            //float offset = (width) / 2f;
            // canvas.DrawBitmap(bitmap, offset, offset);

            // Draw the cross hairs
            canvas.DrawLine(radius, 0, radius, width, thinLinePaint);
            canvas.DrawLine(0, radius, width, radius, thinLinePaint);

            //if (!(canvasView is SKCanvasView) && !(canvasView is SKGLView))
            //    return;

            //Loop trough the fingers touching the screen.
            foreach (long id in idDictionary.Keys)
            {
                FingerInfo fingerInfo = idDictionary[id];
                //SKSurface surface = args.Surface;
                //SKCanvas canvas = surface.Canvas;
                //canvas.Clear();
                //SKPoint center = new SKPoint(info.Width / 2, info.Height / 2);

                //SKPoint p1 = new SKPoint(width / 2, 0);
                //SKPoint p2 = new SKPoint(width / 2, height);

                //using (SKPaint paint = new SKPaint())
                //{
                //    paint.Style = SKPaintStyle.Stroke;
                //    paint.Color = SKColors.Red;
                //    paint.StrokeWidth = 5;
                //    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 5, 5 }, dashPhase);
                //    //canvas.DrawLine(p1, p2, paint);
                //    bitmapCanvas.DrawLine(p1, p2, paint);
                //}

                var pos = fingerInfo.ThisPosition;

                canvas.DrawCircle((float)pos.X, (float)pos.Y, 30, currentPositionCircle);


            }


        }

        static void PrepBitmap(SKCanvas bitmapCanvas, int width, int height)
        {
            // TODO: CHECK

            // Set clipping path based on bitmap size
            using (SKPath bitmapClipPath = new SKPath())
            {
                bitmapClipPath.AddCircle(width / 2, height / 2, height / 2);
                bitmapCanvas.ClipPath(bitmapClipPath);
            }

            // Color the bitmap background
            bitmapCanvas.Clear(SKColor.FromHsv(20, 10, 28));
        }

        // For each touch event, simply store information in idDictionary.
        // Do not draw at this time!
        void OnTouchEffectAction(object sender, TouchActionEventArgs args)
        {
            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    if (args.IsInContact)
                    {
                        idDictionary.Add(args.Id, new FingerInfo
                        {
                            ThisPosition = args.Location,
                            LastPosition = new SKPoint(float.PositiveInfinity, float.PositiveInfinity)
                        });

                      //  UpdateLabel(args.Id);
                    }
                    break;

                case TouchActionType.Moved:
                    if (idDictionary.ContainsKey(args.Id))
                    {
                        idDictionary[args.Id].ThisPosition = args.Location;

                     //   UpdateLabel(args.Id);
                    }
                    break;

                case TouchActionType.Released:
                case TouchActionType.Cancelled:
                    if (idDictionary.ContainsKey(args.Id))
                    {
                        idDictionary.Remove(args.Id);
                    }
                    break;
            }
        }

        void UpdateLabel(long id)
        {
            //var idPosInfo = idDictionary[id];
            //titleLabel.Text =
            //    $"Last: {idPosInfo.LastPosition.X}, {idPosInfo.LastPosition.Y} This: {idPosInfo.ThisPosition.X}, {idPosInfo.ThisPosition.Y}";

        }

        private async void CanvasView_SizeChanged(object sender, EventArgs e)
        {
            if (!_bluetoothDeviceHelper.Connected)
                await _bluetoothDeviceHelper.Connect(_btDevice.Address);
        }




        //private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        //{
        //SKImageInfo info = args.Info;
        //SKSurface surface = args.Surface;
        //SKCanvas canvas = surface.Canvas;

        //canvas.Clear();

        ////SKPoint center = new SKPoint(info.Width / 2, info.Height / 2);

        //SKPoint p1 = new SKPoint(info.Width / 2, 0);
        //SKPoint p2 = new SKPoint(info.Width / 2, info.Height);

        //using (SKPaint paint = new SKPaint())
        //{
        //    paint.Style = SKPaintStyle.Stroke;
        //    paint.Color = SKColors.Red;
        //    paint.StrokeWidth = 5;
        //    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 5, 5 }, dashPhase);

        //    canvas.DrawLine(p1, p2, paint);
        //}

        //float radius = Math.Min(center.X, center.Y);

        //using (SKPath path = new SKPath())
        //{
        //    for (float angle = 0; angle < 3600; angle += 1)
        //    {
        //        float scaledRadius = radius * angle / 3600;
        //        double radians = Math.PI * angle / 180;
        //        float x = center.X + scaledRadius * (float)Math.Cos(radians);
        //        float y = center.Y + scaledRadius * (float)Math.Sin(radians);
        //        SKPoint point = new SKPoint(x, y);

        //        if (angle == 0)
        //        {
        //            path.MoveTo(point);
        //        }
        //        else
        //        {
        //            path.LineTo(point);
        //        }
        //    }

        //    using (SKPaint paint = new SKPaint())
        //    {
        //        paint.Style = SKPaintStyle.Stroke;
        //        paint.Color = SKColors.Red;
        //        paint.StrokeWidth = 5;
        //        paint.PathEffect = SKPathEffect.CreateDash(new float[] { 5, 5 }, dashPhase);

        //        canvas.DrawPath(path, paint);
        //    }
        //}
        //}

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            pageIsActive = false;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            pageIsActive = true;
        }

        private async Task ConnectBtDevice()
        {
            await _bluetoothDeviceHelper.Connect(_btDevice.Address);
        }
    }
}
