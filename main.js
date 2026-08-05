const { app, globalShortcut, safeStorage } = require('electron');
const path = require('path');
const fs = require('fs');
const { initConfig } = require('./src/main/config');
const { createWindow, getMainWindow } = require('./src/main/window');
const { setupIPC } = require('./src/main/ipc');
const { registerShortcuts, unregisterShortcuts } = require('./src/main/shortcuts');
const monitor = require('./src/main/monitor');
const adapters = require('./src/main/adapters');

// 固定用户数据目录为 %APPDATA%\plan-usage（与文档一致），不受应用名命名规则影响。
// 若旧版默认目录 %APPDATA%\Plan Usage 已有数据，首次启动时复制迁移，避免配置丢失。
function pinUserData() {
  const appData = app.getPath('appData');
  const newDir = path.join(appData, 'plan-usage');
  const oldDir = path.join(appData, 'Plan Usage');
  if (oldDir !== newDir && fs.existsSync(oldDir) && !fs.existsSync(newDir)) {
    try {
      fs.cpSync(oldDir, newDir, { recursive: true });
    } catch (e) {
      console.error('Failed to migrate user data:', e);
    }
  }
  app.setPath('userData', newDir);
}
pinUserData();

app.whenReady().then(() => {
  initConfig(app.getPath('userData'));

  monitor.initMonitor({
    adapters,
    ctx: { app, safeStorage },
    broadcast: (payload) => {
      const win = getMainWindow();
      if (win && !win.isDestroyed()) win.webContents.send('monitor:data', payload);
    },
  });

  createWindow();
  setupIPC();
  registerShortcuts();
  monitor.startAll();
});

app.on('window-all-closed', () => {
  unregisterShortcuts();
  monitor.stopAll();
  app.quit();
});

app.on('will-quit', () => {
  globalShortcut.unregisterAll();
  monitor.stopAll();
});
