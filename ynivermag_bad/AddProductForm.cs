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

namespace ynivermag_bad
{
    public partial class AddProductForm : Form
    {
        private string _connection;
        public ProductModel NewProduct { get; private set; }
        private Image _selectedImage;
        private string _defaultImagePath;
        private string _productsImagesPath;

        // Константа для ограничения размера файла (3 МБ в байтах)
        private const long MAX_IMAGE_SIZE = 3 * 1024 * 1024; // 3 МБ

        public AddProductForm()
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            NewProduct = new ProductModel();

            // Инициализация путей для изображений
            InitializeImagePaths();

            // Загружаем категории
            LoadCategories();

            // Загружаем заглушку
            LoadDefaultImage();

            // Настройка PictureBox для приема перетаскивания
            pictureBoxProduct.AllowDrop = true;
        }

        private void InitializeImagePaths()
        {
            try
            {
                // Путь к папке проекта (не к bin\Debug)
                string projectRoot = GetProjectRootDirectory();

                // Путь к папке с изображениями продуктов
                _productsImagesPath = Path.Combine(projectRoot, "Images", "Products");

                // Путь к заглушке
                _defaultImagePath = Path.Combine(_productsImagesPath, "Default.jpg");

                // Создаем папку если ее нет
                if (!Directory.Exists(_productsImagesPath))
                {
                    Directory.CreateDirectory(_productsImagesPath);
                }

                // Если заглушки нет, создаем ее
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

        // Получение корневой директории проекта
        private string GetProjectRootDirectory()
        {
            string startupPath = Application.StartupPath;

            // Если запущено из bin\Debug или bin\Release
            if (startupPath.Contains(@"\bin\Debug") || startupPath.Contains(@"\bin\Release"))
            {
                return Directory.GetParent(Directory.GetParent(startupPath).FullName).FullName;
            }

            return startupPath;
        }

        // Загружает изображение-заглушку
        private void LoadDefaultImage()
        {
            try
            {
                if (File.Exists(_defaultImagePath))
                {
                    pictureBoxProduct.Image = Image.FromFile(_defaultImagePath);
                    pictureBoxProduct.SizeMode = PictureBoxSizeMode.Zoom;
                }
                else
                {
                    // Создаем простую заглушку
                    CreateDefaultImage();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заглушки: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Создает изображение-заглушку
        private void CreateDefaultImage()
        {
            try
            {
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

                pictureBoxProduct.Image = defaultImage;

                // Сохраняем заглушку в файл
                if (!Directory.Exists(_productsImagesPath))
                {
                    Directory.CreateDirectory(_productsImagesPath);
                }
                defaultImage.Save(_defaultImagePath, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания заглушки: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
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

                    // Устанавливаем значение по умолчанию
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

        // Загрузка изображения из файла
        private void LoadImageFromFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
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

                    // Загружаем изображение
                    _selectedImage = Image.FromFile(filePath);

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

                    // Показываем информацию о загруженном изображении
                    ShowImageInfo(fileInfo, _selectedImage);
                }
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

        private void ShowImageInfo(FileInfo fileInfo, Image image)
        {
            string info = $"Файл: {fileInfo.Name}\n" +
                         $"Размер: {FormatFileSize(fileInfo.Length)}\n" +
                         $"Разрешение: {image.Width}x{image.Height} пикселей\n" +
                         $"Формат: {image.RawFormat}";

            // Можно вывести информацию в статусную строку или всплывающую подсказку
            toolTip1.SetToolTip(pictureBoxProduct, info);
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "Б", "КБ", "МБ", "ГБ" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        // Масштабирование изображения
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

        // Сохраняет изображение продукта в файл
        private string SaveProductImage()
        {
            try
            {
                if (_selectedImage == null || IsDefaultImage())
                    return null;

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

                string fileName = $"product_{productName}_{DateTime.Now:yyyyMMddHHmmss}.jpg";
                string filePath = Path.Combine(_productsImagesPath, fileName);

                // Оптимизируем и сохраняем изображение
                SaveOptimizedImage(_selectedImage, filePath);

                // Возвращаем относительный путь (только имя файла)
                return fileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось сохранить изображение: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        private void SaveOptimizedImage(Image image, string filePath)
        {
            // Определяем параметры сжатия для JPEG
            var encoder = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()
                .FirstOrDefault(c => c.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);

            if (encoder != null)
            {
                var encoderParams = new System.Drawing.Imaging.EncoderParameters(1);

                // Устанавливаем качество сжатия (от 0 до 100, где 100 - лучшее качество)
                // 85 - хороший баланс между качеством и размером
                encoderParams.Param[0] = new System.Drawing.Imaging.EncoderParameter(
                    System.Drawing.Imaging.Encoder.Quality, 85L);

                image.Save(filePath, encoder, encoderParams);
            }
            else
            {
                // Если не нашли JPEG кодек, сохраняем стандартным способом
                image.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }

        // Проверяет, является ли изображение заглушкой
        private bool IsDefaultImage()
        {
            try
            {
                return _selectedImage == null;
            }
            catch
            {
                return true;
            }
        }

        private bool ValidateData()
        {
            // Проверка названия
            if (string.IsNullOrWhiteSpace(NameTB.Text))
            {
                MessageBox.Show("Введите название продукта", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                NameTB.Focus();
                return false;
            }

            // Проверка цены
            if (string.IsNullOrWhiteSpace(Price.Text))
            {
                MessageBox.Show("Введите цену продукта", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Price.Focus();
                return false;
            }

            // Проверка количества
            if (string.IsNullOrWhiteSpace(Count.Text))
            {
                MessageBox.Show("Введите количество продукта", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Count.Focus();
                return false;
            }

            if (!int.TryParse(Count.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("Количество должно быть неотрицательным целым числом", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Count.Focus();
                Count.SelectAll();
                return false;
            }

            return true;
        }

        private void SaveProductData()
        {
            decimal.TryParse(Price.Text, out decimal price);
            int.TryParse(Count.Text, out int stock);

            NewProduct.name = NameTB.Text.Trim();
            NewProduct.price = price;
            NewProduct.stock_quantity = stock;

            if (CategoryCb.SelectedValue != null)
            {
                NewProduct.category_id = (int)CategoryCb.SelectedValue;
            }

            // Сохраняем путь к фото
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

                    // Добавляем photo_path в запрос
                    string query = @"INSERT INTO product 
                            (name, price, stock_quantity, category_id, photo_path) 
                            VALUES (@Name, @Price, @StockQuantity, @CategoryId, @PhotoPath)";

                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Name", NewProduct.name);
                        cmd.Parameters.AddWithValue("@Price", NewProduct.price);
                        cmd.Parameters.AddWithValue("@StockQuantity", NewProduct.stock_quantity);
                        cmd.Parameters.AddWithValue("@CategoryId", NewProduct.category_id);

                        // Добавляем параметр для фото
                        if (!string.IsNullOrEmpty(NewProduct.photo_path))
                        {
                            cmd.Parameters.AddWithValue("@PhotoPath", NewProduct.photo_path);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@PhotoPath", DBNull.Value);
                        }

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("Не удалось добавить продукт", "Ошибка",
                                          MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                    }
                }
            }
            catch (MySqlException sqlEx)
            {
                // Обработка специфичных ошибок MySQL
                if (sqlEx.Number == 1452) // Ошибка внешнего ключа
                {
                    MessageBox.Show("Выбранная категория не существует", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (sqlEx.Number == 1062) // Ошибка дублирования
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

        private void AddProduct_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                SaveProductData();
                if (AddProductToDatabase())
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }

        private void Back_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        // Обработчики для работы с изображением

        private void pictureBoxProduct_Click(object sender, EventArgs e)
        {
            LoadImage();
        }

        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            LoadImage();
        }

        private void LoadImage()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Изображения (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif|Все файлы (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.Title = "Выберите изображение товара (макс. 3 МБ)";
                openFileDialog.RestoreDirectory = true;

                // Добавляем проверку размера в событие FileOk
                openFileDialog.FileOk += OpenFileDialog_FileOk;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    LoadImageFromFile(openFileDialog.FileName);
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

        // Перетаскивание файла на PictureBox
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

                        LoadImageFromFile(filePath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка проверки файла: {ex.Message}",
                                      "Ошибка",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Выберите файл изображения (jpg, jpeg, png, bmp, gif)", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        // Кнопка очистки изображения
        private void btnClearImage_Click(object sender, EventArgs e)
        {
            if (_selectedImage != null)
            {
                _selectedImage.Dispose();
                _selectedImage = null;
            }
            LoadDefaultImage();
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

        // Фильтрация ввода для цены (только цифры и запятая)
        private void Price_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем цифры, запятую и backspace
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != ',')
            {
                e.Handled = true;
            }

            // Разрешаем только одну запятую
            if (e.KeyChar == ',' && (sender as TextBox).Text.Contains(','))
            {
                e.Handled = true;
            }
        }

        // Фильтрация ввода для количества (только цифры)
        private void Count_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

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

       
    }
}