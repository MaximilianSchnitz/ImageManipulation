using System.Drawing;
using System.Drawing.Imaging;

namespace ImageManipulation
{
    public class MemoryImage
    {
        Bitmap bmp;
        BitmapData bmpData;

        public int Width { get => bmp.Width; }
        public int Height { get => bmp.Height; }

        public MemoryImage(Image img)
        {
            bmp = (Bitmap) img.Clone();
            bmpData = bmp.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        }

        unsafe byte* GetPixelPointer(int x, int y)
        {
            return (byte*)bmpData.Scan0 + x * 4 + y * bmpData.Stride;
        }

        public unsafe Color GetPixel(int x, int y)
        {
            byte* pixel = GetPixelPointer(x, y);

            byte a = pixel[3];
            byte r = pixel[2];
            byte g = pixel[1];
            byte b = pixel[0];

            return Color.FromArgb(a, r, g, b);
        }

        public unsafe void SetPixel(int x, int y, Color c)
        {
            byte* pixel = GetPixelPointer(x, y);

            pixel[3] = c.A;
            pixel[2] = c.R;
            pixel[1] = c.G;
            pixel[0] = c.B;
        }

        public Bitmap ToBitmap()
        {
            bmp.UnlockBits(bmpData);
            return bmp;
        }

        public void Dispose()
        {
            bmp.UnlockBits(bmpData);
            bmp.Dispose();
        }

    }
}