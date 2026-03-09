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
    /// <summary>
    /// Форма для добавления нового товара в систему.
    /// Обеспечивает ввод всех характеристик товара:
    /// - Название (с фильтрацией допустимых символов)
    /// - Цена (с автоматическим форматированием)
    /// - Количество (только цифры)
    /// - Категория (выбор из списка)
    /// - Изображение (загрузка, drag-and-drop, оптимизация)
    /// </summary>
    public partial class AddProductForm : Form
    {
        /// <summary>
        /// Строка подключения к базе данных
        /// </summary>
        private string _connection;

        /// <summary>
        /// Модель данных нового товара
        /// </summary>
        public ProductModel NewProduct { get; private set; }

        /// <summary>
        /// Выбранное изображение товара
        /// </summary>
        private Image _selectedImage;

        /// <summary>
        /// Путь к изображению-заглушке (когда нет фото)
        /// </summary>
        private string _defaultImagePath;

        /// <summary>
        /// Флаг для предотвращения рекурсивного обновления поля цены
        /// </summary>
        private bool _isUpdatingPrice = false;

        /// <summary>
        /// Путь к папке с изображениями товаров
        /// </summary>
        private string _productsImagesPath;

        /// <summary>
        /// Максимальный допустимый размер изображения (3 МБ)
        /// </summary>
        private const long MAX_IMAGE_SIZE = 3 * 1024 * 1024; // 3 МБ

        /// <summary>
        /// Конструктор формы добавления товара
        /// Инициализирует компоненты, загружает категории и настраивает обработчики событий
        /// </summary>
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
            // Фильтрация происходит в реальном времени при вводе текста
            NameTB.TextChanged += NameTB_TextChanged;
            Price.TextChanged += Price_TextChanged;
            Count.TextChanged += Count_TextChanged;
        }

        #region Инициализация

        /// <summary>
        /// Инициализирует пути к папкам с изображениями
        /// Создает необходимые директории, если они не существуют
        /// </summary>
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

        /// <summary>
        /// Определяет корневую директорию проекта
        /// Корректно работает как в режиме отладки, так и в релизе
        /// </summary>
        /// <returns>Путь к корневой папке проекта</returns>
        private string GetProjectRootDirectory()
        {
            string startupPath = Application.StartupPath;

            // Если приложение запущено из папки bin/Debug или bin/Release,
            // поднимаемся на два уровня выше к корню проекта
            if (startupPath.Contains(@"\bin\Debug") || startupPath.Contains(@"\bin\Release"))
            {
                return Directory.GetParent(Directory.GetParent(startupPath).FullName).FullName;
            }

            return startupPath;
        }

        /// <summary>
        /// Загружает список активных категорий из базы данных в комбобокс
        /// </summary>
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

        #region Фильтрация ввода

        /// <summary>
        /// Фильтрация ввода в поле названия товара
        /// Разрешены: буквы (любые), цифры, пробел, дефис, скобки
        /// </summary>
        private void NameTB_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = NameTB.SelectionStart;
            string filteredText = FilterToProductName(NameTB.Text);

            if (filteredText != NameTB.Text)
            {
                NameTB.Text = filteredText;
                // Корректируем позицию курсора после фильтрации
                NameTB.SelectionStart = Math.Min(selectionStart, NameTB.Text.Length);
            }
        }

        /// <summary>
        /// Фильтрует строку, оставляя только разрешенные символы для названия товара
        /// </summary>
        /// <param name="input">Входная строка</param>
        /// <returns>Отфильтрованная строка</returns>
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
        /// Фильтрация и форматирование ввода в поле цены
        /// Оставляет только цифры и одну точку, ограничивает количество знаков
        /// </summary>
        private void Price_TextChanged(object sender, EventArgs e)
        {
            // Блокируем повторный вход в обработчик (предотвращает рекурсию)
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

                // Заменяем запятую на точку (для единообразия)
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

                    // Ограничиваем количество цифр после точки до 2 (копейки)
                    if (afterDot.Length > 2)
                    {
                        afterDot = afterDot.Substring(0, 2);
                        filteredText = beforeDot + "." + afterDot;
                    }

                    // Ограничиваем количество цифр до точки до 6 (миллион)
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
                // Например, "0123" -> "123"
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

                    // Корректируем позицию курсора после изменения текста
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
        /// Фильтрация ввода в поле количества
        /// Разрешены только цифры, максимум 6 цифр
        /// </summary>
        private void Count_TextChanged(object sender, EventArgs e)
        {
            int selectionStart = Count.SelectionStart;
            string filteredText = new string(Count.Text.Where(char.IsDigit).ToArray());

            // Ограничиваем до 6 цифр (максимум 999999)
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

        #region Валидация перед сохранением

        /// <summary>
        /// Комплексная проверка всех полей перед сохранением
        /// Собирает все ошибки в список и показывает их одной группой
        /// </summary>
        /// <returns>true, если все поля заполнены корректно</returns>
        private bool ValidateData()
        {
            List<string> errors = new List<string>();

            // ===== ПРОВЕРКА НАЗВАНИЯ =====
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

            // ===== ПРОВЕРКА ЦЕНЫ =====
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

            // ===== ПРОВЕРКА КОЛИЧЕСТВА =====
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

            // ===== ПРОВЕРКА КАТЕГОРИИ =====
            if (CategoryCb.SelectedValue == null || CategoryCb.SelectedValue == DBNull.Value)
            {
                errors.Add("Выберите категорию");
                CategoryCb.BackColor = Color.LightPink;
            }

            // ===== ПРОВЕРКА ИЗОБРАЖЕНИЯ (необязательно) =====
            // Показываем предупреждение, но не блокируем сохранение
            if (_selectedImage == null || IsDefaultImage())
            {
                var result = MessageBox.Show("Вы не загрузили изображение товара. Продолжить без изображения?",
                    "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    return false;
                }
            }

            // Если есть ошибки, показываем их все
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

        /// <summary>
        /// Проверяет уникальность названия товара в базе данных
        /// </summary>
        /// <returns>true, если название уникально</returns>
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

        /// <summary>
        /// Загружает изображение по умолчанию (заглушку)
        /// Если файл заглушки не найден, создает его
        /// </summary>
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

        /// <summary>
        /// Создает изображение-заглушку с текстом "Нет изображения"
        /// </summary>
        /// <returns>Созданное изображение</returns>
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

        /// <summary>
        /// Устанавливает изображение в PictureBox с масштабированием
        /// </summary>
        /// <param name="image">Изображение для отображения</param>
        private void SetPictureBoxImage(Image image)
        {
            if (image == null) return;

            // Освобождаем предыдущее изображение, если оно было
            if (pictureBoxProduct.Image != null)
            {
                Image oldImage = pictureBoxProduct.Image;
                pictureBoxProduct.Image = null;
                if (oldImage != _selectedImage)
                {
                    oldImage.Dispose();
                }
            }

            // Масштабируем изображение под размер PictureBox
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

        /// <summary>
        /// Масштабирует изображение до указанных максимальных размеров
        /// Сохраняет пропорции
        /// </summary>
        /// <param name="image">Исходное изображение</param>
        /// <param name="maxWidth">Максимальная ширина</param>
        /// <param name="maxHeight">Максимальная высота</param>
        /// <returns>Масштабированное изображение</returns>
        private Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            if (image == null) return null;

            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            // Не увеличиваем изображение, если оно меньше указанных размеров
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

        /// <summary>
        /// Освобождает ресурсы изображений для предотвращения утечек памяти
        /// </summary>
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

            // Принудительный сбор мусора для освобождения ресурсов GDI+
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        /// Проверяет, используется ли изображение-заглушка
        /// </summary>
        /// <returns>true, если используется заглушка</returns>
        private bool IsDefaultImage()
        {
            try
            {
                if (_selectedImage == null)
                    return true;

                // Простая эвристика: заглушка обычно маленького размера
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

        /// <summary>
        /// Загружает изображение из файла с проверками размера и формата
        /// </summary>
        /// <param name="filePath">Путь к файлу изображения</param>
        private void LoadImageFromFile(string filePath)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(filePath);

                // Проверка размера файла
                if (fileInfo.Length > MAX_IMAGE_SIZE)
                {
                    MessageBox.Show($"Размер файла слишком большой ({fileInfo.Length / (1024 * 1024)} МБ).\n" +
                                   $"Максимальный разрешенный размер: {MAX_IMAGE_SIZE / (1024 * 1024)} МБ.",
                                   "Ошибка размера файла",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Warning);
                    return;
                }

                // Проверка расширения файла
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

                // Загрузка изображения с освобождением блокировки файла
                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    _selectedImage = Image.FromStream(stream);
                    _selectedImage = new Bitmap(_selectedImage);
                }

                // Проверка на слишком большое разрешение
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

                // Добавляем подсказку с информацией о файле
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

        /// <summary>
        /// Форматирует размер файла в человекочитаемый вид
        /// </summary>
        /// <param name="bytes">Размер в байтах</param>
        /// <returns>Отформатированная строка (Б, КБ, МБ)</returns>
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

        /// <summary>
        /// Сохраняет изображение товара на диск с оптимизацией
        /// </summary>
        /// <returns>Имя сохраненного файла или null, если изображение не сохранено</returns>
        private string SaveProductImage()
        {
            try
            {
                if (_selectedImage == null || IsDefaultImage())
                {
                    return null;
                }

                // Генерируем имя файла на основе названия товара и времени
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

        /// <summary>
        /// Сохраняет изображение с оптимизацией (сжатие JPEG)
        /// </summary>
        /// <param name="image">Изображение для сохранения</param>
        /// <param name="filePath">Путь для сохранения</param>
        private void SaveOptimizedImage(Image image, string filePath)
        {
            using (Bitmap bmp = new Bitmap(image))
            {
                string tempFile = Path.GetTempFileName();
                try
                {
                    // Используем JPEG кодек с качеством 85% для оптимального соотношения размер/качество
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

        /// <summary>
        /// Сохраняет данные из полей формы в объект NewProduct
        /// </summary>
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

        /// <summary>
        /// Добавляет новый товар в базу данных
        /// </summary>
        /// <returns>true, если добавление прошло успешно</returns>
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
                // Обработка специфических ошибок MySQL
                if (sqlEx.Number == 1452) // Ошибка внешнего ключа
                {
                    MessageBox.Show("Выбранная категория не существует", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (sqlEx.Number == 1062) // Ошибка дубликата (unique constraint)
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

        /// <summary>
        /// Обработчик нажатия кнопки "Добавить"
        /// Выполняет валидацию, сохранение и закрытие формы
        /// </summary>
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

        /// <summary>
        /// Обработчик кнопки "Назад"/"Отмена"
        /// Проверяет наличие несохраненных изменений
        /// </summary>
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

        /// <summary>
        /// Проверяет наличие несохраненных изменений в форме
        /// </summary>
        private bool HasUnsavedChanges()
        {
            return !string.IsNullOrWhiteSpace(NameTB.Text) ||
                   !string.IsNullOrWhiteSpace(Price.Text) ||
                   !string.IsNullOrWhiteSpace(Count.Text) ||
                   (_selectedImage != null && !IsDefaultImage());
        }

        /// <summary>
        /// Обработчик клика по PictureBox - открывает диалог выбора изображения
        /// </summary>
        private void pictureBoxProduct_Click(object sender, EventArgs e)
        {
            LoadImage();
        }

        /// <summary>
        /// Обработчик кнопки загрузки изображения
        /// </summary>
        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            LoadImage();
        }

        /// <summary>
        /// Обработчик кнопки очистки изображения
        /// Возвращает изображение-заглушку
        /// </summary>
        private void btnClearImage_Click(object sender, EventArgs e)
        {
            ReleaseImageResources();
            LoadDefaultImage();
        }

        /// <summary>
        /// Открывает диалог выбора файла и загружает изображение
        /// </summary>
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

        /// <summary>
        /// Обработчик перетаскивания файла в PictureBox
        /// </summary>
        private void pictureBoxProduct_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ?
                DragDropEffects.Copy : DragDropEffects.None;
        }

        /// <summary>
        /// Обработчик завершения перетаскивания файла
        /// </summary>
        private void pictureBoxProduct_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                LoadImageFromFile(files[0]);
            }
        }

        /// <summary>
        /// Обработчик наведения мыши на PictureBox - показывает подсказку
        /// </summary>
        private void pictureBoxProduct_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(pictureBoxProduct,
                "Кликните для выбора изображения\n" +
                "Или перетащите файл сюда\n" +
                $"Максимальный размер: {MAX_IMAGE_SIZE / (1024 * 1024)} МБ\n" +
                "Поддерживаемые форматы: JPG, JPEG, PNG, BMP, GIF");
        }

        /// <summary>
        /// Ограничение ввода в поле цены (только цифры, точка, запятая)
        /// </summary>
        private void Price_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                e.KeyChar != ',' && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // Запрещаем вторую точку или запятую
            if ((e.KeyChar == ',' || e.KeyChar == '.') &&
                (Price.Text.Contains(',') || Price.Text.Contains('.')))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Ограничение ввода в поле количества (только цифры)
        /// </summary>
        private void Count_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Обработчик потери фокуса полем цены
        /// Форматирует цену до двух знаков после запятой
        /// </summary>
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

        /// <summary>
        /// Обработчик валидации поля названия
        /// Делает первую букву заглавной
        /// </summary>
        private void NameTB_Validating(object sender, CancelEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NameTB.Text))
            {
                string name = NameTB.Text.Trim();
                if (name.Length > 0)
                {
                    name = char.ToUpper(name[0]) + name.Substring(1);
                    NameTB.Text = name;
                }
            }
        }

        /// <summary>
        /// Переопределенный метод закрытия формы
        /// Освобождает ресурсы изображений
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            ReleaseImageResources();
            base.OnFormClosing(e);
        }

        #endregion
    }
}