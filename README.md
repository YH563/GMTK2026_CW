# 🎮 GameTemplate — GMTK Game Jam 2D 游戏模板

基于 **Godot 4.6 + C# (.NET 8)** 的 2D 游戏快速启动模板。

## 项目结构

```
GameTemplate/
├── project.godot              # 项目配置文件（1280×720 窗口 + 输入映射 + 自动加载）
├── GameTemplate.csproj         # .NET 项目文件
├── GameTemplate.sln            # .NET 解决方案
│
├── scenes/                    # ── 场景文件 ──
│   ├── Main.tscn              #   主菜单场景
│   └── Level.tscn             #   游戏关卡场景
│
└── src/                       # ── C# 源码 ──
    ├── Global.cs              #   [全局] 游戏管理器（自动加载）
    │
    ├── Player/
    │   └── Player.cs          #   GameTemplate.Player — 2D 角色控制器
    │
    ├── Camera/
    │   └── GameCamera.cs      #   GameTemplate.Camera — 摄像机跟随 + 震动
    │
    ├── Managers/
    │   └── AudioManager.cs    #   GameTemplate.Managers — 音频管理（自动加载）
    │
    ├── UI/
    │   ├── HUD.cs             #   GameTemplate.UI — 游戏内 HUD
    │   └── MainMenu.cs        #   GameTemplate.UI — 主菜单逻辑
    │
    └── Utils/
        └── Extensions.cs      #   GameTemplate.Utils — 工具扩展方法
```

## 快速开始

### 前置要求

- [Godot 4.6](https://godotengine.org/) 或更高版本
- [.NET 8 SDK](https://dotnet.microsoft.com/download)

### 启动项目

1. 用 **Godot 4.6** 打开本项目的 `project.godot`
2. 点击 ▶ 运行（或按 `F5`）
3. 主菜单会显示，点击 **Start Game** 进入关卡

## 核心系统

### 1. 全局游戏管理器 (`Global.cs`)

自动加载的全局单例，管理游戏核心状态。

| API | 功能 |
|-----|------|
| `Global.Instance` | 获取单例 |
| `AddScore(points)` | 增加分数 |
| `LoseLife()` | 减少一条生命 |
| `TriggerGameOver()` | 触发游戏结束 |
| `TogglePause()` | 切换暂停 |
| `ResetGame()` | 重置所有状态 |
| `LoadScene(path)` | 切换场景 |
| `ReloadCurrentScene()` | 重新加载当前场景 |
| `QuitGame()` | 退出游戏 |

**信号：**

| 信号 | 参数 | 说明 |
|------|------|------|
| `ScoreChanged` | `int newScore` | 分数变化 |
| `LivesChanged` | `int newLives` | 生命变化 |
| `GameOver` | — | 游戏结束 |
| `GamePaused` | — | 暂停 |
| `GameResumed` | — | 恢复 |

### 2. 角色控制器 (`Player.cs`)

支持两种移动模式，在 Inspector 面板切换：

| 属性 | 默认值 | 说明 |
|------|--------|------|
| `EnableJump` | `false` | `true`=平台跳跃, `false`=俯视角 |
| `Speed` | 200 | 移动速度 |
| `Acceleration` | 1200 | 加速度 |
| `Friction` | 800 | 摩擦/减速度 |
| `EnableDash` | `false` | 是否启用冲刺 |
| `DashSpeed` | 600 | 冲刺速度 |

### 3. 摄像机 (`GameCamera.cs`)

| 功能 | 方法 |
|------|------|
| 平滑跟随 | 设置 `Target` 节点 + `FollowSpeed` |
| 屏幕震动 | `Camera.Shake(intensity)` |
| 边界限制 | 启用 `UseLimits` 设置范围 |

### 4. 音频管理器 (`AudioManager.cs`)

自动加载的全局单例。

| API | 功能 |
|-----|------|
| `PlayMusic(stream, fadeTime)` | 播放背景音乐（支持淡入） |
| `StopMusic(fadeTime)` | 停止音乐（支持淡出） |
| `PlaySfx(stream)` | 播放音效（自动找空闲通道） |

### 5. HUD (`HUD.cs`）

游戏内覆盖层，显示分数、生命、计时器、暂停遮罩和 Game Over。

### 6. 工具扩展 (`Extensions.cs`)

| 方法 | 功能 |
|------|------|
| `GetRandomViewportPosition()` | 获取视口内随机位置 |
| `Shuffle()` | Fisher-Yates 打乱列表 |
| `RandomElement()` | 随机取一个元素 |
| `Wrap()` | 数值循环回绕 |
| `IsZero()` | 浮点数约等于零判断 |
| `FadeIn()` / `FadeOut()` | 控件淡入/淡出动画 |

## 输入映射

| 操作 | 按键 |
|------|------|
| 上移 | W / ↑ |
| 下移 | S / ↓ |
| 左移 | A / ← |
| 右移 | D / → |
| 跳跃 | 空格 |
| 冲刺 | Shift |
| 交互 | E |
| 暂停 | Escape |
| 确认 | Enter / 空格 |
| 取消 | Escape |

## 自定义指南

### 添加玩家动画

1. 在 `Player` 节点下选择 `AnimatedSprite2D`
2. 在 `SpriteFrames` 中创建 `idle` 和 `walk` 动画
3. 代码会自动根据移动状态切换动画

### 绘制关卡地形

1. 打开 `Level.tscn`
2. 在 `Ground` 节点上（或新建 TileMapLayer）绘制地形
3. 为墙壁添加 `StaticBody2D` + `CollisionShape2D`

### 添加收集物

```csharp
// 示例：Area2D 收集物
public partial class Coin : Area2D
{
    private void OnBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            Global.Instance?.AddScore(100);
            AudioManager.Instance?.PlaySfx(myCoinSound);
            QueueFree();
        }
    }
}
```

## 技术栈

| 技术 | 版本 |
|------|------|
| Godot Engine | 4.6 |
| .NET | 8.0 |
| 渲染方式 | GL Compatibility |
| 脚本语言 | C# |

---
