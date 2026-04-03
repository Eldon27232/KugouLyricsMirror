# KugouLyricsMirror

把酷狗桌面歌词所在的屏幕区域，镜像到一个 SteamVR 能识别的普通窗口里。

## 现在的目录结构

```text
KugouLyricsMirror/
├─ Program.cs
├─ KugouLyricsMirror.csproj
├─ app.manifest
├─ README.md
├─ DEVELOPING.md
├─ LICENSE
├─ Forms/
│  ├─ ColorPickerForm.cs
│  ├─ ControlForm.cs
│  ├─ PreviewForm.cs
│  └─ RegionPickerForm.cs
├─ Core/
│  ├─ AppConfig.cs
│  ├─ ChromaKeyProcessor.cs
│  └─ ScreenCapture.cs
├─ Interop/
│  └─ NativeMethods.cs
├─ Models/
│  └─ Config.cs
└─ scripts/
   ├─ build.bat
   ├─ publish.ps1
   └─ package.bat
```

## 运行
```powershell
dotnet run
```

## 生成 exe
```powershell
.\scripts\publish.ps1
```

生成后会在：

```text
dist\win-x64\KugouLyricsMirror.exe
```

## 打 zip 包
```bat
scripts\package.bat
```

打完会在：

```text
release\KugouLyricsMirror-win-x64.zip
```

## 说明
- `publish.ps1`：用 `dotnet publish` 生成单文件 exe
- `package.bat`：把 `dist\win-x64\` 里的发布结果压缩成 zip
- 你要“exe”的话，重点用 `publish.ps1`
- 你要“方便发给别人下载”的压缩包，再用 `package.bat`
