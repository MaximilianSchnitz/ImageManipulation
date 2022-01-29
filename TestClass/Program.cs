using System;
using System.Drawing;

namespace ImageManipulation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var bmp = new Bitmap("C:/Users/felix/desktop/img.jpg");
            var img = new MemoryImage(bmp);

            img.SetPixel(2700, 1700, Color.FromArgb(255, 200, 200, 200));
            Console.WriteLine(img.GetPixel(2700, 1700));
        }
    }
}