namespace Linkbar;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

        // Find settings file
        string? settingsFile = null;

        // Check command line for /f parameter
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith("/f", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                settingsFile = args[i + 1];
            else if (args[i].StartsWith("/f", StringComparison.OrdinalIgnoreCase) && args[i].Length > 2)
                settingsFile = args[i][2..];
        }

        // Default: look for .lbr files in app folder and roaming folder
        if (settingsFile == null)
        {
            var appDir = Path.GetDirectoryName(Application.ExecutablePath) ?? ".";
            var sharedBars = Path.Combine(appDir, "Shared bars");
            var userBars = Path.Combine(LinkbarSettings.GetRoamingFolder(), "User bars");

            var lbrFiles = new List<string>();
            if (Directory.Exists(sharedBars))
                lbrFiles.AddRange(Directory.GetFiles(sharedBars, "*.lbr"));
            if (Directory.Exists(userBars))
                lbrFiles.AddRange(Directory.GetFiles(userBars, "*.lbr"));

            if (lbrFiles.Count > 0)
                settingsFile = lbrFiles[0];
        }

        // If no settings file found, create default
        if (settingsFile == null || !File.Exists(settingsFile))
        {
            settingsFile = Path.Combine(
                Path.GetDirectoryName(Application.ExecutablePath) ?? ".",
                "default.lbr");

            if (!File.Exists(settingsFile))
            {
                var defaultSettings = new LinkbarSettings(settingsFile);

                // Create default links directory
                var linksDir = Path.Combine(
                    Path.GetDirectoryName(Application.ExecutablePath) ?? ".", "links");
                Directory.CreateDirectory(linksDir);
                defaultSettings.DirLinks = linksDir;

                defaultSettings.Save();
            }
        }

        var settings = new LinkbarSettings(settingsFile);
        Application.Run(new LinkbarForm(settings));
    }
}
