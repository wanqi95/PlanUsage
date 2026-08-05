# 启动器（启动.exe）

`启动.exe` 是 `启动.bat` 的编译版：双击即可启动 Plan Usage，无控制台黑窗闪动。

行为与脚本一致：

1. 检查本机是否安装了 Node.js，没有则弹窗提示。
2. 若 `node_modules\electron\dist\electron.exe` 不存在，自动打开控制台执行 `npm install`。
3. 拉起 Electron 应用后启动器立即退出。

## 重新生成

```bat
csc /nologo /out:icon-gen.exe make_icon.cs /r:System.Drawing.dll
icon-gen.exe launcher\icon.ico launcher\icon-preview.png build\icon.ico build\icon.png
csc /nologo /target:winexe /r:System.Windows.Forms.dll /win32icon:icon.ico /out:启动.exe Launcher.cs
```

`csc` 位于 `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`。
