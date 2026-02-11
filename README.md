# 头文字D 追逐战系统 (Chase Battle System)

## 概述
ChaseBattlePlugin 是一个为 AssettoServer 开发的插件，旨在实现类似《头文字D》风格的山路追逐战系统。它通过服务端插件管理比赛逻辑，并通过嵌入的 Lua 脚本在客户端展示视觉效果。

## 主要特性
- **服务端管理**: 自动处理比赛状态、判定胜负。
- **自动分发**: 客户端 Lua 脚本嵌入在插件 DLL 中，由服务器自动分发，无需玩家手动安装。
- **.NET 9.0**: 基于最新的 .NET 9.0 构建，利用高性能特性。

## 技术架构

```mermaid
graph TD
    ClientA[玩家 A (客户端)] <-->|Lua 脚本视觉效果| ClientA_Lua
    ClientB[玩家 B (客户端)] <-->|Lua 脚本视觉效果| ClientB_Lua
    Server[AssettoServer] <-->|插件逻辑| ChasePlugin
    ChasePlugin -- "指令 / 状态更新" --> ClientA
    ChasePlugin -- "指令 / 状态更新" --> ClientB
```

## 开发环境要求
- **.NET SDK**: 必须安装 **.NET 9.0 SDK** (与 AssettoServer 一致)。
- **AssettoServer**: 需要引用 AssettoServer 的核心库。

## 项目结构
项目位于 `AssettoServerSource/ChaseBattlePlugin`，与 AssettoServer 源码结构保持一致。

```
ChaseBattlePlugin/
├── ChaseBattlePlugin.csproj       # 项目文件 (嵌入 Lua 资源)
├── ChaseBattlePlugin.cs           # 插件入口 (注册脚本)
├── ChaseManager.cs                # 核心逻辑
└── client/
    └── lua/
        └── chase_battle/
            └── chase_battle.lua   # 客户端 Lua 脚本 (嵌入式)
```

## 编译指南

1.  进入项目目录:
    ```bash
    cd AssettoServerSource/ChaseBattlePlugin
    ```

2.  执行构建命令:
    ```bash
    dotnet build
    ```

3.  构建成功后，生成的 DLL 文件位于 `bin/Debug/net9.0/` (或 `Release` 目录)。

## 安装部署

由于采用了脚本自动分发机制，安装非常简单：

1.  **服务端**:
    - 将编译生成的 `ChaseBattlePlugin.dll` (及其依赖) 放入 AssettoServer 的 `plugins` 目录。
    - 确保 `AssettoServer` 正常运行。

2.  **客户端**:
    - **无需手动安装**: 玩家进入服务器时，CSP (Custom Shaders Patch) 会自动加载服务端分发的 `chase_battle.lua` 脚本。

## 详细设计 (参考)

### 服务端插件 (`ChaseBattlePlugin.cs`)
插件实现了 `IHostedService` 或 `BackgroundService`，并在启动时通过 `CSPServerScriptProvider` 注册嵌入的 Lua 脚本。

```csharp
// 自动注册嵌入的 Lua 脚本
scriptProvider.AddScript(
    Assembly.GetExecutingAssembly().GetManifestResourceStream("ChaseBattlePlugin.client.lua.chase_battle.chase_battle.lua")!,
    "chase_battle.lua"
);
```

### 客户端脚本 (`chase_battle.lua`)
负责接收服务端的状态更新，并在本地绘制 UI (如距离条、胜负提示)。

## 贡献
欢迎提交 Pull Request 改进代码或增加新功能 (如更复杂的胜负判定逻辑、更多 UI 特效)。
