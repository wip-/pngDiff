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

            int bitmapStride = bitmapData0.Stride;

            if (bitmapStride != bitmapData1.Stride)
            {
                Console.WriteLine("The files strides are different. {0} vs {1}",
                    bitmapStride, bitmapData1.Stride);
                return;
            }

            if (bitmap0.PixelFormat != bitmap1.PixelFormat)
            {
                Console.WriteLine("The files formats are different. {0} vs {1}",
                    bitmap0.PixelFormat.ToString(), bitmap1.PixelFormat.ToString());
                return;
            }

            int bitmapComponents = GetComponentsNumber(bitmap0.PixelFormat);
            int diffPixels = 0;

            Console.WriteLine();
            Console.WriteLine(path0);
            Console.WriteLine(path1);

            if (bitmap0.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                for (int y = 0; y <= bitmapHeight - 1; y++)
                {
                    for (int x = 0; x <= bitmapWidth - 1; x++)
                    {
                        byte index0 = Marshal.ReadByte(bitmapData0.Scan0, (bitmapStride * y) + (bitmapComponents * x));
                        byte index1 = Marshal.ReadByte(bitmapData1.Scan0, (bitmapStride * y) + (bitmapComponents * x));
                        if (index0 != index1)
                        {
                            ++diffPixels;
                        }
                    }
                }
            }
            else
            {
                for (int y = 0; y <= bitmapHeight - 1; y++)
                {
                    for (int x = 0; x <= bitmapWidth - 1; x++)
                    {
                        Color color0 = Color.FromArgb(
                            Marshal.ReadInt32(bitmapData0.Scan0, (bitmapStride * y) + (bitmapComponents * x)));
                        Color color1 = Color.FromArgb(
                            Marshal.ReadInt32(bitmapData1.Scan0, (bitmapStride * y) + (bitmapComponents * x)));

                        if (color0.ToArgb() != color1.ToArgb())
                        {
                            ++diffPixels;
                        }

                    }
                }
            }

            bitmap0.UnlockBits(bitmapData0);
            bitmap1.UnlockBits(bitmapData1);

            Console.WriteLine("Number of different pixels: {0}", diffPixels);

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
