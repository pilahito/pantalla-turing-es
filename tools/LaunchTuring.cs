using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Text.RegularExpressions;

internal static class Program
{
    private const string Root = @"E:\turing-smart-screen-python";

    private static bool IsAdmin()
    {
        WindowsIdentity id = WindowsIdentity.GetCurrent();
        WindowsPrincipal p = new WindowsPrincipal(id);
        return p.IsInRole(WindowsBuiltInRole.Administrator);
    }

    private static void KillByName(string name)
    {
        try
        {
            Process[] list = Process.GetProcessesByName(name);
            for (int i = 0; i < list.Length; i++)
            {
                try { list[i].Kill(); } catch { }
            }
        }
        catch { }
    }

    private static void KillPythonMain()
    {
        string[] names = new string[] { "python", "pythonw" };
        for (int n = 0; n < names.Length; n++)
        {
            Process[] list = Process.GetProcessesByName(names[n]);
            for (int i = 0; i < list.Length; i++)
            {
                try
                {
                    string fn = list[i].MainModule.FileName;
                    if (fn != null && fn.IndexOf("turing-smart-screen-python", StringComparison.OrdinalIgnoreCase) >= 0)
                        list[i].Kill();
                }
                catch { }
            }
        }
    }

    private static void SetTheme(string theme)
    {
        string cfg = Path.Combine(Root, "config.yaml");
        string text = File.ReadAllText(cfg);
        text = Regex.Replace(text, @"(?m)^(\s*THEME:\s*).+$", "${1}" + theme);
        text = Regex.Replace(text, @"(?m)^(\s*HW_SENSORS:\s*).+$", "${1}AUTO");
        File.WriteAllText(cfg, text);
    }

    [STAThread]
    private static void Main(string[] args)
    {
        string exe = "Turing-Iniciar";
        try
        {
            exe = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
        }
        catch { }

        Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        map["Turing-Iniciar"] = "";
        map["Turing-Admin"] = "";
        map["Turing-Horizonte"] = "HorizonES";
        map["Turing-NocheNeon"] = "NocheNeon";
        map["Turing-Terminal"] = "TerminalES";
        map["Turing-Cyberdeck"] = "Cyberdeck";
        map["Turing-6Celdas"] = "Landscape6Grid";
        map["Turing-Tierra"] = "LandscapeEarth";
        map["Turing-AzulMagico"] = "LandscapeMagicBlue";
        map["Turing-CyberArasaka"] = "CyberArasaka - Landscape";
        map["Turing-OnePiece"] = "OnePiece1";
        map["Turing-Reloj"] = "BigClock";
        map["Turing-Servidor"] = "LandscapeModernDevice35";
        map["Turing-Conil"] = "ConilES";
        map["Turing-Ember"] = "EmberES";
        map["Turing-Hielo"] = "HieloES";
        map["Turing-Atardecer"] = "AtardecerES";
        map["Turing-Violeta"] = "VioletaES";
        map["Turing-Minimal"] = "MinimalES";
        map["Turing-Circuito"] = "CircuitoES";
        map["Turing-Bosque"] = "BosqueES";
        map["Turing-Tareas"] = "AdminES";

        string theme = "";
        if (args != null && args.Length > 0)
            theme = args[0];
        else if (map.ContainsKey(exe))
            theme = map[exe];

        if (!IsAdmin())
        {
            ProcessStartInfo uac = new ProcessStartInfo();
            uac.FileName = Process.GetCurrentProcess().MainModule.FileName;
            uac.Verb = "runas";
            uac.UseShellExecute = true;
            if (!string.IsNullOrEmpty(theme))
                uac.Arguments = "\"" + theme + "\"";
            try { Process.Start(uac); }
            catch { }
            return;
        }

        KillByName("UsbPCMonitor");
        KillPythonMain();

        if (!string.IsNullOrEmpty(theme))
            SetTheme(theme);

        string pyw = Path.Combine(Root, @"venv\Scripts\pythonw.exe");
        if (!File.Exists(pyw))
            pyw = Path.Combine(Root, @"venv\Scripts\python.exe");

        ProcessStartInfo run = new ProcessStartInfo();
        run.FileName = pyw;
        run.Arguments = "main.py";
        run.WorkingDirectory = Root;
        run.UseShellExecute = false;
        run.CreateNoWindow = true;
        Process.Start(run);
    }
}
