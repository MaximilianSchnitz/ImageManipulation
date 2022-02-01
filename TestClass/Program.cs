using System;
using System.Diagnostics;
using System.Drawing;

namespace ImageManipulation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Start();
            var bmp = new Bitmap("C:/Users/felix/desktop/landscape.jpg");
            ImageEffect.MultiThreadBoxBlur(bmp, 1, 100).Save("C:/Users/felix/desktop/MTBlur1.100.jpg");
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
        }
    }
}