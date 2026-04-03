# Developing

## 目录职责

### Forms
放所有 WinForms 窗体：
- `ControlForm`：主控制面板
- `PreviewForm`：透明预览窗
- `RegionPickerForm`：框选区域
- `ColorPickerForm`：吸管工具

### Core
放不依赖具体 UI 的核心逻辑：
- `ScreenCapture`：抓屏
- `ChromaKeyProcessor`：按颜色扣背景
- `AppConfig`：配置读写

### Interop
放 Windows API P/Invoke：
- `NativeMethods`

### Models
放简单数据模型：
- `Config`

## publish.ps1 有什么用

它不是普通编译，而是 **发布**。

普通：
```powershell
dotnet build
```
只是在 `bin\Release\...` 里生成编译产物，通常还依赖本机 .NET 环境。

发布：
```powershell
.\scripts\publish.ps1
```
会执行：
- 指定 `win-x64`
- `--self-contained true`
- `PublishSingleFile=true`

结果就是直接给你一个 **可分发的 exe**，默认输出到：

```text
dist\win-x64\KugouLyricsMirror.exe
```

这才是你要发给别人的那种“像成品一样的 exe”。

## package.bat 在干什么

它会把：

```text
dist\win-x64\
```

里的东西压缩到：

```text
release\KugouLyricsMirror-win-x64.zip
```

所以如果你双击 `package.bat` 之后找不到包，就去项目目录下找：

```text
release\
```

## 最常用流程

### 本地开发
```powershell
dotnet run
```

### 做一个 exe
```powershell
.\scripts\publish.ps1
```

### 再压缩成 zip
```bat
scripts\package.bat
```

## 你如果只想要 exe
只跑这个就够了：

```powershell
.\scripts\publish.ps1
```

然后去这里拿：

```text
dist\win-x64\KugouLyricsMirror.exe
```
