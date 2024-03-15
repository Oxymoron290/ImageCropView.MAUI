using ImageSource = Microsoft.Maui.Controls.ImageSource;
using IImage = Microsoft.Maui.Graphics.IImage;
using Microsoft.Maui.Graphics.Platform;
using ImageCropView.MAUI.Drawables;

namespace ImageCropView.MAUI;

public partial class ImageCropView : ContentView
{
    readonly PinchGestureRecognizer _pinchGesture;
    readonly PanGestureRecognizer _panGesture;
    HttpClient client = new HttpClient();
    const double MIN_SCALE = 1;
    double minOffsetX = 0;
    double maxOffsetX = 0;
    double minOffsetY = 0;
    double maxOffsetY = 0;
    double imageActualWidth = 0;
    double imageActualHeight = 0;
    double imageActualScale = 1;
    byte[] imageData = [];
    public ImageCropView()
    {
        InitializeComponent();
        this.SizeChanged += ImageCropView_SizeChanged;
        _root.BindingContext = this;

        _pinchGesture = new PinchGestureRecognizer();
        _pinchGesture.PinchUpdated += PinchGesture_PinchUpdated;

        _panGesture = new PanGestureRecognizer();
        _panGesture.PanUpdated += PanGesture_PanUpdated;
        AddGestureRecognizers();
        ResetCrop();
        timer = new Timer(UpdateImage, null, 0, Delay);
    }

    private async void ImageCropView_SizeChanged(object? sender, EventArgs e)
    {
        await LoadSourceAsync(Source);
        ResetCrop();
    }

    ~ImageCropView()
    {
        timer.Dispose();
    }

    private void AddGestureRecognizers()
    {
        _root.GestureRecognizers.Clear();
        _root.GestureRecognizers.Add(_pinchGesture);
        _root.GestureRecognizers.Add(_panGesture);
    }

    public static readonly BindableProperty AspectProperty = BindableProperty.Create(nameof(Aspect), typeof(Aspect), typeof(ImageCropView), Aspect.Fill);

    public Aspect Aspect
    {
        get { return (Aspect)GetValue(AspectProperty); }
        set { SetValue(AspectProperty, value); }
    }

    public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(ImageSource), typeof(ImageCropView), default(ImageSource));

    public ImageSource Source
    {
        get { return (ImageSource)GetValue(SourceProperty); }
        set { SetValue(SourceProperty, value); }
    }


    public static readonly BindableProperty FrameViewProperty = BindableProperty.Create(nameof(FrameView), typeof(View), typeof(ImageCropView), default(View));

    public View FrameView
    {
        get { return (View)GetValue(FrameViewProperty); }
        set { SetValue(FrameViewProperty, value); }
    }

    public static readonly BindableProperty PanSpeedProperty = BindableProperty.Create(nameof(PanSpeed), typeof(double), typeof(ImageCropView), 1d);

    public double PanSpeed
    {
        get { return (double)GetValue(PanSpeedProperty); }
        set { SetValue(PanSpeedProperty, value); }
    }


    public static readonly BindableProperty ZoomSpeedProperty = BindableProperty.Create(nameof(ZoomSpeed), typeof(double), typeof(ImageCropView), 1d);

    public double ZoomSpeed
    {
        get { return (double)GetValue(ZoomSpeedProperty); }
        set { SetValue(ZoomSpeedProperty, value); }
    }


    public static readonly BindableProperty ZoomProperty = BindableProperty.Create(nameof(Zoom), typeof(double), typeof(ImageCropView), 1d);

    public double Zoom
    {
        get { return (double)GetValue(ZoomProperty); }
        set { SetValue(ZoomProperty, value); }
    }

    public static readonly BindableProperty OffsetProperty = BindableProperty.Create(nameof(Offset), typeof(Point), typeof(ImageCropView), default(Point));

    public Point Offset
    {
        get { return (Point)GetValue(OffsetProperty); }
        set { SetValue(OffsetProperty, value); }
    }


    public static readonly BindableProperty DelayProperty = BindableProperty.Create(nameof(Delay), typeof(int), typeof(ImageCropView), 100);

    public int Delay
    {
        get { return (int)GetValue(DelayProperty); }
        set { SetValue(DelayProperty, value); }
    }

    public static readonly BindableProperty MaxZoomProperty = BindableProperty.Create(nameof(MaxZoom), typeof(double), typeof(ImageCropView), 4d);

    public double MaxZoom
    {
        get { return (double)GetValue(MaxZoomProperty); }
        set { SetValue(MaxZoomProperty, value); }
    }


    public static readonly BindableProperty TouchGesturesEnabledProperty = BindableProperty.Create(nameof(TouchGesturesEnabled), typeof(bool), typeof(ImageCropView), true);

    public bool TouchGesturesEnabled
    {
        get { return (bool)GetValue(TouchGesturesEnabledProperty); }
        set { SetValue(TouchGesturesEnabledProperty, value); }
    }


    public static readonly BindableProperty ImageRotationProperty = BindableProperty.Create(nameof(ImageRotation), typeof(int), typeof(ImageCropView), default(int));

    public int ImageRotation
    {
        get { return (int)GetValue(ImageRotationProperty); }
        set { SetValue(ImageRotationProperty, value); }
    }

    void PinchGesture_PinchUpdated(object? sender, PinchGestureUpdatedEventArgs e)
    {
        switch (e.Status)
        {
            case GestureStatus.Running:
                double current = (e.Scale - 1) / 2 * ZoomSpeed;
                Zoom = Clamp(Zoom + current, MIN_SCALE, MaxZoom);
                break;
        }
    }

    double prevOffsetX = 0;
    double prevOffsetY = 0;
    void PanGesture_PanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                prevOffsetX = Offset.X;
                prevOffsetY = Offset.Y;
                break;
            case GestureStatus.Running:
                var xOffset = e.TotalX / imageActualScale;
                var yOffset = e.TotalY / imageActualScale;
                Offset = new Point(Clamp(prevOffsetX - xOffset, minOffsetX, maxOffsetX), Clamp(prevOffsetY - yOffset, minOffsetY, maxOffsetY));
                break;
        }
    }

    void ResetCrop()
    {
        if (imageActualHeight > 0)
        {
            Zoom = 1;
            CalculateActualScale();
            Offset = new Point(imageActualWidth / 2, imageActualHeight / 2);
            Invalidate();
        }
    }

    private void UpdateMinMaxOffsets()
    {
        double halfWidth = this.Width / 2 / imageActualScale;
        double halfHeight = this.Height / 2 / imageActualScale;
        minOffsetX = halfWidth;
        maxOffsetX = imageActualWidth - halfWidth;
        minOffsetY = halfHeight;
        maxOffsetY = imageActualHeight - halfHeight;
        Offset = new Point(Clamp(Offset.X, minOffsetX, maxOffsetX), Clamp(Offset.Y, minOffsetY, maxOffsetY));
    }

    private void CalculateActualScale()
    {
        var scaleX = this.Width / imageActualWidth * Zoom;
        var scaleY = this.Height / imageActualHeight * Zoom;
        imageActualScale = Math.Max(scaleX, scaleY);
        UpdateMinMaxOffsets();
    }

    private void UpdateImage(object? state = null)
    {
        if (needsUpdate)
        {
            this.Dispatcher.Dispatch(() =>
            {
                needsUpdate = false;
                double halfWidth = this.Width / 2 / imageActualScale;
                double halfHeight = this.Height / 2 / imageActualScale;
                var newWidth = imageActualWidth * imageActualScale;
                if (newWidth != image.Width)
                {
                    image.WidthRequest = newWidth;
                }
                var newHeight = imageActualHeight * imageActualScale;
                if (newHeight != image.Height)
                {
                    image.HeightRequest = newHeight;
                }
                var newMargin = new Thickness(-(int)((Offset.X - halfWidth) * imageActualScale), -(int)((Offset.Y - halfHeight) * imageActualScale), 0, 0);
                if (newMargin != image.Margin)
                {
                    image.Margin = newMargin;
                }
            });
        }
    }

    void Invalidate()
    {
        needsUpdate = true;
    }

    protected override async void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == SourceProperty.PropertyName)
        {
            await LoadSourceAsync(Source);
            ResetCrop();
        }
        else if (propertyName == DelayProperty.PropertyName)
        {
        }
        else if (propertyName == TouchGesturesEnabledProperty.PropertyName)
        {
            GestureRecognizers.Clear();

            if (TouchGesturesEnabled)
            {
                AddGestureRecognizers();
            }
        }
        else if (propertyName == ZoomProperty.PropertyName)
        {
            CalculateActualScale();
            Invalidate();
        }
        else if (propertyName == OffsetProperty.PropertyName)
        {
            Invalidate();
        }
        else if (propertyName == ImageRotationProperty.PropertyName)
        {
            ResetCrop();
            imageRotation = ImageRotation;
        }
        else if (propertyName == AspectProperty.PropertyName)
        {
            aspect = Aspect;
        }
        else if (propertyName == FrameViewProperty.PropertyName)
        {
            frameContainer.Content = FrameView;
        }
        else if (propertyName == DelayProperty.PropertyName)
        {
            if (timer != null)
            {
                timer.Dispose();
            }
            timer = new Timer(UpdateImage, null, 0, Delay);
        }
    }

    private async Task LoadSourceAsync(ImageSource source)
    {
        if (this.Width <= 0 || this.Height <= 0) { return; }
        if (Source is UriImageSource urlSource)
        {
            imageData = await client.GetByteArrayAsync(urlSource.Uri?.OriginalString);
        }
        else if (Source is StreamImageSource streamSource)
        {
            using var s = await streamSource.Stream(CancellationToken.None);
            MemoryStream ms = new MemoryStream();
            s.CopyTo(ms);
            imageData = ms.ToArray();
        }
        var stream = new MemoryStream(imageData);
        // Downsize the image to the size at maximum zoom to save memory and improve performance.
        // On Windows, Downsize is not supported, so we will call into a platform API for rescaling.
#if !WINDOWS
        IImage img = PlatformImage.FromStream(stream);
        float maxWidth, maxHeight;
        if (img.Width > img.Height)
        {
            maxHeight = (float)(this.Height * MaxZoom);
            maxWidth = maxHeight * (img.Width / img.Height);
        }
        else
        {
            maxWidth = (float)(this.Width * MaxZoom);
            maxHeight = maxWidth * (img.Height / img.Width);
        }
        if (maxWidth < img.Width && maxHeight < img.Height)
        {
            img = img.Downsize(maxWidth, maxHeight, true);
        }
#else
        imageData = ImageHelper.Resize(imageData, this.Width, this.Height, MaxZoom);
        IImage img = PlatformImage.FromStream(new MemoryStream(imageData));
#endif
        imageActualWidth = (int)img.Width;
        imageActualHeight = (int)img.Height;
        imageData = img.AsBytes();
        stream = new MemoryStream(imageData);
        image.Source = ImageSource.FromStream(() => stream);
        ResetCrop();
    }

    double _width = -1;
    double _height = -1;
    private int imageRotation;
    private Aspect aspect;
    private Timer timer;
    private bool needsUpdate = true;

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (width > 0 && height > 0 && (Math.Abs(_width - width) > double.Epsilon || Math.Abs(_height - height) > double.Epsilon))
        {
            _width = width;
            _height = height;
            ResetCrop();
        }
    }

    public async Task<Stream> GetCroppedImageStreamAsync(double scale = 1, Rect? crop = null)
    {
        var stream = new MemoryStream(imageData);
        IImage img = PlatformImage.FromStream(stream);
        double insetX = 0;
        double insetY = 0;
        double outputWidth;
        double outputHeight;
        if (crop != null)
        {
            outputWidth = crop.Value.Width;
            outputHeight = crop.Value.Height;
            insetX = crop.Value.X;
            insetY = crop.Value.Y;
        }
        else
        {
            outputWidth = this.Width;
            outputHeight = this.Height;
        }
        gv.WidthRequest = outputWidth * scale;
        gv.HeightRequest = outputHeight * scale;
        while (Math.Abs(gv.Width - outputWidth * scale) > 2 || Math.Abs(gv.Height - outputHeight * scale) > 2)
        {
            await Task.Delay(1);
        }
        var overallScale = scale;
        double x = (-insetX - Offset.X * imageActualScale + this.Width / 2) * overallScale;
        double y = (-insetY - Offset.Y * imageActualScale + this.Height / 2) * overallScale;
        var drawable = new ImageDrawable(img, imageActualScale * overallScale, x, y);
        gv.Drawable = drawable;
        var screenshotResult = await gv.CaptureAsync();
        if (screenshotResult != null)
        {
            Stream outputStream = new MemoryStream();
            await screenshotResult.CopyToAsync(outputStream);
            outputStream.Seek(0, SeekOrigin.Begin);
            return outputStream;
        }
        else
        {
            throw new Exception();
        }
    }

    T Clamp<T>(T value, T minimum, T maximum) where T : IComparable
    {
        if (value.CompareTo(minimum) < 0)
            return minimum;
        if (value.CompareTo(maximum) > 0)
            return maximum;

        return value;
    }
}