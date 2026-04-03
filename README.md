# KugouLyricsMirror

将音乐软件的桌面歌词镜像为一个**可被 SteamVR 识别的透明窗口**。  
Mirror desktop lyrics from music apps into a **transparent window that SteamVR can capture**.

这个工具适合在 **VRChat / SteamVR / OVR Toolkit / Desktop+** 中使用，把桌面歌词固定到手柄、手腕或视野边缘，而不用把整个播放器窗口一起显示出来。  
This tool is designed for **VRChat / SteamVR / OVR Toolkit / Desktop+**, so you can pin lyrics to your controller, wrist, or view without capturing the entire player window.

---

## 功能 / Features

- 捕获指定屏幕区域  
  Capture a selected screen region
- 吸管取背景色  
  Pick a background color with an eyedropper
- 阈值抠除背景  
  Remove background by color threshold
- 输出无边框透明预览窗  
  Output a borderless transparent preview window
- 预览窗可被 SteamVR 捕获  
  Make the preview window capturable by SteamVR
- 左键拖动预览窗  
  Drag the preview window with left mouse button
- 右键隐藏 / 退出  
  Right-click to hide / exit
- 自动保存配置  
  Automatically save configuration

---

## 适用场景 / Use Cases

- 在 VRChat 里边玩边看歌词  
  View lyrics while playing in VRChat
- SteamVR 抓不到桌面歌词  
  SteamVR cannot detect desktop lyrics directly
- 不想固定整个音乐播放器窗口  
  You do not want to pin the whole music player window
- 想只显示歌词本身，减少遮挡  
  You only want the lyrics themselves with minimal obstruction

---

## 已测试 / Tested

已在以下场景验证可用：  
Tested working in the following scenarios:

- 酷狗音乐桌面歌词 / Kugou Music desktop lyrics
- 网易云音乐桌面歌词 / NetEase Cloud Music desktop lyrics
- SteamVR
- OVR Toolkit
- Desktop+

---

## 工作原理 / How It Works

有些桌面歌词并不是标准应用窗口，所以 SteamVR 往往抓不到。  
Some desktop lyric overlays are not standard application windows, so SteamVR often cannot detect them.

`KugouLyricsMirror` 的做法是：  
`KugouLyricsMirror` works by:

1. 捕获桌面上歌词所在的区域  
   Capturing the screen region where lyrics are displayed
2. 按背景色进行透明抠除  
   Removing the selected background color
3. 输出到一个普通透明窗口  
   Rendering the result into a normal transparent window
4. 让 SteamVR / OVR Toolkit / Desktop+ 去抓这个新窗口  
   Letting SteamVR / OVR Toolkit / Desktop+ capture that new window

它**不会读取播放器内部歌词数据**，也**不会修改播放器本体**。  
It **does not read internal lyric data** from the player, and **does not modify the player itself**.

---

## 下载 / Download

你可以在仓库的 **Releases** 页面下载已经打包好的版本。  
You can download prebuilt releases from the **Releases** page of this repository.

---

## 使用方法 / Usage

### 1. 打开桌面歌词 / Enable desktop lyrics
先让你的音乐软件把歌词显示在桌面上，并把它放到一个固定位置。  
First, make sure your music app shows lyrics on the desktop and place them in a fixed position.

### 2. 启动程序 / Launch the app
运行 `KugouLyricsMirror.exe`。  
Run `KugouLyricsMirror.exe`.

### 3. 框选歌词区域 / Select lyric region
点击 **框选区域**，框住桌面歌词所在的位置。  
Click **框选区域** to select the lyric area on screen.

### 4. 吸取背景色 / Pick background color
点击 **吸管取色**，选择歌词背后的背景颜色。  
Click **吸管取色** and pick the background color behind the lyrics.

### 5. 调整阈值 / Adjust threshold
阈值越高，抠除越激进；阈值越低，抠除越保守。  
Higher threshold removes more aggressively; lower threshold is more conservative.

建议先从 **24 ~ 48** 开始尝试。  
A good starting range is **24 ~ 48**.

### 6. 启动镜像 / Start mirroring
点击 **启动镜像**，程序会弹出一个透明预览窗。  
Click **启动镜像** to open a transparent preview window.

### 7. 在 SteamVR 中捕获 / Capture in SteamVR
然后在以下工具中捕获 `Lyrics Mirror Preview` 窗口：  
Then capture the `Lyrics Mirror Preview` window in:

- SteamVR
- OVR Toolkit
- Desktop+

---

## 自行编译 / Build From Source

### 环境要求 / Requirements

- Windows 10 / 11
- .NET 8 SDK

### 本地运行 / Run Locally

```powershell
dotnet run
