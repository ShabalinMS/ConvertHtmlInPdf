namespace ConvertHTMLInPDF
{
	public class WkHtmlInput
	{

		public string Input
		{
			get;
			set;
		}

		public string CustomWkHtmlPageArgs
		{
			get;
			set;
		}

		public string PageHeaderHtml
		{
			get;
			set;
		}

		public string PageFooterHtml
		{
			get;
			set;
		}

		internal string HeaderFilePath
		{
			get;
			set;
		}

		internal string FooterFilePath
		{
			get;
			set;
		}

		public WkHtmlInput(string inputFileOrUrl)
		{
			Input = inputFileOrUrl;
		}
	}
}
