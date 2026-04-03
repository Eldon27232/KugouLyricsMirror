namespace KugouLyricsMirror;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        AppConfig.Load();
        Application.Run(new ControlForm());
    }
}
