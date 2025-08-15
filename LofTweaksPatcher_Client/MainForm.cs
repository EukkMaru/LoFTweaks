using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LoFTweaksPatcher_Client
{
    public class MainForm : Form
    {
        TextBox txtGame;
        Button btnBrowse;
        TextBox txtSteamId;
        Label lblSidHint;

        CheckBox chkRng, chkJump, chkFly, chkInsta, chkWatermark, chkDebug;
        Button btnPatch, btnOpenCfg, btnRevert;

        public MainForm()
        {
            Text = "LoF Tweaks Patcher by EukkMaru";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            ClientSize = new System.Drawing.Size(660, 330);

            var y = 16;

            Controls.Add(new Label { Left = 12, Top = y + 4, Width = 120, Text = "Game folder:" });
            txtGame = new TextBox { Left = 130, Top = y, Width = 430 };
            btnBrowse = new Button { Left = 570, Top = y - 1, Width = 70, Text = "Browse" };
            btnBrowse.Click += delegate { BrowseGameFolder(); };
            Controls.Add(txtGame);
            Controls.Add(btnBrowse);
            y += 36;

            Controls.Add(new Label { Left = 12, Top = y + 4, Width = 120, Text = "SteamID (opt.):" });
            txtSteamId = new TextBox { Left = 130, Top = y, Width = 180 };
            lblSidHint = new Label { Left = 320, Top = y + 4, Width = 320, Text = "(optional — leave empty to auto-detect)" };
            Controls.Add(txtSteamId);
            Controls.Add(lblSidHint);
            y += 36;

            var grp = new GroupBox { Left = 12, Top = y, Width = 628, Height = 126, Text = "Tweaks to enable" };
            chkRng = new CheckBox { Left = 16, Top = 25, Width = 260, Text = "Fix padlock RNG to Rat" };
            chkJump = new CheckBox { Left = 16, Top = 50, Width = 260, Text = "Enable Jump" };
            chkFly = new CheckBox { Left = 16, Top = 75, Width = 260, Text = "Enable Fly" };
            chkInsta = new CheckBox { Left = 320, Top = 25, Width = 260, Text = "Instant Padlock" };
            chkWatermark = new CheckBox { Left = 320, Top = 50, Width = 260, Text = "Watermark (required)", Checked = true, Enabled = false };
            chkDebug = new CheckBox
            {
                Left = 320,
                Top = 75,
                Width = 260,
                Height = 36,
                AutoSize = false,
                Text = "Debug logs\n(Don't enable unless you know what you're doing)"
            };

            grp.Controls.Add(chkRng);
            grp.Controls.Add(chkJump);
            grp.Controls.Add(chkFly);
            grp.Controls.Add(chkInsta);
            grp.Controls.Add(chkWatermark);
            grp.Controls.Add(chkDebug);
            Controls.Add(grp);
            y += grp.Height + 12;

            btnPatch = new Button { Left = 12, Top = y, Width = 160, Height = 32, Text = "Patch" };
            btnOpenCfg = new Button { Left = 184, Top = y, Width = 160, Height = 32, Text = "Open config folder" };
            btnRevert = new Button { Left = 356, Top = y, Width = 160, Height = 32, Text = "Revert…" };
            btnPatch.Click += delegate { Patch(); };
            btnOpenCfg.Click += delegate { OpenCfgFolder(); };
            btnRevert.Click += delegate { ShowRevertHelp(); };
            Controls.AddRange(new Control[] { btnPatch, btnOpenCfg, btnRevert });

            var guess = TryDetectGameFolder();
            if (!string.IsNullOrEmpty(guess))
                txtGame.Text = guess;
        }

        void BrowseGameFolder()
        {
            using (var f = new FolderBrowserDialog())
            {
                f.Description = "Select the game ROOT folder (contains Layers Of FearSub.exe)";
                f.ShowNewFolderButton = false;
                if (Directory.Exists(txtGame.Text)) f.SelectedPath = txtGame.Text;
                if (f.ShowDialog(this) == DialogResult.OK)
                    txtGame.Text = f.SelectedPath;
            }
        }

        void Patch()
        {
            try
            {
                if (IsProcessRunning("Layers of Fear") || IsProcessRunning("Layers Of FearSub"))
                {
                    MessageBox.Show(this, "Close the game first.", "Game running",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var root = (txtGame.Text ?? "").Trim();
                if (root.Length == 0 || !Directory.Exists(root))
                {
                    MessageBox.Show(this, "Pick the game folder first.", "Missing path",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var managed = Path.Combine(root, "Layers Of FearSub_Data", "Managed");
                var targetDll = Path.Combine(managed, "Assembly-CSharp.dll");
                if (!File.Exists(targetDll))
                {
                    MessageBox.Show(this,
                        "Could not find Assembly-CSharp.dll.\nMake sure you selected the folder that contains Layers Of FearSub.exe.",
                        "Not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var tweak = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tweak.dll");
                if (!File.Exists(tweak))
                {
                    MessageBox.Show(this, "tweak.dll is missing next to the patcher exe.", "Missing tweak.dll",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                File.Copy(tweak, targetDll, true);

                var cfgPath = GetConfigFilePath((txtSteamId.Text ?? "").Trim());
                var dir = Path.GetDirectoryName(cfgPath);
                if (dir != null) Directory.CreateDirectory(dir);
                File.WriteAllText(cfgPath, BuildJson());

                MessageBox.Show(this, "Patched successfully.",
                    "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Patch failed:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void OpenCfgFolder()
        {
            try
            {
                var cfg = GetConfigFilePath((txtSteamId.Text ?? "").Trim());
                var dir = Path.GetDirectoryName(cfg);
                if (dir != null)
                {
                    Directory.CreateDirectory(dir);
                    Process.Start("explorer.exe", dir);
                }
            }
            catch { }
        }

        void ShowRevertHelp()
        {
            MessageBox.Show(this,
                "To revert to the vanilla game:\n\n" +
                "1) Open Steam\n" +
                "2) Right-click “Layers of Fear” → Properties\n" +
                "3) Installed Files → “Verify integrity of game files…”\n\n" +
                "Steam will restore the original files automatically.",
                "How to Revert", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        string BuildJson()
        {
            Func<bool, string> J = b => b ? "true" : "false";
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("  \"FixPadlock2Rng\": " + J(chkRng.Checked) + ",");
            sb.AppendLine("  \"EnableJump\": " + J(chkJump.Checked) + ",");
            sb.AppendLine("  \"EnableFly\": " + J(chkFly.Checked) + ",");
            sb.AppendLine("  \"InstaPadlock\": " + J(chkInsta.Checked) + ",");
            sb.AppendLine("  \"s_Debug\": " + J(chkDebug.Checked));
            sb.AppendLine("}");
            return sb.ToString();
        }

        static bool IsProcessRunning(string name)
        {
            try { return Process.GetProcessesByName(name).Any(); }
            catch { return false; }
        }

        string TryDetectGameFolder()
        {
            try
            {
                var steamPath = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null) as string;
                if (!string.IsNullOrEmpty(steamPath) && Directory.Exists(steamPath))
                {
                    var libs = ParseLibraryFolders(steamPath);
                    foreach (var lib in libs.Distinct())
                    {
                        var guess = Path.Combine(lib, "steamapps", "common", "Layers of Fear");
                        var subExe = Path.Combine(guess, "Layers Of FearSub.exe");
                        if (File.Exists(subExe))
                            return guess;
                    }
                }
            }
            catch { }
            return null;
        }

        static string[] ParseLibraryFolders(string steamPath)
        {
            try
            {
                var vdf = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
                if (!File.Exists(vdf)) return new string[0];
                var lines = File.ReadAllLines(vdf);
                return lines
                    .Where(l => l.IndexOf("\"path\"", StringComparison.OrdinalIgnoreCase) >= 0)
                    .Select(l =>
                    {
                        int q2 = l.LastIndexOf('"');
                        if (q2 <= 0) return null;
                        int q1 = l.LastIndexOf('"', q2 - 1);
                        if (q1 < 0) return null;
                        var p = l.Substring(q1 + 1, q2 - q1 - 1).Replace(@"\\", @"\");
                        return Directory.Exists(p) ? p : null;
                    })
                    .Where(p => !string.IsNullOrEmpty(p))
                    .Cast<string>()
                    .ToArray();
            }
            catch { }
            return new string[0];
        }

        static string GetConfigFilePath(string steamIdOverride)
        {
            // %LocalAppData%\..\LocalLow\Bloober Team\Layers of Fear\[steamId?]\cfg\LoFTweaks.json
            string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string parent = Directory.GetParent(localApp) != null ? Directory.GetParent(localApp).FullName : localApp;
            string localLow = Path.Combine(parent, "LocalLow");
            string basePath = Path.Combine(localLow, "Bloober Team", "Layers of Fear");

            string userRoot;
            if (!string.IsNullOrEmpty(steamIdOverride) && steamIdOverride.All(char.IsDigit))
            {
                userRoot = Path.Combine(basePath, steamIdOverride);
            }
            else
            {
                string first = FirstNumericSubdir(basePath);
                userRoot = !string.IsNullOrEmpty(first) ? first : basePath;
            }

            return Path.Combine(userRoot, "cfg", "LoFTweaks.json");
        }

        static string FirstNumericSubdir(string parent)
        {
            try
            {
                if (!Directory.Exists(parent)) return null;
                foreach (var d in Directory.GetDirectories(parent))
                {
                    var name = Path.GetFileName(d);
                    if (!string.IsNullOrEmpty(name) && name.All(char.IsDigit))
                        return d;
                }
            }
            catch { }
            return null;
        }
    }
}
// Why am i even doing this