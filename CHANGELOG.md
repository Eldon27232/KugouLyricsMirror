# Changelog

## v0.2.0

- Added `窗口捕获抠色`, the recommended mode for desktop lyrics. It captures the selected source lyrics window by HWND, removes the black background, and outputs to `Lyrics Mirror Preview`.
- Added `窗口代理 DWM`, which proxies a selected source window into `Lyrics Mirror Preview` through DWM thumbnails. This mode does not perform black-background transparency.
- Added and consolidated `VR 叠加模式` for window capture:
  - follows the source lyrics window;
  - overlays and resizes with the source window;
  - enables mouse click-through;
  - uses soft no-focus movement with `SWP_NOACTIVATE`;
  - avoids `WS_EX_NOACTIVATE`.
- Added automatic background color detection for `区域抠色`, while keeping manual eyedropper, RGB, hex, and threshold controls.
- Fixed SteamVR capture problems caused by `WS_EX_NOACTIVATE`; the preview window now rejects that style for capture modes.
- Migrated the project to .NET 10 / `net10.0-windows`.
- Improved `scripts/publish.ps1` so restore or publish failures stop the script instead of printing a false success message.
