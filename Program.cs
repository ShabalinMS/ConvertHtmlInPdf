using System;
using System.IO;
using System.Text;
using static ConvertHTMLInPDF.HtmlToPdfConverter;

namespace ConvertHTMLInPDF
{
	public class Program
	{
		/// <summary>
		/// Исходный файл
		/// </summary>
		private static FileInfo _path;

		public static void Main(string[] args)
		{
			while (true)
			{
				if (_path == null)
				{
					Console.WriteLine("Enter the path to the file");
					string path = Console.ReadLine();

					if (string.IsNullOrWhiteSpace(path))
					{
						Console.WriteLine("Incorrect path");
					}

					_path = new FileInfo(path);
				}

				try
				{
					string html = GetContentFromFile(_path);

					Byte[] bytes = Convert(html);

					CreateFile(_path, bytes);
				} catch (Exception ex)
				{
					Console.WriteLine($"Error: {ex.ToString()}");
				}

				Console.WriteLine("Ready. Repeat the conversion n or any key?");
				if(Console.ReadLine() == "n")
				{
					break;
				}
			}
		}

		#region Methods private

		/// <summary>
		/// Получить содержимое файла
		/// </summary>
		/// <param name="file">Файл</param>
		/// <returns>Контект файла</returns>
		private static string GetContentFromFile(FileInfo file)
		{
			if(!file.Exists)
			{
				Console.WriteLine("The file was not found");
			}

			using (FileStream fstream = file.OpenRead())
			{
				byte[] buffer = new byte[fstream.Length];
				fstream.Read(buffer, 0, buffer.Length);
				return Encoding.UTF8.GetString(buffer);
			}
		}

		/// <summary>
		/// Конверт html в зва 
		/// </summary>
		/// <param name="html">Исходная строка html</param>
		/// <returns>Набор байтов для файла</returns>
		private static Byte[] Convert(string html)
		{
			Byte[] res = null;
			res = new HtmlToPdf().GeneratePdf(html);
			return res;
		}


		/// <summary>
		/// Запись в файл
		/// </summary>
		/// <param name="file">Файл</param>
		/// <param name="bytes">Конект файла</param>
		private static void CreateFile(FileInfo file, Byte[] bytes)
		{
			string path = $"{file.FullName}.pdf";
			File.Delete(path);
			File.WriteAllBytes(path, bytes);
		}

		#endregion

	}
}
