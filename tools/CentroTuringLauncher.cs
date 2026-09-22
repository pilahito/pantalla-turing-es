using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

class CentroTuringLauncher {
  [STAThread]
  static void Main() {
    try {
      string root = Path.GetDirectoryName(Application.ExecutablePath);
      // Prefer install next to project; also allow Desktop shortcut pointing here via same folder
      string[] candidates = new string[] {
        root,
        Path.Combine(root, ".."),
        @"E:\turing-smart-screen-python"
      };
      string project = null;
      foreach (string c in candidates) {
        string full = Path.GetFullPath(c);
        if (File.Exists(Path.Combine(full, "tools", "centro_pro_main.py")) ||
            File.Exists(Path.Combine(full, "tools", "centro_turing.py"))) {
          project = full;
          break;
        }
      }
      if (project == null) {
        MessageBox.Show("No encuentro E:\\turing-smart-screen-python (centro_pro_main.py).", "Centro Turing",
          MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }
      string pyw = Path.Combine(project, "venv", "Scripts", "pythonw.exe");
      string py = Path.Combine(project, "venv", "Scripts", "python.exe");
      string scriptPro = Path.Combine(project, "tools", "centro_pro_main.py");
      string scriptOld = Path.Combine(project, "tools", "centro_turing.py");
      string script = File.Exists(scriptPro) ? scriptPro : scriptOld;
      string exe = File.Exists(pyw) ? pyw : py;
      if (!File.Exists(exe)) {
        MessageBox.Show("No hay venv. Ejecuta Instalar.ps1 en la carpeta del proyecto.", "Centro Turing",
          MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }
      var psi = new ProcessStartInfo();
      psi.FileName = exe;
      psi.Arguments = "\"" + script + "\"";
      psi.WorkingDirectory = project;
      psi.UseShellExecute = false;
      psi.CreateNoWindow = true;
      psi.WindowStyle = ProcessWindowStyle.Hidden;
      Process.Start(psi);
    } catch (Exception ex) {
      MessageBox.Show(ex.Message, "Centro Turing", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
  }
}
