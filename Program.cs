using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace pngDiff
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("pngDiff");
            Sub(args);
            Console.WriteLine();
            Console.WriteLine("Press key to exit");
            Console.ReadKey();
        }


        static private void Sub(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("usage: pngDiff <input image path 1> <input image path 2>");
                return;
            }

            var path0 = args[0];
            var path1 = args[1];

            if (!File.Exists(path0))
            {
                Console.WriteLine("The file you specified does not exist. {0}", path0);
                return;
            }
            if (!File.Exists(path1))
            {
                Console.WriteLine("The file you specified does not exist. {0}", path1);
                return;
            }

            var bitmap0 = new Bitmap(path0);
            var bitmap1 = new Bitmap(path1);
            int bitmapWidth = bitmap0.Width;
            int bitmapHeight = bitmap0.Height;

            if (bitmapWidth != bitmap1.Width
             || bitmapHeight != bitmap1.Height)
            {
                Console.WriteLine("The files sizes are different. {0}x{1} vs {2}x{3} ", 
                    bitmapWidth, bitmapHeight, bitmap1.Width, bitmap1.Height);
                return;
            }

            BitmapData bitmapData0 = bitmap0.LockBits(
                Rectangle.FromLTRB(0, 0, bitmapWidth, bitmapHeight),
                ImageLockMode.ReadOnly,
                bitmap0.PixelFormat);

            BitmapData bitmapData1 = bitmap1.LockBits(
                Rectangle.FromLTRB(0, 0, bitmapWidth, bitmapHeight),
                ImageLockMode.ReadOnly,
                bitmap1.PixelFormat);

            int bitmapStride0 = bitmapData0.Stride;
            int bitmapStride1 = bitmapData1.Stride;

            if (bitmapStride0 != bitmapStride1)
            {
                Console.WriteLine("Note: The files strides are different. {0} vs {1}",
                    bitmapStride0, bitmapStride1);
            }

            if (bitmap0.PixelFormat != bitmap1.PixelFormat)
            {
                Console.WriteLine("Note: The files formats are different. {0} vs {1}",
                    bitmap0.PixelFormat.ToString(), bitmap1.PixelFormat.ToString());
            }

            int bitmapComponents0 = GetComponentsNumber(bitmap0.PixelFormat);
            int bitmapComponents1 = GetComponentsNumber(bitmap1.PixelFormat);

            int diffPixels = 0;

            Console.WriteLine();
            Console.WriteLine(path0);
            Console.WriteLine(path1);

            var colorDictionary0 = new Dictionary<Color, int>();
            var colorDictionary1 = new Dictionary<Color, int>();

            for (int y = 0; y < bitmapHeight; y++)
            {
                for (int x = 0; x < bitmapWidth; x++)
                {
                    Color color0 = GetColor(bitmapData0, x, y, bitmap0.PixelFormat, bitmapStride0, bitmapComponents0, bitmap0);
                    Color color1 = GetColor(bitmapData1, x, y, bitmap1.PixelFormat, bitmapStride1, bitmapComponents1, bitmap1);

                    if (color0.ToArgb() != color1.ToArgb())
                    {
                        ++diffPixels;
                    }

                    if (colorDictionary0.ContainsKey(color0))
                        colorDictionary0[color0] += 1;
                    else
                        colorDictionary0[color0] = 1;


                    if (colorDictionary1.ContainsKey(color1))
                        colorDictionary1[color1] += 1;
                    else
                        colorDictionary1[color1] = 1;
                }
            }

            bitmap0.UnlockBits(bitmapData0);
            bitmap1.UnlockBits(bitmapData1);

            Console.WriteLine("Number of pixels: {0}", bitmapWidth * bitmapHeight);
            Console.WriteLine("Number of different pixels: {0} ({1}%)", diffPixels, 100 * (float)diffPixels / (bitmapWidth*bitmapHeight));
            Console.WriteLine();

            if (colorDictionary0.Keys.Count != colorDictionary1.Keys.Count)
            {
                Console.WriteLine("Color palette sizes differ: {0} vs {1}", colorDictionary0.Keys.Count, colorDictionary1.Keys.Count);
            }
            
            //else

            {
                int diffColorPalette = 0;
                foreach (Color color0 in colorDictionary0.Keys)
                {
                    if (!colorDictionary1.ContainsKey(color0))
                    {
                        ++diffColorPalette;
                    }
                }

                if (diffColorPalette > 0)
                {
                    Console.WriteLine("Number of colors: {0}", colorDictionary0.Keys.Count);
                    Console.WriteLine("Number of different colors: {0} ({1}%)", diffColorPalette, 100 * (float)diffColorPalette / colorDictionary0.Keys.Count);
                }
                else
                {
                    int diffColorDistribution = 0;

                    foreach (Color color0 in colorDictionary0.Keys)
                    {
                        if (colorDictionary0[color0] != colorDictionary1[color0])
                        {
                            ++diffColorDistribution;
                        }
                    }


                    Console.WriteLine("Number of colors: {0}", colorDictionary0.Keys.Count);
                    Console.WriteLine("Number of colors with different distribution: {0} ({1}%)", diffColorDistribution, 100 * (float)diffColorDistribution / colorDictionary0.Keys.Count);
                }

            }

        }






        static private Color GetColor(BitmapData bitmapData, int x, int y, PixelFormat pixelFormat, int bitmapStride, int bitmapComponents,
            Bitmap bitmap)
        {
            if (pixelFormat == PixelFormat.Format8bppIndexed)
            {
                byte index = Marshal.ReadByte(bitmapData.Scan0, (bitmapStride * y) + (bitmapComponents * x));
                return bitmap.Palette.Entries[index];
            }
            else
            {
                Color color = Color.FromArgb(
                            Marshal.ReadInt32(bitmapData.Scan0, (bitmapStride * y) + (bitmapComponents * x)));
                return color;
            }
        }






        static private int GetComponentsNumber(PixelFormat pixelFormat)
        {
            switch(pixelFormat)
            {
                case PixelFormat.Format8bppIndexed:
                    return 1;

                case PixelFormat.Format24bppRgb:
                    return 3;

                case PixelFormat.Format32bppArgb:
                    return 4;

                default:
                    Debug.Assert(false);
                    return 0;
            }
        }
    }
}
