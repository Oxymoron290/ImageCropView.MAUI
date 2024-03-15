using Microsoft.Maui.Graphics.Platform;
using Font = Microsoft.Maui.Graphics.Font;
using IImage = Microsoft.Maui.Graphics.IImage;
namespace ImageCropView.MAUI.Drawables
{
    internal class ImageDrawable : IDrawable
    {
        IImage image;
        double zoom;
        double offsetX = 0;
        double offsetY = 0;
        public ImageDrawable(IImage image, double zoom, double offsetX, double offsetY)
        {
            this.image = image;
            this.zoom = zoom;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (image != null)
            {
                canvas.DrawImage(image, (int)offsetX, (int)offsetY, (int)(image.Width * zoom), (int)(image.Height * zoom));
            }
        }
    }
}
