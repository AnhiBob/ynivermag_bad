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
    /// Форма для редактирования существующего товара.
    /// Позволяет изменять все характеристики товара:
    /// - Название (с фильтрацией символов, проверка уникальности)
    /// - Цена (с автоформатированием, ограничениями)
    /// - Количество (только цифры)
    /// - Категория (выбор из списка)
    /// - Изображение (загрузка, drag-and-drop, оптимизация)
    /// </summary>
    public partial class EditProductForm : Form
    {
        /// <summary>
        /// Строка подключения к базе данных
        /// </summary>
        private string _connection;

        /// <summary>
        /// Модель данных редактируемого товара
        /// </summary>
        public ProductModel Product { get; private set; }

        /// <summary>
        /// Выбранное изображение товара
        /// </summary>
        private Image _selectedImage;

        /// <summary>
        /// Путь к папке с изображениями товаров
        /// </summary>
        private string _productsImagesPath;

        /// <summary>
        /// Путь к изображению-заглушке (когда нет фото)
        /// </summary>
        private string _defaultImagePath;

        /// <summary>
        /// Флаг, указывающий, было ли изменено изображение
        /// </summary>
        private bool _imageChanged = false;

        /// <summary>
        /// Флаг для предотвращения рекурсивного обновления поля цены
        /// </summary>
        private bool _isUpdatingPrice = false;

        /// <summary>
        /// Сервис для работы с изображениями товаров
        /// </summary>
        private ProductImageService _productImageService;

        // Константы для валидации
        private const long MAX_IMAGE_SIZE = 3 * 1024 * 1024; // 3 МБ - максимальный размер изображения
        private const int MAX_NAME_LENGTH = 100;             // Максимальная длина названия
        private const int MIN_NAME_LENGTH = 2;               // Минимальная длина названия
        private const int MAX_PRICE = 1000000;                // Максимальная цена (1 млн)
        private const int MAX_QUANTITY = 999999;              // Максимальное количество на складе

        /// <summary>
        /// Конструктор формы редактирования товара
        /// </summary>
        /// <param name="product">Модель товара с данными для редактирования</param>
        public EditProductForm(ProductModel product)
        {
            InitializeComponent();
            _connection = Connection.ConnectionString;
            Product = product;
            _productImageService = new ProductImageService();

            InitializeImagePaths();
            LoadProductData();
            LoadCategories();
            LoadProductImage();

            pictureBoxProduct.AllowDrop = true;

            // Подписываемся на события для фильтрации ввода
            SubscribeToEvents();
        }

        #region Инициализация

        /// <summary>
        /// Подписывается на события для фильтрации ввода и валидации
        /// </summary>
        private void SubscribeToEvents()
        {
            NameTB.TextChanged += NameTB_TextChanged;
            Price.TextChanged += Price_TextChanged;
            Count.TextChanged += Count_TextChanged;
            NameTB.Validating += NameTB_Validating;
        }

        /// <summary>
        /// Инициализирует пути к папкам с изображениями
        /// Создает необходимые директории, если они не существуют
        /// </summary>
        private void InitializeImagePaths()
        {
            try
            {
                string startupPath = Application.StartupPath;

                // Корректировка пути для режима отладки (bin/Debug или bin/Release)
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

        #endregion

        #region Загрузка данных

        /// <summary>
        /// Загружает данные товара в поля формы
        /// </summary>
        private void LoadProductData()
        {
            _isUpdatingPrice = true;

            NameTB.Text = Product.name;
            Price.Text = Product.price.ToString("F2"); // Формат с двумя знаками после запятой
            Count.Text = Product.stock_quantity.ToString();

            _isUpdatingPrice = false;
        }

        /// <summary>
        /// Загружает список категорий из базы данных в комбобокс
        /// Добавляет пустую строку для возможности не выбирать категорию
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

                    // Добавляем пустую строку для возможности не выбирать категорию
                    DataRow emptyRow = dt.NewRow();
                    emptyRow["category_id"] = DBNull.Value;
                    emptyRow["name"] = "— Без категории —";
                    dt.Rows.InsertAt(emptyRow, 0);

                    CategoryCb.DataSource = dt;
                    CategoryCb.DisplayMember = "name";
                    CategoryCb.ValueMember = "category_id";

                    // Устанавливаем текущую категорию с проверкой на null
                    if (Product.category_id.HasValue)
                    {
                        bool found = false;
                        for (int i = 0; i < CategoryCb.Items.Count; i++)
                        {
                            DataRowView row = (DataRowView)CategoryCb.Items[i];
                            if (row["category_id"] != DBNull.Value &&
                                Convert.ToInt32(row["category_id"]) == Product.category_id.Value)
                            {
                                CategoryCb.SelectedIndex = i;
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            CategoryCb.SelectedIndex = 0; // Выбираем "Без категории"
                        }
                    }
                    else
                    {
                        CategoryCb.SelectedIndex = 0; // Выбираем "Без категории"
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
            if (_isUpdatingPrice) return;

            _isUpdatingPrice = true;

            try
            {
                int cursorPosition = Price.SelectionStart;
                int oldLength = Price.Text.Length;
                string text = Price.Text;

                if (string.IsNullOrEmpty(text))
                {
                    _isUpdatingPrice = false;
                    return;
                }

                // Фильтруем: оставляем только цифры и точку
                string filteredText = new string(text.Where(c => char.IsDigit(c) || c == '.').ToArray());

                // Заменяем возможные запятые на точку (для единообразия)
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

                    // Ограничиваем количество цифр до точки до 6 (до миллиона)
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

        #region Работа с изображением

        /// <summary>
        /// Загружает изображение товара
        /// Если файл не найден, загружает изображение-заглушку
        /// </summary>
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
                            SetPictureBoxImage(_selectedImage);
                            return;
                        }
                    }
                }

                LoadDefaultImage();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}");
                LoadDefaultImage();
            }
        }

        /// <summary>
        /// Загружает изображение-заглушку
        /// Если файл заглушки не найден, создает его
        /// </summary>
        private void LoadDefaultImage()
        {
            try
            {
                ReleaseImageResources();

                if (File.Exists(_defaultImagePath))
                {
                    _selectedImage = LoadImageWithoutLock(_defaultImagePath);
                }

                if (_selectedImage == null)
                {
                    _selectedImage = CreatePlaceholderImage(300, 300);

                    try
                    {
                        if (!Directory.Exists(_productsImagesPath))
                        {
                            Directory.CreateDirectory(_productsImagesPath);
                        }
                        _selectedImage.Save(_defaultImagePath, ImageFormat.Jpeg);
                    }
                    catch { }
                }

                SetPictureBoxImage(_selectedImage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заглушки: {ex.Message}");
            }
        }

        /// <summary>
        /// Загружает изображение из файла без блокировки файла
        /// </summary>
        /// <param name="filePath">Путь к файлу</param>
        /// <returns>Загруженное изображение или null при ошибке</returns>
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
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Создает изображение-заглушку с текстом "Нет фото"
        /// </summary>
        /// <param name="width">Ширина изображения</param>
        /// <param name="height">Высота изображения</param>
        /// <returns>Созданное изображение</returns>
        private Image CreatePlaceholderImage(int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(240, 240, 240));
                using (Pen pen = new Pen(Color.FromArgb(200, 200, 200)))
                {
                    g.DrawRectangle(pen, 1, 1, width - 3, height - 3);
                }
                using (Font font = new Font("Arial", 12, FontStyle.Regular))
                using (Brush brush = new SolidBrush(Color.FromArgb(150, 150, 150)))
                {
                    string text = "Нет фото";
                    SizeF textSize = g.MeasureString(text, font);
                    float x = (width - textSize.Width) / 2;
                    float y = (height - textSize.Height) / 2;
                    g.DrawString(text, font, brush, x, y);
                }
            }
            return bmp;
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

                if (!string.IsNullOrEmpty(Product.photo_path))
                {
                    return Product.photo_path == "Default.jpg";
                }

                return false;
            }
            catch
            {
                return true;
            }
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
                    DeleteOldProductImage();
                    return null;
                }

                string fileName = GenerateImageFileName();
                string filePath = Path.Combine(_productsImagesPath, fileName);

                DeleteOldProductImage();

                bool saved = SaveOptimizedImage(_selectedImage, filePath);

                return saved ? fileName : Product.photo_path;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось сохранить изображение: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return Product.photo_path;
            }
        }

        /// <summary>
        /// Генерирует имя файла для изображения на основе названия товара и времени
        /// </summary>
        /// <returns>Сгенерированное имя файла</returns>
        private string GenerateImageFileName()
        {
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

            if (productName.Length > 50)
            {
                productName = productName.Substring(0, 50);
            }

            return $"product_{productName}_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
        }

        /// <summary>
        /// Удаляет старое изображение товара с диска
        /// </summary>
        private void DeleteOldProductImage()
        {
            try
            {
                if (!string.IsNullOrEmpty(Product.photo_path) &&
                    Product.photo_path != "Default.jpg")
                {
                    string oldFilePath = Path.Combine(_productsImagesPath, Product.photo_path);
                    if (File.Exists(oldFilePath))
                    {
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении старого изображения: {ex.Message}");
            }
        }

        /// <summary>
        /// Сохраняет изображение с оптимизацией (сжатие JPEG)
        /// </summary>
        /// <param name="image">Изображение для сохранения</param>
        /// <param name="filePath">Путь для сохранения</param>
        /// <returns>true, если сохранение успешно</returns>
        private bool SaveOptimizedImage(Image image, string filePath)
        {
            try
            {
                int maxDimension = 1200;
                Image imageToSave = image;

                // Масштабируем слишком большие изображения
                if (image.Width > maxDimension || image.Height > maxDimension)
                {
                    imageToSave = _productImageService.ScaleImageHighQuality(image, maxDimension, maxDimension);
                }

                // Используем JPEG кодек с качеством 95% для оптимального соотношения размер/качество
                var encoder = ImageCodecInfo.GetImageEncoders()
                    .FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);

                if (encoder != null)
                {
                    var encoderParams = new EncoderParameters(1);
                    encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 95L);
                    imageToSave.Save(filePath, encoder, encoderParams);
                }
                else
                {
                    imageToSave.Save(filePath, ImageFormat.Jpeg);
                }

                if (imageToSave != image)
                {
                    imageToSave.Dispose();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Валидация перед сохранением

        /// <summary>
        /// Проверяет корректность цены
        /// </summary>
        /// <param name="priceText">Текст с ценой</param>
        /// <param name="price">Распарсенная цена (out параметр)</param>
        /// <returns>true, если цена корректна</returns>
        private bool ValidatePrice(string priceText, out decimal price)
        {
            price = 0;

            if (string.IsNullOrWhiteSpace(priceText)) return false;

            bool parsed = decimal.TryParse(priceText.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out price);

            return parsed && price >= 0 && price <= MAX_PRICE;
        }

        /// <summary>
        /// Проверяет корректность количества
        /// </summary>
        /// <param name="quantityText">Текст с количеством</param>
        /// <param name="quantity">Распарсенное количество (out параметр)</param>
        /// <returns>true, если количество корректно</returns>
        private bool ValidateQuantity(string quantityText, out int quantity)
        {
            quantity = 0;

            if (string.IsNullOrWhiteSpace(quantityText)) return false;

            return int.TryParse(quantityText, out quantity) &&
                   quantity >= 0 && quantity <= MAX_QUANTITY;
        }

        /// <summary>
        /// Проверяет уникальность названия товара (исключая текущий товар)
        /// </summary>
        /// <returns>true, если название уникально</returns>
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
            else if (NameTB.Text.Length < MIN_NAME_LENGTH)
            {
                errors.Add($"Название должно содержать минимум {MIN_NAME_LENGTH} символа");
                NameTB.BackColor = Color.LightPink;
            }
            else if (NameTB.Text.Length > MAX_NAME_LENGTH)
            {
                errors.Add($"Название должно содержать не более {MAX_NAME_LENGTH} символов");
                NameTB.BackColor = Color.LightPink;
            }

            // ===== ПРОВЕРКА ЦЕНЫ =====
            decimal price;
            if (string.IsNullOrWhiteSpace(Price.Text))
            {
                errors.Add("Введите цену продукта");
                Price.BackColor = Color.LightPink;
            }
            else if (!ValidatePrice(Price.Text, out price))
            {
                errors.Add($"Цена должна быть числом от 0 до {MAX_PRICE:N0}");
                Price.BackColor = Color.LightPink;
            }

            // ===== ПРОВЕРКА КОЛИЧЕСТВА =====
            int quantity;
            if (string.IsNullOrWhiteSpace(Count.Text))
            {
                errors.Add("Введите количество продукта");
                Count.BackColor = Color.LightPink;
            }
            else if (!ValidateQuantity(Count.Text, out quantity))
            {
                errors.Add($"Количество должно быть целым числом от 0 до {MAX_QUANTITY}");
                Count.BackColor = Color.LightPink;
            }

            // ===== ПРОВЕРКА КАТЕГОРИИ =====
            if (CategoryCb.SelectedValue == null || CategoryCb.SelectedValue == DBNull.Value)
            {
                errors.Add("Выберите категорию");
                CategoryCb.BackColor = Color.LightPink;
            }

            // ===== ПРОВЕРКА НА УНИКАЛЬНОСТЬ НАЗВАНИЯ =====
            if (!string.IsNullOrWhiteSpace(NameTB.Text) && !IsProductNameUnique())
            {
                errors.Add("Продукт с таким названием уже существует");
                NameTB.BackColor = Color.LightPink;
            }

            // ===== ПРОВЕРКА ИЗОБРАЖЕНИЯ (предупреждение, не ошибка) =====
            if (_selectedImage == null || IsDefaultImage())
            {
                var result = MessageBox.Show("У товара нет изображения. Продолжить сохранение?",
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

        #endregion

        #region Сохранение данных

        /// <summary>
        /// Сохраняет данные из полей формы в объект Product
        /// </summary>
        private void SaveProductData()
        {
            ValidatePrice(Price.Text, out decimal price);
            ValidateQuantity(Count.Text, out int stock);

            Product.name = NameTB.Text.Trim();
            Product.price = price;
            Product.stock_quantity = stock;

            if (CategoryCb.SelectedValue != null && CategoryCb.SelectedValue != DBNull.Value)
            {
                Product.category_id = Convert.ToInt32(CategoryCb.SelectedValue);
            }
            else
            {
                Product.category_id = null;
            }
        }

        #endregion

        #region Обработчики событий

        /// <summary>
        /// Обработчик нажатия кнопки "Сохранить"
        /// Выполняет валидацию, сохранение и закрытие формы
        /// </summary>
        private void EditProduct_Click(object sender, EventArgs e)
        {
            if (!ValidateData()) return;

            SaveProductData();

            // Сохраняем изображение, если оно было изменено
            if (_imageChanged)
            {
                string imageFileName = SaveProductImage();
                if (!string.IsNullOrEmpty(imageFileName))
                {
                    Product.photo_path = imageFileName;
                    _imageChanged = false;
                }
                else
                {
                    Product.photo_path = null;
                }
            }

            EditClass editClass = new EditClass();
            bool updated = editClass.UpdateProductInDatabase(Product);

            if (updated)
            {
                MessageBox.Show("✅ Товар успешно обновлен!", "Успех",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Не удалось сохранить изменения в базе данных", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        /// Сравнивает текущие значения полей с исходными данными товара
        /// </summary>
        /// <returns>true, если есть изменения</returns>
        private bool HasUnsavedChanges()
        {
            decimal currentPrice;
            ValidatePrice(Price.Text, out currentPrice);

            int currentQuantity;
            ValidateQuantity(Count.Text, out currentQuantity);

            return NameTB.Text != Product.name ||
                   currentPrice != Product.price ||
                   currentQuantity != Product.stock_quantity ||
                   (CategoryCb.SelectedValue != null &&
                    CategoryCb.SelectedValue != DBNull.Value &&
                    Convert.ToInt32(CategoryCb.SelectedValue) != Product.category_id) ||
                   _imageChanged;
        }

        /// <summary>
        /// Обработчик кнопки загрузки изображения
        /// </summary>
        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            LoadImageFromFile();
        }

        /// <summary>
        /// Обработчик кнопки удаления изображения
        /// Возвращает изображение-заглушку
        /// </summary>
        private void btnRemoveImage_Click(object sender, EventArgs e)
        {
            LoadDefaultImage();
            _imageChanged = true;
        }

        /// <summary>
        /// Обработчик клика по PictureBox - открывает диалог выбора изображения
        /// </summary>
        private void pictureBoxProduct_Click(object sender, EventArgs e)
        {
            LoadImageFromFile();
        }

        /// <summary>
        /// Загружает изображение из файла с проверками размера и формата
        /// </summary>
        private void LoadImageFromFile()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Изображения (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                openFileDialog.FilterIndex = 1;
                openFileDialog.Title = $"Выберите изображение товара (макс. {MAX_IMAGE_SIZE / (1024 * 1024)} МБ)";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;

                    // Проверка размера файла
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

                    try
                    {
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
                                                       "Продолжить?",
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

                        SetPictureBoxImage(_selectedImage);
                        _imageChanged = true;

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

        #endregion

        #region Drag & Drop

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
                string filePath = files[0];

                // Проверка расширения
                string extension = Path.GetExtension(filePath).ToLower();
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

                if (!allowedExtensions.Contains(extension))
                {
                    MessageBox.Show("Выберите файл изображения (jpg, jpeg, png, bmp, gif)",
                                  "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Проверка размера
                FileInfo fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > MAX_IMAGE_SIZE)
                {
                    MessageBox.Show($"Размер файла слишком большой ({fileInfo.Length / (1024 * 1024)} МБ).",
                                  "Ошибка размера файла",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    ReleaseImageResources();

                    using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        _selectedImage = Image.FromStream(stream);
                        _selectedImage = new Bitmap(_selectedImage);
                    }

                    SetPictureBoxImage(_selectedImage);
                    _imageChanged = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось загрузить изображение: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region Дополнительные обработчики

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

        #endregion

        #region Жизненный цикл формы

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