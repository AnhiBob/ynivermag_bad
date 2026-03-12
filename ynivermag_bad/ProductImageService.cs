using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace ynivermag_bad
{
    public class ProductImageService
    {
        private string _productsImagesPath;
        private Image _defaultImage;
        private static readonly object _lock = new object();

        public ProductImageService()
        {
            InitializePaths();
        }

        private void InitializePaths()
        {
            try
            {
                string startupPath = Application.StartupPath;
                if (startupPath.Contains(@"\bin\Debug") || startupPath.Contains(@"\bin\Release"))
                {
                    string projectRoot = Directory.GetParent(Directory.GetParent(startupPath).FullName).FullName;
                    _productsImagesPath = Path.Combine(projectRoot, "Images", "Products");
                }
                else
                {
                    _productsImagesPath = Path.Combine(startupPath, "Images", "Products");
                }

                if (!Directory.Exists(_productsImagesPath))
                {
                    Directory.CreateDirectory(_productsImagesPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации путей: {ex.Message}");
            }
        }

        public string GetProductsImagesPath()
        {
            return _productsImagesPath;
        }

        // Загрузка изображения с высоким качеством
        public Image LoadHighQualityImage(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return LoadDefaultProductImage();

                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Image original = Image.FromStream(stream);

                    // Создаем копию с высоким качеством
                    Bitmap highQualityCopy = new Bitmap(original.Width, original.Height, PixelFormat.Format32bppArgb);
                    using (Graphics g = Graphics.FromImage(highQualityCopy))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.CompositingQuality = CompositingQuality.HighQuality;
                        g.DrawImage(original, 0, 0, original.Width, original.Height);
                    }

                    original.Dispose();
                    return highQualityCopy;
                }
            }
            catch
            {
                return LoadDefaultProductImage();
            }
        }

        // Улучшенное масштабирование с высоким качеством
        public Image CreateHighQualityThumbnail(Image image, int width, int height)
        {
            if (image == null) return null;

            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        // Масштабирование с сохранением пропорций
        public Image ScaleImageHighQuality(Image image, int maxWidth, int maxHeight)
        {
            if (image == null) return null;

            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            return CreateHighQualityThumbnail(image, newWidth, newHeight);
        }

        // Загрузка изображения для отображения в DataGridView
        public Image LoadProductThumbnail(string fileName, int size)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                    return CreateDefaultThumbnail(size);

                string imagePath = Path.Combine(_productsImagesPath, fileName);

                if (!File.Exists(imagePath))
                    return CreateDefaultThumbnail(size);

                // Кэширование для часто используемых изображений
                string cacheKey = $"{fileName}_{size}";

                // Загружаем оригинал
                using (FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (Image original = Image.FromStream(stream))
                {
                    // Создаем миниатюру с высоким качеством
                    return CreateHighQualityThumbnail(original, size, size);
                }
            }
            catch
            {
                return CreateDefaultThumbnail(size);
            }
        }

        public Image CreateDefaultThumbnail(int size)
        {
            var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(240, 240, 240));

                // Рисуем рамку
                using (Pen pen = new Pen(Color.FromArgb(200, 200, 200)))
                {
                    g.DrawRectangle(pen, 1, 1, size - 3, size - 3);
                }

                // Рисуем иконку фотоаппарата
                using (Font font = new Font("Segoe UI", size / 4, FontStyle.Regular))
                using (Brush brush = new SolidBrush(Color.FromArgb(180, 180, 180)))
                {
                    string text = "📷";
                    SizeF textSize = g.MeasureString(text, font);
                    float x = (size - textSize.Width) / 2;
                    float y = (size - textSize.Height) / 2;

                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    g.DrawString(text, font, brush, x, y);
                }
            }
            return bmp;
        }

        public Image LoadDefaultProductImage()
        {
            lock (_lock)
            {
                if (_defaultImage != null && !_defaultImageIsDisposed())
                    return _defaultImage;

                try
                {
                    string defaultPath = Path.Combine(_productsImagesPath, "Default.jpg");
                    if (File.Exists(defaultPath))
                    {
                        using (FileStream stream = new FileStream(defaultPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            _defaultImage = Image.FromStream(stream);
                        }
                    }
                    else
                    {
                        _defaultImage = CreateDefaultThumbnail(200);

                        // Сохраняем для будущего использования
                        try
                        {
                            _defaultImage.Save(defaultPath, ImageFormat.Jpeg);
                        }
                        catch { }
                    }
                }
                catch
                {
                    _defaultImage = CreateDefaultThumbnail(200);
                }

                return _defaultImage;
            }
        }

        private bool _defaultImageIsDisposed()
        {
            try
            {
                return _defaultImage == null || _defaultImage.Width == 0;
            }
            catch
            {
                return true;
            }
        }


        public Image LoadImageFromFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return LoadDefaultProductImage();

                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return Image.FromStream(stream);
                }
            }
            catch
            {
                return LoadDefaultProductImage();
            }
        }
    }
}