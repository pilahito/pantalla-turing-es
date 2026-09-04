using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;

internal static class Program
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    private static bool IsAdmin()
    {
        WindowsIdentity id = WindowsIdentity.GetCurrent();
        WindowsPrincipal p = new WindowsPrincipal(id);
        return p.IsInRole(WindowsBuiltInRole.Administrator);
    }

    private static string FindRoot()
    {
        string[] candidates = new string[]
        {
            @"E:\turing-smart-screen-python",
            Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "turing-smart-screen-python"),
        };
        for (int i = 0; i < candidates.Length; i++)
        {
            if (string.IsNullOrEmpty(candidates[i]))
                continue;
            if (File.Exists(Path.Combine(candidates[i], "main.py"))
                && File.Exists(Path.Combine(candidates[i], "config.yaml")))
                return candidates[i];
        }
        return @"E:\turing-smart-screen-python";
    }

    [STAThread]
    private static void Main(string[] args)
    {
        string root = FindRoot();
        if (!File.Exists(Path.Combine(root, "main.py")))
        {
            MessageBox(IntPtr.Zero,
                "No encuentro el proyecto.\nDebe estar en:\nE:\\turing-smart-screen-python",
                "Pantalla Turing", 0x10);
            return;
        }

        string exe = "PantallaTuring";
        try
        {
            exe = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
        }
        catch { }

        Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        map["PantallaTuring"] = "ConilES";
        map["Pantalla Turing"] = "ConilES";
        map["Turing-Iniciar"] = "ConilES";
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
        map["Turing-Fallout"] = "Fallout_H";
        map["Turing-CyberpunkH"] = "Cyberpunk_H";
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

        string py = Path.Combine(root, @"venv\Scripts\python.exe");
        if (!File.Exists(py))
        {
            MessageBox(IntPtr.Zero, "No hay Python en:\n" + py + "\nEjecuta Instalar.ps1", "Pantalla Turing", 0x10);
            return;
        }

        string lanzar = Path.Combine(root, @"tools\lanzar.py");
        ProcessStartInfo run = new ProcessStartInfo();
        run.FileName = py;
        run.Arguments = string.IsNullOrEmpty(theme)
            ? "\"" + lanzar + "\""
            : "\"" + lanzar + "\" \"" + theme.Replace("\"", "") + "\"";
        run.WorkingDirectory = root;
        run.UseShellExecute = false;
        run.CreateNoWindow = true;
        try
        {
            Process p = Process.Start(run);
            if (p == null)
            {
                MessageBox(IntPtr.Zero, "No se pudo iniciar Python.", "Pantalla Turing", 0x10);
                return;
            }
            // El script mata la instancia anterior y arranca main.py
            p.WaitForExit(15000);
        }
        catch (Exception ex)
        {
            MessageBox(IntPtr.Zero, ex.Message, "Pantalla Turing", 0x10);
        }
    }
}
