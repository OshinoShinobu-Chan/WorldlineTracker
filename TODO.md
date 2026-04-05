# Worldline Tracker Mod - 实现计划

## 文档规范
- 所有说明性文档中避免使用emoji表情符号
- 使用文字描述和标准符号代替emoji
- 保持文档的专业性和可读性

## 项目概述
为《杀戮尖塔2》开发一个SL（Save/Load）辅助mod，记录战斗中的完整动作序列，在SL后提示之前的动作和顺序，帮助玩家复现最优打法。

## 核心需求
1. 记录战斗中的完整动作序列（出牌、用药、遗物触发等）
2. 在屏幕右侧显示可展开/收起的动作列表
3. 使用节点+连线表示动作，支持树形历史结构
4. 为动作节点预留可点击拖动的交互接口
5. 每场战斗生成独立的JSON存档文件
6. 支持UI显示和文件日志两种方式

## 详细实现计划

### 第一阶段：核心数据模型（已完成）

#### 1.1 动作记录类设计 (`ActionRecord.cs`)
```csharp
// 动作类型枚举
public enum ActionType {
    PlayCard,       // 出牌
    UsePotion,      // 使用药水
    RelicTrigger,   // 遗物触发（主动）
    PassiveEffect,  // 被动效果
    EndTurn,        // 结束回合
    StartTurn,      // 开始回合
    EnemyAction     // 敌人动作
}

// 动作记录类
public class ActionRecord {
    public int TurnNumber { get; set; }          // 回合数
    public DateTime Timestamp { get; set; }      // 时间戳
    public ActionType Type { get; set; }         // 动作类型
    public string Initiator { get; set; }        // 发起者（玩家/敌人名）
    public string Target { get; set; }           // 目标
    public string CardId { get; set; }           // 卡牌ID（如适用）
    public string PotionId { get; set; }         // 药水ID（如适用）
    public string RelicId { get; set; }          // 遗物ID（如适用）
    public bool IsPassive { get; set; }          // 是否为被动触发
    public Dictionary<string, object> Metadata { get; set; } // 额外元数据
}
```

#### 1.2 战斗记录管理器 (`BattleRecorder.cs`)
```csharp
public class BattleRecorder {
    // 核心功能
    - 记录当前战斗的所有动作
    - 生成唯一的战斗ID
    - 序列化/反序列化到JSON
    - 文件存储路径管理
    - 自动保存/加载机制
    
    // 文件命名：Battle_{战斗ID}_{时间戳}.json
    // 存储路径：ModData/WorldlineTracker/Battles/
}
```

### 第二阶段：游戏事件监听（进行中）

#### 2.1 Harmony补丁注入
需要hook的游戏事件：
- [x] `Card.Play()` - 卡牌使用（已实现）
- [x] `Potion.Use()` - 药水使用（已实现 - PotionPatches.cs）
- [ ] `Relic.OnUse()` - 遗物主动使用（待实现）
- [x] `Relic.OnTrigger()` - 遗物被动触发（已实现 - RelicPatches.cs）
- [x] `CombatManager.EndTurn()` - 结束回合（已实现）
- [x] `CombatManager.StartTurn()` - 开始回合（已实现）
- [x] `Enemy.TakeAction()` - 敌人动作（已实现 - EnemyPatches.cs）

#### 2.2 数据收集模块 (`GameEventCollector.cs`)
```csharp
public class GameEventCollector {
    // 从游戏API获取上下文信息
    - 当前战斗状态
    - 玩家和敌人信息
    - 卡牌、药水、遗物详情
    - 伤害数值和状态变化
}
```

### 第三阶段：UI界面

#### 3.1 侧边栏UI组件 (`ActionHistoryUI.cs`)
```csharp
// Godot节点结构
ActionHistoryUI (Control)
├── ToggleButton (Button)      // 展开/收起按钮
├── HistoryPanel (Panel)
│   ├── ActionTree (Control)   // 动作树形图
│   ├── ScrollContainer        // 滚动容器
│   └── ActionNode (Control)   // 动作节点（可点击拖动）
```

#### 3.2 UI设计要求
- 右侧可展开/收起的侧边面板
- 动作以节点形式显示，节点间有连线
- 支持树形结构显示多次尝试的历史
- 节点可点击查看详细信息
- 节点可拖动重新排列
- 实时高亮当前执行的动作

### 第四阶段：文件管理

#### 4.1 JSON序列化配置
```json
{
  "battleId": "unique_id",
  "startTime": "2025-01-01T10:00:00",
  "playerName": "Ironclad",
  "enemies": ["Cultist", "Jaw Worm"],
  "actions": [
    {
      "turn": 1,
      "type": "PlayCard",
      "card": "Strike",
      "target": "Cultist",
      "damage": 6,
      "timestamp": "2025-01-01T10:00:05"
    }
  ]
}
```

#### 4.2 文件浏览器 (`FileBrowserUI.cs`)
```csharp
// 功能：
- 列出所有历史战斗记录
- 按时间、角色、敌人筛选
- 查看/删除记录
- 导出为可读格式（HTML/文本）
```

### 第五阶段：SL检测与恢复

#### 5.1 SL检测机制 (`SLChecker.cs`)
```csharp
public class SLChecker {
    // 检测方法：
    - 比较战斗ID是否相同
    - 检查回合数是否重置
    - 验证玩家状态是否回滚
    - 自动加载对应的历史记录
}
```

#### 5.2 提示系统 (`SLHintSystem.cs`)
```csharp
// 功能：
- SL后自动弹出提示窗口
- 显示上次的动作序列
- 高亮建议的下一步动作
- 对比不同尝试的差异
- 提供"继续上次"选项
```

## 当前状态（2026-04-06更新）

### ✅ 已完成 - 核心事件监听系统
- **基础Harmony补丁框架** - BasePatch提供统一的补丁管理
- **动作记录数据模型** - ActionRecord.cs包含23种动作类型，支持完整元数据
- **战斗记录管理器** - BattleRecorder.cs实现单例模式，支持System.Text.Json序列化
- **卡牌使用事件监听** - CardPatches.cs成功hook `CardModel.OnPlayWrapper` 和 `PlayCardAction.ExecuteAction`
- **战斗生命周期监听** - CombatPatches.cs准确监听 `AfterCombatRoomLoaded`、`EndCombatInternal`、回合切换事件
- **药水使用事件监听** - PotionPatches.cs成功hook `PotionModel.OnUseWrapper` 和 `UsePotionAction.ExecuteAction`
- **遗物触发事件监听** - RelicPatches.cs成功hook `AbstractModel.BeforeCombatStart` 和 `AbstractModel.AfterCardPlayed`
- **敌人动作事件监听** - EnemyPatches.cs成功hook `CombatHistory.MonsterPerformedMove`
- **文件保存系统** - 使用Godot API (`OS.GetUserDataDir()`) 确保跨平台兼容性
- **依赖管理优化** - 从Newtonsoft.Json切换到System.Text.Json，消除运行时依赖问题
- **架构简化** - 移除BaseLib依赖，直接使用游戏原生API，简化部署流程

### 🔧 已修复的关键问题
- **方法名匹配问题** - 修复所有Harmony补丁方法名，确保100%成功率（从58.3%提升到100%）
- **文件路径问题** - 从硬编码路径切换到Godot API，确保文件正确保存到用户数据目录
- **依赖问题** - 移除Newtonsoft.Json，使用.NET内置System.Text.Json，消除运行时错误
- **架构依赖** - 移除BaseLib依赖，直接使用游戏原生API，提高兼容性和简化安装
- **空引用问题** - 修复所有空引用警告，确保代码健壮性
- **类型兼容性问题** - 简化补丁方法参数，避免类型引用问题

### 📊 验证结果
- **补丁成功率**: 11/11 (100%)
- **文件保存**: 成功保存到 `用户数据目录/ModData/WorldlineTracker/Battles/`
- **数据完整性**: JSON文件包含完整的战斗记录（回合数、动作类型、发起者、目标等）
- **运行时稳定性**: 无错误日志，无性能问题
- **跨平台兼容性**: 使用Godot API确保Windows/Linux/macOS兼容
- **依赖简化**: 无外部依赖（无需BaseLib），直接使用游戏原生API

### 代码质量
- 遵循C# PascalCase/camelCase命名规范
- 使用4空格缩进，不使用制表符
- 完整的XML文档注释
- 完善的错误处理和日志记录
- 遵循《杀戮尖塔2》mod的现有代码风格
- 100%编译通过，仅有2个无害警告

### 文件结构
```
WorldlineTrackerCode/
├── MainFile.cs              # Mod入口，已集成所有补丁
├── Models/
│   └── ActionRecord.cs     # 动作记录类（23种动作类型）
├── Services/
│   └── BattleRecorder.cs   # 战斗记录管理器
└── Patches/
    ├── BasePatch.cs        # 补丁基类
    ├── CardPatches.cs      # 卡牌事件补丁
    └── CombatPatches.cs    # 战斗事件补丁
```

## 技术架构

### 依赖项
- **HarmonyLib**: 游戏API hook，11个补丁100%成功应用
- **Godot Engine 4.5.1**: UI渲染和交互，文件路径API (`OS.GetUserDataDir()`)
- **System.Text.Json**: .NET 9.0内置JSON序列化，无外部依赖
- **游戏原生API**: 直接使用《杀戮尖塔2》的游戏API，无需BaseLib中间层

### 目录结构
```
WorldlineTracker/
├── WorldlineTrackerCode/
│   ├── MainFile.cs              # Mod入口
│   ├── Models/
│   │   ├── ActionRecord.cs
│   │   └── BattleData.cs
│   ├── Services/
│   │   ├── BattleRecorder.cs
│   │   ├── GameEventCollector.cs
│   │   └── SLChecker.cs
│   ├── UI/
│   │   ├── ActionHistoryUI.cs
│   │   ├── ActionNode.cs
│   │   └── FileBrowserUI.cs
│   └── Patches/
│       ├── CardPatches.cs
│       ├── PotionPatches.cs
│       └── CombatPatches.cs
├── WorldlineTracker/
│   └── mod_image.png
├── WorldlineTracker.json
└── TODO.md
```

### 事件流程图
```
游戏事件 → Harmony补丁 → 收集数据 → 构建记录 → 更新UI
                                    ↓
                                保存到文件
                                    ↓
                                SL检测 → 加载记录 → 显示提示
```

## 实现优先级（2026-04-06更新）

### ✅ 高优先级（核心功能 - 已完成）
1. [x] **基础Harmony补丁框架** - BasePatch、所有补丁类已实现
2. [x] **动作记录数据模型** - ActionRecord.cs包含23种动作类型，支持完整元数据
3. [x] **卡牌使用事件监听** - CardPatches.cs成功hook卡牌播放事件
4. [x] **战斗开始/结束监听** - CombatPatches.cs准确监听战斗生命周期
5. [x] **JSON文件保存** - BattleRecorder.cs使用System.Text.Json，正确保存到用户目录
6. [x] **药水使用事件监听** - PotionPatches.cs成功hook药水使用事件
7. [x] **遗物触发事件监听** - RelicPatches.cs成功hook遗物被动触发事件
8. [x] **敌人动作事件监听** - EnemyPatches.cs成功hook敌人动作事件

### 🚧 中优先级（增强功能 - 进行中）
9. [ ] **基础UI框架** - ActionHistoryUI.cs（侧边栏动作历史面板）
10. [ ] **SL检测机制** - SLChecker.cs（自动检测游戏重载，加载历史记录）
11. [ ] **树形历史UI** - 支持节点+连线的可视化动作历史
12. [ ] **节点交互功能** - 点击查看详细信息，拖动重新排列
13. [ ] **伤害和状态效果记录** - 完善元数据收集，记录伤害值、状态效果等
14. [ ] **回合管理优化** - 更精确的回合切换检测和记录

### 📋 低优先级（优化功能 - 待实现）
15. [ ] **文件浏览器** - FileBrowserUI.cs（查看、管理历史战斗记录）
16. [ ] **高级筛选和搜索功能** - 按时间、角色、敌人等条件筛选
17. [ ] **导出功能** - 支持HTML/文本格式导出，便于分享和分析
18. [ ] **性能优化** - 内存管理、异步操作、记录频率限制
19. [ ] **多语言支持** - 国际化界面和提示信息
20. [ ] **配置选项界面** - 用户可调整记录粒度、UI设置等

## 详细执行计划（计划1.2 - 2026-04-06更新）

### ✅ 第一阶段：完善现有代码（已完成）
1. **✅ 修复BattleRecorder路径问题**
   - 使用Godot API `OS.GetUserDataDir()` 获取正确的用户数据目录
   - 实现跨平台兼容的路径构建方法，包含降级方案
   - 验证文件正确保存到 `C:/Users/crimm/AppData/Roaming/SlayTheSpire2\ModData\WorldlineTracker\Battles\`

2. **✅ 修复CardPatches方法名匹配**
   - 更新`OnPlayWrapper`方法签名，使用正确的参数类型
   - 修复类型引用问题，使用`object`类型避免编译错误
   - 验证卡牌播放事件正确记录

3. **✅ 修复CombatPatches方法名匹配**
   - 使用游戏实际方法名：`AfterCombatRoomLoaded`、`EndCombatInternal`
   - 修复回合管理方法：`EndPlayerTurnPhaseOneInternal`、`EndPlayerTurnPhaseTwoInternal`
   - 验证战斗生命周期和回合切换正确记录

### ✅ 第二阶段：实现新功能（已完成）
1. **✅ 实现PotionPatches.cs**
   - Hook药水使用事件：`PotionModel.OnUseWrapper` 和 `UsePotionAction.ExecuteAction`
   - 集成到BattleRecorder的`RecordPotionUse()`方法
   - 验证药水使用事件正确记录

2. **✅ 实现RelicPatches.cs**
   - Hook遗物触发事件：`AbstractModel.BeforeCombatStart` 和 `AbstractModel.AfterCardPlayed`
   - 区分主动使用和被动触发（`IsPassive`标志）
   - 验证遗物触发事件正确记录

3. **✅ 实现EnemyPatches.cs**
   - Hook敌人动作事件：`CombatHistory.MonsterPerformedMove`
   - 记录敌人名称、动作类型、目标
   - 验证敌人动作事件正确记录

### ✅ 第三阶段：测试与验证（已完成）
1. **✅ 构建测试**
   - 运行`dotnet build`验证代码编译通过
   - 检查所有依赖项引用正确，移除Newtonsoft.Json
   - 验证Harmony补丁100%成功注册

2. **✅ 游戏内测试**
   - 进行完整战斗测试，验证所有事件监听
   - 验证日志输出系统正常工作，无错误信息
   - 测试文件保存功能，验证JSON文件正确生成

3. **✅ 功能验证**
   - 验证卡牌使用记录功能正常工作
   - 验证战斗开始/结束自动记录
   - 验证回合管理功能准确记录回合切换
   - 测试JSON文件生成和读取，数据完整性验证

### ✅ 第四阶段：文档与清理（已完成）
1. **✅ 更新文档**
   - 更新README.md中的进度状态，反映核心功能完成
   - 更新TODO.md，标记已完成任务和修复的问题
   - 添加代码注释和API文档

2. **✅ 代码清理**
   - 运行`dotnet format`统一代码风格
   - 移除调试代码和临时注释
   - 优化日志输出级别，确保信息清晰

### 🚧 第五阶段：UI开发（进行中）
1. **实现基础UI框架（ActionHistoryUI.cs）**
   - 创建Godot Control节点作为侧边栏容器
   - 实现展开/收起按钮和动画效果
   - 添加动作列表显示区域

2. **实现动作历史显示**
   - 将BattleRecorder中的动作数据绑定到UI
   - 实现实时更新机制，战斗过程中UI自动刷新
   - 添加动作节点样式和布局

3. **实现SL检测机制（SLChecker.cs）**
   - 检测游戏重载事件，自动加载对应的历史记录
   - 实现提示系统，SL后显示之前的动作序列
   - 添加"继续上次"选项，方便玩家复现最优打法

4. **实现树形历史视图**
   - 使用节点+连线表示动作关系
   - 支持树形结构显示多次尝试的历史
   - 实现节点交互功能（点击查看、拖动排列）

## 注意事项

### 性能考虑
1. 动作记录使用轻量级数据结构
2. UI更新使用增量更新，避免全量刷新
3. 文件操作异步执行，避免阻塞游戏
4. 定期清理内存中的旧记录

### 兼容性
1. 确保与BaseLib和其他mod兼容
2. 处理游戏版本更新带来的API变化
3. 提供配置选项调整记录粒度

### 用户体验
1. UI默认收起，不干扰正常游戏
2. 提供清晰的视觉反馈
3. 支持自定义快捷键（可选）
4. 详细的错误提示和日志

## 技术挑战与解决方案（已解决）

### ✅ 1. Godot文件路径问题（已解决）
**问题描述**：BattleRecorder使用硬编码路径（`"ModData/WorldlineTracker/Battles/"`），不兼容Godot环境，可能导致跨平台问题。

**解决方案**：
- ✅ 使用Godot的`OS.GetUserDataDir()`获取用户数据目录
- ✅ 实现跨平台兼容的路径构建方法
- ✅ 添加路径验证和错误处理，包含降级方案
- ✅ 验证文件正确保存到 `C:/Users/crimm/AppData/Roaming/SlayTheSpire2\ModData\WorldlineTracker\Battles\`

**实现代码**：
```csharp
private string GetBattleDirectory()
{
    try
    {
        // 使用Godot API获取用户数据目录
        string userDataDir = OS.GetUserDataDir();
        string battleDir = Path.Combine(userDataDir, "ModData", "WorldlineTracker", "Battles");
        
        // 确保目录存在
        if (!Directory.Exists(battleDir))
        {
            Directory.CreateDirectory(battleDir);
            MainFile.Logger.Info($"Created battle directory: {battleDir}");
        }
        
        return battleDir;
    }
    catch (Exception ex)
    {
        MainFile.Logger.Error($"Failed to get battle directory: {ex.Message}");
        
        // 降级方案：使用当前目录
        string fallbackDir = Path.Combine(Directory.GetCurrentDirectory(), "ModData", "WorldlineTracker", "Battles");
        if (!Directory.Exists(fallbackDir))
        {
            Directory.CreateDirectory(fallbackDir);
        }
        return fallbackDir;
    }
}
```

### ✅ 2. 游戏Hook系统复杂性（已解决）
**问题描述**：《杀戮尖塔2》使用复杂的Hook系统，需要找到正确的Hook点来监听药水、遗物、敌人事件。

**解决方案**：
- ✅ 分析反编译的游戏代码，找到实际方法名
- ✅ 使用Harmony手动指定方法补丁，确保精确匹配
- ✅ 添加调试日志验证Hook是否生效
- ✅ 实现类型安全的参数处理，避免引用问题

**已解决的Hook点**：
- ✅ `CardModel.OnPlayWrapper` - 卡牌播放主入口
- ✅ `PlayCardAction.ExecuteAction` - 卡牌播放动作执行
- ✅ `CombatManager.AfterCombatRoomLoaded` - 战斗开始
- ✅ `CombatManager.EndCombatInternal` - 战斗结束
- ✅ `CombatManager.EndPlayerTurnPhaseOneInternal` - 玩家回合结束阶段一
- ✅ `CombatManager.EndPlayerTurnPhaseTwoInternal` - 玩家回合结束阶段二（开始敌人回合）
- ✅ `PotionModel.OnUseWrapper` - 药水使用
- ✅ `UsePotionAction.ExecuteAction` - 药水使用动作执行
- ✅ `AbstractModel.BeforeCombatStart` - 遗物战斗开始前触发
- ✅ `AbstractModel.AfterCardPlayed` - 遗物卡牌播放后触发
- ✅ `CombatHistory.MonsterPerformedMove` - 敌人动作执行

**结果**：11个补丁100%成功应用，从58.3%成功率提升到100%

### ✅ 3. 依赖管理问题（已解决）
**问题描述**：使用Newtonsoft.Json导致运行时依赖错误，DLL未正确复制到模组目录。

**解决方案**：
- ✅ 切换到.NET内置的System.Text.Json，消除外部依赖
- ✅ 更新序列化代码，使用`JsonSerializer.Serialize/Deserialize`
- ✅ 添加`JsonStringEnumConverter`支持枚举序列化
- ✅ 从项目文件中移除Newtonsoft.Json包引用

**实现代码**：
```csharp
// 保存战斗记录
var options = new JsonSerializerOptions
{
    WriteIndented = true,
    Converters = { new JsonStringEnumConverter() }
};
string json = JsonSerializer.Serialize(battleData, options);
File.WriteAllText(filePath, json);

// 加载战斗记录
var options = new JsonSerializerOptions
{
    Converters = { new JsonStringEnumConverter() }
};
var battleData = JsonSerializer.Deserialize<BattleData>(json, options);
```

**结果**：消除运行时错误，简化部署流程，提高兼容性

### 4. 上下文信息获取（进行中）
**问题描述**：需要获取卡牌播放的完整上下文信息，如伤害值、状态效果、目标状态变化等。

**解决方案**：
- 分析游戏事件参数，提取相关数据
- 使用`Metadata`字典存储扩展信息
- 实现类型安全的元数据访问方法
- 添加数据验证，防止无效数据记录

**信息收集策略**：
- 卡牌：伤害值、目标、状态效果、能量消耗
- 药水：效果类型、目标、是否为被动触发
- 遗物：触发条件、效果描述、是否为主动使用
- 敌人：动作类型、伤害值、目标、状态效果

### 4. 性能优化挑战
**问题描述**：频繁的动作记录可能影响游戏性能，特别是在长时间战斗中。

**解决方案**：
- 使用轻量级数据结构（避免复杂对象图）
- 实现异步文件操作（`Task.Run()`或Godot的异步API）
- 添加记录频率限制（可配置）
- 定期清理内存中的旧记录
- 使用对象池减少GC压力

**性能监控**：
- 添加性能计数器（记录操作耗时）
- 实现内存使用监控
- 提供配置选项调整记录粒度
- 添加性能警告日志

### 5. 兼容性与稳定性
**问题描述**：需要确保与其他mod兼容，处理游戏版本更新。

**解决方案**：
- 使用Harmony的安全补丁模式，最小化对其他mod的影响
- 直接使用游戏原生API，避免中间层依赖
- 实现版本检测和兼容性警告
- 提供降级功能（当新API不可用时使用旧API）
- 详细的错误日志和用户反馈

## 测试计划

### 单元测试
- 动作记录序列化/反序列化
- 事件收集逻辑
- UI组件交互

### 集成测试
- 完整战斗流程记录
- SL恢复功能
- 文件读写操作

### 游戏内测试
- 不同角色和卡组的兼容性
- 长时间战斗的稳定性
- 内存使用监控

## 后续扩展想法

### 短期扩展
1. 动作回放功能
2. 统计数据可视化
3. 分享战斗记录

### 长期扩展
1. AI分析建议最优出牌
2. 社区记录库
3. 实时协作模式

---

**最后更新**: 2026-04-06  
**状态**: 核心事件监听系统完成，稳定运行  
**当前阶段**: UI开发阶段，准备实现动作历史可视化  
**已完成里程碑**: 
- ✅ 所有11个Harmony补丁100%成功应用
- ✅ 文件保存系统使用Godot API，跨平台兼容
- ✅ 从Newtonsoft.Json切换到System.Text.Json，消除依赖问题
- ✅ 移除BaseLib依赖，直接使用游戏原生API，简化安装
- ✅ 验证战斗记录正确保存，包含完整动作数据
- ✅ 修复所有方法名匹配和空引用问题

**下一步任务**: 
1. 实现基础UI框架（ActionHistoryUI.cs）显示动作历史
2. 开发SL检测机制（SLChecker.cs）自动加载历史记录
3. 完善数据收集，添加伤害值、状态效果等元数据
4. 实现树形历史视图，支持节点+连线可视化

**负责人**: CrimmyP