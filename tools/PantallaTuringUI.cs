using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Ports;
using System.Security.Principal;
using System.Text;
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

    internal static class UiColors
    {
        public static readonly Color Bg = Color.FromArgb(0x0a, 0x16, 0x28);
        public static readonly Color Panel = Color.FromArgb(0x0e, 0x1f, 0x36);
        public static readonly Color PanelAlt = Color.FromArgb(0x12, 0x28, 0x44);
        public static readonly Color Cyan = Color.FromArgb(0x00, 0xe5, 0xff);
        public static readonly Color CyanDim = Color.FromArgb(0x00, 0xa8, 0xbc);
        public static readonly Color Text = Color.FromArgb(0xe6, 0xf4, 0xff);
        public static readonly Color Muted = Color.FromArgb(0x7a, 0x9a, 0xb8);
        public static readonly Color Danger = Color.FromArgb(0xe0, 0x55, 0x55);
        public static readonly Color Ok = Color.FromArgb(0x2e, 0xc4, 0x9a);
        public static readonly Color Border = Color.FromArgb(0x1a, 0x3a, 0x58);
    }

    internal static class I18n
    {
        public static string Lang = "es";

        public static string T(string key)
        {
            string[,] pairs = new string[,]
            {
                {"title", "Pantalla Turing", "Turing Screen"},
                {"subtitle", "HUD cyberdeck · 3.5\" landscape", "Cyberdeck HUD · 3.5\" landscape"},
                {"tab_themes", "Temas", "Themes"},
                {"tab_config", "Configuración", "Settings"},
                {"tab_lang", "Idioma", "Language"},
                {"btn_start", "Iniciar", "Start"},
                {"btn_admin", "Iniciar admin", "Start admin"},
                {"btn_stop", "Detener", "Stop"},
                {"btn_save", "Guardar", "Save"},
                {"btn_classic", "Configurador clásico", "Classic configurator"},
                {"btn_folder", "Abrir carpeta", "Open folder"},
                {"btn_refresh", "Actualizar puertos", "Refresh ports"},
                {"theme_list", "Temas 3.5\" landscape (480×320)", "3.5\" landscape themes (480×320)"},
                {"theme_note", "Solo temas compatibles. Los que faltaban DISPLAY_SIZE se corrigen al listar si WIDTH/HEIGHT encajan.", "Compatible themes only. Missing DISPLAY_SIZE is fixed when WIDTH/HEIGHT match."},
                {"preview", "Vista previa", "Preview"},
                {"es_mark", "ES", "ES"},
                {"com", "Puerto COM", "COM port"},
                {"brightness", "Brillo", "Brightness"},
                {"reverse", "Invertir pantalla (DISPLAY_REVERSE)", "Reverse display (DISPLAY_REVERSE)"},
                {"hw", "Sensores HW", "HW sensors"},
                {"fan", "CPU_FAN", "CPU_FAN"},
                {"advanced", "Avanzado (clima opcional — no hace falta API)", "Advanced (optional weather — API not required)"},
                {"weather_key", "WEATHER_API_KEY (opcional)", "WEATHER_API_KEY (optional)"},
                {"weather_lat", "Latitud", "Latitude"},
                {"weather_lon", "Longitud", "Longitude"},
                {"weather_lang", "WEATHER_LANGUAGE", "WEATHER_LANGUAGE"},
                {"lang_ui", "Idioma de esta interfaz", "This UI language"},
                {"lang_hint", "No requiere claves API. WEATHER_LANGUAGE solo se escribe si usas clima.", "No API keys required. WEATHER_LANGUAGE is written only if you use weather."},
                {"status_ready", "Listo. Elige tema e Iniciar (admin recomendado para temps reales).", "Ready. Pick a theme and Start (admin recommended for real temps)."},
                {"status_saved", "Configuración guardada.", "Settings saved."},
                {"status_started", "Iniciado", "Started"},
                {"status_stopped", "Monitor detenido.", "Monitor stopped."},
                {"status_uac", "Solicitando UAC…", "Requesting UAC…"},
                {"pick_theme", "Selecciona un tema.", "Select a theme."},
                {"no_preview", "(sin preview.png)", "(no preview.png)"},
                {"filter_note", "Filtrado: DISPLAY_SIZE 3.5\" · landscape · 480×320", "Filter: DISPLAY_SIZE 3.5\" · landscape · 480×320"},
            };
            int col = string.Equals(Lang, "en", StringComparison.OrdinalIgnoreCase) ? 2 : 1;
            for (int i = 0; i < pairs.GetLength(0); i++)
                if (pairs[i, 0] == key) return pairs[i, col];
            return key;
        }
    }

    internal static class UiSettings
    {
        public static string PathFor(string root)
        {
            return System.IO.Path.Combine(root, "tools", "pantalla-turing-ui.json");
        }

        public static string LoadLang(string root)
        {
            try
            {
                string p = PathFor(root);
                if (!File.Exists(p)) return "es";
                string t = File.ReadAllText(p);
                Match m = Regex.Match(t, "\"lang\"\\s*:\\s*\"(es|en)\"", RegexOptions.IgnoreCase);
                if (m.Success) return m.Groups[1].Value.ToLowerInvariant();
            }
            catch { }
            return "es";
        }

        public static void SaveLang(string root, string lang)
        {
            string p = PathFor(root);
            string dir = System.IO.Path.GetDirectoryName(p);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(p, "{\n  \"lang\": \"" + (lang == "en" ? "en" : "es") + "\"\n}\n", Encoding.UTF8);
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
                Match m = Regex.Match(line,
                    @"^\s*(THEME|COM_PORT|DISPLAY_REVERSE|BRIGHTNESS|HW_SENSORS|CPU_FAN|WEATHER_API_KEY|WEATHER_LATITUDE|WEATHER_LONGITUDE|WEATHER_LANGUAGE)\s*:\s*(.+?)\s*$");
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

        public static string[] ListComPorts()
        {
            try { return SerialPort.GetPortNames(); }
            catch { return new string[0]; }
        }

        public static List<ThemeItem> DiscoverCompatibleThemes(string root)
        {
            Dictionary<string, string> labels = FriendlyLabels();
            List<ThemeItem> list = new List<ThemeItem>();
            string themesRoot = Path.Combine(root, "res", "themes");
            if (!Directory.Exists(themesRoot)) return list;

            string[] dirs = Directory.GetDirectories(themesRoot);
            Array.Sort(dirs, StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < dirs.Length; i++)
            {
                string name = Path.GetFileName(dirs[i]);
                string yaml = Path.Combine(dirs[i], "theme.yaml");
                if (!File.Exists(yaml)) continue;

                ThemeMeta meta = ParseThemeMeta(yaml);
                // Prefer fixing missing DISPLAY_SIZE when already 480x320 landscape
                if (string.IsNullOrEmpty(meta.DisplaySize)
                    && meta.Width == 480 && meta.Height == 320
                    && (string.IsNullOrEmpty(meta.Orientation)
                        || string.Equals(meta.Orientation, "landscape", StringComparison.OrdinalIgnoreCase)))
                {
                    TryFixDisplaySize(yaml);
                    meta.DisplaySize = "3.5\"";
                    if (string.IsNullOrEmpty(meta.Orientation)) meta.Orientation = "landscape";
                }

                if (!IsCompatible35Landscape(meta)) continue;

                string label;
                if (!labels.TryGetValue(name, out label))
                    label = name;
                bool es = name.EndsWith("ES", StringComparison.OrdinalIgnoreCase)
                    || name.IndexOf("ES", StringComparison.OrdinalIgnoreCase) >= 0 && Regex.IsMatch(name, "ES$");
                if (name.EndsWith("ES", StringComparison.OrdinalIgnoreCase))
                    label = "[ES] " + label;
                string preview = Path.Combine(dirs[i], "preview.png");
                list.Add(new ThemeItem(name, label, File.Exists(preview) ? preview : null, name.EndsWith("ES", StringComparison.OrdinalIgnoreCase)));
            }

            // Prefer curated order: ES first, then known names
            list.Sort(delegate(ThemeItem a, ThemeItem b)
            {
                int ae = a.IsEs ? 0 : 1;
                int be = b.IsEs ? 0 : 1;
                if (ae != be) return ae.CompareTo(be);
                return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            });
            return list;
        }

        private static bool IsCompatible35Landscape(ThemeMeta m)
        {
            if (m.Width != 480 || m.Height != 320) return false;
            if (!string.IsNullOrEmpty(m.Orientation)
                && !string.Equals(m.Orientation, "landscape", StringComparison.OrdinalIgnoreCase))
                return false;
            if (!string.IsNullOrEmpty(m.DisplaySize))
            {
                string ds = m.DisplaySize.Replace(" ", "");
                if (ds.IndexOf("3.5", StringComparison.OrdinalIgnoreCase) < 0) return false;
            }
            return true;
        }

        private static void TryFixDisplaySize(string yamlPath)
        {
            try
            {
                string text = File.ReadAllText(yamlPath);
                if (Regex.IsMatch(text, @"(?m)^\s*DISPLAY_SIZE\s*:")) return;
                // Insert DISPLAY_SIZE under display: block
                Match dm = Regex.Match(text, @"(?m)^(display:\s*\r?\n)");
                if (dm.Success)
                {
                    string insert = dm.Groups[1].Value + "  DISPLAY_SIZE: 3.5\"\n";
                    text = text.Substring(0, dm.Index) + insert + text.Substring(dm.Index + dm.Length);
                    File.WriteAllText(yamlPath, text);
                }
            }
            catch { }
        }

        private static ThemeMeta ParseThemeMeta(string yamlPath)
        {
            ThemeMeta m = new ThemeMeta();
            try
            {
                string text = File.ReadAllText(yamlPath);
                Match ds = Regex.Match(text, @"(?m)^\s*DISPLAY_SIZE\s*:\s*(.+?)\s*$");
                if (ds.Success) m.DisplaySize = ds.Groups[1].Value.Trim().Trim('"', '\'');
                Match ori = Regex.Match(text, @"(?m)^\s*DISPLAY_ORIENTATION\s*:\s*(\w+)");
                if (ori.Success) m.Orientation = ori.Groups[1].Value.Trim();
                Match w = Regex.Match(text, @"(?m)^\s*WIDTH\s*:\s*(\d+)");
                Match h = Regex.Match(text, @"(?m)^\s*HEIGHT\s*:\s*(\d+)");
                if (w.Success) int.TryParse(w.Groups[1].Value, out m.Width);
                if (h.Success) int.TryParse(h.Groups[1].Value, out m.Height);
            }
            catch { }
            return m;
        }

        private static Dictionary<string, string> FriendlyLabels()
        {
            Dictionary<string, string> m = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            m["HorizonES"] = "HorizonES — horizonte";
            m["NocheNeon"] = "NocheNeon — neón nocturno";
            m["TerminalES"] = "TerminalES — terminal";
            m["ConilES"] = "ConilES — playa / Conil";
            m["EmberES"] = "EmberES — ember / fuego";
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
            m["MonodarkSimpleLandscape"] = "Monodark simple";
            return m;
        }

        public static void OpenClassicConfigurator(string root)
        {
            string py = Path.Combine(root, @"venv\Scripts\python.exe");
            string cfg = Path.Combine(root, "configure.py");
            if (!File.Exists(py) || !File.Exists(cfg))
                throw new Exception("Falta venv o configure.py");
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = py;
            psi.Arguments = "\"" + cfg + "\"";
            psi.WorkingDirectory = root;
            psi.UseShellExecute = false;
            Process.Start(psi);
        }
    }

    internal sealed class ThemeMeta
    {
        public string DisplaySize;
        public string Orientation;
        public int Width;
        public int Height;
    }

    internal sealed class ThemeItem
    {
        public string Name;
        public string Label;
        public string PreviewPath;
        public bool IsEs;
        public ThemeItem(string name, string label, string preview, bool isEs)
        {
            Name = name; Label = label; PreviewPath = preview; IsEs = isEs;
        }
        public override string ToString() { return Label; }
    }

    internal sealed class HudTabControl : TabControl
    {
        public HudTabControl()
        {
            DrawMode = TabDrawMode.OwnerDrawFixed;
            SizeMode = TabSizeMode.Fixed;
            ItemSize = new Size(140, 32);
            BackColor = UiColors.Bg;
            Padding = new Point(12, 6);
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = GetTabRect(e.Index);
            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            using (SolidBrush b = new SolidBrush(selected ? UiColors.PanelAlt : UiColors.Panel))
                g.FillRectangle(b, r);
            if (selected)
            {
                using (Pen p = new Pen(UiColors.Cyan, 2))
                    g.DrawLine(p, r.Left + 8, r.Bottom - 2, r.Right - 8, r.Bottom - 2);
            }
            string text = TabPages[e.Index].Text;
            TextRenderer.DrawText(g, text, Font, r, selected ? UiColors.Cyan : UiColors.Muted,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    internal sealed class MainForm : Form
    {
        private readonly string _root;
        private HudTabControl _tabs;
        private ListBox _themes;
        private PictureBox _preview;
        private Label _status, _filterNote, _previewLbl;
        private ComboBox _com;
        private NumericUpDown _brightness;
        private CheckBox _reverse;
        private ComboBox _hw, _fan;
        private Panel _advPanel;
        private CheckBox _advToggle;
        private TextBox _weatherKey, _weatherLat, _weatherLon, _weatherLang;
        private RadioButton _langEs, _langEn;
        private Button _btnStart, _btnAdmin, _btnStop, _btnSave, _btnClassic, _btnFolder, _btnRefreshPorts;
        private Label _lblThemeList, _lblCom, _lblBright, _lblHw, _lblFan, _lblLang, _lblLangHint;
        private Label _lblWKey, _lblWLat, _lblWLon, _lblWLang;
        private Image _previewImage;

        public MainForm(string root)
        {
            _root = root;
            I18n.Lang = UiSettings.LoadLang(root);

            Text = "Pantalla Turing";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(760, 560);
            BackColor = UiColors.Bg;
            ForeColor = UiColors.Text;
            Font = new Font("Segoe UI", 9.5f);
            DoubleBuffered = true;

            try
            {
                string ico = Path.Combine(root, @"res\icons\monitor-icon-17865\icon.ico");
                if (File.Exists(ico)) Icon = new Icon(ico);
            }
            catch { }

            BuildChrome();
            BuildTabs();
            BuildButtons();
            ApplyI18n();
            LoadAll();
        }

        private void BuildChrome()
        {
            Panel top = new Panel();
            top.Location = new Point(0, 0);
            top.Size = new Size(760, 72);
            top.BackColor = UiColors.Panel;
            Controls.Add(top);

            Label accent = new Label();
            accent.BackColor = UiColors.Cyan;
            accent.Location = new Point(0, 0);
            accent.Size = new Size(4, 72);
            top.Controls.Add(accent);

            Label title = new Label();
            title.Name = "title";
            title.Text = I18n.T("title");
            title.Font = new Font("Segoe UI Semibold", 18f);
            title.ForeColor = UiColors.Cyan;
            title.Location = new Point(20, 12);
            title.AutoSize = true;
            top.Controls.Add(title);

            Label sub = new Label();
            sub.Name = "subtitle";
            sub.Text = I18n.T("subtitle");
            sub.ForeColor = UiColors.Muted;
            sub.Location = new Point(22, 44);
            sub.AutoSize = true;
            top.Controls.Add(sub);
        }

        private void BuildTabs()
        {
            _tabs = new HudTabControl();
            _tabs.Location = new Point(16, 84);
            _tabs.Size = new Size(728, 390);
            _tabs.Font = new Font("Segoe UI Semibold", 9.5f);
            Controls.Add(_tabs);

            TabPage tpThemes = new TabPage();
            tpThemes.Name = "tab_themes";
            tpThemes.BackColor = UiColors.Bg;
            tpThemes.ForeColor = UiColors.Text;
            _tabs.TabPages.Add(tpThemes);
            BuildThemesTab(tpThemes);

            TabPage tpCfg = new TabPage();
            tpCfg.Name = "tab_config";
            tpCfg.BackColor = UiColors.Bg;
            tpCfg.ForeColor = UiColors.Text;
            _tabs.TabPages.Add(tpCfg);
            BuildConfigTab(tpCfg);

            TabPage tpLang = new TabPage();
            tpLang.Name = "tab_lang";
            tpLang.BackColor = UiColors.Bg;
            tpLang.ForeColor = UiColors.Text;
            _tabs.TabPages.Add(tpLang);
            BuildLangTab(tpLang);
        }

        private void BuildThemesTab(TabPage page)
        {
            _lblThemeList = new Label();
            _lblThemeList.Location = new Point(12, 10);
            _lblThemeList.AutoSize = true;
            _lblThemeList.ForeColor = UiColors.CyanDim;
            page.Controls.Add(_lblThemeList);

            _themes = new ListBox();
            _themes.Location = new Point(12, 34);
            _themes.Size = new Size(380, 280);
            _themes.BackColor = UiColors.Panel;
            _themes.ForeColor = UiColors.Text;
            _themes.BorderStyle = BorderStyle.FixedSingle;
            _themes.IntegralHeight = false;
            _themes.SelectedIndexChanged += delegate { ShowPreview(); };
            page.Controls.Add(_themes);

            _previewLbl = new Label();
            _previewLbl.Location = new Point(410, 10);
            _previewLbl.AutoSize = true;
            _previewLbl.ForeColor = UiColors.CyanDim;
            page.Controls.Add(_previewLbl);

            _preview = new PictureBox();
            _preview.Location = new Point(410, 34);
            _preview.Size = new Size(290, 200);
            _preview.BackColor = UiColors.Panel;
            _preview.SizeMode = PictureBoxSizeMode.Zoom;
            _preview.BorderStyle = BorderStyle.FixedSingle;
            page.Controls.Add(_preview);

            _filterNote = new Label();
            _filterNote.Location = new Point(410, 250);
            _filterNote.Size = new Size(290, 60);
            _filterNote.ForeColor = UiColors.Muted;
            page.Controls.Add(_filterNote);
        }

        private void BuildConfigTab(TabPage page)
        {
            int y = 16;
            _lblCom = MakeLbl(page, 16, y);
            y += 22;
            _com = new ComboBox();
            _com.DropDownStyle = ComboBoxStyle.DropDown;
            _com.Location = new Point(16, y);
            _com.Size = new Size(200, 28);
            StyleCombo(_com);
            page.Controls.Add(_com);

            _btnRefreshPorts = MakeBtn(page, "…", 228, y - 2, 120, 30, UiColors.PanelAlt);
            _btnRefreshPorts.Click += delegate { RefreshPorts(); };

            y += 40;
            _lblBright = MakeLbl(page, 16, y);
            y += 22;
            _brightness = new NumericUpDown();
            _brightness.Minimum = 0;
            _brightness.Maximum = 100;
            _brightness.Location = new Point(16, y);
            _brightness.Size = new Size(100, 28);
            _brightness.BackColor = UiColors.Panel;
            _brightness.ForeColor = UiColors.Text;
            page.Controls.Add(_brightness);

            y += 40;
            _reverse = new CheckBox();
            _reverse.Location = new Point(16, y);
            _reverse.AutoSize = true;
            _reverse.ForeColor = UiColors.Text;
            _reverse.FlatStyle = FlatStyle.Flat;
            page.Controls.Add(_reverse);

            y += 36;
            _lblHw = MakeLbl(page, 16, y);
            y += 22;
            _hw = new ComboBox();
            _hw.DropDownStyle = ComboBoxStyle.DropDownList;
            _hw.Location = new Point(16, y);
            _hw.Size = new Size(200, 28);
            StyleCombo(_hw);
            _hw.Items.AddRange(new object[] { "AUTO", "PYTHON", "LHM" });
            page.Controls.Add(_hw);

            y += 40;
            _lblFan = MakeLbl(page, 16, y);
            y += 22;
            _fan = new ComboBox();
            _fan.DropDownStyle = ComboBoxStyle.DropDown;
            _fan.Location = new Point(16, y);
            _fan.Size = new Size(200, 28);
            StyleCombo(_fan);
            _fan.Items.Add("AUTO");
            page.Controls.Add(_fan);

            y += 44;
            _advToggle = new CheckBox();
            _advToggle.Location = new Point(16, y);
            _advToggle.AutoSize = true;
            _advToggle.ForeColor = UiColors.CyanDim;
            _advToggle.FlatStyle = FlatStyle.Flat;
            _advToggle.CheckedChanged += delegate { _advPanel.Visible = _advToggle.Checked; };
            page.Controls.Add(_advToggle);

            y += 28;
            _advPanel = new Panel();
            _advPanel.Location = new Point(16, y);
            _advPanel.Size = new Size(680, 110);
            _advPanel.BackColor = UiColors.Panel;
            _advPanel.Visible = false;
            page.Controls.Add(_advPanel);

            _lblWKey = MakeLbl(_advPanel, 10, 8);
            _weatherKey = MakeTb(_advPanel, 10, 28, 320);
            _weatherKey.UseSystemPasswordChar = true;
            _lblWLat = MakeLbl(_advPanel, 350, 8);
            _weatherLat = MakeTb(_advPanel, 350, 28, 120);
            _lblWLon = MakeLbl(_advPanel, 490, 8);
            _weatherLon = MakeTb(_advPanel, 490, 28, 120);
            _lblWLang = MakeLbl(_advPanel, 10, 58);
            _weatherLang = MakeTb(_advPanel, 10, 78, 120);
        }

        private void BuildLangTab(TabPage page)
        {
            _lblLang = MakeLbl(page, 16, 20);
            _langEs = new RadioButton();
            _langEs.Text = "Español (ES)";
            _langEs.Location = new Point(16, 50);
            _langEs.AutoSize = true;
            _langEs.ForeColor = UiColors.Text;
            _langEs.FlatStyle = FlatStyle.Flat;
            page.Controls.Add(_langEs);

            _langEn = new RadioButton();
            _langEn.Text = "English (EN)";
            _langEn.Location = new Point(16, 80);
            _langEn.AutoSize = true;
            _langEn.ForeColor = UiColors.Text;
            _langEn.FlatStyle = FlatStyle.Flat;
            page.Controls.Add(_langEn);

            _lblLangHint = new Label();
            _lblLangHint.Location = new Point(16, 120);
            _lblLangHint.Size = new Size(680, 60);
            _lblLangHint.ForeColor = UiColors.Muted;
            page.Controls.Add(_lblLangHint);

            EventHandler langChange = delegate
            {
                I18n.Lang = _langEs.Checked ? "es" : "en";
                UiSettings.SaveLang(_root, I18n.Lang);
                ApplyI18n();
            };
            _langEs.CheckedChanged += langChange;
            _langEn.CheckedChanged += langChange;
        }

        private void BuildButtons()
        {
            int y = 488;
            _btnStart = MakeBtn(this, "", 16, y, 120, 36, Color.FromArgb(0x14, 0x6b, 0x5a));
            _btnAdmin = MakeBtn(this, "", 144, y, 130, 36, Color.FromArgb(0x1a, 0x4a, 0x7a));
            _btnStop = MakeBtn(this, "", 282, y, 110, 36, UiColors.Danger);
            _btnSave = MakeBtn(this, "", 400, y, 110, 36, UiColors.CyanDim);
            _btnClassic = MakeBtn(this, "", 518, y, 160, 36, UiColors.PanelAlt);
            _btnFolder = MakeBtn(this, "", 686, y, 58, 36, UiColors.PanelAlt);

            _btnStart.Click += delegate { Start(false); };
            _btnAdmin.Click += delegate { Start(true); };
            _btnStop.Click += delegate
            {
                Project.StopMonitor(_root);
                SetStatus(I18n.T("status_stopped"));
            };
            _btnSave.Click += delegate { SaveAll(true); };
            _btnClassic.Click += delegate
            {
                try { Project.OpenClassicConfigurator(_root); }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Pantalla Turing", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            };
            _btnFolder.Click += delegate
            {
                try { Process.Start("explorer.exe", _root); } catch { }
            };

            _status = new Label();
            _status.Location = new Point(16, 530);
            _status.Size = new Size(728, 22);
            _status.ForeColor = UiColors.Ok;
            Controls.Add(_status);
        }

        private Label MakeLbl(Control parent, int x, int y)
        {
            Label l = new Label();
            l.Location = new Point(x, y);
            l.AutoSize = true;
            l.ForeColor = UiColors.Muted;
            parent.Controls.Add(l);
            return l;
        }

        private TextBox MakeTb(Control parent, int x, int y, int w)
        {
            TextBox t = new TextBox();
            t.Location = new Point(x, y);
            t.Size = new Size(w, 24);
            t.BackColor = UiColors.PanelAlt;
            t.ForeColor = UiColors.Text;
            t.BorderStyle = BorderStyle.FixedSingle;
            parent.Controls.Add(t);
            return t;
        }

        private Button MakeBtn(Control parent, string text, int x, int y, int w, int h, Color bg)
        {
            Button b = new Button();
            b.Text = text;
            b.Location = new Point(x, y);
            b.Size = new Size(w, h);
            b.BackColor = bg;
            b.ForeColor = Color.White;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderColor = UiColors.Border;
            b.FlatAppearance.BorderSize = 1;
            b.Cursor = Cursors.Hand;
            parent.Controls.Add(b);
            return b;
        }

        private void StyleCombo(ComboBox c)
        {
            c.BackColor = UiColors.Panel;
            c.ForeColor = UiColors.Text;
            c.FlatStyle = FlatStyle.Flat;
        }

        private void ApplyI18n()
        {
            foreach (Control c in Controls)
            {
                if (c.Name == "title") c.Text = I18n.T("title");
                if (c.Name == "subtitle") c.Text = I18n.T("subtitle");
            }
            if (_tabs.TabPages.Count >= 3)
            {
                _tabs.TabPages[0].Text = I18n.T("tab_themes");
                _tabs.TabPages[1].Text = I18n.T("tab_config");
                _tabs.TabPages[2].Text = I18n.T("tab_lang");
            }
            _btnStart.Text = I18n.T("btn_start");
            _btnAdmin.Text = I18n.T("btn_admin");
            _btnStop.Text = I18n.T("btn_stop");
            _btnSave.Text = I18n.T("btn_save");
            _btnClassic.Text = I18n.T("btn_classic");
            _btnFolder.Text = "📁";
            _btnRefreshPorts.Text = I18n.T("btn_refresh");
            _lblThemeList.Text = I18n.T("theme_list");
            _previewLbl.Text = I18n.T("preview");
            _filterNote.Text = I18n.T("filter_note") + "\n" + I18n.T("theme_note");
            _lblCom.Text = I18n.T("com");
            _lblBright.Text = I18n.T("brightness");
            _reverse.Text = I18n.T("reverse");
            _lblHw.Text = I18n.T("hw");
            _lblFan.Text = I18n.T("fan");
            _advToggle.Text = I18n.T("advanced");
            _lblWKey.Text = I18n.T("weather_key");
            _lblWLat.Text = I18n.T("weather_lat");
            _lblWLon.Text = I18n.T("weather_lon");
            _lblWLang.Text = I18n.T("weather_lang");
            _lblLang.Text = I18n.T("lang_ui");
            _lblLangHint.Text = I18n.T("lang_hint");
            _tabs.Invalidate();
        }

        private void LoadAll()
        {
            _themes.Items.Clear();
            List<ThemeItem> items = Project.DiscoverCompatibleThemes(_root);
            for (int i = 0; i < items.Count; i++)
                _themes.Items.Add(items[i]);

            Dictionary<string, string> cfg = Project.ReadConfig(_root);
            string theme = cfg.ContainsKey("THEME") ? cfg["THEME"] : "";
            RefreshPorts();
            if (cfg.ContainsKey("COM_PORT"))
            {
                string com = cfg["COM_PORT"];
                int ix = _com.FindStringExact(com);
                if (ix >= 0) _com.SelectedIndex = ix;
                else _com.Text = com;
            }

            int bright = 25;
            if (cfg.ContainsKey("BRIGHTNESS")) int.TryParse(cfg["BRIGHTNESS"], out bright);
            if (bright < 0) bright = 0;
            if (bright > 100) bright = 100;
            _brightness.Value = bright;

            string rev = cfg.ContainsKey("DISPLAY_REVERSE") ? cfg["DISPLAY_REVERSE"] : "false";
            _reverse.Checked = string.Equals(rev, "true", StringComparison.OrdinalIgnoreCase);

            string hw = cfg.ContainsKey("HW_SENSORS") ? cfg["HW_SENSORS"] : "AUTO";
            int hwi = _hw.FindStringExact(hw.ToUpperInvariant());
            _hw.SelectedIndex = hwi >= 0 ? hwi : 0;

            string fan = cfg.ContainsKey("CPU_FAN") ? cfg["CPU_FAN"] : "AUTO";
            _fan.Text = fan;

            // Weather fields optional — prefill but advanced stays collapsed
            _weatherKey.Text = cfg.ContainsKey("WEATHER_API_KEY") ? cfg["WEATHER_API_KEY"] : "";
            _weatherLat.Text = cfg.ContainsKey("WEATHER_LATITUDE") ? cfg["WEATHER_LATITUDE"] : "";
            _weatherLon.Text = cfg.ContainsKey("WEATHER_LONGITUDE") ? cfg["WEATHER_LONGITUDE"] : "";
            _weatherLang.Text = cfg.ContainsKey("WEATHER_LANGUAGE") ? cfg["WEATHER_LANGUAGE"] : (I18n.Lang == "en" ? "en" : "es");

            if (string.Equals(I18n.Lang, "en", StringComparison.OrdinalIgnoreCase))
                _langEn.Checked = true;
            else
                _langEs.Checked = true;

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

            SetStatus(I18n.T("status_ready") + "  [" + _themes.Items.Count + " temas]");
        }

        private void RefreshPorts()
        {
            string cur = _com.Text;
            _com.Items.Clear();
            string[] ports = Project.ListComPorts();
            for (int i = 0; i < ports.Length; i++)
                _com.Items.Add(ports[i]);
            if (!string.IsNullOrEmpty(cur))
            {
                int ix = _com.FindStringExact(cur);
                if (ix >= 0) _com.SelectedIndex = ix;
                else _com.Text = cur;
            }
            else if (_com.Items.Count > 0)
                _com.SelectedIndex = 0;
        }

        private void ShowPreview()
        {
            ThemeItem ti = _themes.SelectedItem as ThemeItem;
            if (_previewImage != null)
            {
                _preview.Image = null;
                _previewImage.Dispose();
                _previewImage = null;
            }
            if (ti == null || string.IsNullOrEmpty(ti.PreviewPath) || !File.Exists(ti.PreviewPath))
            {
                _preview.Image = null;
                _previewLbl.Text = I18n.T("preview") + " " + I18n.T("no_preview");
                return;
            }
            try
            {
                using (FileStream fs = new FileStream(ti.PreviewPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    _previewImage = Image.FromStream(fs);
                }
                _preview.Image = _previewImage;
                _previewLbl.Text = I18n.T("preview") + " — " + ti.Name;
            }
            catch
            {
                _preview.Image = null;
                _previewLbl.Text = I18n.T("preview") + " " + I18n.T("no_preview");
            }
        }

        private string SelectedThemeName()
        {
            ThemeItem ti = _themes.SelectedItem as ThemeItem;
            return ti != null ? ti.Name : "";
        }

        private void SaveAll(bool showStatus)
        {
            string theme = SelectedThemeName();
            string err;
            if (!string.IsNullOrEmpty(theme))
                Project.WriteConfigKey(_root, "THEME", theme, out err);

            string com = (_com.Text ?? "").Trim();
            if (!string.IsNullOrEmpty(com))
                Project.WriteConfigKey(_root, "COM_PORT", com, out err);

            Project.WriteConfigKey(_root, "BRIGHTNESS", ((int)_brightness.Value).ToString(), out err);
            Project.WriteConfigKey(_root, "DISPLAY_REVERSE", _reverse.Checked ? "true" : "false", out err);

            string hw = _hw.SelectedItem != null ? _hw.SelectedItem.ToString() : "AUTO";
            Project.WriteConfigKey(_root, "HW_SENSORS", hw, out err);

            string fan = (_fan.Text ?? "AUTO").Trim();
            if (string.IsNullOrEmpty(fan)) fan = "AUTO";
            Project.WriteConfigKey(_root, "CPU_FAN", fan, out err);

            // Weather only when advanced is open OR fields non-empty and user opted in
            if (_advToggle.Checked)
            {
                string key = (_weatherKey.Text ?? "").Trim();
                // Never wipe an existing key with empty unless user cleared while advanced open
                Project.WriteConfigKey(_root, "WEATHER_API_KEY", key, out err);
                string lat = (_weatherLat.Text ?? "").Trim();
                string lon = (_weatherLon.Text ?? "").Trim();
                if (!string.IsNullOrEmpty(lat)) Project.WriteConfigKey(_root, "WEATHER_LATITUDE", lat, out err);
                if (!string.IsNullOrEmpty(lon)) Project.WriteConfigKey(_root, "WEATHER_LONGITUDE", lon, out err);
                string wl = (_weatherLang.Text ?? "").Trim();
                if (!string.IsNullOrEmpty(wl)) Project.WriteConfigKey(_root, "WEATHER_LANGUAGE", wl, out err);
            }

            UiSettings.SaveLang(_root, I18n.Lang);
            if (showStatus) SetStatus(I18n.T("status_saved"));
        }

        private void Start(bool asAdmin)
        {
            string theme = SelectedThemeName();
            if (string.IsNullOrEmpty(theme))
            {
                SetStatus(I18n.T("pick_theme"));
                return;
            }
            SaveAll(false);
            if (asAdmin && !Project.IsAdmin())
            {
                Project.RelaunchElevated(theme);
                SetStatus(I18n.T("status_uac"));
                return;
            }
            string err;
            if (!Project.LaunchMonitor(_root, theme, out err))
                MessageBox.Show(err, "Pantalla Turing", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                SetStatus(I18n.T("status_started") + (asAdmin ? " (admin)" : "") + ": " + theme);
        }

        private void SetStatus(string s)
        {
            _status.Text = s;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (_previewImage != null)
            {
                _preview.Image = null;
                _previewImage.Dispose();
                _previewImage = null;
            }
            base.OnFormClosed(e);
        }
    }
}
