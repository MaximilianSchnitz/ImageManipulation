using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageManipulation
{
    public static class ImageEffect
    {

        public static Bitmap BoxBlur(Image img, int size, int iterations = 1)
        {
            return BoxBlur(img, size, iterations, new Rectangle(0, 0, img.Width, img.Height));
        }

        static object padlock = new object();

        public static Bitmap MultiThreadBoxBlur(Image img, int size, int iterations)
        {
            int threads = Environment.ProcessorCount;

            int horizontalParts = (int) Math.Ceiling(Math.Sqrt(threads));
            int verticalParts = (int) Math.Floor(Math.Sqrt(threads));

            threads = horizontalParts * verticalParts;

            int maxWidth = (int) Math.Ceiling((decimal) img.Width / horizontalParts);
            int maxHeight = (int) Math.Ceiling((decimal) img.Height / verticalParts);

            var emptyBmp = new Bitmap(img.Width, img.Height);
            var copyBmp = (Bitmap)img.Clone();

            for(int iter = 0; iter < iterations; iter++)
            {
                var parts = new Bitmap[threads];
                var tasks = new Task[threads];

                for (int i = 0; i < threads; i++)
                {
                    int rectX = (i * maxWidth) % emptyBmp.Width;
                    int rectY = (i * maxWidth) / emptyBmp.Width * maxHeight;

                    int rectWidth = Math.Min(emptyBmp.Width - rectX, maxWidth);
                    int rectHeight = Math.Min(emptyBmp.Height - rectY, maxHeight);

                    var rect = new Rectangle(rectX, rectY, rectWidth, rectHeight);

                    int part = i;

                    var t = Task.Factory.StartNew(() => ThreadBlur(copyBmp, size, rect, part, parts));
                    tasks[i] = t;
                }

                Task.WaitAll(tasks);

                MergeBitmaps(emptyBmp, parts, maxWidth, maxHeight, horizontalParts, verticalParts);

                copyBmp = emptyBmp;
                emptyBmp = new Bitmap(img.Width, img.Height);
            }

            return copyBmp;

            static void ThreadBlur(Image originalImg, int size, Rectangle rect, int part, Bitmap[] parts)
            {
                Thread.CurrentThread.IsBackground = true;
                var img = BoxBlur(originalImg, size, 1, rect);
                parts[part] = img;
            }

            static void MergeBitmaps(Bitmap fullBmp, Bitmap[] bmps, int maxWidth, int maxHeight, int horizontalParts, int verticalParts)
            {
                using (var g = Graphics.FromImage(fullBmp))
                {
                    for(int i = 0; i < bmps.Length; i++)
                    {
                        int x = (i * maxWidth) % fullBmp.Width;
                        int y = (i * maxWidth) / fullBmp.Width * maxHeight;

                        g.DrawImage(bmps[i], x, y);
                    }
                }
            }
        }

        public static Bitmap BoxBlur(Image img, int size, int iterations, Rectangle rect)
        {
            Bitmap bmp;
            MemoryImage original;
            MemoryImage memImg;

            lock(padlock)
            {
                original = new MemoryImage(img);
                memImg = new MemoryImage(new Bitmap(rect.Width, rect.Height));
            }

            for (int iter = 0; iter < iterations; iter++)
            {
                for (int i = 0; i < rect.Width * rect.Height; i++)
                {
                    int px = i % rect.Width + rect.X;
                    int py = i / rect.Width + rect.Y;

                    int avgA = 0;
                    int avgR = 0;
                    int avgG = 0;
                    int avgB = 0;

                    int realSize = size * 2 + 1;
                    int counter = 0;

                    for (int x = 0; x < realSize; x++)
                    {
                        for (int y = 0; y < realSize; y++)
                        {
                            int xPos = px - realSize / 2 + x;
                            int yPos = py - realSize / 2 + y;

                            if (xPos >= 0 && xPos < original.Width &&
                                yPos >= 0 && yPos < original.Height)
                            {
                                var c = original.GetPixel(xPos, yPos);

                                avgA += c.A;
                                avgR += c.R;
                                avgG += c.G;
                                avgB += c.B;

                                counter++;
                            }
                        }
                    }

                    avgA /= counter;
                    avgR /= counter;
                    avgG /= counter;
                    avgB /= counter;

                    memImg.SetPixel(px - rect.X, py - rect.Y, Color.FromArgb(avgA, avgR, avgG, avgB));
                }
                original = memImg;
            }
            original.Dispose();
            return memImg.ToBitmap();
        }

        public static Bitmap GreyScale(Image img)
        {
            var bmp = (Bitmap)img.Clone();
            var memImg = new MemoryImage(bmp);

            for (int i = 0; i < memImg.Width * memImg.Height; i++)
            {
                int x = i % memImg.Width;
                int y = i / memImg.Width;

                var c = bmp.GetPixel(x, y);

                int avg = (c.R + c.G + c.B) / 3;

                memImg.SetPixel(x, y, Color.FromArgb(c.A, avg, avg, avg));
            }

            return memImg.ToBitmap();
        }

        public static Bitmap Invert(Image img)
        {
            var bmp = (Bitmap)img.Clone();
            var memImg = new MemoryImage(bmp);

            for(int i = 0; i < memImg.Width * memImg.Height; i++)
            {
                int x = i % memImg.Width;
                int y = i / memImg.Width;

                var c = memImg.GetPixel(x, y);
                int r = 255 - c.R;
                int g = 255 - c.G;
                int b = 255 - c.B;

                memImg.SetPixel(x, y, Color.FromArgb(c.A, r, g, b));
            }

            return memImg.ToBitmap();
        }
    }
}
