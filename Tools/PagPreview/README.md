# PAG Editor Preview

Unity Editor 下通过 **libpag Web SDK + Puppeteer(Chromium)** 渲染真实 `.pag` 文件，并在 Game 视图 overlay 显示。

## 首次使用

```bash
cd Tools/PagPreview
npm install
```

依赖：`libpag`、`puppeteer`（会自动下载 Chromium，体积较大，仅开发机需要）。

## 运行方式

1. Unity 菜单 **Tools → PAG → 启动预览服务**（可选，Play 时也会自动启动）
2. 进入 Play Mode，调用 `PagController.PlayPag("transition_bmp.pag", "full")`
3. Game 视图上方会出现 PAG overlay 动画

## 手动启动服务

```bash
cd Tools/PagPreview
node server.mjs
```

健康检查：`http://127.0.0.1:17420/health`

## 说明

- Editor 预览走 HTTP 拉帧（约 30fps），性能低于 Android 原生 overlay。
- 文本替换 / 多 PAG 合成 / 导出视频等高级能力目前仍只在 Android 真机可用。
- 关闭 Unity 或菜单 **Tools → PAG → 停止预览服务** 可结束 Node 进程。
