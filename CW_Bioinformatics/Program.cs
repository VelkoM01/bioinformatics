using AForge.Imaging;
using AForge.Imaging.Filters;
using System.Drawing.Imaging;
using System.Drawing;
using AForge.Math.Geometry;
using AForge;
using System.Collections.Generic;
using System.Linq;
using System;

namespace CW_Bioinformatics
{
    class Program
    {
        static void Main()
        {
            string imagePath = "C:\\Users\\veljk\\OneDrive\\Radna površina\\bioinf.png";

            Bitmap image = new Bitmap(imagePath);

            // Извлечение на информация за цветове
            ColorInformation colorInfo = ExtractColorInformation(image);

            // Извеждане на резултата
            Console.WriteLine($"Average Red: {colorInfo.AverageRed}");
            Console.WriteLine($"Average Green: {colorInfo.AverageGreen}");
            Console.WriteLine($"Average Blue: {colorInfo.AverageBlue}");

            // Преобразуване на изображението в черно-бяло
            Grayscale filter = new Grayscale(0.2125, 0.7154, 0.0721);
            Bitmap grayImage = filter.Apply(image);

            // Използване на филтър за подобряване на контраста
            ContrastCorrection contrastFilter = new ContrastCorrection(20);
            contrastFilter.ApplyInPlace(grayImage);

            // Използване на филтър за бинаризация
            BradleyLocalThresholding thresholdFilter = new BradleyLocalThresholding();
            Bitmap binaryImage = thresholdFilter.Apply(grayImage);

            // Извличане на областите с клетки
            BlobCounter blobCounter = new BlobCounter();
            blobCounter.FilterBlobs = true;
            blobCounter.MinHeight = 10;
            blobCounter.MinWidth = 10;
            blobCounter.ProcessImage(binaryImage);

            // Маркиране на областите с клетки в оригиналното изображение
            Blob[] blobs = blobCounter.GetObjectsInformation();
            SimpleShapeChecker shapeChecker = new SimpleShapeChecker();

            Graphics g = Graphics.FromImage(image);
            Pen pen = new Pen(Color.Red, 2);

            foreach (var blob in blobs)
            {
                List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blob);
                if (shapeChecker.IsConvexPolygon(edgePoints, out _))
                {
                    g.DrawPolygon(pen, ToPointsList(edgePoints).ToArray());
                }
            }

            // Добавяне на функционалност за откриване на ръбове и маркиране
            Bitmap edgeDetectedImage = DetectAndMarkEdges(grayImage);
            g.DrawImage(edgeDetectedImage, 0, 0);

            // Запазване на резултата
            image.Save("output_image.jpg", ImageFormat.Jpeg);

            // Освобождаване на ресурсите
            g.Dispose();
            pen.Dispose();
            image.Dispose();
            grayImage.Dispose();
            binaryImage.Dispose();
            edgeDetectedImage.Dispose();
        }

        private static List<System.Drawing.PointF> ToPointsList(List<IntPoint> points)
        {
            return points.Select(p => new System.Drawing.PointF(p.X, p.Y)).ToList();
        }

        private static ColorInformation ExtractColorInformation(Bitmap image)
        {
            int totalRed = 0, totalGreen = 0, totalBlue = 0;

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixelColor = image.GetPixel(x, y);
                    totalRed += pixelColor.R;
                    totalGreen += pixelColor.G;
                    totalBlue += pixelColor.B;
                }
            }

            int totalPixels = image.Width * image.Height;
            int averageRed = totalRed / totalPixels;
            int averageGreen = totalGreen / totalPixels;
            int averageBlue = totalBlue / totalPixels;

            return new ColorInformation(averageRed, averageGreen, averageBlue);
        }

        private static Bitmap DetectAndMarkEdges(Bitmap image)
        {
            CannyEdgeDetector edgeDetector = new CannyEdgeDetector();
            return edgeDetector.Apply(image);
        }
    }
}
