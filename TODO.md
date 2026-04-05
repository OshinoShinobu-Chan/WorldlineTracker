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

### 第一阶段：核心数据模型

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

### 第二阶段：游戏事件监听

#### 2.1 Harmony补丁注入
需要hook的游戏事件：
- `Card.Play()` - 卡牌使用
- `Potion.Use()` - 药水使用
- `Relic.OnUse()` - 遗物主动使用
- `Relic.OnTrigger()` - 遗物被动触发
- `CombatManager.EndTurn()` - 结束回合
- `CombatManager.StartTurn()` - 开始回合
- `Enemy.TakeAction()` - 敌人动作

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

## 技术架构

### 依赖项
- **HarmonyLib**: 游戏API hook
- **Godot Engine**: UI渲染和交互
- **Newtonsoft.Json**: JSON序列化
- **BaseLib**: 《杀戮尖塔2》mod基础库

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

## 实现优先级

### 高优先级（核心功能）
1. [ ] 基础Harmony补丁框架
2. [ ] 动作记录数据模型
3. [ ] 卡牌使用事件监听
4. [ ] JSON文件保存
5. [ ] 基础UI框架

### 中优先级（增强功能）
6. [ ] 完整事件监听（药水、遗物）
7. [ ] 树形历史UI
8. [ ] 节点交互功能
9. [ ] SL检测机制

### 低优先级（优化功能）
10. [ ] 文件浏览器
11. [ ] 高级筛选和搜索
12. [ ] 导出功能
13. [ ] 性能优化

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

**最后更新**: 2025-04-05  
**状态**: 计划阶段  
**负责人**: CrimmyP