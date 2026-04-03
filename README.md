# KugouLyricsMirror

把 **音乐软件桌面歌词** 转成一个 **可被 SteamVR 识别的透明窗口**。

这个工具适合在 **SteamVR / OVR Toolkit / Desktop+** 里使用，把桌面歌词固定到手柄、手腕或视野边缘，而不用把整个播放器窗口一起显示出来。

---

## 这是什么

有些桌面歌词并不是标准应用窗口，所以 SteamVR 往往抓不到。  
`KugouLyricsMirror` 的做法是：

1. 捕获桌面上歌词所在的区域  
2. 按背景色进行透明抠除  
3. 输出到一个普通窗口  
4. 让 SteamVR / OVR Toolkit / Desktop+ 去抓这个新窗口

它**不会读取酷狗内部歌词数据**，也**不会修改酷狗本体**。  
本质上是一个“歌词区域镜像器”。

---

## 功能

- 捕获指定屏幕区域
- 吸管取背景色
- 阈值抠除背景
- 输出无边框透明预览窗
- 预览窗可被 SteamVR 捕获
- 左键拖动预览窗
- 右键隐藏 / 退出
- 自动保存配置

---

## 适用场景

- 在 VRChat 里边玩边看歌词
- SteamVR 抓不到酷狗桌面歌词
- 不想固定整个酷狗窗口
- 想只显示歌词本身，尽量减少遮挡

---

## 截图

<img width="520" height="395" alt="image" src="https://github.com/user-attachments/assets/f6c593b1-fc4a-4f9d-a2dd-8beff6e9a502" />
![55b96bcf32491a1b55a99aa559bcfcab](https://github.com/user-attachments/assets/d51f6087-07c2-4538-a0d6-c5cd72215252)

---

## 使用方法

### 1. 打开酷狗桌面歌词
先让酷狗把歌词显示在桌面上，并把它放到一个固定位置。

### 2. 启动本程序
运行 `KugouLyricsMirror.exe`。

### 3. 框选歌词区域
点击 **框选区域**，框住桌面歌词所在位置。

### 4. 吸取背景色
点击 **吸管取色**，选择歌词背后的背景颜色。

一般可以吸：
- 黑色背景
- 深灰背景
- 某块固定纯色背景

### 5. 调整阈值
阈值越高，抠除越激进；阈值越低，抠除越保守。

建议先从 **24 ~ 48** 开始尝试。

### 6. 启动镜像
点击 **启动镜像**，程序会弹出一个透明预览窗。

### 7. 在 SteamVR 中捕获预览窗
然后在：
- SteamVR
- OVR Toolkit
- Desktop+

里抓取 `Lyrics Mirror Preview` 这个窗口即可。

---

## 下载

你可以在 GitHub Releases 页面下载已经打包好的版本。

如果没有现成发布包，也可以自己编译。

---

## 自行编译

### 环境要求
- Windows 10 / 11
- .NET 8 SDK

### 运行
```powershell
dotnet run
