<div align="center">

# 🐧 RunLoki365

### **Animated System Monitor for Linux**

*Monitor your system with style! RunLoki365 displays CPU, memory, and storage usage with an animated character in your system tray.*

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/platform-Linux-FCC624?logo=linux&logoColor=black)](https://www.linux.org/)
[![GTK](https://img.shields.io/badge/GTK-3.24-7FE719?logo=gnome)](https://www.gtk.org/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Serilog](https://img.shields.io/badge/Serilog-4.1-00ADD8)](https://serilog.net/)

![Status](https://img.shields.io/badge/Status-Production%20Ready-success?style=for-the-badge)

[Features](#-features) • [Installation](#-installation) • [Usage](#-usage) • [Building](#-building-from-source) • [Contributing](#-contributing)

</div>

---

## ✨ Features

<table>
<tr>
<td width="50%">

### 📊 **Real-time Monitoring**
- 🔥 CPU usage tracking with exponential smoothing
- 💾 Memory usage from `/proc/meminfo`
- 💿 Storage usage via `statvfs()` system call
- ⚡ Live tooltip updates every second

</td>
<td width="50%">

### 🎨 **Beautiful Animations**
- 🏃 Animated tray icon (speeds up with CPU!)
- 🐱 **Cat** - Classic runner (5 frames)
- 🐴 **Horse** - Galloping runner (14 frames)
- 🦜 **Parrot** - Flying runner (10 frames)
- 🌓 Auto light/dark theme variants

</td>
</tr>
<tr>
<td width="50%">

### ⚙️ **Highly Customizable**
- 🎯 Choose your favorite runner
- 🎞️ FPS limits: 10 / 20 / 30 / 40
- 🎨 Theme override (Light/Dark/Auto)
- 🚀 Launch at startup option
- 💾 Persistent settings in JSON

</td>
<td width="50%">

### 💪 **Performance**
- 🪶 Lightweight (~170 MB RAM)
- ⚡ Low CPU usage (<5%)
- 📦 Single-file deployment (26 MB)
- 🔧 Self-contained .NET runtime
- 🐧 Native Linux integration

</td>
</tr>
</table>

---

## 📥 Installation

### 🎯 Quick Install (Debian/Ubuntu)

```bash
# Download the latest release
wget https://github.com/mohamedEmad23/RunLoki365/releases/download/v1.0.0/runloki365_1.0.0_amd64.deb

# Install the package
sudo dpkg -i runloki365_1.0.0_amd64.deb
sudo apt-get install -f  # Install dependencies if needed
```

### 📋 Dependencies

<div align="center">

| Package | Version | Purpose |
|---------|---------|---------|
| ![GTK](https://img.shields.io/badge/libgtk--3--0-≥3.24-7FE719?logo=gnome) | ≥ 3.24 | GUI framework |
| ![AppIndicator](https://img.shields.io/badge/libayatana--appindicator3-≥0.5-00ADD8) | ≥ 0.5 | System tray icon |

</div>

### 🔧 GNOME Users

GNOME requires an extension to show system tray icons:

```bash
sudo apt install gnome-shell-extension-appindicator
```

Or install from [GNOME Extensions](https://extensions.gnome.org/extension/615/appindicator-support/).

---

## 🚀 Usage

### Starting the Application

```bash
# Run from terminal
runloki365

# Or launch from Applications menu
# Look for "RunLoki365" in System Tools
```

### 🎮 Interactive Menu

**Right-click** the animated icon to access:

- **Runners** → Switch between Cat 🐱, Horse 🐴, Parrot 🦜
- **Theme** → Choose Light, Dark, or System Auto
- **FPS Limit** → Adjust animation speed
- **Launch at Startup** → Auto-start on login
- **About** → Version and build information

### 📂 File Locations

```bash
~/.config/runloki365/settings.json    # User settings
~/.local/share/runloki365/logs/       # Application logs
~/.config/autostart/runloki365.desktop # Startup configuration
```

---

## 🛠️ Building from Source

### Prerequisites

<div align="center">

![.NET SDK](https://img.shields.io/badge/.NET%20SDK-9.0-512BD4?style=for-the-badge&logo=dotnet)
![GTK Dev](https://img.shields.io/badge/GTK--3--dev-≥3.24-7FE719?style=for-the-badge&logo=gnome)
![Git](https://img.shields.io/badge/Git-Latest-F05032?style=for-the-badge&logo=git)

</div>

```bash
# Install dependencies
sudo apt install dotnet-sdk-9.0 libgtk-3-dev libayatana-appindicator3-dev

# Clone repository
git clone https://github.com/mohamedEmad23/RunLoki365.git
cd RunLoki365/RunLoki365
```

### 🔨 Build Options

```bash
# Quick build (development)
dotnet build -c Release

# Create .deb package (production)
./build-package.sh

# Manual publish
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
```

---

## 🧪 Tech Stack

<div align="center">

| Technology | Usage |
|------------|-------|
| ![C#](https://img.shields.io/badge/C%23-12.0-239120?logo=csharp&logoColor=white) | Primary language |
| ![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white) | Runtime framework |
| ![GTK](https://img.shields.io/badge/GTK-3.24-7FE719?logo=gnome&logoColor=white) | GUI toolkit |
| ![Serilog](https://img.shields.io/badge/Serilog-4.1-00ADD8) | Structured logging |
| ![JSON](https://img.shields.io/badge/System.Text.Json-9.0-000000?logo=json) | Configuration |
| ![DI](https://img.shields.io/badge/Dependency%20Injection-9.0-5C2D91) | Architecture pattern |

</div>

### 🏗️ Architecture

```
┌─────────────────────────────────────────────┐
│         ApplicationController               │
│  (Orchestrates all services & lifecycle)    │
└─────────────────┬───────────────────────────┘
                  │
      ┌───────────┴───────────┐
      │                       │
┌─────▼──────┐        ┌──────▼─────────┐
│  Services  │        │   Monitors     │
├────────────┤        ├────────────────┤
│ AppInd     │        │ CPU (procstat) │
│ Animation  │        │ Memory (procfs)│
│ Theme      │        │ Storage (vfs)  │
│ Settings   │        │ SystemMonitor  │
│ Startup    │        └────────────────┘
└────────────┘
      │
┌─────▼──────┐
│  Runners   │
├────────────┤
│ Cat (5f)   │
│ Horse (14f)│
│ Parrot(10f)│
└────────────┘
```

---

## 🤝 Contributing

Contributions are welcome! Here's how you can help:

1. 🍴 **Fork** the repository
2. 🌿 **Create** a feature branch (`git checkout -b feature/AmazingFeature`)
3. 💾 **Commit** your changes (`git commit -m 'Add AmazingFeature'`)
4. 📤 **Push** to the branch (`git push origin feature/AmazingFeature`)
5. 🎉 **Open** a Pull Request

See [CONTRIBUTING.md](CONTRIBUTING.md) for detailed guidelines.

---

## 📝 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

```
MIT License

Copyright (c) 2025 mohamedEmad23

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction...
```

---

## 🙏 Credits & Acknowledgments

<div align="center">

**Inspired by** [RunCat for Windows](https://github.com/Kyome22/RunCat_for_windows) by **Takuto Nakamura**

**Animal Animations** from **RunCat365** project

**Linux Port & Development** by **[@mohamedEmad23](https://github.com/mohamedEmad23)**

---

### 🌟 Support

If you find this project useful, please consider giving it a ⭐ on GitHub!

[![GitHub stars](https://img.shields.io/github/stars/mohamedEmad23/RunLoki365?style=social)](https://github.com/mohamedEmad23/RunLoki365/stargazers)
[![GitHub forks](https://img.shields.io/github/forks/mohamedEmad23/RunLoki365?style=social)](https://github.com/mohamedEmad23/RunLoki365/network/members)

</div>

---

<div align="center">

**Made with ❤️ for the Linux community**

![Tux](https://img.shields.io/badge/🐧-Linux-FCC624?style=for-the-badge)
![Open Source](https://img.shields.io/badge/💚-Open%20Source-success?style=for-the-badge)

</div>
