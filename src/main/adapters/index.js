// 适配器注册表：provider id → adapter
// usage 型：async fetchUsage(monitor, ctx) => { periods: { fiveHours?, week?, month? } }
// balance 型：async fetchBalance(monitor, ctx) => { balance, currency }
// supportedKinds 声明支持的类型，monitor.provider + monitor.kind 决定走哪条路。
// ctx: { app, safeStorage }（官网同步类适配器用于 session/凭证）
const opencodeGo = require('./opencode-go');
const minimax = require('./minimax');
const volcengine = require('./volcengine');
const apiBalance = require('./api-balance');
const apiUsage = require('./api-usage');

const adapters = {
  'opencode-go': opencodeGo, // 官网同步，usage
  deepseek: apiBalance, // API，balance
  kimi: apiBalance, // API，balance
  volcengine, // 签名 OpenAPI，balance
  minimax, // API，usage + balance
  'custom-usage': apiUsage,
  'custom-balance': apiBalance,
};

function getAdapter(provider) {
  return adapters[provider] || null;
}

// 渲染层设置面板用的厂商元信息；supportedKinds 与适配器实现保持一致（测试保证）
const PROVIDER_META = [
  { id: 'opencode-go', isWebSync: true },
  { id: 'deepseek', needsApiKey: true, defaultBaseUrl: 'https://api.deepseek.com' },
  { id: 'kimi', needsApiKey: true, defaultBaseUrl: 'https://api.moonshot.cn' },
  { id: 'minimax', needsApiKey: true, defaultBaseUrl: 'https://www.minimaxi.com' },
  { id: 'volcengine', needsAkSk: true },
  { id: 'custom-usage', needsApiKey: true, needsBaseUrl: true },
  { id: 'custom-balance', needsApiKey: true, needsBaseUrl: true },
];

function listProviderMeta() {
  return PROVIDER_META.map((m) => ({
    needsApiKey: false,
    needsAkSk: false,
    needsBaseUrl: false,
    isWebSync: false,
    defaultBaseUrl: '',
    ...m,
    supportedKinds: (adapters[m.id] && adapters[m.id].supportedKinds) || [],
  }));
}

module.exports = Object.assign(adapters, { getAdapter, listProviderMeta });
