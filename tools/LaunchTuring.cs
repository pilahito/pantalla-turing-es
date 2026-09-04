using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;

internal static class Program
{
    private const string Root = @"E:\turing-smart-screen-python";

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    private static bool IsAdmin()
    {
        WindowsIdentity id = WindowsIdentity.GetCurrent();
        WindowsPrincipal p = new WindowsPrincipal(id);
        return p.IsInRole(WindowsBuiltInRole.Administrator);
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
        map["Turing-Clasico"] = "3.5inchTheme2_H";
        map["Turing-Azul"] = "SimpleBlue_H";
        map["Turing-Naranja"] = "SimpleOrange_H";
        map["Turing-Verde"] = "SimpleGreen_H";
        map["Turing-CyberpunkH"] = "Cyberpunk_H";
        map["Turing-Fallout"] = "Fallout_H";
        map["Turing-TermH"] = "Terminal_H";

        string theme = "";
        if (args != null && args.Length > 0)
            theme = args[0];
        else if (map.ContainsKey(exe))
            theme = map[exe];

        bool wantAdmin = exe.IndexOf("Admin", StringComparison.OrdinalIgnoreCase) >= 0
            || exe.IndexOf("Tareas", StringComparison.OrdinalIgnoreCase) >= 0;

        if (wantAdmin && !IsAdmin())
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

        string py = Path.Combine(Root, @"venv\Scripts\pythonw.exe");
        if (!File.Exists(py))
            py = Path.Combine(Root, @"venv\Scripts\python.exe");
        if (!File.Exists(py))
        {
            MessageBox(IntPtr.Zero, "No hay Python en venv.\\nEjecuta Instalar.ps1", "Pantalla Turing", 0x10);
            return;
        }

        ProcessStartInfo run = new ProcessStartInfo();
        run.FileName = py;
        run.Arguments = string.IsNullOrEmpty(theme)
            ? "tools\\lanzar.py"
            : "tools\\lanzar.py \"" + theme.Replace("\"", "") + "\"";
        run.WorkingDirectory = Root;
        run.UseShellExecute = false;
        run.CreateNoWindow = true;
        try
        {
            Process.Start(run);
        }
        catch (Exception ex)
        {
            MessageBox(IntPtr.Zero, ex.Message, "Pantalla Turing", 0x10);
        }
    }
}
