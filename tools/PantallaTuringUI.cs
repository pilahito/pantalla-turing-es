using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Reflection;
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
        public static readonly Color CyanHover = Color.FromArgb(0x33, 0xec, 0xff);
        public static readonly Color Text = Color.FromArgb(0xe6, 0xf4, 0xff);
        public static readonly Color Muted = Color.FromArgb(0x7a, 0x9a, 0xb8);
        public static readonly Color Danger = Color.FromArgb(0xe0, 0x55, 0x55);
        public static readonly Color DangerDim = Color.FromArgb(0x8a, 0x2a, 0x2a);
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
                {"search", "Buscar tema...", "Search theme..."},
                {"themes_count", "temas", "themes"},
                {"grp_port", "Puerto", "Port"},
                {"grp_display", "Pantalla", "Display"},
                {"grp_sensors", "Sensores", "Sensors"},
                {"lang_es_card", "Espanol", "Spanish"},
                {"lang_en_card", "English", "English"},
                {"lang_es_sub", "Interfaz y textos en espanol", "UI and labels in Spanish"},
                {"lang_en_sub", "UI and labels in English", "UI and labels in English"},
                {"dblclick_hint", "Doble clic = aplicar e iniciar", "Double-click = apply & start"},
                {"grp_weather", "Clima", "Weather"},
                {"detect_loc", "Detectar mi ubicacion", "Detect my location"},
                {"auto_weather", "Usar clima automatico (sin API / Open-Meteo)", "Use automatic weather (no API / Open-Meteo)"},
                {"status_loc_ok", "Ubicacion detectada", "Location detected"},
                {"status_loc_fail", "No se pudo detectar la ubicacion", "Could not detect location"},
                {"weather_hint", "Lat/lon opcionales. Sin API key se usa Open-Meteo.", "Optional lat/lon. Without API key uses Open-Meteo."},
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


    internal static class UiHelper
    {
        public static void EnableDoubleBuffer(Control c)
        {
            try
            {
                PropertyInfo pi = typeof(Control).GetProperty("DoubleBuffered",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (pi != null) pi.SetValue(c, true, null);
            }
            catch { }
        }
    }

    internal sealed class HudButton : Button
    {
        private bool _hover;
        private bool _primary;
        private bool _danger;
        private bool _outline;

        public HudButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.MouseOverBackColor = Color.Transparent;
            FlatAppearance.MouseDownBackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            ForeColor = Color.White;
            Font = new Font("Segoe UI Semibold", 9.5f);
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        public void SetPrimary() { _primary = true; _danger = false; _outline = false; Invalidate(); }
        public void SetOutline() { _primary = false; _danger = false; _outline = true; Invalidate(); }
        public void SetDanger() { _primary = false; _danger = true; _outline = false; Invalidate(); }
        public void SetSecondary() { _primary = false; _danger = false; _outline = false; Invalidate(); }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hover = true; Invalidate(); base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hover = false; Invalidate(); base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = ClientRectangle;
            r.Width -= 1; r.Height -= 1;

            Color fill, border, text = UiColors.Text;
            if (_primary)
            {
                fill = _hover ? UiColors.CyanHover : UiColors.Cyan;
                border = fill;
                text = Color.FromArgb(0x0a, 0x16, 0x28);
            }
            else if (_danger)
            {
                fill = _hover ? Color.FromArgb(0xc0, 0x40, 0x40) : UiColors.DangerDim;
                border = UiColors.Danger;
                text = Color.White;
            }
            else if (_outline)
            {
                fill = _hover ? UiColors.PanelAlt : UiColors.Panel;
                border = _hover ? UiColors.CyanHover : UiColors.CyanDim;
                text = UiColors.Cyan;
            }
            else
            {
                fill = _hover ? UiColors.PanelAlt : UiColors.Panel;
                border = UiColors.Border;
                text = UiColors.Text;
            }

            using (SolidBrush b = new SolidBrush(fill))
                g.FillRectangle(b, r);
            using (Pen p = new Pen(border, (_outline || _primary) ? 2f : 1f))
                g.DrawRectangle(p, r);

            int tick = 6;
            using (Pen p2 = new Pen(_primary ? text : UiColors.CyanDim, 1.5f))
            {
                g.DrawLine(p2, r.Left, r.Top + tick, r.Left, r.Top);
                g.DrawLine(p2, r.Left, r.Top, r.Left + tick, r.Top);
                g.DrawLine(p2, r.Right - tick, r.Top, r.Right, r.Top);
                g.DrawLine(p2, r.Right, r.Top, r.Right, r.Top + tick);
                g.DrawLine(p2, r.Left, r.Bottom - tick, r.Left, r.Bottom);
                g.DrawLine(p2, r.Left, r.Bottom, r.Left + tick, r.Bottom);
                g.DrawLine(p2, r.Right - tick, r.Bottom, r.Right, r.Bottom);
                g.DrawLine(p2, r.Right, r.Bottom - tick, r.Right, r.Bottom);
            }

            TextRenderer.DrawText(g, Text, Font, r, text,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    internal sealed class HudTabControl : TabControl
    {
        public HudTabControl()
        {
            DrawMode = TabDrawMode.OwnerDrawFixed;
            SizeMode = TabSizeMode.Fixed;
            ItemSize = new Size(160, 34);
            BackColor = UiColors.Bg;
            Padding = new Point(14, 7);
            UiHelper.EnableDoubleBuffer(this);
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(UiColors.Bg);
            Rectangle cr = DisplayRectangle;
            using (SolidBrush b = new SolidBrush(UiColors.Bg))
                e.Graphics.FillRectangle(b, cr);
            using (Pen p = new Pen(UiColors.Border, 1))
                e.Graphics.DrawRectangle(p, cr.X, cr.Y, cr.Width - 1, cr.Height - 1);
            for (int i = 0; i < TabCount; i++)
            {
                DrawItemEventArgs dea = new DrawItemEventArgs(e.Graphics, Font, GetTabRect(i), i,
                    i == SelectedIndex ? DrawItemState.Selected : DrawItemState.None);
                OnDrawItem(dea);
            }
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = e.Bounds;
            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            using (SolidBrush b = new SolidBrush(selected ? UiColors.PanelAlt : UiColors.Panel))
                g.FillRectangle(b, r);
            if (selected)
            {
                using (Pen p = new Pen(UiColors.Cyan, 2))
                    g.DrawLine(p, r.Left + 10, r.Bottom - 2, r.Right - 10, r.Bottom - 2);
            }
            string text = TabPages[e.Index].Text;
            TextRenderer.DrawText(g, text, Font, r, selected ? UiColors.Cyan : UiColors.Muted,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    internal sealed class HudGroupPanel : Panel
    {
        private string _title = "";
        public string Title
        {
            get { return _title; }
            set { _title = value ?? ""; Invalidate(); }
        }
        public HudGroupPanel()
        {
            BackColor = UiColors.Panel;
            Padding = new Padding(12, 28, 12, 12);
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = ClientRectangle;
            r.Width -= 1; r.Height -= 1;
            using (SolidBrush b = new SolidBrush(UiColors.Panel))
                g.FillRectangle(b, r);
            using (Pen p = new Pen(UiColors.Border, 1))
                g.DrawRectangle(p, r);
            using (Pen p2 = new Pen(UiColors.Cyan, 2))
            {
                g.DrawLine(p2, r.Left, r.Top, r.Left + 28, r.Top);
                g.DrawLine(p2, r.Left, r.Top, r.Left, r.Top + 16);
            }
            if (!string.IsNullOrEmpty(_title))
            {
                using (Font f = new Font("Segoe UI Semibold", 9f))
                using (SolidBrush tb = new SolidBrush(UiColors.Cyan))
                    g.DrawString(_title.ToUpperInvariant(), f, tb, 14, 8);
            }
        }
    }

    internal sealed class LangCard : Panel
    {
        private bool _selected;
        private string _code;
        private string _title;
        private string _subtitle;
        private RadioButton _radio;
        public event EventHandler SelectedChanged;
        public string Code { get { return _code; } }
        public bool Selected
        {
            get { return _selected; }
            set { _selected = value; if (_radio != null) _radio.Checked = value; Invalidate(); }
        }
        public LangCard(string code, string title, string subtitle)
        {
            _code = code; _title = title; _subtitle = subtitle;
            Size = new Size(280, 100);
            Cursor = Cursors.Hand;
            BackColor = UiColors.Panel;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            _radio = new RadioButton();
            _radio.Visible = false;
            Controls.Add(_radio);
            Click += delegate { SelectCard(); };
        }
        public void SetTexts(string title, string subtitle)
        {
            _title = title; _subtitle = subtitle; Invalidate();
        }
        private void SelectCard()
        {
            _selected = true; _radio.Checked = true; Invalidate();
            if (SelectedChanged != null) SelectedChanged(this, EventArgs.Empty);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = ClientRectangle;
            r.Width -= 1; r.Height -= 1;
            using (SolidBrush b = new SolidBrush(_selected ? UiColors.PanelAlt : UiColors.Panel))
                g.FillRectangle(b, r);
            using (Pen p = new Pen(_selected ? UiColors.Cyan : UiColors.Border, _selected ? 2f : 1f))
                g.DrawRectangle(p, r);
            int tick = 10;
            using (Pen p2 = new Pen(UiColors.Cyan, 1.5f))
            {
                g.DrawLine(p2, r.Left, r.Top + tick, r.Left, r.Top);
                g.DrawLine(p2, r.Left, r.Top, r.Left + tick, r.Top);
                g.DrawLine(p2, r.Right - tick, r.Bottom, r.Right, r.Bottom);
                g.DrawLine(p2, r.Right, r.Bottom - tick, r.Right, r.Bottom);
            }
            using (Font fBig = new Font("Segoe UI Semibold", 16f))
            using (SolidBrush tb = new SolidBrush(_selected ? UiColors.Cyan : UiColors.Text))
                g.DrawString(_title, fBig, tb, 20, 22);
            using (Font fSm = new Font("Segoe UI", 9f))
            using (SolidBrush mb = new SolidBrush(UiColors.Muted))
                g.DrawString(_subtitle, fSm, mb, 20, 58);
            Rectangle circ = new Rectangle(r.Right - 36, r.Top + 38, 18, 18);
            using (Pen rp = new Pen(_selected ? UiColors.Cyan : UiColors.Muted, 2))
                g.DrawEllipse(rp, circ);
            if (_selected)
            {
                using (SolidBrush fb = new SolidBrush(UiColors.Cyan))
                    g.FillEllipse(fb, circ.X + 4, circ.Y + 4, 10, 10);
            }
        }
    }

    internal sealed class MainForm : Form
    {
        private readonly string _root;
        private HudTabControl _tabs;
        private ListBox _themes;
        private PictureBox _preview;
        private Panel _previewFrame;
        private Label _status, _statusSummary, _filterNote, _previewCaption, _themeCount, _dblHint;
        private TextBox _search;
        private ComboBox _com;
        private TrackBar _brightness;
        private Label _brightVal;
        private CheckBox _reverse;
        private ComboBox _hw, _fan;
        private Panel _advPanel;
        private CheckBox _advToggle;
        private TextBox _weatherKey, _weatherLat, _weatherLon, _weatherLang;
        private HudGroupPanel _grpWeather;
        private HudButton _btnDetectLoc;
        private CheckBox _autoWeather;
        private Label _lblWeatherHint;
        private LangCard _cardEs, _cardEn;
        private HudButton _btnStart, _btnAdmin, _btnStop, _btnSave, _btnClassic, _btnFolder, _btnRefreshPorts;
        private Label _lblThemeList, _lblCom, _lblBright, _lblHw, _lblFan, _lblLang, _lblLangHint;
        private Label _lblWKey, _lblWLat, _lblWLon, _lblWLang;
        private Label _titleLbl, _subLbl;
        private PictureBox _logo;
        private HudGroupPanel _grpPort, _grpDisplay, _grpSensors;
        private Image _previewImage;
        private List<ThemeItem> _allThemes = new List<ThemeItem>();
        private Panel _header;
        private Panel _actionBar;

        public MainForm(string root)
        {
            _root = root;
            I18n.Lang = UiSettings.LoadLang(root);

            Text = "Pantalla Turing";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(920, 640);
            BackColor = UiColors.Bg;
            ForeColor = UiColors.Text;
            Font = new Font("Segoe UI", 9.5f);
            DoubleBuffered = true;
            Padding = new Padding(0);

            try
            {
                string ico = Path.Combine(root, @"res\icons\monitor-icon-17865\icon.ico");
                if (File.Exists(ico)) Icon = new Icon(ico);
            }
            catch { }

            BuildChrome();
            BuildTabs();
            BuildActionBar();
            ApplyI18n();
            LoadAll();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (Pen p = new Pen(UiColors.Cyan, 2))
                g.DrawLine(p, 0, 0, ClientSize.Width, 0);
            int L = 18;
            using (Pen p2 = new Pen(UiColors.Cyan, 2))
            {
                g.DrawLine(p2, 0, 0, L, 0);
                g.DrawLine(p2, 0, 0, 0, L);
                g.DrawLine(p2, ClientSize.Width - L, 0, ClientSize.Width, 0);
                g.DrawLine(p2, ClientSize.Width - 1, 0, ClientSize.Width - 1, L);
                g.DrawLine(p2, 0, ClientSize.Height - L, 0, ClientSize.Height);
                g.DrawLine(p2, 0, ClientSize.Height - 1, L, ClientSize.Height - 1);
                g.DrawLine(p2, ClientSize.Width - 1, ClientSize.Height - L, ClientSize.Width - 1, ClientSize.Height);
                g.DrawLine(p2, ClientSize.Width - L, ClientSize.Height - 1, ClientSize.Width, ClientSize.Height - 1);
            }
        }

        private void BuildChrome()
        {
            _header = new Panel();
            _header.Location = new Point(0, 2);
            _header.Size = new Size(920, 78);
            _header.BackColor = UiColors.Panel;
            _header.Padding = new Padding(16, 10, 16, 10);
            Controls.Add(_header);

            _logo = new PictureBox();
            _logo.Location = new Point(18, 12);
            _logo.Size = new Size(54, 54);
            _logo.SizeMode = PictureBoxSizeMode.Zoom;
            _logo.BackColor = Color.Transparent;
            try
            {
                string png64 = Path.Combine(_root, @"res\icons\monitor-icon-17865\64.png");
                string png48 = Path.Combine(_root, @"res\icons\monitor-icon-17865\48.png");
                if (File.Exists(png64)) _logo.Image = Image.FromFile(png64);
                else if (File.Exists(png48)) _logo.Image = Image.FromFile(png48);
            }
            catch { }
            _header.Controls.Add(_logo);

            _titleLbl = new Label();
            _titleLbl.Name = "title";
            _titleLbl.Text = I18n.T("title");
            _titleLbl.Font = new Font("Segoe UI Semibold", 20f);
            _titleLbl.ForeColor = UiColors.Cyan;
            _titleLbl.Location = new Point(82, 12);
            _titleLbl.AutoSize = true;
            _header.Controls.Add(_titleLbl);

            _subLbl = new Label();
            _subLbl.Name = "subtitle";
            _subLbl.Text = I18n.T("subtitle");
            _subLbl.ForeColor = UiColors.Muted;
            _subLbl.Location = new Point(84, 46);
            _subLbl.AutoSize = true;
            _header.Controls.Add(_subLbl);
        }

        private void BuildTabs()
        {
            _tabs = new HudTabControl();
            _tabs.Location = new Point(16, 90);
            _tabs.Size = new Size(888, 430);
            _tabs.Font = new Font("Segoe UI Semibold", 9.5f);
            Controls.Add(_tabs);

            TabPage tpThemes = new TabPage();
            tpThemes.Name = "tab_themes";
            tpThemes.BackColor = UiColors.Bg;
            tpThemes.ForeColor = UiColors.Text;
            tpThemes.Padding = new Padding(12);
            _tabs.TabPages.Add(tpThemes);
            BuildThemesTab(tpThemes);

            TabPage tpCfg = new TabPage();
            tpCfg.Name = "tab_config";
            tpCfg.BackColor = UiColors.Bg;
            tpCfg.ForeColor = UiColors.Text;
            tpCfg.Padding = new Padding(12);
            tpCfg.AutoScroll = true;
            _tabs.TabPages.Add(tpCfg);
            BuildConfigTab(tpCfg);

            TabPage tpLang = new TabPage();
            tpLang.Name = "tab_lang";
            tpLang.BackColor = UiColors.Bg;
            tpLang.ForeColor = UiColors.Text;
            tpLang.Padding = new Padding(12);
            _tabs.TabPages.Add(tpLang);
            BuildLangTab(tpLang);
        }

        private void BuildThemesTab(TabPage page)
        {
            _lblThemeList = new Label();
            _lblThemeList.Location = new Point(12, 8);
            _lblThemeList.AutoSize = true;
            _lblThemeList.ForeColor = UiColors.CyanDim;
            page.Controls.Add(_lblThemeList);

            _themeCount = new Label();
            _themeCount.Location = new Point(320, 8);
            _themeCount.AutoSize = true;
            _themeCount.ForeColor = UiColors.Muted;
            page.Controls.Add(_themeCount);

            _search = new TextBox();
            _search.Location = new Point(12, 32);
            _search.Size = new Size(380, 26);
            _search.BackColor = UiColors.PanelAlt;
            _search.ForeColor = UiColors.Text;
            _search.BorderStyle = BorderStyle.FixedSingle;
            _search.Font = new Font("Segoe UI", 10f);
            _search.TextChanged += delegate { FilterThemes(); };
            page.Controls.Add(_search);

            Panel listFrame = new Panel();
            listFrame.Location = new Point(12, 64);
            listFrame.Size = new Size(384, 300);
            listFrame.BackColor = UiColors.Cyan;
            listFrame.Padding = new Padding(2);
            page.Controls.Add(listFrame);

            _themes = new ListBox();
            _themes.Dock = DockStyle.Fill;
            _themes.BackColor = UiColors.Panel;
            _themes.ForeColor = UiColors.Text;
            _themes.BorderStyle = BorderStyle.None;
            _themes.IntegralHeight = false;
            _themes.Font = new Font("Segoe UI", 10.5f);
            _themes.ItemHeight = 22;
            _themes.SelectedIndexChanged += delegate
            {
                ShowPreview();
                UpdateSummary();
            };
            _themes.DoubleClick += delegate { Start(false); };
            listFrame.Controls.Add(_themes);

            _dblHint = new Label();
            _dblHint.Location = new Point(12, 370);
            _dblHint.AutoSize = true;
            _dblHint.ForeColor = UiColors.Muted;
            page.Controls.Add(_dblHint);

            _previewFrame = new Panel();
            _previewFrame.Location = new Point(420, 32);
            _previewFrame.Size = new Size(440, 300);
            _previewFrame.BackColor = UiColors.Panel;
            _previewFrame.Padding = new Padding(12);
            page.Controls.Add(_previewFrame);
            _previewFrame.Paint += delegate(object s, PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                Rectangle r = _previewFrame.ClientRectangle;
                r.Width -= 1; r.Height -= 1;
                using (Pen p = new Pen(UiColors.Border, 1))
                    g.DrawRectangle(p, r);
                using (Pen p2 = new Pen(UiColors.CyanDim, 1.5f))
                {
                    int t = 12;
                    g.DrawLine(p2, r.Left, r.Top + t, r.Left, r.Top);
                    g.DrawLine(p2, r.Left, r.Top, r.Left + t, r.Top);
                    g.DrawLine(p2, r.Right - t, r.Bottom, r.Right, r.Bottom);
                    g.DrawLine(p2, r.Right, r.Bottom - t, r.Right, r.Bottom);
                }
            };

            _preview = new PictureBox();
            _preview.Location = new Point(40, 16);
            _preview.Size = new Size(360, 240);
            _preview.BackColor = Color.FromArgb(0x08, 0x12, 0x20);
            _preview.SizeMode = PictureBoxSizeMode.Zoom;
            _preview.BorderStyle = BorderStyle.FixedSingle;
            _previewFrame.Controls.Add(_preview);

            _previewCaption = new Label();
            _previewCaption.Location = new Point(16, 264);
            _previewCaption.Size = new Size(408, 24);
            _previewCaption.TextAlign = ContentAlignment.MiddleCenter;
            _previewCaption.ForeColor = UiColors.Cyan;
            _previewCaption.Font = new Font("Segoe UI Semibold", 9.5f);
            _previewFrame.Controls.Add(_previewCaption);

            _filterNote = new Label();
            _filterNote.Location = new Point(420, 340);
            _filterNote.Size = new Size(440, 48);
            _filterNote.ForeColor = UiColors.Muted;
            page.Controls.Add(_filterNote);
        }

        private void BuildConfigTab(TabPage page)
        {
            _grpPort = new HudGroupPanel();
            _grpPort.Location = new Point(12, 8);
            _grpPort.Size = new Size(400, 100);
            page.Controls.Add(_grpPort);

            _lblCom = MakeLbl(_grpPort, 12, 30);
            _com = new ComboBox();
            _com.DropDownStyle = ComboBoxStyle.DropDown;
            _com.Location = new Point(12, 52);
            _com.Size = new Size(200, 28);
            StyleCombo(_com);
            _com.SelectedIndexChanged += delegate { UpdateSummary(); };
            _com.TextChanged += delegate { UpdateSummary(); };
            _grpPort.Controls.Add(_com);

            _btnRefreshPorts = new HudButton();
            _btnRefreshPorts.Location = new Point(226, 50);
            _btnRefreshPorts.Size = new Size(150, 32);
            _btnRefreshPorts.SetSecondary();
            _btnRefreshPorts.Click += delegate { RefreshPorts(); };
            _grpPort.Controls.Add(_btnRefreshPorts);

            _grpDisplay = new HudGroupPanel();
            _grpDisplay.Location = new Point(428, 8);
            _grpDisplay.Size = new Size(430, 130);
            page.Controls.Add(_grpDisplay);

            _lblBright = MakeLbl(_grpDisplay, 12, 30);
            _brightness = new TrackBar();
            _brightness.Minimum = 0;
            _brightness.Maximum = 100;
            _brightness.TickFrequency = 10;
            _brightness.Location = new Point(12, 48);
            _brightness.Size = new Size(300, 45);
            _brightness.BackColor = UiColors.Panel;
            _brightness.ValueChanged += delegate
            {
                _brightVal.Text = _brightness.Value.ToString() + "%";
                UpdateSummary();
            };
            _grpDisplay.Controls.Add(_brightness);

            _brightVal = new Label();
            _brightVal.Location = new Point(320, 52);
            _brightVal.AutoSize = true;
            _brightVal.ForeColor = UiColors.Cyan;
            _brightVal.Font = new Font("Segoe UI Semibold", 11f);
            _brightVal.Text = "0%";
            _grpDisplay.Controls.Add(_brightVal);

            _reverse = new CheckBox();
            _reverse.Location = new Point(12, 96);
            _reverse.AutoSize = true;
            _reverse.ForeColor = UiColors.Text;
            _reverse.FlatStyle = FlatStyle.Flat;
            _reverse.FlatAppearance.BorderColor = UiColors.CyanDim;
            _grpDisplay.Controls.Add(_reverse);

            _grpSensors = new HudGroupPanel();
            _grpSensors.Location = new Point(12, 116);
            _grpSensors.Size = new Size(400, 120);
            page.Controls.Add(_grpSensors);

            _lblHw = MakeLbl(_grpSensors, 12, 30);
            _hw = new ComboBox();
            _hw.DropDownStyle = ComboBoxStyle.DropDownList;
            _hw.Location = new Point(12, 52);
            _hw.Size = new Size(170, 28);
            StyleCombo(_hw);
            _hw.Items.AddRange(new object[] { "AUTO", "PYTHON", "LHM" });
            _grpSensors.Controls.Add(_hw);

            _lblFan = MakeLbl(_grpSensors, 200, 30);
            _fan = new ComboBox();
            _fan.DropDownStyle = ComboBoxStyle.DropDown;
            _fan.Location = new Point(200, 52);
            _fan.Size = new Size(170, 28);
            StyleCombo(_fan);
            _fan.Items.Add("AUTO");
            _grpSensors.Controls.Add(_fan);

            // Weather group — always visible, optional
            _grpWeather = new HudGroupPanel();
            _grpWeather.Location = new Point(428, 146);
            _grpWeather.Size = new Size(430, 200);
            page.Controls.Add(_grpWeather);

            _autoWeather = new CheckBox();
            _autoWeather.Location = new Point(12, 30);
            _autoWeather.AutoSize = true;
            _autoWeather.ForeColor = UiColors.Cyan;
            _autoWeather.FlatStyle = FlatStyle.Flat;
            _autoWeather.Checked = true;
            _autoWeather.CheckedChanged += delegate
            {
                if (_weatherKey != null)
                    _weatherKey.Enabled = !_autoWeather.Checked;
            };
            _grpWeather.Controls.Add(_autoWeather);

            _lblWLat = MakeLbl(_grpWeather, 12, 56);
            _weatherLat = MakeTb(_grpWeather, 12, 76, 120);
            _lblWLon = MakeLbl(_grpWeather, 150, 56);
            _weatherLon = MakeTb(_grpWeather, 150, 76, 120);
            _lblWLang = MakeLbl(_grpWeather, 290, 56);
            _weatherLang = MakeTb(_grpWeather, 290, 76, 100);

            _btnDetectLoc = new HudButton();
            _btnDetectLoc.Location = new Point(12, 112);
            _btnDetectLoc.Size = new Size(220, 34);
            _btnDetectLoc.SetOutline();
            _btnDetectLoc.Click += delegate { DetectLocationByIp(); };
            _grpWeather.Controls.Add(_btnDetectLoc);

            _lblWeatherHint = new Label();
            _lblWeatherHint.Location = new Point(12, 154);
            _lblWeatherHint.Size = new Size(400, 36);
            _lblWeatherHint.ForeColor = UiColors.Muted;
            _grpWeather.Controls.Add(_lblWeatherHint);

            // Advanced: optional OpenWeatherMap API key only
            _advToggle = new CheckBox();
            _advToggle.Location = new Point(12, 246);
            _advToggle.AutoSize = true;
            _advToggle.ForeColor = UiColors.CyanDim;
            _advToggle.FlatStyle = FlatStyle.Flat;
            _advToggle.CheckedChanged += delegate { _advPanel.Visible = _advToggle.Checked; };
            page.Controls.Add(_advToggle);

            _advPanel = new Panel();
            _advPanel.Location = new Point(12, 272);
            _advPanel.Size = new Size(400, 70);
            _advPanel.BackColor = UiColors.Panel;
            _advPanel.Visible = false;
            _advPanel.Padding = new Padding(10);
            page.Controls.Add(_advPanel);

            _lblWKey = MakeLbl(_advPanel, 10, 8);
            _weatherKey = MakeTb(_advPanel, 10, 28, 360);
            _weatherKey.UseSystemPasswordChar = true;
            _weatherKey.Enabled = !_autoWeather.Checked;
        }

        private void BuildLangTab(TabPage page)
        {
            _lblLang = MakeLbl(page, 16, 16);
            _lblLang.Font = new Font("Segoe UI Semibold", 11f);
            _lblLang.ForeColor = UiColors.Cyan;

            _cardEs = new LangCard("es", "Espanol", "");
            _cardEs.Location = new Point(16, 48);
            _cardEs.SelectedChanged += delegate
            {
                if (_cardEs.Selected)
                {
                    _cardEn.Selected = false;
                    I18n.Lang = "es";
                    UiSettings.SaveLang(_root, I18n.Lang);
                    ApplyI18n();
                }
            };
            page.Controls.Add(_cardEs);

            _cardEn = new LangCard("en", "English", "");
            _cardEn.Location = new Point(316, 48);
            _cardEn.SelectedChanged += delegate
            {
                if (_cardEn.Selected)
                {
                    _cardEs.Selected = false;
                    I18n.Lang = "en";
                    UiSettings.SaveLang(_root, I18n.Lang);
                    ApplyI18n();
                }
            };
            page.Controls.Add(_cardEn);

            _lblLangHint = new Label();
            _lblLangHint.Location = new Point(16, 170);
            _lblLangHint.Size = new Size(820, 60);
            _lblLangHint.ForeColor = UiColors.Muted;
            page.Controls.Add(_lblLangHint);
        }

        private void BuildActionBar()
        {
            _actionBar = new Panel();
            _actionBar.Location = new Point(0, 530);
            _actionBar.Size = new Size(920, 110);
            _actionBar.BackColor = UiColors.Panel;
            _actionBar.Padding = new Padding(16, 12, 16, 12);
            Controls.Add(_actionBar);

            int y = 12;
            int gap = 12;
            int x = 16;
            int h = 40;

            _btnStart = new HudButton();
            _btnStart.Location = new Point(x, y);
            _btnStart.Size = new Size(130, h);
            _btnStart.SetPrimary();
            _btnStart.Click += delegate { Start(false); };
            _actionBar.Controls.Add(_btnStart);
            x += 130 + gap;

            _btnAdmin = new HudButton();
            _btnAdmin.Location = new Point(x, y);
            _btnAdmin.Size = new Size(140, h);
            _btnAdmin.SetOutline();
            _btnAdmin.Click += delegate { Start(true); };
            _actionBar.Controls.Add(_btnAdmin);
            x += 140 + gap;

            _btnStop = new HudButton();
            _btnStop.Location = new Point(x, y);
            _btnStop.Size = new Size(110, h);
            _btnStop.SetDanger();
            _btnStop.Click += delegate
            {
                Project.StopMonitor(_root);
                SetStatus(I18n.T("status_stopped"));
            };
            _actionBar.Controls.Add(_btnStop);
            x += 110 + gap;

            _btnSave = new HudButton();
            _btnSave.Location = new Point(x, y);
            _btnSave.Size = new Size(110, h);
            _btnSave.SetSecondary();
            _btnSave.Click += delegate { SaveAll(true); };
            _actionBar.Controls.Add(_btnSave);
            x += 110 + gap;

            _btnClassic = new HudButton();
            _btnClassic.Location = new Point(x, y);
            _btnClassic.Size = new Size(170, h);
            _btnClassic.SetSecondary();
            _btnClassic.Click += delegate
            {
                try { Project.OpenClassicConfigurator(_root); }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Pantalla Turing", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            };
            _actionBar.Controls.Add(_btnClassic);
            x += 170 + gap;

            _btnFolder = new HudButton();
            _btnFolder.Location = new Point(x, y);
            _btnFolder.Size = new Size(110, h);
            _btnFolder.SetSecondary();
            _btnFolder.Click += delegate
            {
                try { Process.Start("explorer.exe", _root); } catch { }
            };
            _actionBar.Controls.Add(_btnFolder);

            Panel statusBar = new Panel();
            statusBar.Location = new Point(0, 62);
            statusBar.Size = new Size(920, 48);
            statusBar.BackColor = UiColors.Bg;
            _actionBar.Controls.Add(statusBar);

            _status = new Label();
            _status.Location = new Point(16, 12);
            _status.Size = new Size(520, 24);
            _status.ForeColor = UiColors.Ok;
            statusBar.Controls.Add(_status);

            _statusSummary = new Label();
            _statusSummary.Location = new Point(540, 12);
            _statusSummary.Size = new Size(360, 24);
            _statusSummary.TextAlign = ContentAlignment.MiddleRight;
            _statusSummary.ForeColor = UiColors.CyanDim;
            _statusSummary.Font = new Font("Consolas", 9.5f);
            statusBar.Controls.Add(_statusSummary);
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

        private void StyleCombo(ComboBox c)
        {
            c.BackColor = UiColors.PanelAlt;
            c.ForeColor = UiColors.Text;
            c.FlatStyle = FlatStyle.Flat;
        }

        private void ApplyI18n()
        {
            _titleLbl.Text = I18n.T("title");
            _subLbl.Text = I18n.T("subtitle");
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
            _btnFolder.Text = I18n.T("btn_folder");
            _btnRefreshPorts.Text = I18n.T("btn_refresh");
            _lblThemeList.Text = I18n.T("theme_list");
            _filterNote.Text = I18n.T("filter_note") + "\n" + I18n.T("theme_note");
            _dblHint.Text = I18n.T("dblclick_hint");
            _search.AccessibleName = I18n.T("search");
            if (_grpPort != null) _grpPort.Title = I18n.T("grp_port");
            if (_grpDisplay != null) _grpDisplay.Title = I18n.T("grp_display");
            if (_grpSensors != null) _grpSensors.Title = I18n.T("grp_sensors");
            if (_grpWeather != null) _grpWeather.Title = I18n.T("grp_weather");
            if (_btnDetectLoc != null) _btnDetectLoc.Text = I18n.T("detect_loc");
            if (_autoWeather != null) _autoWeather.Text = I18n.T("auto_weather");
            if (_lblWeatherHint != null) _lblWeatherHint.Text = I18n.T("weather_hint");
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
            if (_cardEs != null) _cardEs.SetTexts(I18n.T("lang_es_card"), I18n.T("lang_es_sub"));
            if (_cardEn != null) _cardEn.SetTexts(I18n.T("lang_en_card"), I18n.T("lang_en_sub"));
            // Keep WEATHER_LANGUAGE aligned with UI language
            if (_weatherLang != null)
                _weatherLang.Text = (I18n.Lang == "en") ? "en" : "es";
            UpdateThemeCount();
            ShowPreview();
            UpdateSummary();
            _tabs.Invalidate();
            Invalidate();
        }
        private void LoadAll()
        {
            _allThemes = Project.DiscoverCompatibleThemes(_root);
            FilterThemes();

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
            _brightVal.Text = bright.ToString() + "%";

            string rev = cfg.ContainsKey("DISPLAY_REVERSE") ? cfg["DISPLAY_REVERSE"] : "false";
            _reverse.Checked = string.Equals(rev, "true", StringComparison.OrdinalIgnoreCase);

            string hw = cfg.ContainsKey("HW_SENSORS") ? cfg["HW_SENSORS"] : "AUTO";
            int hwi = _hw.FindStringExact(hw.ToUpperInvariant());
            _hw.SelectedIndex = hwi >= 0 ? hwi : 0;

            string fan = cfg.ContainsKey("CPU_FAN") ? cfg["CPU_FAN"] : "AUTO";
            _fan.Text = fan;

            _weatherKey.Text = cfg.ContainsKey("WEATHER_API_KEY") ? cfg["WEATHER_API_KEY"] : "";
            _weatherLat.Text = cfg.ContainsKey("WEATHER_LATITUDE") ? cfg["WEATHER_LATITUDE"] : "";
            _weatherLon.Text = cfg.ContainsKey("WEATHER_LONGITUDE") ? cfg["WEATHER_LONGITUDE"] : "";
            _weatherLang.Text = cfg.ContainsKey("WEATHER_LANGUAGE") ? cfg["WEATHER_LANGUAGE"] : (I18n.Lang == "en" ? "en" : "es");
            string existingKey = (_weatherKey.Text ?? "").Trim();
            bool hasRealKey = existingKey.Length > 0
                && !string.Equals(existingKey, "YOUR_OPENWEATHERMAP_API_KEY", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(existingKey, "none", StringComparison.OrdinalIgnoreCase);
            _autoWeather.Checked = !hasRealKey;
            _weatherKey.Enabled = !_autoWeather.Checked;

            if (string.Equals(I18n.Lang, "en", StringComparison.OrdinalIgnoreCase))
            {
                _cardEn.Selected = true;
                _cardEs.Selected = false;
            }
            else
            {
                _cardEs.Selected = true;
                _cardEn.Selected = false;
            }

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

            SetStatus(I18n.T("status_ready"));
            UpdateSummary();
        }

        private void FilterThemes()
        {
            string q = (_search.Text ?? "").Trim();
            string selected = SelectedThemeName();
            _themes.BeginUpdate();
            _themes.Items.Clear();
            for (int i = 0; i < _allThemes.Count; i++)
            {
                ThemeItem ti = _allThemes[i];
                if (q.Length == 0
                    || ti.Name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                    || ti.Label.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                    _themes.Items.Add(ti);
            }
            _themes.EndUpdate();

            int idx = -1;
            if (!string.IsNullOrEmpty(selected))
            {
                for (int i = 0; i < _themes.Items.Count; i++)
                {
                    if (string.Equals(((ThemeItem)_themes.Items[i]).Name, selected, StringComparison.OrdinalIgnoreCase))
                    {
                        idx = i;
                        break;
                    }
                }
            }
            if (idx >= 0) _themes.SelectedIndex = idx;
            else if (_themes.Items.Count > 0) _themes.SelectedIndex = 0;

            UpdateThemeCount();
            ShowPreview();
        }

        private void UpdateThemeCount()
        {
            if (_themeCount == null) return;
            _themeCount.Text = _themes.Items.Count.ToString() + " " + I18n.T("themes_count");
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
            UpdateSummary();
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
                if (_previewCaption != null)
                    _previewCaption.Text = I18n.T("preview") + " " + I18n.T("no_preview");
                return;
            }
            try
            {
                using (FileStream fs = new FileStream(ti.PreviewPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    _previewImage = Image.FromStream(fs);
                }
                _preview.Image = _previewImage;
                _previewCaption.Text = ti.Name;
            }
            catch
            {
                _preview.Image = null;
                _previewCaption.Text = I18n.T("preview") + " " + I18n.T("no_preview");
            }
        }

        private void UpdateSummary()
        {
            if (_statusSummary == null) return;
            string com = (_com != null && !string.IsNullOrEmpty(_com.Text)) ? _com.Text.Trim() : "-";
            string theme = SelectedThemeName();
            if (string.IsNullOrEmpty(theme)) theme = "-";
            int br = _brightness != null ? _brightness.Value : 0;
            _statusSummary.Text = com + " · " + theme + " · brillo " + br;
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

            Project.WriteConfigKey(_root, "BRIGHTNESS", _brightness.Value.ToString(), out err);
            Project.WriteConfigKey(_root, "DISPLAY_REVERSE", _reverse.Checked ? "true" : "false", out err);

            string hw = _hw.SelectedItem != null ? _hw.SelectedItem.ToString() : "AUTO";
            Project.WriteConfigKey(_root, "HW_SENSORS", hw, out err);

            string fan = (_fan.Text ?? "AUTO").Trim();
            if (string.IsNullOrEmpty(fan)) fan = "AUTO";
            Project.WriteConfigKey(_root, "CPU_FAN", fan, out err);

            // Weather: always persist lat/lon/lang; API key only when not in auto mode
            string lat = (_weatherLat.Text ?? "").Trim();
            string lon = (_weatherLon.Text ?? "").Trim();
            if (!string.IsNullOrEmpty(lat)) Project.WriteConfigKey(_root, "WEATHER_LATITUDE", lat, out err);
            if (!string.IsNullOrEmpty(lon)) Project.WriteConfigKey(_root, "WEATHER_LONGITUDE", lon, out err);
            string wl = (_weatherLang.Text ?? "").Trim();
            if (string.IsNullOrEmpty(wl)) wl = (I18n.Lang == "en") ? "en" : "es";
            Project.WriteConfigKey(_root, "WEATHER_LANGUAGE", wl, out err);
            if (_autoWeather != null && _autoWeather.Checked)
            {
                // Empty key => Open-Meteo in stats.py
                Project.WriteConfigKey(_root, "WEATHER_API_KEY", "", out err);
            }
            else if (_advToggle != null && _advToggle.Checked)
            {
                string key = (_weatherKey.Text ?? "").Trim();
                Project.WriteConfigKey(_root, "WEATHER_API_KEY", key, out err);
            }

            UiSettings.SaveLang(_root, I18n.Lang);
            if (showStatus) SetStatus(I18n.T("status_saved"));
            UpdateSummary();
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
            UpdateSummary();
        }


        private void DetectLocationByIp()
        {
            try
            {
                SetStatus(I18n.Lang == "en" ? "Detecting location..." : "Detectando ubicacion...");
                string json = null;
                try
                {
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | (SecurityProtocolType)768;
                    using (WebClient wc = new WebClient())
                    {
                        wc.Headers.Add("User-Agent", "PantallaTuring/1.1");
                        json = wc.DownloadString("https://ipapi.co/json/");
                    }
                }
                catch
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.Headers.Add("User-Agent", "PantallaTuring/1.1");
                        json = wc.DownloadString("http://ip-api.com/json/?fields=status,message,lat,lon,city,country");
                    }
                }
                if (string.IsNullOrEmpty(json))
                {
                    SetStatus(I18n.T("status_loc_fail"));
                    return;
                }

                string lat = ExtractJsonNumber(json, "latitude");
                if (string.IsNullOrEmpty(lat)) lat = ExtractJsonNumber(json, "lat");
                string lon = ExtractJsonNumber(json, "longitude");
                if (string.IsNullOrEmpty(lon)) lon = ExtractJsonNumber(json, "lon");
                string city = ExtractJsonString(json, "city");

                if (string.IsNullOrEmpty(lat) || string.IsNullOrEmpty(lon))
                {
                    SetStatus(I18n.T("status_loc_fail"));
                    return;
                }

                _weatherLat.Text = lat;
                _weatherLon.Text = lon;
                _weatherLang.Text = (I18n.Lang == "en") ? "en" : "es";
                if (_autoWeather != null) _autoWeather.Checked = true;

                string err;
                Project.WriteConfigKey(_root, "WEATHER_LATITUDE", lat, out err);
                Project.WriteConfigKey(_root, "WEATHER_LONGITUDE", lon, out err);
                Project.WriteConfigKey(_root, "WEATHER_LANGUAGE", _weatherLang.Text, out err);
                if (_autoWeather != null && _autoWeather.Checked)
                    Project.WriteConfigKey(_root, "WEATHER_API_KEY", "", out err);

                string where = string.IsNullOrEmpty(city) ? (lat + ", " + lon) : city;
                SetStatus(I18n.T("status_loc_ok") + ": " + where + " (" + lat + ", " + lon + ")");
            }
            catch (Exception ex)
            {
                SetStatus(I18n.T("status_loc_fail") + " — " + ex.Message);
            }
        }

        private static string ExtractJsonNumber(string json, string key)
        {
            Match m = Regex.Match(json, "\"" + Regex.Escape(key) + "\"\\s*:\\s*(-?\\d+(?:\\.\\d+)?)");
            return m.Success ? m.Groups[1].Value : "";
        }

        private static string ExtractJsonString(string json, string key)
        {
            Match m = Regex.Match(json, "\"" + Regex.Escape(key) + "\"\\s*:\\s*\"([^\"]*)\"");
            return m.Success ? m.Groups[1].Value : "";
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
            if (_logo != null && _logo.Image != null)
            {
                Image img = _logo.Image;
                _logo.Image = null;
                img.Dispose();
            }
            base.OnFormClosed(e);
        }
    }
}
