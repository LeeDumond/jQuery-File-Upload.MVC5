using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace jQuery_File_Upload.MVC5.Helpers
{
    public class ImageHandler
    {
        /// <summary>
        ///     Method to resize, convert and save the image.
        /// </summary>
        /// <param name="image">Bitmap image.</param>
        /// <param name="maxWidth">resize width.</param>
        /// <param name="maxHeight">resize height.</param>
        /// <param name="quality">quality setting value.</param>
        /// <param name="filePath">file path.</param>
        public void Save(Bitmap image, int maxWidth, int maxHeight, int quality, string filePath)
        {
            // Get the image's original width and height
            var originalWidth = image.Width;
            var originalHeight = image.Height;

            // To preserve the aspect ratio
            var ratioX = maxWidth / (float) originalWidth;
            var ratioY = maxHeight / (float) originalHeight;
            var ratio = Math.Min(ratioX, ratioY);

            // New width and height based on aspect ratio
            var newWidth = (int) (originalWidth * ratio);
            var newHeight = (int) (originalHeight * ratio);

            // Convert other formats (including CMYK) to RGB.
            var newImage = new Bitmap(newWidth, newHeight, PixelFormat.Format24bppRgb);

            // Draws the image in the specified size with quality mode set to HighQuality
            using (var graphics = Graphics.FromImage(newImage))
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            // Get an ImageCodecInfo object that represents the JPEG codec.
            var imageCodecInfo = GetEncoderInfo(ImageFormat.Jpeg);

            // Create an Encoder object for the Quality parameter.
            var encoder = Encoder.Quality;

            // Create an EncoderParameters object. 
            var encoderParameters = new EncoderParameters(1);

            // Save the image as a JPEG file with quality level.
            var encoderParameter = new EncoderParameter(encoder, quality);
            encoderParameters.Param[0] = encoderParameter;
            newImage.Save(filePath, imageCodecInfo, encoderParameters);
        }

        public static Bitmap LoadImage(string path)
        {
            var ms = new MemoryStream(File.ReadAllBytes(path));
            GC.KeepAlive(ms);
            return (Bitmap) Image.FromStream(ms);
        }

        public static Size GetThumbnailSize(Image original)
        {
            // Maximum size of any dimension.
            const int MAX_PIXELS = 40;

            // Width and height.
            var originalWidth = original.Width;
            var originalHeight = original.Height;

            // Compute best factor to scale entire image based on larger dimension.
            double factor;
            if (originalWidth > originalHeight)
            {
                factor = (double) MAX_PIXELS / originalWidth;
            }
            else
            {
                factor = (double) MAX_PIXELS / originalHeight;
            }

            // Return thumbnail size.
            return new Size((int) (originalWidth * factor), (int) (originalHeight * factor));
        }

        /// <summary>
        ///     Method to get encoder infor for given image format.
        /// </summary>
        /// <param name="format">Image format</param>
        /// <returns>image codec info.</returns>
        private ImageCodecInfo GetEncoderInfo(ImageFormat format)
        {
            return ImageCodecInfo.GetImageDecoders().SingleOrDefault(c => c.FormatID == format.Guid);
        }
    }
}