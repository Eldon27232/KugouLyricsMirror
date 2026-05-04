# KugouLyricsMirror

将音乐软件的桌面歌词窗口转换 / 镜像为一个 **SteamVR、OVR Toolkit、Desktop+ 更容易捕获的普通透明窗口**。

Mirror desktop lyrics from music apps into a normal transparent window that SteamVR / OVR Toolkit / Desktop+ can capture.

很多桌面歌词，比如酷狗音乐、网易云音乐的桌面歌词，并不一定是标准应用窗口。它们可能是工具窗口、特殊 overlay 或透明悬浮窗，SteamVR 经常抓不到，或者只能抓到整块黑底。

`KugouLyricsMirror` 的目标是把这些已有桌面歌词窗口或区域画面，输出为普通顶层窗口：

```text
Lyrics Mirror Preview
```

你在 SteamVR / OVR Toolkit / Desktop+ 里捕获这个预览窗口，而不是直接捕获原播放器或原歌词窗口。

它不读取酷狗、网易云或其他播放器的歌词 API，也不会修改播放器本体。

---

## 功能 / Features

### 窗口捕获抠色 / Window Capture Chroma Key

推荐优先使用。

- 扫描桌面顶层窗口，包括工具窗口
- 绑定酷狗 / 网易云等桌面歌词窗口 HWND
- 按源窗口 HWND 捕获窗口内容，不走屏幕区域截图
- 抠除黑色或指定背景色
- 输出透明的 `Lyrics Mirror Preview`
- 预览窗口自动覆盖源歌词窗口的位置和大小
- 源窗口移动或缩放时，预览窗口跟随
- 预览窗口鼠标穿透、不抢焦点

This mode captures the lyric window by HWND, chroma-keys the background, and renders the result into `Lyrics Mirror Preview`.

### 窗口代理 DWM / DWM Window Proxy

- 通过 DWM thumbnail 将源窗口代理到 `Lyrics Mirror Preview`
- 适合源窗口本身就可以直接显示的情况
- 不负责把黑色背景变透明

Use this when directly proxying the source window is enough. It does not remove the background.

### 区域抠色 / Region Chroma Key

保留作为 fallback。

- 手动框选屏幕区域
- 吸管取背景色
- 通过阈值抠除背景
- 输出透明预览窗口

Use this when window capture or DWM proxy does not work for a specific player window.

---

## 推荐使用顺序 / Recommended Order

1. 先试 `窗口捕获抠色`
2. 如果捕获不到内容，再试 `窗口代理 DWM`
3. 如果窗口模式都不可用，最后使用 `区域抠色`

SteamVR / OVR Toolkit / Desktop+ 中应捕获：

```text
Lyrics Mirror Preview
```

不要捕获原始歌词窗口。

---

## 使用方法 / Usage

### 1. 打开桌面歌词

先在音乐软件里打开桌面歌词，例如：

- `桌面歌词 - 酷狗音乐`
- 网易云音乐桌面歌词窗口

### 2. 启动 KugouLyricsMirror

运行：

```text
KugouLyricsMirror.exe
```

### 3. 选择捕获模式

优先选择：

```text
窗口捕获抠色
```

### 4. 扫描并绑定歌词窗口

点击 `扫描窗口`，在窗口列表里找到类似酷狗 / 网易云桌面歌词的窗口。

窗口列表会显示：

- 标题
- 类名
- HWND
- 可见性
- 窗口矩形
- 扩展样式

选择目标窗口后点击 `绑定窗口`。

### 5. 调整背景色和阈值

黑底歌词窗口通常使用：

```text
Key color: RGB(0, 0, 0)
Threshold: 16
```

建议从 `12 ~ 24` 开始调。

阈值越高，抠除越激进；阈值过高可能吃掉歌词的黑色描边或阴影。

### 6. 启动镜像

点击 `启动镜像`。

程序会弹出：

```text
Lyrics Mirror Preview
```

在 `窗口捕获抠色` 模式下，这个预览窗口会自动叠到源歌词窗口上方，并跟随源窗口移动 / 缩放。它会鼠标穿透，不挡点击，也不抢焦点。

### 7. 在 VR 工具里捕获

在以下工具中捕获 `Lyrics Mirror Preview`：

- SteamVR
- OVR Toolkit
- Desktop+

---

## 注意事项 / Notes

- 不要开启 `预览窗不参与屏幕捕获` / `ExcludeFromCapture`，否则 SteamVR 可能抓黑或抓不到。
- `窗口代理 DWM` 不会自动把黑底变透明。
- 个别播放器窗口可能无法被 `PrintWindow` 捕获，需要切换到 `窗口代理 DWM` 或 `区域抠色`。
- SteamVR 应捕获 `Lyrics Mirror Preview`，不是原始歌词窗口。
- `Lyrics Mirror Preview` 是普通 top-level window，不是 tool window。

---

## 工作原理 / How It Works

`KugouLyricsMirror` 会枚举当前桌面顶层窗口，包括常见工具窗口。用户绑定源歌词窗口 HWND 后，程序根据模式处理画面：

1. `窗口捕获抠色`：按 HWND 捕获源窗口内容，抠除指定背景色，再绘制到透明预览窗口。
2. `窗口代理 DWM`：使用 DWM thumbnail 将源窗口代理到普通预览窗口。
3. `区域抠色`：捕获用户框选的屏幕区域，按背景色阈值抠除。

最终输出都是普通窗口：

```text
Lyrics Mirror Preview
```

让 SteamVR / OVR Toolkit / Desktop+ 捕获这个窗口。

---

## 已测试场景 / Tested

已在以下方向验证或设计支持：

- 酷狗音乐桌面歌词 / Kugou Music desktop lyrics
- 网易云音乐桌面歌词 / NetEase Cloud Music desktop lyrics
- SteamVR
- OVR Toolkit
- Desktop+

不同播放器窗口实现不一样。如果某个模式不可用，按推荐顺序切换模式。

---

## 构建 / Build From Source

### 环境要求 / Requirements

- Windows 10 / 11
- .NET 10 SDK

### 本地运行 / Run Locally

```powershell
dotnet run
```

如果 `dotnet` 不在 PATH，但安装在默认位置：

```powershell
& "C:\Program Files\dotnet\dotnet.exe" run
```

### 发布单文件 EXE / Publish Single-File EXE

```powershell
.\scripts\publish.ps1
```

输出位置：

```text
dist\win-x64\KugouLyricsMirror.exe
```

发布脚本使用：

- `win-x64`
- `--self-contained true`
- `PublishSingleFile=true`

---

## License

MIT
