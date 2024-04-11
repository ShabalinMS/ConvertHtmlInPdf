using System;

namespace ConvertHTMLInPDF
{
	public class WkHtmlToPdfException : Exception
	{
		public int ErrorCode
		{
			get;
			private set;
		}

		public WkHtmlToPdfException(int errCode, string message)
			: base($"{message} (exit code: {errCode})")
		{
			ErrorCode = errCode;
		}
	}
}
