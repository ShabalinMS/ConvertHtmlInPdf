# Preparation
- Приложение создано на базе .Net  framework 4.7.2 [ссылка на источник для установки](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net472);
- При запуске приложения рядом с запускаемым файлом "ConvertHTMLInPDF.exe" обязательно должна быть папка wkhtmltopdf (из корня solution).

# Start application
При запуске приложения, оно запросит ввести путь до файла (Application: "Enter the path to the file"). Введите полный путь до файла (пример: C:\Users\user\Desktop\ConvertHTMLInPDF\index.html) и дождитесь выполнения (если в процессе обработки будут ошибки, то в консоле будет выведен стек ошибки (Application: Error: ex)). По завершению процедуры, рядом с вашим файлом будет создан файл в формате pdf (Пример: index.html.pdf) и в консоле приложения будет строка на выбор следующего действия (Application: Ready. Repeat the conversion n or any key?). Для выхода из приложения нужно нажать клавишу n, а для перезапуска нажать любую клавишу и приложение выполнит повторно свой функционал с уже введенным ранее путем до файла.  

## Верстка шаблона
Для разделения в шаблоне на разные странице в pdf, нужно:
- Добавить стиль:

~~~

.page-break {
    display: block;
    page-break-after: always;
}

~~~

- В необходимом месте разрыва страниц добавить:

~~~

<hr class="page-break">

~~~