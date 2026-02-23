using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ynivermag_bad
{
    public class ProductImageService
    {
        private string _productsImagesPath;
        private string _defaultImagePath;

        public ProductImageService()
        {
            InitializeImagePaths();
        }

        private void InitializeImagePaths()
        {
            try
            {
                string startupPath = Application.StartupPath;

                // Если запущено из bin\Debug или bin\Release
                if (startupPath.Contains(@"\bin\Debug") || startupPath.Contains(@"\bin\Release"))
                {
                    string projectRoot = Directory.GetParent(Directory.GetParent(startupPath).FullName).FullName;
                    _productsImagesPath = Path.Combine(projectRoot, "Images", "Products");
                }
                else
                {
                    _productsImagesPath = Path.Combine(startupPath, "Images", "Products");
                }

                _defaultImagePath = Path.Combine(_productsImagesPath, "Default.jpg");

                // Создаем папку если ее нет
                if (!Directory.Exists(_productsImagesPath))
                {
                    Directory.CreateDirectory(_productsImagesPath);
                }

                // Создаем заглушку если ее нет
                if (!File.Exists(_defaultImagePath))
                {
                    CreateDefaultImage();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации путей: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CreateDefaultImage()
        {
            try
            {
                Bitmap defaultImage = new Bitmap(200, 200);
                using (Graphics g = Graphics.FromImage(defaultImage))
                {
                    g.Clear(Color.LightGray);
                    using (Font font = new Font("Arial", 12, FontStyle.Bold))
                    using (Brush brush = new SolidBrush(Color.DarkGray))
                    {
                        string text = "Нет фото";
                        SizeF textSize = g.MeasureString(text, font);
                        float x = (defaultImage.Width - textSize.Width) / 2;
                        float y = (defaultImage.Height - textSize.Height) / 2;
                        g.DrawString(text, font, brush, x, y);
                    }
                }

                if (!Directory.Exists(_productsImagesPath))
                {
                    Directory.CreateDirectory(_productsImagesPath);
                }
                defaultImage.Save(_defaultImagePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                defaultImage.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания заглушки: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public string GetProductsImagesPath()
        {
            return _productsImagesPath;
        }

        public Image LoadImageFromFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    return Image.FromFile(filePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки изображения: {ex.Message}");
            }
            return null;
        }

        public Image LoadDefaultProductImage()
        {
            try
            {
                if (File.Exists(_defaultImagePath))
                {
                    return Image.FromFile(_defaultImagePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки заглушки: {ex.Message}");
            }

            // Создаем простую заглушку в памяти
            Bitmap defaultImage = new Bitmap(100, 100);
            using (Graphics g = Graphics.FromImage(defaultImage))
            {
                g.Clear(Color.LightGray);
                using (Font font = new Font("Arial", 8, FontStyle.Bold))
                using (Brush brush = new SolidBrush(Color.DarkGray))
                {
                    string text = "Нет фото";
                    SizeF textSize = g.MeasureString(text, font);
                    float x = (defaultImage.Width - textSize.Width) / 2;
                    float y = (defaultImage.Height - textSize.Height) / 2;
                    g.DrawString(text, font, brush, x, y);
                }
            }
            return defaultImage;
        }

        // НОВЫЙ МЕТОД: Получение миниатюры товара (как GetServiceThumbnail)
        public Image GetProductThumbnail(string photoFileName, int width, int height)
        {
            try
            {
                if (string.IsNullOrEmpty(photoFileName))
                {
                    return ScaleImageToFit(LoadDefaultProductImage(), width, height);
                }

                string imagePath = Path.Combine(_productsImagesPath, photoFileName);
                if (File.Exists(imagePath))
                {
                    using (var img = Image.FromFile(imagePath))
                    {
                        return ScaleImageToFit(img, width, height);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения миниатюры: {ex.Message}");
            }

            return ScaleImageToFit(LoadDefaultProductImage(), width, height);
        }

        // НОВЫЙ МЕТОД: Масштабирование изображения с сохранением пропорций (как в ImageService)
        public Image ScaleImageToFit(Image image, int maxWidth, int maxHeight)
        {
            if (image == null) return null;

            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);
            using (var graphics = Graphics.FromImage(newImage))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }

        // НОВЫЙ МЕТОД: Расчет оптимального размера миниатюры (как в ImageService)
        public Size CalculateOptimalThumbnailSize(DataGridView dgv, int defaultHeight)
        {
            if (dgv == null || dgv.RowTemplate == null)
                return new Size(defaultHeight, defaultHeight);

            int rowHeight = dgv.RowTemplate.Height;
            if (rowHeight <= 0)
                rowHeight = defaultHeight;

            return new Size(rowHeight - 4, rowHeight - 4);
        }
    }
}