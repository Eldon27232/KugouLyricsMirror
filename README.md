# KugouLyricsMirror

KugouLyricsMirror converts an existing desktop lyrics window into a normal capturable window named:

```text
Lyrics Mirror Preview
```

It is mainly for VR users who want to capture desktop lyrics in SteamVR, OVR Toolkit, Desktop+, VRChat, or similar overlays.

Many music apps, including Kugou and NetEase Cloud Music, render desktop lyrics as tool windows, overlays, or special transparent windows. SteamVR may not list those windows, may capture a black rectangle, or may fail to capture them reliably. KugouLyricsMirror does not read lyrics APIs and does not modify the player. It only captures or mirrors the lyrics window or a selected screen region, then outputs a regular top-level preview window for VR tools to capture.

## Features

### Window Capture Chroma Key (Recommended)

`窗口捕获抠色` is the recommended mode for desktop lyrics.

- Scans top-level desktop windows, including tool windows.
- Binds a source lyrics window by HWND.
- Captures the source window directly by HWND, with `PrintWindow` first and `GetWindowDC + BitBlt` as fallback.
- Removes the black lyrics background with a conservative threshold.
- Outputs the result to `Lyrics Mirror Preview`.
- Supports `VR 叠加模式`, enabled by default:
  - follows the source lyrics window position and size;
  - overlays the source window;
  - allows mouse click-through;
  - uses soft no-focus behavior while moving with `SWP_NOACTIVATE`;
  - does not use `WS_EX_NOACTIVATE`, because that breaks SteamVR capture on some systems.

When `VR 叠加模式` is turned off, the preview becomes a normal draggable window while still capturing the source HWND.

### DWM Window Proxy

`窗口代理 DWM` maps the selected source window into `Lyrics Mirror Preview` using a DWM thumbnail.

Use it when direct window proxying is enough. It does not chroma-key the image and does not make a black background transparent.

### Region Chroma Key Fallback

`区域抠色` is the fallback mode.

- Captures a manually selected screen region.
- Supports automatic background color detection.
- Supports manual eyedropper color selection.
- Supports RGB and hex color input.
- Uses the configured threshold to remove the selected background color.

Use this mode when a player window cannot be captured by HWND or proxied by DWM.

## Recommended Usage

1. Open desktop lyrics in your music app.
2. Start KugouLyricsMirror.
3. Select `窗口捕获抠色`.
4. Click `扫描窗口`.
5. Select a lyrics window, for example `桌面歌词 - 酷狗音乐` or a NetEase desktop lyrics window.
6. Click `绑定窗口`.
7. Keep `高级：VR 叠加模式` enabled for VR overlay use.
8. Click `启动镜像`.
9. In SteamVR / OVR Toolkit / Desktop+, capture:

```text
Lyrics Mirror Preview
```

Do not capture the original player or original lyrics window.

## Color Notes

For `窗口捕获抠色`, the black background removal key is internally fixed to black:

```text
RGB(0, 0, 0)
```

The default threshold is conservative, usually around `16`, to avoid eating black outlines or shadows in lyrics.

For `区域抠色`, enable `自动` to let the app estimate the background color from the selected region. Disable it if you want to use the eyedropper or enter RGB / hex values manually.

## Important Notes

- Do not enable `ExcludeFromCapture`; SteamVR may capture black or fail to capture the window.
- `Lyrics Mirror Preview` is a normal top-level app window, not a tool window.
- `窗口代理 DWM` does not remove black backgrounds.
- Some player windows cannot be captured by `PrintWindow`; try DWM proxy or region fallback in that case.
- KugouLyricsMirror does not read Kugou, NetEase, or other lyrics APIs.
- KugouLyricsMirror does not modify the music player.

## How It Works

KugouLyricsMirror enumerates desktop top-level windows and lets you bind a source lyrics HWND. Depending on the selected mode:

1. `窗口捕获抠色` captures the source HWND, removes the black background, and renders to `Lyrics Mirror Preview`.
2. `窗口代理 DWM` registers a DWM thumbnail from the source HWND to `Lyrics Mirror Preview`.
3. `区域抠色` captures a selected screen region and removes a detected or selected background color.

All VR tools should capture `Lyrics Mirror Preview`.

## Build

Requirements:

- Windows 10 / 11
- .NET 10 SDK

Run locally:

```powershell
dotnet run
```

If `dotnet` is not on PATH but installed in the default location:

```powershell
& "C:\Program Files\dotnet\dotnet.exe" run
```

Publish a self-contained single-file EXE:

```powershell
.\scripts\publish.ps1
```

Output:

```text
dist\win-x64\KugouLyricsMirror.exe
```

The publish script targets `win-x64`, uses `--self-contained true`, and produces a single-file executable.

## License

MIT
