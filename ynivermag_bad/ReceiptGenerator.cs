using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Word = Microsoft.Office.Interop.Word;

namespace ynivermag_bad
{
    public class ReceiptGenerator
    {
        private string _companyName = "Универмаг";
        private string _companyAddress = "г. Москва, ул. Тверская, д. 1";
        private string _companyPhone = "+7 (495) 123-45-67";
        private string _companyInn = "7701234567";

        public void GenerateReceipt(int orderId, string clientName, string sellerName,
     List<OrderItem> items, decimal totalAmount, DateTime orderDate)
        {
            Word.Application wordApp = null;
            Word.Document doc = null;

            try
            {
                // Создаем приложение Word
                wordApp = new Word.Application();
                wordApp.Visible = false;
                wordApp.DisplayAlerts = Word.WdAlertLevel.wdAlertsNone;

                // Создаем новый документ
                doc = wordApp.Documents.Add();

                // Настраиваем страницу
                doc.PageSetup.Orientation = Word.WdOrientation.wdOrientPortrait;
                doc.PageSetup.LeftMargin = wordApp.CentimetersToPoints(1.5f);
                doc.PageSetup.RightMargin = wordApp.CentimetersToPoints(1.5f);
                doc.PageSetup.TopMargin = wordApp.CentimetersToPoints(1.5f);
                doc.PageSetup.BottomMargin = wordApp.CentimetersToPoints(1.5f);

                // ЗАГОЛОВОК (по центру)
                Word.Paragraph para = doc.Content.Paragraphs.Add();
                para.Range.Text = _companyName;
                para.Range.Font.Size = 24;
                para.Range.Font.Bold = 1;
                para.Range.Font.Name = "Arial";
                para.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                para.Range.InsertParagraphAfter();

                // Подзаголовок (по центру)
                para = doc.Content.Paragraphs.Add();
                para.Range.Text = "ТОВАРНЫЙ ЧЕК";
                para.Range.Font.Size = 18;
                para.Range.Font.Bold = 1;
                para.Range.Font.Name = "Arial";
                para.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                para.Range.InsertParagraphAfter();

                // Пустая строка
                para = doc.Content.Paragraphs.Add();
                para.Range.Text = "";
                para.Range.InsertParagraphAfter();

                // Информация о компании (по левому краю)
                AddInfoLine(doc, "Адрес:", _companyAddress);
                AddInfoLine(doc, "Телефон:", _companyPhone);
                AddInfoLine(doc, "ИНН:", _companyInn);

                // Пустая строка
                para = doc.Content.Paragraphs.Add();
                para.Range.Text = "";
                para.Range.InsertParagraphAfter();

                // Информация о заказе (по левому краю)
                AddInfoLine(doc, "Номер заказа:", $"№{orderId}");
                AddInfoLine(doc, "Дата:", orderDate.ToString("dd.MM.yyyy HH:mm"));
                AddInfoLine(doc, "Клиент:", clientName);
                AddInfoLine(doc, "Продавец:", sellerName);

                // Пустая строка
                para = doc.Content.Paragraphs.Add();
                para.Range.Text = "";
                para.Range.InsertParagraphAfter();

                // ТАБЛИЦА С ТОВАРАМИ
                Word.Table table = doc.Tables.Add(para.Range, items.Count + 1, 5);
                table.Borders.Enable = 1;
                table.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                table.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;

                // Заголовки таблицы
                table.Cell(1, 1).Range.Text = "№";
                table.Cell(1, 2).Range.Text = "Наименование";
                table.Cell(1, 3).Range.Text = "Цена";
                table.Cell(1, 4).Range.Text = "Кол-во";
                table.Cell(1, 5).Range.Text = "Сумма";

                // Форматирование заголовков
                for (int i = 1; i <= 5; i++)
                {
                    table.Cell(1, i).Range.Font.Bold = 1;
                    table.Cell(1, i).Range.Font.Size = 12;
                    table.Cell(1, i).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    table.Cell(1, i).Shading.BackgroundPatternColor = Word.WdColor.wdColorGray15;
                }

                // Заполняем товары
                int rowIndex = 2;
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];

                    table.Cell(rowIndex, 1).Range.Text = (i + 1).ToString();
                    table.Cell(rowIndex, 2).Range.Text = item.ProductName;
                    table.Cell(rowIndex, 3).Range.Text = item.Price.ToString("C2");
                    table.Cell(rowIndex, 4).Range.Text = item.Quantity.ToString();
                    table.Cell(rowIndex, 5).Range.Text = (item.Price * item.Quantity).ToString("C2");

                    // Выравнивание в таблице
                    table.Cell(rowIndex, 1).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    table.Cell(rowIndex, 2).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft; // Название по левому краю
                    table.Cell(rowIndex, 3).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
                    table.Cell(rowIndex, 4).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    table.Cell(rowIndex, 5).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;

                    rowIndex++;
                }

                // Пустая строка
                para = doc.Content.Paragraphs.Add();
                para.Range.Text = "";
                para.Range.InsertParagraphAfter();

                // ИТОГОВАЯ СУММА (по правому краю, как обычно)
                para = doc.Content.Paragraphs.Add();
                para.Range.Text = $"ИТОГО К ОПЛАТЕ: {totalAmount:C2}";
                para.Range.Font.Size = 16;
                para.Range.Font.Bold = 1;
                para.Range.Font.Name = "Arial";
                para.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
                para.Range.InsertParagraphAfter();

                // Пустая строка
                para = doc.Content.Paragraphs.Add();
                para.Range.Text = "";
                para.Range.InsertParagraphAfter();

                // ПОДПИСИ (по правому краю)
                para = doc.Content.Paragraphs.Add();
                para.Range.Text = "_______________  (____________________)";
                para.Range.Font.Size = 12;
                para.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
                para.Range.InsertParagraphAfter();

                para = doc.Content.Paragraphs.Add();
                para.Range.Text = "Подпись продавца";
                para.Range.Font.Size = 10;
                para.Range.Font.Italic = 1;
                para.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;

                // Пустая строка
                para = doc.Content.Paragraphs.Add();
                para.Range.Text = "";
                para.Range.InsertParagraphAfter();

                // Спасибо за покупку (по центру)
                para = doc.Content.Paragraphs.Add();
                para.Range.Text = "Спасибо за покупку!";
                para.Range.Font.Size = 14;
                para.Range.Font.Bold = 1;
                para.Range.Font.Name = "Arial";
                para.Range.Font.Color = Word.WdColor.wdColorGreen;
                para.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                // Сохраняем файл
                string fileName = $"Чек_№{orderId}_{DateTime.Now:yyyyMMdd_HHmmss}.docx";
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filePath = Path.Combine(desktopPath, fileName);

                doc.SaveAs2(filePath);
                doc.Close();
                wordApp.Quit();

                // Открываем файл
                System.Diagnostics.Process.Start(filePath);

                MessageBox.Show($"Чек сохранен на рабочий стол:\n{fileName}", "Чек создан",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании чека: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Освобождаем ресурсы
                if (doc != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(doc);
               
            }
        }

        private void AddInfoLine(Word.Document doc, string label, string value)
        {
            Word.Paragraph para = doc.Content.Paragraphs.Add();
            para.Range.Text = $"{label} {value}";
            para.Range.Font.Size = 11;
            para.Range.Font.Name = "Arial";
            para.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
            para.Range.InsertParagraphAfter();
        }
    }
}