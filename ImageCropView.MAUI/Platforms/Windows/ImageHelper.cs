using System.Drawing;

namespace ImageCropView.MAUI
{
    // All the code in this file is only included on Windows.
    public static class ImageHelper
    {
        public static byte[] Resize(byte[] imageData, double viewportWidth, double viewportHeight, double maxZoom)
        {
            Bitmap bmp = new Bitmap(new MemoryStream(imageData));
            float scaledWidth, scaledHeight;
            float width = bmp.Width;
            float height = bmp.Height;
            if (width > height)
            {
                scaledHeight = (float)(viewportHeight * maxZoom);
                scaledWidth = scaledHeight * (width / height);
            }
            else
            {
                scaledWidth = (float)(viewportWidth * maxZoom);
                scaledHeight = scaledWidth * (height / width);
            }
            if (scaledWidth < width && scaledHeight < height)
            {
                bmp = new Bitmap(bmp, (int)scaledWidth, (int)scaledHeight);
                MemoryStream ms = new MemoryStream();
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
            else
            {
                return imageData;
            }
        }
    }
}
