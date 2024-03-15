using Microsoft.Maui.Storage;

namespace ImageCropViewSample
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            LoadUriImage();
        }

        private async void SaveClicked(object sender, EventArgs e)
        {
            var stream = await crop.GetCroppedImageStreamAsync(2, new Rect(75, 50, 150, 200));
            ImageSource src = StreamImageSource.FromStream(() => stream);
            result.Source = src;
        }

        private void zoomStepper_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            crop.Zoom = zoomStepper.Value;
        }

        private void urlSourceButton_Clicked(object sender, EventArgs e)
        {
            LoadUriImage();
        }

        private void LoadUriImage()
        {
            var src = new UriImageSource()
            {
                Uri = new System.Uri("https://photos2.insidercdn.com/iphone4scamera-111004-full.JPG")
            };
            crop.Source = src;
        }

        private async void assetSourceButton_Clicked(object sender, EventArgs e)
        {
            Stream inputStream = await FileSystem.Current.OpenAppPackageFileAsync("horses.jpg");
            ImageSource src = StreamImageSource.FromStream(() => inputStream);
            crop.Source = src;
        }
    }
}

