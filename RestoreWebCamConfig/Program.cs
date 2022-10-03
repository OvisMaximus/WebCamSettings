namespace RestoreWebCamConfig
{
	static class Program
	{
		static void Main(string[] args)
		{
			WebCamConfigUtility program = new WebCamConfigUtility(args);
			program.Run();
		}
	}
}