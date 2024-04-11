using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;

namespace ConvertHTMLInPDF
{
	internal class HtmlToPdfConverter
	{	
		
		//
		// Сводка:
		//     Html to PDF converter component (C# WkHtmlToPdf process wrapper).
		public class HtmlToPdf
		{
			private Process WkHtmlToPdfProcess;

			private bool batchMode;

			private const string headerFooterHtmlTpl = "<!DOCTYPE html><html><head>\r\n<meta http-equiv=\"content-type\" content=\"text/html; charset=utf-8\" />\r\n<script>\r\nfunction subst() {{\r\n    var vars={{}};\r\n    var x=document.location.search.substring(1).split('&');\r\n\r\n    for(var i in x) {{var z=x[i].split('=',2);vars[z[0]] = unescape(z[1]);}}\r\n    var x=['frompage','topage','page','webpage','section','subsection','subsubsection'];\r\n    for(var i in x) {{\r\n      var y = document.getElementsByClassName(x[i]);\r\n      for(var j=0; j<y.length; ++j) y[j].textContent = vars[x[i]];\r\n    }}\r\n}}\r\n</script></head><body style=\"border:0; margin: 0;\" onload=\"subst()\">{0}</body></html>\r\n";

			private static object globalObj = new object();

			private static string[] ignoreWkHtmlToPdfErrLines = new string[6]
			{
				"Exit with code 1 due to network error: ContentNotFoundError",
				"QFont::setPixelSize: Pixel size <= 0",
				"Exit with code 1 due to network error: ProtocolUnknownError",
				"Exit with code 1 due to network error: HostNotFoundError",
				"Exit with code 1 due to network error: ContentOperationNotPermittedError",
				"Exit with code 1 due to network error: UnknownContentError"
			};

			//
			// Сводка:
			//     Get or set path where WkHtmlToPdf tool is located
			//
			// Примечания:
			//     By default this property points to the folder where application assemblies are
			//     located. If WkHtmlToPdf tool files are not present PdfConverter expands them
			//     from DLL resources.
			public string PdfToolPath
			{
				get;
				set;
			}

			//
			// Сводка:
			//     Get or set WkHtmlToPdf tool EXE file name ('wkhtmltopdf.exe' by default)
			public string WkHtmlToPdfExeName
			{
				get;
				set;
			}

			//
			// Сводка:
			//     Get or set location for temp files (if not specified location returned by System.IO.Path.GetTempPath
			//     is used for temp files)
			//
			// Примечания:
			//     Temp files are used for providing cover page/header/footer HTML templates to
			//     wkhtmltopdf tool.
			public string TempFilesPath
			{
				get;
				set;
			}

			//
			// Сводка:
			//     Get or set PDF page orientation
			public PageOrientation Orientation
			{
				get;
				set;
			}

			//
			// Сводка:
			//     Get or set PDF page orientation
			public PageSize Size
			{
				get;
				set;
			}

			//
			// Сводка:
			//     Gets or sets option to generate low quality PDF (shrink the result document space)
			public bool LowQuality
			{
				get;
				set;
			}

			//
			// Сводка:
			//     Gets or sets option to generate grayscale PDF
			public bool Grayscale
			{
				get;
				set;
			}

			//
			// Сводка:
			//     Gets or sets zoom factor
			public float Zoom
			{
				get;
				set;
			}

			//
			// Сводка:
			//     Gets or sets PDF page margins (in mm)
			public PageMargins Margins
			{
				get;
				set;
			}

			//
			// Сводка:
			//     Gets or sets PDF page width (in mm)
			public float? PageWidth
			{
				get;
				set;
			}

			//
			// Сводка:
			//     Gets or sets PDF page height (in mm)
			public float? PageHeight
			{
				get;
				set;
			}

			//
			// Сводка:
			//     Gets or sets TOC generation flag
			public bool GenerateToc
			{
				get;
				set;
			}

			//
			// Сводка:
			//     Gets or sets custom TOC header text (default: "Table of Contents")
			public string TocHeaderText
			{
				get;
				set;
			}

			//
			// Сводка:
			//     Custom WkHtmlToPdf global options
			public string CustomWkHtmlArgs
			{
				get;
				set;
			}

			//
			// Сводка:
			//     Custom WkHtmlToPdf page options
			public string CustomWkHtmlPageArgs
			{
				get;
				set;
			}

			//
			// Сводка:
			//     Custom WkHtmlToPdf cover options (applied only if cover content is specified)
			public string CustomWkHtmlCoverArgs
			{
				get;
				set;
			}

			//
			// Сводка:
			//     Custom WkHtmlToPdf toc options (applied only if GenerateToc is true)
			public string CustomWkHtmlTocArgs
			{
				get;
				set;
			}

			//
			// Сводка:
			//     Get or set custom page header HTML
			public string PageHeaderHtml
			{
				get;
				set;
			}

			//
			// Сводка:
			//     Get or set custom page footer HTML
			public string PageFooterHtml
			{
				get;
				set;
			}

			//
			// Сводка:
			//     Get or set maximum execution time for PDF generation process (by default is null
			//     that means no timeout)
			public TimeSpan? ExecutionTimeout
			{
				get;
				set;
			}

			//
			// Сводка:
			//     Gets or sets wkhtmltopdf process priority (Normal by default)
			public ProcessPriorityClass ProcessPriority
			{
				get;
				set;
			}

			//
			// Сводка:
			//     Gets or sets wkhtmltopdf processor affinity (bitmask that represents the processors
			//     that may be used by the process threads).
			public IntPtr? ProcessProcessorAffinity
			{
				get;
				set;
			}

			//
			// Сводка:
			//     Suppress wkhtmltopdf debug/info log messages (by default is true)
			public bool Quiet
			{
				get;
				set;
			}

			//
			// Сводка:
			//     Component commercial license information.
			//public LicenseInfo License
			//{
			//	get;
			//	private set;
			//}

			//
			// Сводка:
			//     Occurs when log line is received from WkHtmlToPdf process
			//
			// Примечания:
			//     Quiet mode should be disabled if you want to get wkhtmltopdf info/debug messages
			public event EventHandler<DataReceivedEventArgs> LogReceived;

			//
			// Сводка:
			//     Create new instance of HtmlToPdfConverter
			public HtmlToPdf()
			{
				ProcessPriority = ProcessPriorityClass.Normal;
				ProcessProcessorAffinity = null;
				string pdfToolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wkhtmltopdf");
				if (HttpContext.Current != null)
				{
					pdfToolPath = Path.Combine(HttpRuntime.AppDomainAppPath, "\\wkhtmltopdf");
				}

				PdfToolPath = pdfToolPath;
				TempFilesPath = null;
				WkHtmlToPdfExeName = "wkhtmltopdf.exe";
				Orientation = PageOrientation.Default;
				Size = PageSize.Default;
				LowQuality = false;
				Grayscale = false;
				Quiet = true;
				Zoom = 1f;
				Margins = new PageMargins();
			}

			/// <summary>
			/// Generates PDF by specifed HTML content
			/// </summary>
			/// <param name="htmlContent"HTML content></param>
			/// <returns>PDF bytes</returns>
			/// <exception cref="ArgumentNullException">htmlContent is empty or null</exception>
			public byte[] GeneratePdf(string htmlContent)
			{
				if (string.IsNullOrWhiteSpace(htmlContent)) throw new ArgumentNullException("htmlContent");
				byte[] pdfFile = null;
				using (MemoryStream memoryStream = new MemoryStream())
				{
					GeneratePdfInternal(htmlContent, "-", memoryStream);
					pdfFile = memoryStream.ToArray();
				}
				return pdfFile;
			}

			private void CheckWkHtmlProcess()
			{
				if (!batchMode && WkHtmlToPdfProcess != null)
				{
					throw new InvalidOperationException("WkHtmlToPdf process is already started");
				}
			}

			private string GetTempPath()
			{
				if (!string.IsNullOrEmpty(TempFilesPath) && !Directory.Exists(TempFilesPath))
				{
					Directory.CreateDirectory(TempFilesPath);
				}

				return TempFilesPath ?? Path.GetTempPath();
			}

			private string GetToolExePath()
			{
				if (string.IsNullOrEmpty(PdfToolPath))
				{
					throw new ArgumentException("PdfToolPath property is not initialized with path to wkhtmltopdf binaries");
				}

				string text = Path.Combine(PdfToolPath, WkHtmlToPdfExeName);
				if (!System.IO.File.Exists(text))
				{
					throw new FileNotFoundException("Cannot find wkhtmltopdf executable: " + text);
				}

				return text;
			}

			private string CreateTempFile(string content, string tempPath, List<string> tempFilesList)
			{
				string text = Path.Combine(tempPath, "pdfgen-" + Path.GetRandomFileName() + ".html");
				tempFilesList.Add(text);
				if (content != null)
				{
					System.IO.File.WriteAllBytes(text, Encoding.UTF8.GetBytes(content));
				}

				return text;
			}

			private string ComposeArgs(PdfSettings pdfSettings)
			{
				StringBuilder stringBuilder = new StringBuilder();
				if (Quiet)
				{
					stringBuilder.Append(" -q ");
				}

				if (Orientation != 0)
				{
					stringBuilder.AppendFormat(" -O {0} ", Orientation.ToString());
				}

				if (Size != 0)
				{
					stringBuilder.AppendFormat(" -s {0} ", Size.ToString());
				}

				if (LowQuality)
				{
					stringBuilder.Append(" -l ");
				}

				if (Grayscale)
				{
					stringBuilder.Append(" -g ");
				}

				if (Margins != null)
				{
					if (Margins.Top.HasValue)
					{
						stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " -T {0}", new object[1]
						{
						Margins.Top
						});
					}

					if (Margins.Bottom.HasValue)
					{
						stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " -B {0}", new object[1]
						{
						Margins.Bottom
						});
					}

					if (Margins.Left.HasValue)
					{
						stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " -L {0}", new object[1]
						{
						Margins.Left
						});
					}

					if (Margins.Right.HasValue)
					{
						stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " -R {0}", new object[1]
						{
						Margins.Right
						});
					}
				}

				if (PageWidth.HasValue)
				{
					stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " --page-width {0}", new object[1]
					{
					PageWidth
					});
				}

				if (PageHeight.HasValue)
				{
					stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " --page-height {0}", new object[1]
					{
					PageHeight
					});
				}

				if (pdfSettings.HeaderFilePath != null)
				{
					stringBuilder.AppendFormat(" --header-html \"{0}\"", pdfSettings.HeaderFilePath);
				}

				if (pdfSettings.FooterFilePath != null)
				{
					stringBuilder.AppendFormat(" --footer-html \"{0}\"", pdfSettings.FooterFilePath);
				}

				if (!string.IsNullOrEmpty(CustomWkHtmlArgs))
				{
					stringBuilder.AppendFormat(" {0} ", CustomWkHtmlArgs);
				}

				if (pdfSettings.CoverFilePath != null)
				{
					stringBuilder.AppendFormat(" cover \"{0}\" ", pdfSettings.CoverFilePath);
					if (!string.IsNullOrEmpty(CustomWkHtmlCoverArgs))
					{
						stringBuilder.AppendFormat(" {0} ", CustomWkHtmlCoverArgs);
					}
				}

				if (GenerateToc)
				{
					stringBuilder.Append(" toc ");
					if (!string.IsNullOrEmpty(TocHeaderText))
					{
						stringBuilder.AppendFormat(" --toc-header-text \"{0}\"", TocHeaderText.Replace("\"", "\\\""));
					}

					if (!string.IsNullOrEmpty(CustomWkHtmlTocArgs))
					{
						stringBuilder.AppendFormat(" {0} ", CustomWkHtmlTocArgs);
					}
				}

				WkHtmlInput[] inputFiles = pdfSettings.InputFiles;
				foreach (WkHtmlInput wkHtmlInput in inputFiles)
				{
					stringBuilder.AppendFormat(" \"{0}\" ", wkHtmlInput.Input);
					string text = wkHtmlInput.CustomWkHtmlPageArgs ?? CustomWkHtmlPageArgs;
					if (!string.IsNullOrEmpty(text))
					{
						stringBuilder.AppendFormat(" {0} ", text);
					}

					if (wkHtmlInput.HeaderFilePath != null)
					{
						stringBuilder.AppendFormat(" --header-html \"{0}\"", wkHtmlInput.HeaderFilePath);
					}

					if (wkHtmlInput.FooterFilePath != null)
					{
						stringBuilder.AppendFormat(" --footer-html \"{0}\"", wkHtmlInput.FooterFilePath);
					}

					if (Zoom != 1f)
					{
						stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " --zoom {0} ", new object[1]
						{
						Zoom
						});
					}
				}

				stringBuilder.AppendFormat(" \"{0}\" ", pdfSettings.OutputFile);
				return stringBuilder.ToString();
			}

			private void GeneratePdfInternal(string inputContent, string outputPdfFilePath, Stream outputStream)
			{
				string tempPath = GetTempPath();
				PdfSettings pdfSettings = new PdfSettings
				{
					InputFiles = null,
					OutputFile = outputPdfFilePath
				};
				List<string> list = new List<string>();
				pdfSettings.CoverFilePath = null;
				pdfSettings.HeaderFilePath = ((!string.IsNullOrEmpty(PageHeaderHtml)) ? CreateTempFile($"<!DOCTYPE html><html><head>\r\n<meta http-equiv=\"content-type\" content=\"text/html; charset=utf-8\" />\r\n<script>\r\nfunction subst() {{\r\n    var vars={{}};\r\n    var x=document.location.search.substring(1).split('&');\r\n\r\n    for(var i in x) {{var z=x[i].split('=',2);vars[z[0]] = unescape(z[1]);}}\r\n    var x=['frompage','topage','page','webpage','section','subsection','subsubsection'];\r\n    for(var i in x) {{\r\n      var y = document.getElementsByClassName(x[i]);\r\n      for(var j=0; j<y.length; ++j) y[j].textContent = vars[x[i]];\r\n    }}\r\n}}\r\n</script></head><body style=\"border:0; margin: 0;\" onload=\"subst()\">{PageHeaderHtml}</body></html>\r\n", tempPath, list) : null);
				pdfSettings.FooterFilePath = ((!string.IsNullOrEmpty(PageFooterHtml)) ? CreateTempFile($"<!DOCTYPE html><html><head>\r\n<meta http-equiv=\"content-type\" content=\"text/html; charset=utf-8\" />\r\n<script>\r\nfunction subst() {{\r\n    var vars={{}};\r\n    var x=document.location.search.substring(1).split('&');\r\n\r\n    for(var i in x) {{var z=x[i].split('=',2);vars[z[0]] = unescape(z[1]);}}\r\n    var x=['frompage','topage','page','webpage','section','subsection','subsubsection'];\r\n    for(var i in x) {{\r\n      var y = document.getElementsByClassName(x[i]);\r\n      for(var j=0; j<y.length; ++j) y[j].textContent = vars[x[i]];\r\n    }}\r\n}}\r\n</script></head><body style=\"border:0; margin: 0;\" onload=\"subst()\">{PageFooterHtml}</body></html>\r\n", tempPath, list) : null);

				try
				{
					if (inputContent != null)
					{
						pdfSettings.InputFiles = new WkHtmlInput[1]
						{
						new WkHtmlInput(CreateTempFile(inputContent, tempPath, list))
						};
					}

					if (outputStream != null)
					{
						pdfSettings.OutputFile = CreateTempFile(null, tempPath, list);
					}

					InvokeWkHtmlToPdf(pdfSettings, null, null);

					if (outputStream != null)
					{
						using (FileStream inputStream = new FileStream(pdfSettings.OutputFile, FileMode.Open, FileAccess.Read, FileShare.Read))
						{
							CopyStream(inputStream, outputStream, 65536);
						}
					}
				}
				catch (Exception ex)
				{
					if (!batchMode)
					{
						EnsureWkHtmlProcessStopped();
					}

					throw new Exception("Cannot generate PDF: " + ex.Message, ex);
				}
				finally
				{
					foreach (string item in list)
					{
						DeleteFileIfExists(item);
					}
				}
			}

			private void InvokeWkHtmlToPdf(PdfSettings pdfSettings, string inputContent, Stream outputStream)
			{
				lock (globalObj)
				{
					string lastErrorLine = string.Empty;
					DataReceivedEventHandler value = delegate (object o, DataReceivedEventArgs args)
					{
						if (args.Data != null)
						{
							if (!string.IsNullOrEmpty(args.Data))
							{
								lastErrorLine = args.Data;
							}

							if (this.LogReceived != null)
							{
								this.LogReceived(this, args);
							}
						}
					};
					byte[] array = (inputContent != null) ? Encoding.UTF8.GetBytes(inputContent) : null;
					try
					{
						string arguments = ComposeArgs(pdfSettings);
						ProcessStartInfo processStartInfo = new ProcessStartInfo(GetToolExePath(), arguments);
						processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
						processStartInfo.CreateNoWindow = true;
						processStartInfo.UseShellExecute = false;
						processStartInfo.WorkingDirectory = Path.GetDirectoryName(PdfToolPath);
						processStartInfo.RedirectStandardInput = (array != null);
						processStartInfo.RedirectStandardOutput = (outputStream != null);
						processStartInfo.RedirectStandardError = true;
						WkHtmlToPdfProcess = Process.Start(processStartInfo);
						if (ProcessPriority != ProcessPriorityClass.Normal)
						{
							WkHtmlToPdfProcess.PriorityClass = ProcessPriority;
						}

						if (ProcessProcessorAffinity.HasValue)
						{
							WkHtmlToPdfProcess.ProcessorAffinity = ProcessProcessorAffinity.Value;
						}

						WkHtmlToPdfProcess.ErrorDataReceived += value;
						WkHtmlToPdfProcess.BeginErrorReadLine();
						if (array != null)
						{
							WkHtmlToPdfProcess.StandardInput.BaseStream.Write(array, 0, array.Length);
							WkHtmlToPdfProcess.StandardInput.BaseStream.Flush();
							WkHtmlToPdfProcess.StandardInput.Close();
						}

						long num = 0L;
						if (outputStream != null)
						{
							num = ReadStdOutToStream(WkHtmlToPdfProcess, outputStream);
						}

						WaitWkHtmlProcessForExit();
						if (outputStream == null && System.IO.File.Exists(pdfSettings.OutputFile))
						{
							num = new FileInfo(pdfSettings.OutputFile).Length;
						}

						CheckExitCode(WkHtmlToPdfProcess.ExitCode, lastErrorLine, num > 0);
					}
					finally
					{
						EnsureWkHtmlProcessStopped();
					}
				}
			}

			private void WaitWkHtmlProcessForExit()
			{
				if (ExecutionTimeout.HasValue)
				{
					if (!WkHtmlToPdfProcess.WaitForExit((int)ExecutionTimeout.Value.TotalMilliseconds))
					{
						EnsureWkHtmlProcessStopped();
						throw new WkHtmlToPdfException(-2, $"WkHtmlToPdf process exceeded execution timeout ({ExecutionTimeout}) and was aborted");
					}
				}
				else
				{
					WkHtmlToPdfProcess.WaitForExit();
				}
			}

			private void EnsureWkHtmlProcessStopped()
			{
				if (WkHtmlToPdfProcess == null)
				{
					return;
				}

				if (!WkHtmlToPdfProcess.HasExited)
				{
					try
					{
						WkHtmlToPdfProcess.Kill();
						WkHtmlToPdfProcess.Close();
						WkHtmlToPdfProcess = null;
					}
					catch (Exception)
					{
					}
				}
				else
				{
					WkHtmlToPdfProcess.Close();
					WkHtmlToPdfProcess = null;
				}
			}

			private int ReadStdOutToStream(Process proc, Stream outputStream)
			{
				byte[] array = new byte[32768];
				int num = 0;
				int num2;
				while ((num2 = proc.StandardOutput.BaseStream.Read(array, 0, array.Length)) > 0)
				{
					outputStream.Write(array, 0, num2);
					num += num2;
				}

				return num;
			}

			private void CheckExitCode(int exitCode, string lastErrLine, bool outputNotEmpty)
			{
				int num;
				switch (exitCode)
				{
					case 0:
						return;
					case 1:
						num = ((Array.IndexOf(ignoreWkHtmlToPdfErrLines, lastErrLine.Trim()) >= 0) ? 1 : 0);
						break;
					default:
						num = 0;
						break;
				}

				if (((uint)num & (outputNotEmpty ? 1u : 0u)) != 0)
				{
					return;
				}

				throw new WkHtmlToPdfException(exitCode, lastErrLine);
			}

			private void DeleteFileIfExists(string filePath)
			{
				if (filePath != null && System.IO.File.Exists(filePath))
				{
					try
					{
						System.IO.File.Delete(filePath);
					}
					catch
					{
					}
				}
			}

			private void CopyStream(Stream inputStream, Stream outputStream, int bufSize)
			{
				byte[] array = new byte[bufSize];
				int count;
				while ((count = inputStream.Read(array, 0, array.Length)) > 0)
				{
					outputStream.Write(array, 0, count);
				}
			}
		}
	}
}
