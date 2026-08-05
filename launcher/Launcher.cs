// Plan Usage 启动器：双击 exe 直接启动应用。
// 与 启动.bat 等价：检查 Node.js → 缺依赖自动 npm install → 拉起 Electron 应用。
// 编译：csc /nologo /target:winexe /r:System.Windows.Forms.dll /win32icon:icon.ico /out:启动.exe Launcher.cs
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

static class Launcher
{
    [STAThread]
    static void Main()
    {
        string dir = AppDomain.CurrentDomain.BaseDirectory;
        string electronExe = Path.Combine(dir, "node_modules", "electron", "dist", "electron.exe");

        if (!HasNode())
        {
            MessageBox.Show(
                "未找到 Node.js，请先安装 Node.js 18 或更高版本：\r\nhttps://nodejs.org/zh-cn/download",
                "Plan Usage",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            return;
        }

        if (!File.Exists(electronExe))
        {
            var install = Process.Start(
                new ProcessStartInfo("cmd.exe", "/c npm install")
                {
                    WorkingDirectory = dir,
                    UseShellExecute = true,
                }
            );
            if (install != null) install.WaitForExit();
            if (!File.Exists(electronExe))
            {
                MessageBox.Show(
                    "依赖安装失败，请检查网络后重试（或手动运行 npm install）。",
                    "Plan Usage",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }
        }

        // 与启动.bat 一致：避免继承 ELECTRON_RUN_AS_NODE 导致 Electron 被当成 Node 运行
        Environment.SetEnvironmentVariable("ELECTRON_RUN_AS_NODE", "");
        Process.Start(
            new ProcessStartInfo(electronExe, ".")
            {
                WorkingDirectory = dir,
                UseShellExecute = true,
            }
        );
    }

    static bool HasNode()
    {
        try
        {
            var p = Process.Start(
                new ProcessStartInfo("node", "--version")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                }
            );
            if (p == null) return false;
            p.WaitForExit();
            return p.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
