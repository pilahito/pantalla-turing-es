using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PantallaTuring
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string root = Project.FindRoot();
            if (!File.Exists(Path.Combine(root, "main.py")))
            {
                MessageBox.Show(
                    "No encuentro el proyecto.\nDebe estar en:\nE:\\turing-smart-screen-python\n(o junto a este .exe)",
                    "Pantalla Turing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // CLI: PantallaTuring.exe [ThemeName]  — sin GUI (compatibilidad)
            if (args != null && args.Length > 0)
            {
                string theme = args[0].Trim().Trim('"');
                bool wantAdmin = theme.IndexOf("Admin", StringComparison.OrdinalIgnoreCase) >= 0
                    || string.Equals(theme, "AdminES", StringComparison.OrdinalIgnoreCase);
                if (wantAdmin && !Project.IsAdmin())
                {
                    Project.RelaunchElevated(theme);
                    return;
                }
                string err;
                if (!Project.LaunchMonitor(root, theme, out err))
                    MessageBox.Show(err, "Pantalla Turing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.Run(new MainForm(root));
        }
    }

    internal static class Project
    {
        public static string FindRoot()
        {
            string[] candidates = new string[]
            {
                @"E:\turing-smart-screen-python",
                Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "turing-smart-screen-python"),
            };
            for (int i = 0; i < candidates.Length; i++)
            {
                if (string.IsNullOrEmpty(candidates[i])) continue;
                if (File.Exists(Path.Combine(candidates[i], "main.py"))
                    && File.Exists(Path.Combine(candidates[i], "config.yaml")))
                    return candidates[i];
            }
            return @"E:\turing-smart-screen-python";
        }

        public static bool IsAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal p = new WindowsPrincipal(id);
            return p.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static void RelaunchElevated(string theme)
        {
            ProcessStartInfo uac = new ProcessStartInfo();
            uac.FileName = Process.GetCurrentProcess().MainModule.FileName;
            uac.Verb = "runas";
            uac.UseShellExecute = true;
            if (!string.IsNullOrEmpty(theme))
                uac.Arguments = "\"" + theme.Replace("\"", "") + "\"";
            try { Process.Start(uac); } catch { }
        }

        public static bool LaunchMonitor(string root, string theme, out string error)
        {
            error = null;
            string py = Path.Combine(root, @"venv\Scripts\python.exe");
            if (!File.Exists(py))
            {
                error = "No hay Python en:\n" + py + "\nEjecuta Instalar.ps1";
                return false;
            }
            string lanzar = Path.Combine(root, @"tools\lanzar.py");
            if (!File.Exists(lanzar))
            {
                error = "No encuentro tools\\lanzar.py";
                return false;
            }
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
                    error = "No se pudo iniciar Python.";
                    return false;
                }
                p.WaitForExit(15000);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public static void StopMonitor(string root)
        {
            string pidFile = Path.Combine(root, @"tmp\monitor.pid");
            if (File.Exists(pidFile))
            {
                try
                {
                    string s = File.ReadAllText(pidFile).Trim();
                    int pid;
                    if (int.TryParse(s, out pid) && pid > 0)
                    {
                        ProcessStartInfo tk = new ProcessStartInfo("taskkill", "/PID " + pid + " /F /T");
                        tk.CreateNoWindow = true;
                        tk.UseShellExecute = false;
                        Process.Start(tk).WaitForExit(5000);
                    }
                }
                catch { }
                try { File.Delete(pidFile); } catch { }
            }
            try
            {
                ProcessStartInfo tk2 = new ProcessStartInfo("taskkill", "/IM UsbPCMonitor.exe /F");
                tk2.CreateNoWindow = true;
                tk2.UseShellExecute = false;
                Process.Start(tk2).WaitForExit(3000);
            }
            catch { }
            // Mata main.py de este proyecto
            try
            {
                foreach (Process p in Process.GetProcessesByName("python"))
                {
                    try
                    {
                        string cmd = GetCommandLine(p);
                        if (cmd != null
                            && cmd.IndexOf("turing-smart-screen-python", StringComparison.OrdinalIgnoreCase) >= 0
                            && cmd.IndexOf("main.py", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            ProcessStartInfo tk = new ProcessStartInfo("taskkill", "/PID " + p.Id + " /F /T");
                            tk.CreateNoWindow = true;
                            tk.UseShellExecute = false;
                            Process.Start(tk).WaitForExit(3000);
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        // Fallback simple: no WMI dependency; Detener usa pidfile + taskkill UsbPCMonitor
        private static string GetCommandLine(Process p)
        {
            try { return p.MainModule != null ? p.MainModule.FileName : null; }
            catch { return null; }
        }

        public static Dictionary<string, string> ReadConfig(string root)
        {
            Dictionary<string, string> d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string path = Path.Combine(root, "config.yaml");
            if (!File.Exists(path)) return d;
            string[] lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                Match m = Regex.Match(line, @"^\s*(THEME|COM_PORT|DISPLAY_REVERSE)\s*:\s*(.+?)\s*$");
                if (m.Success)
                    d[m.Groups[1].Value] = m.Groups[2].Value.Trim().Trim('"', '\'');
            }
            return d;
        }

        public static bool WriteConfigKey(string root, string key, string value, out string error)
        {
            error = null;
            string path = Path.Combine(root, "config.yaml");
            if (!File.Exists(path))
            {
                error = "No existe config.yaml";
                return false;
            }
            string text = File.ReadAllText(path);
            string pattern = @"(?m)^(\s*" + Regex.Escape(key) + @"\s*:\s*).+$";
            if (!Regex.IsMatch(text, pattern))
            {
                error = "No se encontró " + key + " en config.yaml";
                return false;
            }
            text = Regex.Replace(text, pattern, "${1}" + value);
            File.WriteAllText(path, text);
            return true;
        }

        public static List<ThemeItem> DiscoverThemes(string root)
        {
            Dictionary<string, string> labels = FriendlyLabels();
            List<ThemeItem> curated = new List<ThemeItem>();
            string[] preferred = new string[]
            {
                "HorizonES","NocheNeon","TerminalES","ConilES","EmberES","HieloES","AtardecerES",
                "VioletaES","MinimalES","CircuitoES","BosqueES","AdminES",
                "3.5inchTheme2_H","SimpleBlue_H","SimpleOrange_H","SimpleGreen_H","Cyberpunk_H",
                "Fallout_H","Terminal_H","Cyberdeck","LandscapeModernDevice35","Landscape6Grid",
                "LandscapeEarth","LandscapeMagicBlue","CyberArasaka - Landscape","OnePiece1","BigClock"
            };
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string themesRoot = Path.Combine(root, @"res\themes");
            for (int i = 0; i < preferred.Length; i++)
            {
                string name = preferred[i];
                string yaml = Path.Combine(themesRoot, name, "theme.yaml");
                if (File.Exists(yaml))
                {
                    string label;
                    if (!labels.TryGetValue(name, out label)) label = name;
                    curated.Add(new ThemeItem(name, label));
                    seen.Add(name);
                }
            }
            if (Directory.Exists(themesRoot))
            {
                string[] dirs = Directory.GetDirectories(themesRoot);
                Array.Sort(dirs, StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < dirs.Length; i++)
                {
                    string name = Path.GetFileName(dirs[i]);
                    if (seen.Contains(name)) continue;
                    if (!File.Exists(Path.Combine(dirs[i], "theme.yaml"))) continue;
                    // Prefer landscape / _H / ES in "otros"
                    bool interesting = name.EndsWith("_H", StringComparison.OrdinalIgnoreCase)
                        || name.EndsWith("ES", StringComparison.OrdinalIgnoreCase)
                        || name.IndexOf("Landscape", StringComparison.OrdinalIgnoreCase) >= 0
                        || name.IndexOf("Fallout", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (!interesting) continue;
                    string label;
                    if (!labels.TryGetValue(name, out label)) label = name;
                    curated.Add(new ThemeItem(name, label + "  ·  otros"));
                }
            }
            return curated;
        }

        private static Dictionary<string, string> FriendlyLabels()
        {
            Dictionary<string, string> m = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            m["HorizonES"] = "HorizonES — ámbar/cyan ES";
            m["NocheNeon"] = "NocheNeon — ciudad lluvia";
            m["TerminalES"] = "TerminalES — verde fósforo";
            m["ConilES"] = "ConilES — playa Conil";
            m["EmberES"] = "EmberES — lava / fuego";
            m["HieloES"] = "HieloES — hielo";
            m["AtardecerES"] = "AtardecerES — atardecer";
            m["VioletaES"] = "VioletaES — violeta neón";
            m["MinimalES"] = "MinimalES — minimal oscuro";
            m["CircuitoES"] = "CircuitoES — circuito / racing";
            m["BosqueES"] = "BosqueES — bosque";
            m["AdminES"] = "AdminES — administrador de tareas";
            m["3.5inchTheme2_H"] = "Clásico 3.5\" horizontal";
            m["SimpleBlue_H"] = "Azul horizontal";
            m["SimpleOrange_H"] = "Naranja horizontal";
            m["SimpleGreen_H"] = "Verde horizontal";
            m["Cyberpunk_H"] = "Cyberpunk horizontal";
            m["Fallout_H"] = "Fallout horizontal";
            m["Terminal_H"] = "Terminal original horizontal";
            m["Cyberdeck"] = "Cyberdeck — relojes radiales";
            m["LandscapeModernDevice35"] = "Servidor — gráficas";
            m["Landscape6Grid"] = "6 celdas grandes";
            m["LandscapeEarth"] = "Tierra";
            m["LandscapeMagicBlue"] = "Azul mágico";
            m["CyberArasaka - Landscape"] = "Cyber Arasaka";
            m["OnePiece1"] = "One Piece";
            m["BigClock"] = "Reloj grande";
            return m;
        }
    }

    internal sealed class ThemeItem
    {
        public string Name;
        public string Label;
        public ThemeItem(string name, string label) { Name = name; Label = label; }
        public override string ToString() { return Label; }
    }

    internal sealed class MainForm : Form
    {
        private readonly string _root;
        private ComboBox _themes;
        private Label _status;
        private Label _com;
        private CheckBox _reverse;
        private Button _btnStart, _btnAdmin, _btnStop, _btnApply, _btnFolder;

        public MainForm(string root)
        {
            _root = root;
            Text = "Pantalla Turing";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(520, 360);
            BackColor = Color.FromArgb(24, 28, 36);
            ForeColor = Color.FromArgb(230, 236, 245);
            Font = new Font("Segoe UI", 10f);

            Label title = new Label();
            title.Text = "Pantalla Turing — lanzador";
            title.Font = new Font("Segoe UI Semibold", 16f);
            title.ForeColor = Color.FromArgb(80, 200, 255);
            title.Location = new Point(20, 16);
            title.AutoSize = true;
            Controls.Add(title);

            Label sub = new Label();
            sub.Text = "Un solo .exe para temas ES / horizontales";
            sub.ForeColor = Color.FromArgb(150, 165, 185);
            sub.Location = new Point(22, 48);
            sub.AutoSize = true;
            Controls.Add(sub);

            Label lt = new Label();
            lt.Text = "Tema";
            lt.Location = new Point(22, 88);
            lt.AutoSize = true;
            Controls.Add(lt);

            _themes = new ComboBox();
            _themes.DropDownStyle = ComboBoxStyle.DropDownList;
            _themes.Location = new Point(22, 112);
            _themes.Size = new Size(476, 28);
            _themes.BackColor = Color.FromArgb(36, 42, 54);
            _themes.ForeColor = Color.FromArgb(230, 236, 245);
            _themes.FlatStyle = FlatStyle.Flat;
            Controls.Add(_themes);

            _com = new Label();
            _com.Location = new Point(22, 152);
            _com.AutoSize = true;
            _com.ForeColor = Color.FromArgb(160, 180, 200);
            Controls.Add(_com);

            _reverse = new CheckBox();
            _reverse.Text = "Invertir pantalla (DISPLAY_REVERSE)";
            _reverse.Location = new Point(22, 178);
            _reverse.AutoSize = true;
            _reverse.FlatStyle = FlatStyle.Flat;
            Controls.Add(_reverse);

            _btnStart = MakeButton("Iniciar", 22, 220, Color.FromArgb(40, 160, 120));
            _btnAdmin = MakeButton("Iniciar como admin", 188, 220, Color.FromArgb(50, 120, 200));
            _btnStop = MakeButton("Detener", 370, 220, Color.FromArgb(180, 70, 70));
            _btnApply = MakeButton("Aplicar tema", 22, 268, Color.FromArgb(90, 100, 130));
            _btnFolder = MakeButton("Abrir carpeta", 188, 268, Color.FromArgb(90, 100, 130));

            _btnStart.Click += delegate { Start(false); };
            _btnAdmin.Click += delegate { Start(true); };
            _btnStop.Click += delegate
            {
                Project.StopMonitor(_root);
                SetStatus("Monitor detenido.");
            };
            _btnApply.Click += delegate { ApplyTheme(false); };
            _btnFolder.Click += delegate
            {
                try { Process.Start("explorer.exe", _root); } catch { }
            };

            _status = new Label();
            _status.Location = new Point(22, 318);
            _status.Size = new Size(476, 24);
            _status.ForeColor = Color.FromArgb(140, 200, 160);
            Controls.Add(_status);

            LoadThemesAndConfig();
        }

        private Button MakeButton(string text, int x, int y, Color bg)
        {
            Button b = new Button();
            b.Text = text;
            b.Location = new Point(x, y);
            b.Size = new Size(150, 36);
            b.BackColor = bg;
            b.ForeColor = Color.White;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.Cursor = Cursors.Hand;
            Controls.Add(b);
            return b;
        }

        private void LoadThemesAndConfig()
        {
            _themes.Items.Clear();
            List<ThemeItem> items = Project.DiscoverThemes(_root);
            for (int i = 0; i < items.Count; i++)
                _themes.Items.Add(items[i]);

            Dictionary<string, string> cfg = Project.ReadConfig(_root);
            string theme = cfg.ContainsKey("THEME") ? cfg["THEME"] : "";
            string com = cfg.ContainsKey("COM_PORT") ? cfg["COM_PORT"] : "?";
            string rev = cfg.ContainsKey("DISPLAY_REVERSE") ? cfg["DISPLAY_REVERSE"] : "false";
            _com.Text = "Puerto COM: " + com + "   |   Tema actual: " + theme;
            _reverse.Checked = string.Equals(rev, "true", StringComparison.OrdinalIgnoreCase);

            int idx = -1;
            for (int i = 0; i < _themes.Items.Count; i++)
            {
                ThemeItem ti = (ThemeItem)_themes.Items[i];
                if (string.Equals(ti.Name, theme, StringComparison.OrdinalIgnoreCase))
                {
                    idx = i;
                    break;
                }
            }
            if (idx >= 0) _themes.SelectedIndex = idx;
            else if (_themes.Items.Count > 0) _themes.SelectedIndex = 0;

            SetStatus("Listo. Elige tema e Iniciar (admin recomendado para temps reales).");
        }

        private string SelectedThemeName()
        {
            ThemeItem ti = _themes.SelectedItem as ThemeItem;
            return ti != null ? ti.Name : "";
        }

        private void ApplyTheme(bool andStart)
        {
            string theme = SelectedThemeName();
            if (string.IsNullOrEmpty(theme))
            {
                SetStatus("Selecciona un tema.");
                return;
            }
            string err;
            if (!Project.WriteConfigKey(_root, "THEME", theme, out err))
            {
                MessageBox.Show(err, "Pantalla Turing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string rev = _reverse.Checked ? "true" : "false";
            Project.WriteConfigKey(_root, "DISPLAY_REVERSE", rev, out err);
            Dictionary<string, string> cfg = Project.ReadConfig(_root);
            string com = cfg.ContainsKey("COM_PORT") ? cfg["COM_PORT"] : "?";
            _com.Text = "Puerto COM: " + com + "   |   Tema actual: " + theme;
            SetStatus("Tema aplicado: " + theme);
            if (andStart) { /* unused */ }
        }

        private void Start(bool asAdmin)
        {
            ApplyTheme(false);
            string theme = SelectedThemeName();
            if (asAdmin && !Project.IsAdmin())
            {
                Project.RelaunchElevated(theme);
                SetStatus("Solicitando UAC…");
                return;
            }
            string err;
            if (!Project.LaunchMonitor(_root, theme, out err))
                MessageBox.Show(err, "Pantalla Turing", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                SetStatus("Iniciado" + (asAdmin ? " (admin)" : "") + ": " + theme);
        }

        private void SetStatus(string s)
        {
            _status.Text = s;
        }
    }
}
