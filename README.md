# AFR — CAD 缺失字体自动替换工具

AutoCAD 插件，自动检测并替换图纸中缺失的 SHX / TrueType / 大字体，针对MText内联文字（多行文本）会使用HOOK重定向自动映射字体。

**注：本插件并不会主动关闭图纸打开时选择缺失字体的对话框，如果要使用本插件，建议CAD弹出缺少SHX文件对话框时选择`忽略缺少的SHX文件并继续`并勾选`始终执行我的当前选择`。**

## 插件命名说明

以 `AFR-ACAD2026` 为例：

| 部分 | 含义 |
|---|---|
| `AFR` | 插件代号（Auto Font Replace 的缩写，可忽略） |
| `A` | **A**utodesk |
| `CAD2026` | CAD **版本号** |

> 💡 请根据自己安装的 CAD 版本下载对应的 DLL 文件。

## 目前支持的 AutoCAD 版本

| DLL 文件名 | AutoCAD 版本 | .NET | 注册表路径 |
|---|---|---|---|
| `AFR-ACAD2026.dll` | AutoCAD **2026**（R25.1） | .NET 8 | `R25.1\ACAD-xxxx:xxx` |
未来开发计划：
AutoCAD2025
AutoCAD2024
AutoCAD2023
AutoCAD2022

中望CAD2026
已完成支持
AutoCAD2026


插件UI界面采用现代化设计

AFR命令UI
![](https://splrad-img.oss-cn-chengdu.aliyuncs.com/20260407005000713.jpg)

AFRLOG命令UI
![](https://splrad-img.oss-cn-chengdu.aliyuncs.com/20260407005034079.jpg)

## 项目结构

```
src/
├── AFR.Core/              Shared Project — 接口、模型、基础服务（纯 .NET，无 CAD 依赖）
│   ├── Abstractions/        接口定义（ICadPlatform / IFontHook / ICadHost / IFontScanner / ILogService）
│   ├── Models/              数据模型（FontCheckResult / InlineFontFixRecord / StyleFontReplacement）
│   ├── Platform/            平台管理器（PlatformManager — 全局服务注册中心）
│   └── Services/            基础服务（ConfigService / RegistryService — 注册表配置读写）
├── AFR.UI/                Shared Project — WPF 用户界面（字体选择 / 替换日志 / MText 查看器）
│   ├── FontSelection/       AFR 命令的字体配置窗口
│   ├── FontLog/             AFRLOG 命令的替换日志窗口（支持逐行和批量替换）
│   └── MTextEditor/         AFRVIEW 命令的 MText 格式代码查看器（仅 Debug）
└── AutoCAD/
    ├── AFR.AutoCAD/        Shared Project — AutoCAD 通用逻辑
    │   ├── Commands/          命令定义（AFR / AFRLOG / AFRUNLOAD / AFRVIEW）
    │   ├── FontMapping/       字体 Hook 与 MText 内联字体解析
    │   ├── Hosting/           插件生命周期、事件注册、执行控制
    │   └── Services/          字体检测、替换、日志、诊断
    └── AFR-ACAD2026/       版本适配壳 — 仅 PluginEntry + 平台常量（2 个文件）
```

> 💡 AFR.Core、AFR.UI、AFR.AutoCAD 均为 Shared Project，所有源码在编译时直接嵌入最终的 `AFR-ACAD2026.dll`，实现**单 DLL 分发**。

## 功能特性

### 核心功能
- **自动检测** — 打开图纸时自动扫描所有文字样式，识别缺失的 SHX、TrueType、大字体
- **智能替换** — 将缺失字体统一替换为用户配置的默认字体，按类型分流：SHX 主字体 / SHX 大字体 / TrueType
- **二次验证** — 替换后自动重新检测，确认替换字体是否真正可用，避免"替换了但仍然缺失"的情况
- **替换日志** — `AFRLOG` 命令查看检测结果，支持逐行手动调整替换字体
- **批量填充** — 日志界面底部可按字体类型（SHX 主字体 / SHX 大字体 / TrueType）批量填充替换字体

### 高级功能
- **ldfile Hook** — 通过 Inline Hook 拦截 AutoCAD 底层的字体文件加载函数，在 DWG 解析阶段即完成大字体和 TrueType 的重定向
- **MText 内联字体修复** — 扫描所有多行文字（MText）中的 `\F` / `\f` 格式代码，与 Hook 重定向记录交叉比对，精确识别被修复的内联字体
- **SHX 类型校验** — 读取 SHX 文件头判断主字体/大字体类型，防止主字体槽位引用大字体文件导致的类型不匹配
- **残留引用清理** — 自动清除"TrueType 可用但 SHX 引用缺失"的残留引用，消除 ST 命令的"已修改"弹窗
- **替换字体预校验** — 替换前验证配置的替换字体是否可用，不可用时跳过并警告用户

### 平台特性
- **实时状态** — 重新打开日志时读取图纸中的当前实际字体，反映手动替换或 `ST` 命令修改后的状态
- **多文档支持** — MDI 环境下每个图纸独立处理，互不干扰
- **随 CAD 启动** — 首次 NETLOAD 后自动注册，后续启动 AutoCAD 时自动加载
- **完整卸载** — 提供 `AFRUNLOAD` 命令，一键注销事件并清除注册表项
- **单 DLL 分发** — HandyControl 等第三方依赖嵌入为程序集资源，仅需分发一个 DLL 文件

## 安装

1. 编译项目或获取 `AFR-ACAD2026.dll`
2. 启动 AutoCAD 2026
3. 命令行输入 `NETLOAD`，选择 `AFR-ACAD2026.dll`
4. 首次加载后插件会自动注册，后续启动 AutoCAD 将自动加载

## 命令

| 命令 | 说明 |
|---|---|
| `AFR` | 打开字体配置界面，选择替换用的 SHX 主字体、大字体和 TrueType 字体 |
| `AFRLOG` | 打开字体替换日志，查看缺失字体检测结果，支持手动调整和批量填充替换字体 |
| `AFRUNLOAD` | 完整卸载插件 — 注销事件、删除注册表项、清空运行状态 |
| `AFRVIEW` | 查看选中 MText 的格式代码（仅 Debug 构建可用，用于开发调试） |

## 使用流程

```
首次使用：
  NETLOAD → 选择 DLL → 输入 AFR → 配置 SHX 主字体、大字体和 TrueType 字体 → 确认

后续使用（自动）：
  打开 AutoCAD → 自动加载插件 → 打开图纸 → 自动检测并替换缺失字体

手动调整：
  输入 AFRLOG → 查看检测结果 → 逐行或批量调整替换字体 → 应用替换
```

## 技术架构

### 两阶段字体修复

插件采用两阶段协作的字体修复策略，覆盖从 DWG 文件解析到样式表修改的完整流程：

```
┌─ 阶段 1：DWG 解析阶段（ldfile Hook） ──────────────────────────────┐
│  拦截 AutoCAD 底层的字体文件加载函数                                  │
│  ├─ 缺失 SHX 大字体（param2=4）→ 重定向到配置的 BigFont              │
│  ├─ 缺失 TrueType 字族名       → 重定向到配置的 TrueType 字体        │
│  └─ 缺失 SHX 主字体（param2=0）→ 放行给 FONTALT 原生机制处理         │
└────────────────────────────────────────────────────────────────────┘
                                ↓
┌─ 阶段 2：Execute 阶段（FontReplacer） ─────────────────────────────┐
│  修改图纸 TextStyleTable 中的字体引用                                │
│  ├─ 检测缺失字体（FontDetector）                                    │
│  ├─ 预校验替换字体可用性                                             │
│  ├─ 按类型分流替换（SHX 主字体 / 大字体 / TrueType）                 │
│  ├─ 清理 TrueType 可用但 SHX 缺失的残留引用                         │
│  └─ 二次验证：替换后重新检测，确认修复效果                             │
└────────────────────────────────────────────────────────────────────┘
```

### 插件生命周期

```
AutoCAD 启动
  │
  ├─ PluginEntryBase.Initialize()
  │   ├─ 注册平台服务（PlatformManager）
  │   ├─ 安装 ldfile Hook（LdFileHook.Install）
  │   ├─ 写入注册表自动加载项
  │   └─ 注册 DocumentOpened 事件
  │
  ├─ 文档打开事件
  │   ├─ 配置未初始化 → 延迟入队，等待用户执行 AFR 命令后统一处理
  │   └─ 配置已初始化 → ExecutionController.Execute()
  │       ├─ 检测缺失字体
  │       ├─ 替换缺失字体
  │       ├─ 清理残留引用
  │       ├─ MText 内联字体扫描与比对
  │       ├─ 二次验证
  │       └─ 输出日志到命令行
  │
  └─ AFRUNLOAD 命令
      ├─ 注销所有事件监听
      ├─ 卸载 ldfile Hook
      ├─ 删除注册表自动加载项
      └─ 清空运行状态
```

### 关键设计决策

| 决策 | 原因 |
|---|---|
| TrueType 必须用 TrueType 替换，不能用 SHX | 若将 TrueType 误重定向为 SHX，会污染 AutoCAD 内部字体缓存，导致文字乱码 + ST 弹窗 |
| 常规 SHX 主字体（param2=0）不通过 Hook 重定向 | Hook 级别的重定向会干扰块参照的字体缓存渲染，交由 FONTALT 原生机制处理更稳定 |
| SHX 大字体（param2=4）必须通过 Hook 处理 | FONTALT 不区分大字体和主字体，无法正确替换大字体 |
| 原生字符串指针缓存不释放 | ldfile 可能将 fileName 指针存入全局字体表，释放后成为悬空指针导致崩溃 |
| FontDetectionContext 按事务隔离 | 不同图纸、不同执行次数之间 100% 内存隔离，避免缓存污染 |
| ShapeFile 样式始终跳过 | 替换 ShapeFile 样式会破坏复杂线型结构（ltypeshp.shx 等） |

### 配置界面

输入 `AFR` 命令后弹出字体选择窗口：

- **SHX主字体（西文）** — 用于替换缺失的 SHX 字体（必选）
- **SHX大字体（中文）** — 用于替换缺失的大字体（可选）
- **TrueType字体（中文）** — 用于替换缺失的 TrueType 字体（可选）
- 下拉列表自动扫描 AutoCAD Fonts 目录和系统已安装字体，支持搜索输入

### 替换日志界面

输入 `AFRLOG` 命令后弹出日志窗口：

- **状态条** — 显示 SHX 主字体（蓝）、SHX 大字体（橙）、TrueType（紫）的缺失数量统计，以及未替换数
- **数据表格** — 列出每个缺失字体的样式名、类型、缺失字体名称和替换字体选择框
  - ⚠ 前缀标记未成功替换的条目，已替换的条目显示当前实际字体
  - MText 内联字体映射记录单独展示（通过 Hook 重定向修复的内联 `\F` / `\f` 字体）
- **批量填充** — 底部三个下拉框可按类型批量填充替换字体，点击「填充」一次性填入所有对应行
- **应用替换** — 确认后写入当前图纸（不影响全局配置）
- 每次打开日志都会实时读取图纸中的当前字体状态

## 使用教程

如果你是第一次使用 AutoCAD 插件，请按以下步骤操作。

### 第一步：获取插件文件

1. 前往 [Releases](https://github.com/splrad/CADFontAutoReplace/releases) 页面
2. 根据你安装的 AutoCAD 版本，下载对应的 DLL 文件（例如 AutoCAD 2026 → `AFR-ACAD2026.dll`）
3. 保存到一个**固定位置**（建议不要放在桌面或临时文件夹）

> 💡 推荐路径：`D:\CADPlugins\AFR-ACAD2026.dll`

### 第二步：加载插件到 AutoCAD

1. 打开 **AutoCAD 2026**
2. 在底部命令行中输入 `NETLOAD`，按回车

   ```
   命令: NETLOAD
   ```

3. 在弹出的文件选择窗口中，找到并选择刚才保存的 `AFR-ACAD2026.dll`，点击"打开"
4. 如果弹出安全警告，选择 "始终加载"

> ✅ 加载成功后，命令行会显示插件初始化信息。此后每次打开 AutoCAD 都会自动加载，**无需重复操作**。



如果你安装的CAD字体超过100个，则建议安装本插件前删除CAD安装目录下Fonts文件夹中的所有SHX字体（sas_____.pfb、MstnFontConfig.xml、internat.rsc、font.rsc此类型文件不需要删除！） 并将本项目的CAD字体解压拷贝到此文件夹中。本字体包在CAD原版基础上只增加了必要的几款通用型字体（如探索者等），可放心使用。



### 第三步：首次配置替换字体

1. 在命令行输入 `AFR`，按回车

   ```
   命令: AFR
   ```

2. 弹出字体选择窗口：

   - **SHX主字体（西文）**— 在下拉框中选择一个字体，例如 `txt.shx` 或 `simplex.shx`。这个字体将用来替换所有缺失的 SHX 字体
   - **SHX大字体（中文）**— 如果你经常打开含中文的图纸，建议选择 `bigfont.shx` 或 `gbcbig.shx`
   - **TrueType字体（中文）**— 用于替换缺失的 TrueType 字体，建议选择 `宋体` 或 `黑体`
   - 下拉框支持直接输入搜索，比如输入 `txt` 就会筛选出 `txt.shx`

3. 选择完成后，点击 **确认** 按钮

4. 重启CAD（因为要安装HOOK模块，此模块会在图纸解析时重定向多行文本的缺失字体）

> ✅ 配置完成！此后打开任何图纸，插件都会自动替换缺失字体，不需要再做任何操作。

### 第四步：验证效果

打开一张有缺失字体的 DWG 文件，观察命令行底部的输出：

```
==========================================================================
AFR 缺失字体自动替换 v6.0
github.com/splrad/CADFontAutoReplace | gitee.com/splrad/CADFontAutoReplace
命令: AFR(配置) AFRLOG(日志) AFRUNLOAD(卸载)
==========================================================================
[字体修复]已替换缺失字体 3 个(SHX主字体:1,SHX大字体:1,TrueType:1)
```

如果看到类似的替换记录，说明插件正在正常工作。

### 第五步：手动调整（可选）

如果自动替换的字体不理想，可以使用 `AFRLOG` 命令进行手动调整：

1. 在命令行输入 `AFRLOG`，按回车
2. 查看每个缺失字体的当前替换状态
3. 逐行修改替换字体，或使用底部批量填充功能
4. 点击 **应用替换** 写入当前图纸

> 💡 `AFRLOG` 每次打开都会读取图纸中的实际字体状态，即使通过 CAD 的 `ST`（样式管理器）命令修改过，也会正确显示。



MText内联文字（多行文本）因为采用重定向的方案，不支持手动替换

### 如何修改字体配置？

随时在命令行输入 `AFR`，重新选择字体并确认即可。新配置会立即对当前图纸生效。

**注1：请在配置替换字体时清楚的知道SHX字体和大字体的区别，也能正确的选择字体，错误的选择可能导致替换后文字显示异常。**

**注2：如果在选择替换字体插件自动执行替换后发现字体显示异常，请不要保存图纸，而是使用`AFRLOG`命令打开日志窗口，选择其他字体进行替换，直到字体显示正常。**

### 如何卸载插件？

在命令行输入 `AFRUNLOAD`，按回车：

```
命令: AFRUNLOAD
```

插件会自动：
- 停止所有自动替换功能
- 删除注册表中的自动加载配置
- 下次启动 AutoCAD 不再自动加载

> 💡 如果需要重新安装，重启 AutoCAD 后再次 `NETLOAD` 即可。

## 开发说明

### 环境要求

- Visual Studio 2022 / 2026
- .NET 8 SDK
- AutoCAD 2026（运行时测试）

### 构建

打开 `CADFontAutoReplace.sln`，构建 `AFR-ACAD2026` 项目即可。所有 Shared Project 源码会自动编译进最终的 DLL。

### 添加新 AutoCAD 版本支持

1. 在 `src/AutoCAD/` 下创建新的版本目录（如 `AFR-ACAD2025/`）
2. 创建 `PluginEntry.cs` — 继承 `PluginEntryBase`，实现三个工厂方法
3. 创建 `AutoCad2025Platform.cs` — 实现 `ICadPlatform`，填入版本特定常量（注册表路径、DLL 名、导出符号、序言长度）
4. 创建 `.csproj`，导入三个 Shared Project 的 `.projitems`

### 调试日志

Debug 构建会在插件 DLL 所在目录生成 `AFR_Diag_*.log` 诊断日志，记录完整的字体检测、替换、Hook 重定向过程。日志自动按 10MB 分包，保留 7 天。

如果您感觉本插件对您有帮助，可以对开发者进行打赏，支持开发者继续开发
![](https://splrad-img.oss-cn-chengdu.aliyuncs.com/20260406215922295.jpg)

<h2 align="center">⭐ Star History</h2>

<p align="center">
  <a href="https://star-history.com/#splrad/CADFontAutoReplace&Date">
    <img src="https://api.star-history.com/svg?repos=splrad/CADFontAutoReplace&type=Date" />
  </a>
</p>