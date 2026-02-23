using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ynivermag_bad
{
    public partial class EditProductForm : Form
    {
        private string _connection;
        public ProductModel Product { get; private set; }
        private Image _selectedImage;
        private string _productsImagesPath;
        private string _defaultImagePath;
        private bool _imageChanged = false;

        // Константа для ограничения размера файла (3 МБ в байтах)
        private const long MAX_IMAGE_SIZE = 3 * 1024 * 1024; // 3 МБ

        public EditProductForm(ProductModel product)
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            Product = product;

            // Инициализация путей для изображений
            InitializeImagePaths();

            LoadProductData();
            LoadCategories();
            LoadProductImage();

            // Настройка PictureBox для приема перетаскивания
            pictureBoxProduct.AllowDrop = true;
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации путей: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadProductImage()
        {
            try
            {
                if (!string.IsNullOrEmpty(Product.photo_path))
                {
                    string imagePath = Path.Combine(_productsImagesPath, Product.photo_path);

                    if (File.Exists(imagePath))
                    {
                        _selectedImage = LoadImageWithoutLock(imagePath);
                        if (_selectedImage != null)
                        {
                            pictureBoxProduct.Image = ScaleImage(_selectedImage, pictureBoxProduct.Width, pictureBoxProduct.Height);
                        }
                        else
                        {
                            LoadDefaultImage();
                        }
                    }
                    else
                    {
                        LoadDefaultImage();
                    }
                }
                else
                {
                    LoadDefaultImage();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}");
                LoadDefaultImage();
            }
        }

        private void LoadDefaultImage()
        {
            try
            {
                // Освобождаем предыдущее изображение
                if (_selectedImage != null)
                {
                    _selectedImage.Dispose();
                    _selectedImage = null;
                }

                if (File.Exists(_defaultImagePath))
                {
                    using (FileStream stream = new FileStream(_defaultImagePath, FileMode.Open, FileAccess.Read))
                    {
                        _selectedImage = Image.FromStream(stream);
                    }
                }
                else
                {
                    // Создаем заглушку
                    Bitmap defaultImage = new Bitmap(pictureBoxProduct.Width, pictureBoxProduct.Height);
                    using (Graphics g = Graphics.FromImage(defaultImage))
                    {
                        g.Clear(Color.LightGray);
                        using (Font font = new Font("Arial", 12, FontStyle.Bold))
                        using (Brush brush = new SolidBrush(Color.DarkGray))
                        {
                            string text = "Изображение товара";
                            SizeF textSize = g.MeasureString(text, font);
                            float x = (defaultImage.Width - textSize.Width) / 2;
                            float y = (defaultImage.Height - textSize.Height) / 2;
                            g.DrawString(text, font, brush, x, y);
                        }
                    }
                    _selectedImage = defaultImage;

                    // Сохраняем заглушку
                    if (!Directory.Exists(_productsImagesPath))
                    {
                        Directory.CreateDirectory(_productsImagesPath);
                    }
                    defaultImage.Save(_defaultImagePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                }

                pictureBoxProduct.Image = ScaleImage(_selectedImage, pictureBoxProduct.Width, pictureBoxProduct.Height);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заглушки: {ex.Message}");
            }
        }

        private Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);
            using (var graphics = Graphics.FromImage(newImage))
            {
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }

        private void LoadProductData()
        {
            NameTB.Text = Product.name;
            Price.Text = Product.price.ToString();
            Count.Text = Product.stock_quantity.ToString();
        }

        private void LoadCategories()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = "SELECT category_id, name FROM category ORDER BY name";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    CategoryCb.DataSource = dt;
                    CategoryCb.DisplayMember = "name";
                    CategoryCb.ValueMember = "category_id";

                    // Устанавливаем текущую категорию
                    if (CategoryCb.Items.Count > 0 && Product.category_id > 0)
                    {
                        for (int i = 0; i < CategoryCb.Items.Count; i++)
                        {
                            DataRowView row = (DataRowView)CategoryCb.Items[i];
                            if (Convert.ToInt32(row["category_id"]) == Product.category_id)
                            {
                                CategoryCb.SelectedIndex = i;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Back_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void EditProduct_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                SaveProductData();

                // Сохраняем изображение
                if (_imageChanged)
                {
                    // Освобождаем ресурсы перед сохранением
                    ReleaseImageResources();

                    string imageFileName = SaveProductImage();
                    if (!string.IsNullOrEmpty(imageFileName))
                    {
                        Product.photo_path = imageFileName;
                    }
                    else if (_selectedImage == null || IsDefaultImage())
                    {
                        // Если изображение удалено, устанавливаем null
                        Product.photo_path = null;
                    }
                }

                // СОХРАНЯЕМ ИЗМЕНЕНИЯ В БАЗЕ ДАННЫХ
                EditClass editClass = new EditClass();
                bool updated = editClass.UpdateProductInDatabase(Product);

                if (updated)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show("Не удалось сохранить изменения в базе данных", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private Image LoadImageWithoutLock(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return null;

                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return Image.FromStream(stream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки изображения: {ex.Message}");
                return null;
            }
        }

        private bool ValidateData()
        {
            // Проверка названия
            if (string.IsNullOrWhiteSpace(NameTB.Text))
            {
                MessageBox.Show("Введите название продукта", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                NameTB.Focus();
                return false;
            }

            // Проверка цены
            if (string.IsNullOrWhiteSpace(Price.Text))
            {
                MessageBox.Show("Введите цену продукта", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Price.Focus();
                return false;
            }

           

            // Проверка количества
            if (string.IsNullOrWhiteSpace(Count.Text))
            {
                MessageBox.Show("Введите количество продукта", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Count.Focus();
                return false;
            }

            if (!int.TryParse(Count.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("Количество должно быть неотрицательным целым числом", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Count.Focus();
                Count.SelectAll();
                return false;
            }

            // Проверка категории
            if (CategoryCb.SelectedValue == null)
            {
                MessageBox.Show("Выберите категорию", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                CategoryCb.Focus();
                return false;
            }

            // Проверка на уникальность названия продукта
            if (!IsProductNameUnique())
            {
                MessageBox.Show("Продукт с таким названием уже существует",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                NameTB.Focus();
                return false;
            }

            return true;
        }

        private bool IsProductNameUnique()
        {
            using (var connection = new MySqlConnection(_connection))
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT COUNT(*) FROM product 
                            WHERE name = @Name AND product_id != @ProductId";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Name", NameTB.Text.Trim());
                    cmd.Parameters.AddWithValue("@ProductId", Product.product_id);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count == 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка проверки названия продукта: {ex.Message}");
                    return false;
                }
            }
        }

        private void SaveProductData()
        {
            decimal.TryParse(Price.Text, out decimal price);
            int.TryParse(Count.Text, out int stock);

            Product.name = NameTB.Text.Trim();
            Product.price = price;
            Product.stock_quantity = stock;

            if (CategoryCb.SelectedValue != null)
            {
                Product.category_id = (int)CategoryCb.SelectedValue;
            }
        }

        private string SaveProductImage()
        {
            try
            {
                if (_selectedImage == null || IsDefaultImage())
                {
                    // Если выбрана заглушка, удаляем старое фото
                    if (!string.IsNullOrEmpty(Product.photo_path))
                    {
                        string oldFilePath = Path.Combine(_productsImagesPath, Product.photo_path);
                        if (File.Exists(oldFilePath) && !IsDefaultImageFile(oldFilePath))
                        {
                            // Снимаем все блокировки с файла
                            GC.Collect();
                            GC.WaitForPendingFinalizers();

                            try
                            {
                                File.Delete(oldFilePath);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Не удалось удалить старый файл: {ex.Message}");
                            }
                        }
                    }
                    return null;
                }

                // Генерируем уникальное имя файла
                string productName = NameTB.Text.Trim().ToLower()
                    .Replace(" ", "_")
                    .Replace("/", "_")
                    .Replace("\\", "_")
                    .Replace(":", "")
                    .Replace("*", "")
                    .Replace("?", "")
                    .Replace("\"", "")
                    .Replace("<", "")
                    .Replace(">", "")
                    .Replace("|", "");

                // Обрезаем, если слишком длинное имя
                if (productName.Length > 50)
                {
                    productName = productName.Substring(0, 50);
                }

                string fileName = $"product_{productName}_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                string filePath = Path.Combine(_productsImagesPath, fileName);

                // Удаляем старое изображение если оно существует и это не заглушка
                if (!string.IsNullOrEmpty(Product.photo_path) && Product.photo_path != fileName)
                {
                    string oldFilePath = Path.Combine(_productsImagesPath, Product.photo_path);
                    if (File.Exists(oldFilePath) && !IsDefaultImageFile(oldFilePath))
                    {
                        // Снимаем все блокировки
                        GC.Collect();
                        GC.WaitForPendingFinalizers();

                        try
                        {
                            File.Delete(oldFilePath);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Не удалось удалить старый файл: {ex.Message}");
                        }
                    }
                }

                // Сохраняем новое изображение с оптимизацией
                bool saved = SaveOptimizedImage(_selectedImage, filePath);

                if (saved)
                {
                    return fileName;
                }
                else
                {
                    return Product.photo_path; // Возвращаем старое имя файла в случае ошибки
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось сохранить изображение: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return Product.photo_path;
            }
        }

        private bool SaveOptimizedImage(Image image, string filePath)
        {
            try
            {
                // Создаем временный Bitmap для сохранения
                using (Bitmap bitmap = new Bitmap(image))
                {
                    // Определяем параметры сжатия для JPEG
                    var encoder = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()
                        .FirstOrDefault(c => c.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);

                    if (encoder != null)
                    {
                        var encoderParams = new System.Drawing.Imaging.EncoderParameters(1);
                        encoderParams.Param[0] = new System.Drawing.Imaging.EncoderParameter(
                            System.Drawing.Imaging.Encoder.Quality, 85L);

                        // Сохраняем во временный файл сначала
                        string tempFile = Path.GetTempFileName();
                        try
                        {
                            bitmap.Save(tempFile, encoder, encoderParams);

                            // Копируем временный файл в нужное место
                            File.Copy(tempFile, filePath, true);

                            // Удаляем временный файл
                            File.Delete(tempFile);
                        }
                        catch
                        {
                            if (File.Exists(tempFile))
                                File.Delete(tempFile);
                            throw;
                        }
                    }
                    else
                    {
                        // Если не нашли JPEG кодек, сохраняем стандартным способом
                        bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                }

                // Принудительно вызываем сборку мусора
                GC.Collect();
                GC.WaitForPendingFinalizers();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении изображения: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        private void ReleaseImageResources()
        {
            // Освобождаем ресурсы текущего изображения в PictureBox
            if (pictureBoxProduct.Image != null)
            {
                Image oldImage = pictureBoxProduct.Image;
                pictureBoxProduct.Image = null;
                oldImage.Dispose();
            }

            // Принудительно вызываем сборку мусора
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private bool IsDefaultImage()
        {
            try
            {
                return _selectedImage == null ||
                       (_defaultImagePath != null &&
                        pictureBoxProduct.ImageLocation == _defaultImagePath);
            }
            catch
            {
                return true;
            }
        }

        private bool IsDefaultImageFile(string filePath)
        {
            try
            {
                return Path.GetFileName(filePath) == "Default.jpg";
            }
            catch
            {
                return false;
            }
        }

        // Обработчики для работы с изображением
        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            LoadImageFromFile();
        }

        private void btnRemoveImage_Click(object sender, EventArgs e)
        {
            RemoveImage();
        }

        private void pictureBoxProduct_Click(object sender, EventArgs e)
        {
            LoadImageFromFile();
        }

        private void LoadImageFromFile()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Изображения (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                openFileDialog.FilterIndex = 1;
                openFileDialog.Title = $"Выберите изображение товара (макс. {MAX_IMAGE_SIZE / (1024 * 1024)} МБ)";
                openFileDialog.RestoreDirectory = true;

                // Добавляем обработчик для проверки размера файла
                openFileDialog.FileOk += OpenFileDialog_FileOk;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string filePath = openFileDialog.FileName;

                        // Проверяем размер файла
                        FileInfo fileInfo = new FileInfo(filePath);
                        if (fileInfo.Length > MAX_IMAGE_SIZE)
                        {
                            MessageBox.Show($"Размер файла слишком большой ({fileInfo.Length / (1024 * 1024)} МБ).\n" +
                                           $"Максимальный разрешенный размер: {MAX_IMAGE_SIZE / (1024 * 1024)} МБ.\n\n" +
                                           "Пожалуйста, выберите файл меньшего размера или сожмите изображение.",
                                           "Ошибка размера файла",
                                           MessageBoxButtons.OK,
                                           MessageBoxIcon.Warning);
                            return;
                        }

                        // Проверяем расширение файла
                        string extension = Path.GetExtension(filePath).ToLower();
                        string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

                        if (!allowedExtensions.Contains(extension))
                        {
                            MessageBox.Show("Выберите файл с поддерживаемым форматом:\n" +
                                           "JPG, JPEG, PNG, BMP или GIF",
                                           "Неверный формат файла",
                                           MessageBoxButtons.OK,
                                           MessageBoxIcon.Warning);
                            return;
                        }

                        // Освобождаем предыдущее изображение
                        if (_selectedImage != null)
                        {
                            _selectedImage.Dispose();
                            _selectedImage = null;
                        }

                        // Загружаем изображение через FileStream, чтобы не блокировать файл
                        using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            _selectedImage = Image.FromStream(stream);
                        }

                        // Дополнительная проверка размера изображения в пикселях
                        if (_selectedImage.Width > 4000 || _selectedImage.Height > 4000)
                        {
                            var result = MessageBox.Show($"Разрешение изображения очень большое ({_selectedImage.Width}x{_selectedImage.Height}).\n" +
                                                       "Рекомендуется использовать изображения до 2000x2000 пикселей.\n\n" +
                                                       "Хотите продолжить загрузку? (изображение будет сжато)",
                                                       "Большое разрешение",
                                                       MessageBoxButtons.YesNo,
                                                       MessageBoxIcon.Question);

                            if (result == DialogResult.No)
                            {
                                _selectedImage.Dispose();
                                _selectedImage = null;
                                return;
                            }
                        }

                        // Масштабируем изображение для PictureBox
                        pictureBoxProduct.Image = ScaleImage(_selectedImage, pictureBoxProduct.Width, pictureBoxProduct.Height);
                        _imageChanged = true;
                    }
                    catch (OutOfMemoryException)
                    {
                        MessageBox.Show("Файл поврежден или не является корректным изображением.",
                                      "Ошибка загрузки",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось загрузить изображение: {ex.Message}", "Ошибка",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                // Отписываемся от события
                openFileDialog.FileOk -= OpenFileDialog_FileOk;
            }
        }

        private void OpenFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            var openFileDialog = sender as OpenFileDialog;
            if (openFileDialog != null)
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(openFileDialog.FileName);

                    // Проверяем размер файла
                    if (fileInfo.Length > MAX_IMAGE_SIZE)
                    {
                        MessageBox.Show($"Размер файла слишком большой ({fileInfo.Length / (1024 * 1024)} МБ).\n" +
                                       $"Максимальный разрешенный размер: {MAX_IMAGE_SIZE / (1024 * 1024)} МБ.",
                                       "Ошибка размера файла",
                                       MessageBoxButtons.OK,
                                       MessageBoxIcon.Warning);
                        e.Cancel = true;
                        return;
                    }

                    // Проверяем расширение
                    string extension = Path.GetExtension(openFileDialog.FileName).ToLower();
                    string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

                    if (!allowedExtensions.Contains(extension))
                    {
                        MessageBox.Show("Выберите файл с поддерживаемым форматом:\n" +
                                       "JPG, JPEG, PNG, BMP или GIF",
                                       "Неверный формат файла",
                                       MessageBoxButtons.OK,
                                       MessageBoxIcon.Warning);
                        e.Cancel = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка проверки файла: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                    e.Cancel = true;
                }
            }
        }

        private void RemoveImage()
        {
            LoadDefaultImage();
            _imageChanged = true;
        }

        // Drag & Drop для PictureBox
        private void pictureBoxProduct_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void pictureBoxProduct_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                string filePath = files[0];

                // Проверяем расширение файла
                string extension = Path.GetExtension(filePath).ToLower();
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

                if (allowedExtensions.Contains(extension))
                {
                    // Проверяем размер файла перед загрузкой
                    try
                    {
                        FileInfo fileInfo = new FileInfo(filePath);
                        if (fileInfo.Length > MAX_IMAGE_SIZE)
                        {
                            MessageBox.Show($"Размер файла слишком большой ({fileInfo.Length / (1024 * 1024)} МБ).\n" +
                                           $"Максимальный разрешенный размер: {MAX_IMAGE_SIZE / (1024 * 1024)} МБ.",
                                           "Ошибка размера файла",
                                           MessageBoxButtons.OK,
                                           MessageBoxIcon.Warning);
                            return;
                        }

                        // Проверяем размер изображения в пикселях
                        using (Image tempImage = Image.FromFile(filePath))
                        {
                            if (tempImage.Width > 4000 || tempImage.Height > 4000)
                            {
                                var result = MessageBox.Show($"Разрешение изображения очень большое ({tempImage.Width}x{tempImage.Height}).\n" +
                                                           "Рекомендуется использовать изображения до 2000x2000 пикселей.\n\n" +
                                                           "Хотите продолжить загрузку? (изображение будет сжато)",
                                                           "Большое разрешение",
                                                           MessageBoxButtons.YesNo,
                                                           MessageBoxIcon.Question);

                                if (result == DialogResult.No)
                                {
                                    return;
                                }
                            }
                        }

                        // Освобождаем предыдущее изображение
                        if (_selectedImage != null)
                        {
                            _selectedImage.Dispose();
                            _selectedImage = null;
                        }

                        // Загружаем изображение через FileStream
                        using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            _selectedImage = Image.FromStream(stream);
                        }

                        pictureBoxProduct.Image = ScaleImage(_selectedImage, pictureBoxProduct.Width, pictureBoxProduct.Height);
                        _imageChanged = true;
                    }
                    catch (OutOfMemoryException)
                    {
                        MessageBox.Show("Файл поврежден или не является корректным изображением.",
                                      "Ошибка загрузки",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось загрузить изображение: {ex.Message}", "Ошибка",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Выберите файл изображения (jpg, jpeg, png, bmp, gif)", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Освобождаем ресурсы изображения
            if (_selectedImage != null)
            {
                _selectedImage.Dispose();
                _selectedImage = null;
            }

            // Очищаем PictureBox
            if (pictureBoxProduct.Image != null)
            {
                pictureBoxProduct.Image.Dispose();
                pictureBoxProduct.Image = null;
            }

            base.OnFormClosing(e);
        }

        // Показ подсказки при наведении на PictureBox
        private void pictureBoxProduct_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(pictureBoxProduct,
                "Кликните для выбора изображения\n" +
                "Или перетащите файл сюда\n" +
                $"Максимальный размер: {MAX_IMAGE_SIZE / (1024 * 1024)} МБ\n" +
                "Поддерживаемые форматы: JPG, JPEG, PNG, BMP, GIF");
        }

        // Фильтрация ввода для цены
        private void Price_TextChanged(object sender, EventArgs e)
        {
            // Сохраняем позицию курсора
            int cursorPosition = Price.SelectionStart;
            int oldLength = Price.Text.Length;

            // Убираем все нецифровые символы, кроме точки и запятой
            string text = Price.Text;
            string filteredText = new string(text.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());

            // Заменяем запятую на точку
            filteredText = filteredText.Replace(',', '.');

            // Проверяем, чтобы точка была только одна
            int dotCount = filteredText.Count(c => c == '.');
            if (dotCount > 1)
            {
                // Оставляем только первую точку
                int firstDotIndex = filteredText.IndexOf('.');
                filteredText = filteredText.Substring(0, firstDotIndex + 1) +
                              filteredText.Substring(firstDotIndex + 1).Replace(".", "");
            }

            // Проверяем, что после точки не больше 2 цифр
            if (filteredText.Contains('.'))
            {
                int dotIndex = filteredText.IndexOf('.');
                string beforeDot = filteredText.Substring(0, dotIndex);
                string afterDot = filteredText.Substring(dotIndex + 1);

                // Ограничиваем количество цифр после точки до 2
                if (afterDot.Length > 2)
                {
                    afterDot = afterDot.Substring(0, 2);
                    filteredText = beforeDot + "." + afterDot;
                }
            }

            // Проверяем, что перед точкой не больше 6 цифр (разумный максимум для цены)
            if (filteredText.Contains('.'))
            {
                int dotIndex = filteredText.IndexOf('.');
                string beforeDot = filteredText.Substring(0, dotIndex);
                if (beforeDot.Length > 6)
                {
                    beforeDot = beforeDot.Substring(0, 6);
                    filteredText = beforeDot + filteredText.Substring(dotIndex);
                }
            }
            else
            {
                // Если нет точки, ограничиваем длину целой части
                if (filteredText.Length > 6)
                {
                    filteredText = filteredText.Substring(0, 6);
                }
            }

            // Проверяем, что число не начинается с нуля (если есть другие цифры)
            if (filteredText.Length > 1 && filteredText[0] == '0' && filteredText[1] != '.')
            {
                filteredText = filteredText.Substring(1);
            }

            // Если строка пустая или состоит только из точки - оставляем как есть
            if (filteredText == ".")
            {
                filteredText = "0.";
            }

            // Обновляем текст, если он изменился
            if (filteredText != text)
            {
                Price.Text = filteredText;

                // Корректируем позицию курсора
                int newLength = Price.Text.Length;
                if (cursorPosition > newLength)
                {
                    cursorPosition = newLength;
                }
                else if (cursorPosition > 0 && cursorPosition <= newLength)
                {
                    // Если удалили символ перед курсором, сдвигаем курсор
                    if (oldLength > newLength)
                    {
                        cursorPosition = Math.Max(0, cursorPosition - 1);
                    }
                }

                Price.SelectionStart = cursorPosition;
            }
        }

        // Дополнительный метод для валидации при потере фокуса
        private void Price_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Price.Text))
            {
                Price.Text = "0";
                return;
            }

            // Парсим число и форматируем его с 2 знаками после запятой
            if (decimal.TryParse(Price.Text, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out decimal price))
            {
                // Ограничиваем максимальную цену
                if (price > 1000000)
                {
                    price = 1000000;
                    MessageBox.Show("Максимальная цена - 1 000 000", "Предупреждение",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // Форматируем с 2 знаками после запятой
                Price.Text = price.ToString("0.##");
            }
            else
            {
                Price.Text = "0";
            }
        }

        // Обработчик KeyPress для дополнительной фильтрации
        private void Price_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем только цифры, точку, запятую и backspace
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                e.KeyChar != '.' && e.KeyChar != ',')
            {
                e.Handled = true;
            }

            // Разрешаем только одну точку/запятую
            if ((e.KeyChar == '.' || e.KeyChar == ',') &&
                (Price.Text.Contains('.') || Price.Text.Contains(',')))
            {
                e.Handled = true;
            }

            // Не разрешаем точку в начале строки
            if ((e.KeyChar == '.' || e.KeyChar == ',') && string.IsNullOrEmpty(Price.Text))
            {
                Price.Text = "0";
                Price.SelectionStart = Price.Text.Length;
                e.Handled = true;
            }
        }

        // Фильтрация ввода для количества
        private void Count_TextChanged(object sender, EventArgs e)
        {
            // Убираем все нецифровые символы
            string text = Count.Text;
            string filteredText = new string(text.Where(char.IsDigit).ToArray());

            if (filteredText != text)
            {
                int cursorPosition = Count.SelectionStart;
                Count.Text = filteredText;
                Count.SelectionStart = Math.Min(cursorPosition, filteredText.Length);
            }
        }
    }
}