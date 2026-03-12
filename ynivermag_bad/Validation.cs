using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ynivermag_bad
{
    /// <summary>
    /// Класс для валидации различных типов ввода
    /// </summary>
    public static class InputValidator
    {
        // Регулярные выражения
        private static readonly Regex _russianLettersRegex = new Regex(@"^[а-яА-ЯёЁ\s\-]*$");
        private static readonly Regex _russianLettersAndNumbersRegex = new Regex(@"^[а-яА-ЯёЁ0-9\s\-]*$");
        private static readonly Regex _onlyDigitsRegex = new Regex(@"^[0-9]*$");
        private static readonly Regex _phoneRegex = new Regex(@"^[\+\d\s\-\(\)]*$");
        private static readonly Regex _loginRegex = new Regex(@"^[a-zA-Z0-9_\.]*$");

        #region Русские буквы

        /// <summary>
        /// Проверяет, содержит ли строка только русские буквы, пробелы и дефисы
        /// </summary>
        public static bool IsValidRussianText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return true;

            return _russianLettersRegex.IsMatch(text);
        }

        /// <summary>
        /// Проверяет, содержит ли строка только русские буквы, цифры, пробелы и дефисы
        /// </summary>
        public static bool IsValidRussianTextWithNumbers(string text)
        {
            if (string.IsNullOrEmpty(text))
                return true;

            return _russianLettersAndNumbersRegex.IsMatch(text);
        }

        /// <summary>
        /// Устанавливает TextBox для ввода только русских букв
        /// </summary>
        public static void SetupRussianTextBox(TextBox textBox, bool allowNumbers = false, bool allowSpaces = true, bool allowHyphen = true)
        {
            textBox.KeyPress += (sender, e) =>
            {
                var tb = sender as TextBox;

                if (char.IsControl(e.KeyChar))
                {
                    return;
                }

                if (IsRussianLetter(e.KeyChar) ||
                    (allowNumbers && char.IsDigit(e.KeyChar)) ||
                    (allowSpaces && e.KeyChar == ' ') ||
                    (allowHyphen && e.KeyChar == '-'))
                {
                    return;
                }

                e.Handled = true;
            };
        }

        /// <summary>
        /// Проверяет, является ли символ русской буквой
        /// </summary>
        public static bool IsRussianLetter(char c)
        {
            if (c >= 'а' && c <= 'я') return true;
            if (c >= 'А' && c <= 'Я') return true;
            if (c == 'ё' || c == 'Ё') return true;
            return false;
        }

        /// <summary>
        /// Отфильтровывает из текста все не-русские символы
        /// </summary>
        public static string FilterToRussianLetters(string text, bool allowNumbers = false, bool allowSpaces = true, bool allowHyphen = true)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var result = new System.Text.StringBuilder();

            foreach (char c in text)
            {
                if (IsRussianLetter(c) ||
                    (allowNumbers && char.IsDigit(c)) ||
                    (allowSpaces && c == ' ') ||
                    (allowHyphen && c == '-'))
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        #endregion

        #region Цифры и числа

        /// <summary>
        /// Проверяет, содержит ли строка только цифры
        /// </summary>
        public static bool IsValidDigitsOnly(string text)
        {
            if (string.IsNullOrEmpty(text))
                return true;

            return _onlyDigitsRegex.IsMatch(text);
        }

        /// <summary>
        /// Устанавливает TextBox для ввода только цифр
        /// </summary>
        public static void SetupDigitsTextBox(TextBox textBox, bool allowNegative = false, bool allowDecimal = false)
        {
            textBox.KeyPress += (sender, e) =>
            {
                // Разрешаем управляющие символы
                if (char.IsControl(e.KeyChar))
                {
                    return;
                }

                var tb = sender as TextBox;
                string currentText = tb.Text;
                int selectionStart = tb.SelectionStart;

                // Проверяем вводимый символ
                bool isValid = false;

                // Цифры всегда разрешены
                if (char.IsDigit(e.KeyChar))
                {
                    isValid = true;
                }
                // Десятичный разделитель
                else if (allowDecimal && (e.KeyChar == '.' || e.KeyChar == ','))
                {
                    // Заменяем запятую на точку
                    e.KeyChar = '.';

                    // Проверяем, что точки еще нет в тексте
                    if (!currentText.Contains('.'))
                    {
                        isValid = true;
                    }
                }
                // Знак минуса для отрицательных чисел
                else if (allowNegative && e.KeyChar == '-' && selectionStart == 0)
                {
                    // Минус можно ввести только в начале
                    if (!currentText.Contains('-'))
                    {
                        isValid = true;
                    }
                }

                e.Handled = !isValid;
            };
        }

        /// <summary>
        /// Отфильтровывает из текста все нецифровые символы
        /// </summary>
        public static string FilterToDigitsOnly(string text, bool allowDecimal = false, bool allowNegative = false)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var result = new System.Text.StringBuilder();
            bool hasDecimalPoint = false;
            bool hasMinusSign = false;

            foreach (char c in text)
            {
                if (char.IsDigit(c))
                {
                    result.Append(c);
                }
                else if (allowDecimal && (c == '.' || c == ',') && !hasDecimalPoint)
                {
                    result.Append('.');
                    hasDecimalPoint = true;
                }
                else if (allowNegative && c == '-' && !hasMinusSign && result.Length == 0)
                {
                    result.Append(c);
                    hasMinusSign = true;
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Конвертирует строку в целое число с проверкой
        /// </summary>
        public static bool TryParseInteger(string text, out int result)
        {
            result = 0;

            if (string.IsNullOrEmpty(text))
                return false;

            string cleanText = FilterToDigitsOnly(text);
            return int.TryParse(cleanText, out result);
        }

        #endregion

        #region Телефоны

        /// <summary>
        /// Проверяет корректность номера телефона
        /// </summary>
        public static bool IsValidPhone(string text)
        {
            if (string.IsNullOrEmpty(text))
                return true;

            return _phoneRegex.IsMatch(text);
        }

        /// <summary>
        /// Устанавливает TextBox для ввода номера телефона
        /// </summary>
        public static void SetupPhoneTextBox(TextBox textBox)
        {
            textBox.KeyPress += (sender, e) =>
            {
                if (char.IsControl(e.KeyChar))
                {
                    return;
                }

                // Разрешаем: цифры, +, -, (, ), пробел
                if (char.IsDigit(e.KeyChar) ||
                    e.KeyChar == '+' ||
                    e.KeyChar == '-' ||
                    e.KeyChar == '(' ||
                    e.KeyChar == ')' ||
                    e.KeyChar == ' ')
                {
                    // + можно ввести только первым символом
                    if (e.KeyChar == '+' && (sender as TextBox).Text.Length > 0)
                    {
                        e.Handled = true;
                    }
                    else
                    {
                        return;
                    }
                }

                e.Handled = true;
            };
        }

        /// <summary>
        /// Отфильтровывает из текста все символы, кроме разрешенных для телефона
        /// </summary>
        public static string FilterToPhone(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var result = new System.Text.StringBuilder();
            bool hasPlus = false;

            foreach (char c in text)
            {
                if (char.IsDigit(c))
                {
                    result.Append(c);
                }
                else if (c == '+' && !hasPlus)
                {
                    // + можно добавить только в начале
                    if (result.Length == 0)
                    {
                        result.Append(c);
                        hasPlus = true;
                    }
                }
                else if (c == '-' || c == '(' || c == ')' || c == ' ')
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Форматирует номер телефона в стандартном виде +7(XXX)XXX-XX-XX
        /// </summary>
        public static string FormatPhoneNumber(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Убираем все нецифровые символы
            string digitsOnly = new string(text.Where(char.IsDigit).ToArray());

            if (string.IsNullOrEmpty(digitsOnly))
                return text;

            // Приводим к формату +7
            if (digitsOnly.StartsWith("8"))
            {
                digitsOnly = "7" + digitsOnly.Substring(1);
            }
            else if (!digitsOnly.StartsWith("7"))
            {
                digitsOnly = "7" + digitsOnly;
            }

            // Форматируем в зависимости от длины
            if (digitsOnly.Length == 1)
            {
                return $"+{digitsOnly}";
            }
            else if (digitsOnly.Length <= 4)
            {
                return $"+{digitsOnly.Substring(0, 1)} ({digitsOnly.Substring(1)}";
            }
            else if (digitsOnly.Length <= 7)
            {
                return $"+{digitsOnly.Substring(0, 1)} ({digitsOnly.Substring(1, 3)}) {digitsOnly.Substring(4)}";
            }
            else if (digitsOnly.Length <= 9)
            {
                return $"+{digitsOnly.Substring(0, 1)} ({digitsOnly.Substring(1, 3)}) {digitsOnly.Substring(4, 3)}-{digitsOnly.Substring(7)}";
            }
            else
            {
                return $"+{digitsOnly.Substring(0, 1)} ({digitsOnly.Substring(1, 3)}) {digitsOnly.Substring(4, 3)}-{digitsOnly.Substring(7, 2)}-{digitsOnly.Substring(9, Math.Min(2, digitsOnly.Length - 9))}";
            }
        }

        /// <summary>
        /// Возвращает чистый номер телефона (только цифры) для сохранения в БД
        /// </summary>
        public static string GetCleanPhoneNumber(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            // Оставляем только цифры
            string digitsOnly = new string(text.Where(char.IsDigit).ToArray());

            if (string.IsNullOrEmpty(digitsOnly))
                return "";

            // Приводим к формату 7XXXXXXXXXX
            if (digitsOnly.StartsWith("8"))
            {
                digitsOnly = "7" + digitsOnly.Substring(1);
            }
            else if (!digitsOnly.StartsWith("7"))
            {
                digitsOnly = "7" + digitsOnly;
            }

            // Ограничиваем 11 цифрами (7 + 10)
            return digitsOnly.Length > 11 ? digitsOnly.Substring(0, 11) : digitsOnly;
        }

        /// <summary>
        /// Проверяет, является ли номер телефона полным (11 цифр)
        /// </summary>
        public static bool IsCompletePhoneNumber(string text)
        {
            string cleanNumber = GetCleanPhoneNumber(text);
            return cleanNumber.Length == 11;
        }

        #endregion

        #region Логины

        /// <summary>
        /// Проверяет корректность логина
        /// </summary>
        public static bool IsValidLogin(string text)
        {
            if (string.IsNullOrEmpty(text))
                return true;

            return _loginRegex.IsMatch(text);
        }

        /// <summary>
        /// Устанавливает TextBox для ввода логина
        /// </summary>
        public static void SetupLoginTextBox(TextBox textBox)
        {
            textBox.KeyPress += (sender, e) =>
            {
                if (char.IsControl(e.KeyChar))
                {
                    return;
                }

                // Разрешаем: английские буквы, цифры, подчеркивание, точка
                if ((e.KeyChar >= 'a' && e.KeyChar <= 'z') ||
                    (e.KeyChar >= 'A' && e.KeyChar <= 'Z') ||
                    char.IsDigit(e.KeyChar) ||
                    e.KeyChar == '_' ||
                    e.KeyChar == '.')
                {
                    return;
                }

                e.Handled = true;
            };
        }

        #endregion

        #region Универсальные методы

        /// <summary>
        /// Типы ввода для TextBox
        /// </summary>
        public enum InputType
        {
            RussianText,
            RussianTextWithNumbers,
            DigitsOnly,
            Phone,
            Login
        }

        /// <summary>
        /// Устанавливает TextBox для определенного типа ввода
        /// </summary>
        public static void SetupTextBox(TextBox textBox, InputType inputType)
        {
            switch (inputType)
            {
                case InputType.RussianText:
                    SetupRussianTextBox(textBox);
                    break;
                case InputType.RussianTextWithNumbers:
                    SetupRussianTextBox(textBox, true);
                    break;
                case InputType.DigitsOnly:
                    SetupDigitsTextBox(textBox);
                    break;
                case InputType.Phone:
                    SetupPhoneTextBox(textBox);
                    break;
                case InputType.Login:
                    SetupLoginTextBox(textBox);
                    break;
            }
        }

        /// <summary>
        /// Проверяет текст по типу ввода
        /// </summary>
        public static bool ValidateTextBox(TextBox textBox, InputType inputType, string fieldName)
        {
            string text = textBox.Text;
            bool isValid = false;
            string errorMessage = "";

            switch (inputType)
            {
                case InputType.RussianText:
                    isValid = IsValidRussianText(text);
                    errorMessage = $"Поле '{fieldName}' может содержать только русские буквы, пробелы и дефисы.";
                    break;

                case InputType.RussianTextWithNumbers:
                    isValid = IsValidRussianTextWithNumbers(text);
                    errorMessage = $"Поле '{fieldName}' может содержать только русские буквы, цифры, пробелы и дефисы.";
                    break;

                case InputType.DigitsOnly:
                    isValid = IsValidDigitsOnly(text);
                    errorMessage = $"Поле '{fieldName}' может содержать только цифры.";
                    break;

                case InputType.Phone:
                    isValid = IsValidPhone(text);
                    errorMessage = $"Поле '{fieldName}' может содержать только цифры, знак '+', пробелы, скобки и дефисы.";
                    break;

                case InputType.Login:
                    isValid = IsValidLogin(text);
                    errorMessage = $"Поле '{fieldName}' может содержать только английские буквы, цифры, подчеркивание и точку.";
                    break;
            }

            if (!isValid && !string.IsNullOrEmpty(errorMessage))
            {
                MessageBox.Show(errorMessage, "Ошибка ввода",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox.Focus();
                textBox.SelectAll();
            }

            return isValid;
        }

        #endregion
    }
}
