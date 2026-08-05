@echo off
cd /d "%~dp0"
title Plan Usage

rem 本机部分环境全局设置了 ELECTRON_RUN_AS_NODE，会让 Electron 退化成 Node，启动前清除
set ELECTRON_RUN_AS_NODE=

where node >nul 2>nul
if errorlevel 1 (
  echo [错误] 未找到 Node.js，请先安装 Node.js 18 或更高版本：
  echo        https://nodejs.org/zh-cn/download
  echo.
  pause
  exit /b 1
)

if not exist "node_modules\electron\dist\electron.exe" (
  echo 首次运行，正在安装依赖（已配置国内镜像）...
  call npm install
  if errorlevel 1 (
    echo.
    echo [错误] 依赖安装失败，请检查网络后重试。
    pause
    exit /b 1
  )
)

echo 正在启动 Plan Usage，本窗口将自动关闭...
start "" "%~dp0node_modules\electron\dist\electron.exe" .
exit
