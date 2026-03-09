using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ynivermag_bad
{
    public partial class AddProductForm : Form
    {
        private string _connection;
        public ProductModel NewProduct { get; private set; }
        private Image _selectedImage;
        private string _defaultImagePath;
        private bool _isUpdatingPrice = false;
        private string _productsImagesPath;

        private const long MAX_IMAGE_SIZE = 3 * 1024 * 1024; // 3 МБ

        public AddProductForm()
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            NewProduct = new ProductModel();

            InitializeImagePaths();
            LoadCategories();
            LoadDefaultImage();

            pictureBoxProduct.AllowDrop = true;

            // Подписываемся на события для фильтрации ввода
            NameTB.TextChanged += NameTB_TextChanged;
            Price.TextChanged += Price_TextChanged;
            Count.TextChanged += Count_TextChanged;
        }

        #region Инициализация

        private void InitializeImagePaths()
        {
            try
            {
                string projectRoot = GetProjectRootDirectory();
                _productsImagesPath = Path.Combine(projectRoot, "Images", "Products");
                _defaultImagePath = Path.Combine(_productsImagesPath, "Default.jpg");

                if (!Directory.Exists(_productsImagesPath))
                {
                    Directory.CreateDirectory(_productsImagesPath);
                }

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

        private string GetProjectRootDirectory()
        {
            string startupPath = Application.StartupPath;

            if (startupPath.Contains(@"\bin\Debug") || startupPath.Contains(@"\bin\Release"))
            {
                return Directory.GetParent(Directory.GetParent(startupPath).FullName).FullName;
            }

            return startupPath;
        }

        private void LoadCategories()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();
                    string query = "SELECT category_id, name FROM category WHERE isActive = 1 ORDER BY name";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    CategoryCb.DataSource = dt;
                    CategoryCb.DisplayMember = "name";
                    CategoryCb.ValueMember = "category_id";

                    if (CategoryCb.Items.Count > 0)
                    {
                        CategoryCb.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Фильтрация ввода (как в примере)

        /// <summary>
        /// Фильтрация ввода в поле названия (только разрешенные символы)
        /// </summary>
        private void NameTB_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = NameTB.SelectionStart;
            string filteredText = FilterToProductName(NameTB.Text);

            if (filteredText != NameTB.Text)
            {
                NameTB.Text = filteredText;
                NameTB.SelectionStart = Math.Min(selectionStart, NameTB.Text.Length);
            }
        }

        /// <summary>
        /// Фильтр для названия товара: буквы (русские/английские), цифры, пробел, дефис, скобки
        /// </summary>
        private string FilterToProductName(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Разрешаем: буквы (любые), цифры, пробел, дефис, скобки
            char[] allowedChars = { ' ', '-', '(', ')' };

            return new string(input.Where(c =>
                char.IsLetter(c) ||      // Любые буквы
                char.IsDigit(c) ||       // Цифры
                allowedChars.Contains(c) // Разрешенные спецсимволы
            ).ToArray());
        }

        /// <summary>
        /// Фильтрация ввода в поле цены (только цифры, точка, запятая)
        /// </summary>
        private void Price_TextChanged(object sender, EventArgs e)
        {
            // Блокируем повторный вход в обработчик
            if (_isUpdatingPrice) return;

            _isUpdatingPrice = true;

            try
            {
                int cursorPosition = Price.SelectionStart;
                int oldLength = Price.Text.Length;
                string text = Price.Text;

                // Если текст пустой - выходим
                if (string.IsNullOrEmpty(text))
                {
                    return;
                }

                // Фильтруем: оставляем только цифры, точку и запятую
                string filteredText = new string(text.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());

                // Заменяем запятую на точку
                filteredText = filteredText.Replace(',', '.');

                // Проверяем, что точка только одна
                int dotCount = filteredText.Count(c => c == '.');
                if (dotCount > 1)
                {
                    // Оставляем только первую точку
                    int firstDotIndex = filteredText.IndexOf('.');
                    filteredText = filteredText.Substring(0, firstDotIndex + 1) +
                                  filteredText.Substring(firstDotIndex + 1).Replace(".", "");
                }

                // Если есть точка, проверяем количество знаков после запятой
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

                    // Ограничиваем количество цифр до точки до 6
                    if (beforeDot.Length > 6)
                    {
                        beforeDot = beforeDot.Substring(0, 6);
                        filteredText = beforeDot + "." + afterDot;
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

                // Если строка состоит только из точки - делаем "0."
                if (filteredText == ".")
                {
                    filteredText = "0.";
                }

                // Если текст изменился, обновляем поле
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
                        if (oldLength > newLength)
                        {
                            cursorPosition = Math.Max(0, cursorPosition - 1);
                        }
                    }

                    Price.SelectionStart = cursorPosition;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в Price_TextChanged: {ex.Message}");
            }
            finally
            {
                _isUpdatingPrice = false;
            }
        }

        /// <summary>
        /// Фильтрация ввода в поле количества (только цифры)
        /// </summary>
        private void Count_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = Count.SelectionStart;
            string filteredText = new string(Count.Text.Where(char.IsDigit).ToArray());

            // Ограничиваем до 6 цифр
            if (filteredText.Length > 6)
            {
                filteredText = filteredText.Substring(0, 6);
            }

            if (filteredText != Count.Text)
            {
                Count.Text = filteredText;
                Count.SelectionStart = Math.Min(selectionStart, Count.Text.Length);
            }
        }

        #endregion

        #region Валидация перед сохранением (минимальная)

        private bool ValidateData()
        {
            List<string> errors = new List<string>();

            // Проверка названия
            if (string.IsNullOrWhiteSpace(NameTB.Text))
            {
                errors.Add("Введите название продукта");
                NameTB.BackColor = Color.LightPink;
            }
            else if (NameTB.Text.Length < 2)
            {
                errors.Add("Название должно содержать минимум 2 символа");
                NameTB.BackColor = Color.LightPink;
            }
            else if (NameTB.Text.Length > 100)
            {
                errors.Add("Название должно содержать не более 100 символов");
                NameTB.BackColor = Color.LightPink;
            }
            else if (!IsProductNameUnique())
            {
                errors.Add("Продукт с таким названием уже существует");
                NameTB.BackColor = Color.LightPink;
            }

            // Проверка цены
            if (string.IsNullOrWhiteSpace(Price.Text))
            {
                errors.Add("Введите цену продукта");
                Price.BackColor = Color.LightPink;
            }
            else
            {
                decimal price;
                bool parsed = decimal.TryParse(Price.Text.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out price);

                if (!parsed)
                {
                    errors.Add("Введите корректную цену");
                    Price.BackColor = Color.LightPink;
                }
                else if (price < 0)
                {
                    errors.Add("Цена не может быть отрицательной");
                    Price.BackColor = Color.LightPink;
                }
                else if (price > 1000000)
                {
                    errors.Add("Цена не может превышать 1 000 000");
                    Price.BackColor = Color.LightPink;
                }
            }

            // Проверка количества
            if (string.IsNullOrWhiteSpace(Count.Text))
            {
                errors.Add("Введите количество продукта");
                Count.BackColor = Color.LightPink;
            }
            else
            {
                if (!int.TryParse(Count.Text, out int stock) || stock < 0)
                {
                    errors.Add("Количество должно быть целым положительным числом");
                    Count.BackColor = Color.LightPink;
                }
                else if (stock > 999999)
                {
                    errors.Add("Количество не может превышать 999 999");
                    Count.BackColor = Color.LightPink;
                }
            }

            // Проверка категории
            if (CategoryCb.SelectedValue == null || CategoryCb.SelectedValue == DBNull.Value)
            {
                errors.Add("Выберите категорию");
                CategoryCb.BackColor = Color.LightPink;
            }

            // Проверка изображения (необязательно)
            if (_selectedImage == null || IsDefaultImage())
            {
                var result = MessageBox.Show("Вы не загрузили изображение товара. Продолжить без изображения?",
                    "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    return false;
                }
            }

            if (errors.Count > 0)
            {
                string errorMessage = "Пожалуйста, исправьте следующие ошибки:\n\n• " +
                                     string.Join("\n• ", errors);
                MessageBox.Show(errorMessage, "Ошибки валидации",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    string query = "SELECT COUNT(*) FROM product WHERE name = @Name AND isActive = 1";

                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Name", NameTB.Text.Trim());
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count == 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка проверки названия: {ex.Message}");
                    return false;
                }
            }
        }

        #endregion

        #region Работа с изображением

        private void LoadDefaultImage()
        {
            try
            {
                ReleaseImageResources();

                if (File.Exists(_defaultImagePath))
                {
                    using (FileStream stream = new FileStream(_defaultImagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        _selectedImage = Image.FromStream(stream);
                    }
                }
                else
                {
                    _selectedImage = CreateDefaultImage();
                }

                SetPictureBoxImage(_selectedImage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заглушки: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private Image CreateDefaultImage()
        {
            int width = Math.Max(pictureBoxProduct.Width, 200);
            int height = Math.Max(pictureBoxProduct.Height, 200);

            Bitmap defaultImage = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(defaultImage))
            {
                g.Clear(Color.FromArgb(240, 240, 240));
                using (Pen pen = new Pen(Color.FromArgb(200, 200, 200)))
                {
                    g.DrawRectangle(pen, 1, 1, width - 3, height - 3);
                }
                using (Font font = new Font("Arial", 14, FontStyle.Regular))
                using (Brush brush = new SolidBrush(Color.FromArgb(150, 150, 150)))
                {
                    string text = "Нет изображения";
                    SizeF textSize = g.MeasureString(text, font);
                    float x = (width - textSize.Width) / 2;
                    float y = (height - textSize.Height) / 2;
                    g.DrawString(text, font, brush, x, y);
                }
            }
            return defaultImage;
        }

        private void SetPictureBoxImage(Image image)
        {
            if (image == null) return;

            if (pictureBoxProduct.Image != null)
            {
                Image oldImage = pictureBoxProduct.Image;
                pictureBoxProduct.Image = null;
                if (oldImage != _selectedImage)
                {
                    oldImage.Dispose();
                }
            }

            if (pictureBoxProduct.Width > 0 && pictureBoxProduct.Height > 0)
            {
                pictureBoxProduct.Image = ScaleImage(image, pictureBoxProduct.Width, pictureBoxProduct.Height);
                pictureBoxProduct.SizeMode = PictureBoxSizeMode.Zoom;
            }
            else
            {
                pictureBoxProduct.Image = new Bitmap(image);
            }
        }

        private Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            if (image == null) return null;

            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            if (ratio > 1) ratio = 1;

            var newWidth = Math.Max(1, (int)(image.Width * ratio));
            var newHeight = Math.Max(1, (int)(image.Height * ratio));

            var newImage = new Bitmap(newWidth, newHeight);
            using (var graphics = Graphics.FromImage(newImage))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }

        private void ReleaseImageResources()
        {
            if (pictureBoxProduct.Image != null)
            {
                Image oldImage = pictureBoxProduct.Image;
                pictureBoxProduct.Image = null;
                oldImage.Dispose();
            }

            if (_selectedImage != null && _selectedImage != pictureBoxProduct.Image)
            {
                _selectedImage.Dispose();
                _selectedImage = null;
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private bool IsDefaultImage()
        {
            try
            {
                if (_selectedImage == null)
                    return true;

                if (_selectedImage.Width <= 200 && _selectedImage.Height <= 200)
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return true;
            }
        }

        private void LoadImageFromFile(string filePath)
        {
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

                ReleaseImageResources();

                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    _selectedImage = Image.FromStream(stream);
                    _selectedImage = new Bitmap(_selectedImage);
                }

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
                        LoadDefaultImage();
                        return;
                    }
                }

                SetPictureBoxImage(_selectedImage);

                toolTip1.SetToolTip(pictureBoxProduct,
                    $"Файл: {fileInfo.Name}\n" +
                    $"Размер: {FormatFileSize(fileInfo.Length)}\n" +
                    $"Разрешение: {_selectedImage.Width}x{_selectedImage.Height}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить изображение: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "Б", "КБ", "МБ" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        private string SaveProductImage()
        {
            try
            {
                if (_selectedImage == null || IsDefaultImage())
                {
                    return null;
                }

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

                if (string.IsNullOrWhiteSpace(productName))
                {
                    productName = "product";
                }

                if (productName.Length > 50)
                {
                    productName = productName.Substring(0, 50);
                }

                string fileName = $"product_{productName}_{DateTime.Now:yyyyMMddHHmmss}.jpg";
                string filePath = Path.Combine(_productsImagesPath, fileName);

                if (!Directory.Exists(_productsImagesPath))
                {
                    Directory.CreateDirectory(_productsImagesPath);
                }

                SaveOptimizedImage(_selectedImage, filePath);

                return fileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось сохранить изображение: {ex.Message}\n\nПроверьте права на запись в папку:\n{_productsImagesPath}",
                               "Ошибка",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Warning);
                return null;
            }
        }

        private void SaveOptimizedImage(Image image, string filePath)
        {
            using (Bitmap bmp = new Bitmap(image))
            {
                string tempFile = Path.GetTempFileName();
                try
                {
                    var jpegCodec = ImageCodecInfo.GetImageEncoders()
                        .FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);

                    if (jpegCodec != null)
                    {
                        var encoderParams = new EncoderParameters(1);
                        encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 85L);
                        bmp.Save(tempFile, jpegCodec, encoderParams);
                    }
                    else
                    {
                        bmp.Save(tempFile, ImageFormat.Jpeg);
                    }

                    File.Copy(tempFile, filePath, true);
                }
                finally
                {
                    if (File.Exists(tempFile))
                    {
                        try { File.Delete(tempFile); } catch { }
                    }
                }
            }
        }

        #endregion

        #region Сохранение данных

        private void SaveProductData()
        {
            decimal.TryParse(Price.Text.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out decimal price);
            int.TryParse(Count.Text, out int stock);

            NewProduct.name = NameTB.Text.Trim();
            NewProduct.price = price;
            NewProduct.stock_quantity = stock;

            if (CategoryCb.SelectedValue != null && CategoryCb.SelectedValue != DBNull.Value)
            {
                NewProduct.category_id = (int)CategoryCb.SelectedValue;
            }

            string photoPath = SaveProductImage();
            if (!string.IsNullOrEmpty(photoPath))
            {
                NewProduct.photo_path = photoPath;
            }
        }

        private bool AddProductToDatabase()
        {
            try
            {
                using (var connection = new MySqlConnection(_connection))
                {
                    connection.Open();

                    string query = @"INSERT INTO product 
                            (name, price, stock_quantity, category_id, photo_path, isActive) 
                            VALUES (@Name, @Price, @StockQuantity, @CategoryId, @PhotoPath, 1)";

                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Name", NewProduct.name);
                        cmd.Parameters.AddWithValue("@Price", NewProduct.price);
                        cmd.Parameters.AddWithValue("@StockQuantity", NewProduct.stock_quantity);
                        cmd.Parameters.AddWithValue("@CategoryId", NewProduct.category_id);

                        if (!string.IsNullOrEmpty(NewProduct.photo_path))
                        {
                            cmd.Parameters.AddWithValue("@PhotoPath", NewProduct.photo_path);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@PhotoPath", DBNull.Value);
                        }

                        int result = cmd.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (MySqlException sqlEx)
            {
                if (sqlEx.Number == 1452)
                {
                    MessageBox.Show("Выбранная категория не существует", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (sqlEx.Number == 1062)
                {
                    MessageBox.Show("Продукт с таким названием уже существует", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"Ошибка базы данных: {sqlEx.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении продукта: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        #endregion

        #region Обработчики событий

        private void AddProduct_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                SaveProductData();
                if (AddProductToDatabase())
                {
                    MessageBox.Show("✅ Продукт успешно добавлен!", "Успех",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }

        private void Back_Click(object sender, EventArgs e)
        {
            if (HasUnsavedChanges())
            {
                var result = MessageBox.Show("У вас есть несохраненные изменения. Выйти?",
                                            "Подтверждение",
                                            MessageBoxButtons.YesNo,
                                            MessageBoxIcon.Question);
                if (result == DialogResult.No)
                    return;
            }

            DialogResult = DialogResult.Cancel;
            Close();
        }

        private bool HasUnsavedChanges()
        {
            return !string.IsNullOrWhiteSpace(NameTB.Text) ||
                   !string.IsNullOrWhiteSpace(Price.Text) ||
                   !string.IsNullOrWhiteSpace(Count.Text) ||
                   (_selectedImage != null && !IsDefaultImage());
        }

        private void pictureBoxProduct_Click(object sender, EventArgs e)
        {
            LoadImage();
        }

        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            LoadImage();
        }

        private void btnClearImage_Click(object sender, EventArgs e)
        {
            ReleaseImageResources();
            LoadDefaultImage();
        }

        private void LoadImage()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Изображения (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                openFileDialog.FilterIndex = 1;
                openFileDialog.Title = "Выберите изображение товара (макс. 3 МБ)";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    LoadImageFromFile(openFileDialog.FileName);
                }
            }
        }

        private void pictureBoxProduct_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ?
                DragDropEffects.Copy : DragDropEffects.None;
        }

        private void pictureBoxProduct_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                LoadImageFromFile(files[0]);
            }
        }

        private void pictureBoxProduct_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(pictureBoxProduct,
                "Кликните для выбора изображения\n" +
                "Или перетащите файл сюда\n" +
                $"Максимальный размер: {MAX_IMAGE_SIZE / (1024 * 1024)} МБ\n" +
                "Поддерживаемые форматы: JPG, JPEG, PNG, BMP, GIF");
        }

        private void Price_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                e.KeyChar != ',' && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            if ((e.KeyChar == ',' || e.KeyChar == '.') &&
                (Price.Text.Contains(',') || Price.Text.Contains('.')))
            {
                e.Handled = true;
            }
        }

        private void Count_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void Price_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Price.Text))
            {
                Price.Text = "0";
                return;
            }

            if (decimal.TryParse(Price.Text.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out decimal price))
            {
                if (price > 1000000)
                {
                    price = 1000000;
                    MessageBox.Show("Максимальная цена - 1 000 000", "Предупреждение",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                Price.Text = price.ToString("F2");
            }
        }

        private void NameTB_Validating(object sender, CancelEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NameTB.Text))
            {
                // Делаем первую букву заглавной
                string name = NameTB.Text.Trim();
                if (name.Length > 0)
                {
                    name = char.ToUpper(name[0]) + name.Substring(1);
                    NameTB.Text = name;
                }
            }
        }

        #endregion
    }
}