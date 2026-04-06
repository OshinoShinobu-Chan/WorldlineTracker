# WorldlineViewer - 战斗过程可视化工具

## 项目概述
WorldlineViewer 是一个用于可视化《杀戮尖塔2》WorldlineTracker模组生成的战斗记录的网页工具。它可以将JSON格式的战斗记录转换为交互式的节点-连线图，帮助玩家分析和理解战斗过程。

## 功能特性

### 核心功能
- **文件上传**: 支持拖拽和点击选择JSON文件
- **数据可视化**: 将战斗事件显示为彩色圆形节点
- **节点摘要**: 每个节点显示回合数和动作类型（如"T1: 出牌"）
- **连线连接**: 按时间顺序连接事件节点
- **交互操作**: 支持节点拖拽、点击查看详情
- **本地存储**: 节点位置自动保存到浏览器本地存储

### 交互功能
- **节点拖拽**: 自由拖动节点到任意位置
- **节点点击**: 点击节点查看完整详细信息
- **画布操作**: 支持缩放和平移画布
- **布局重置**: 一键重置节点布局
- **数据复制**: 复制节点详情到剪贴板

### 可视化特性
- **颜色编码**: 不同动作类型使用不同颜色
- **响应式设计**: 适应不同屏幕尺寸
- **网格背景**: 辅助对齐和布局
- **图例说明**: 显示颜色与动作类型的对应关系

## 技术栈
- **前端框架**: Vue.js 3.x
- **可视化**: 纯Canvas实现（无外部库依赖）
- **样式**: CSS3 + Flexbox布局
- **文件处理**: 纯前端JavaScript（无后端服务器）

## 使用方法

### 1. 打开网页
直接双击 `index.html` 文件在浏览器中打开，或通过Web服务器访问。

### 2. 上传JSON文件
- **拖拽上传**: 将WorldlineTracker生成的JSON文件拖拽到上传区域
- **点击上传**: 点击上传区域选择文件
- **示例数据**: 可以使用 `assets/sample-battle.json` 进行测试

### 3. 交互操作
- **查看节点**: 鼠标悬停在节点上查看摘要信息
- **点击节点**: 点击节点查看完整详细信息（显示在右侧面板）
- **拖拽节点**: 按住节点并拖动到新位置
- **缩放画布**: 使用鼠标滚轮缩放画布
- **平移画布**: 在空白区域按住并拖动鼠标

### 4. 控制选项
- **重置布局**: 点击"重置布局"按钮恢复节点初始位置
- **清除数据**: 点击"清除数据"按钮移除当前加载的文件
- **调整节点大小**: 使用滑块调整节点显示大小

## 文件格式要求
WorldlineViewer 支持两种命名约定的JSON文件：

### PascalCase格式（WorldlineTracker默认生成）
```json
{
  "BattleId": "唯一战斗ID",
  "StartTime": "战斗开始时间",
  "EndTime": "战斗结束时间",
  "TotalTurns": 总回合数,
  "Actions": [
    {
      "TurnNumber": 回合数,
      "Timestamp": "时间戳",
      "Type": "动作类型",
      "Initiator": "发起者",
      "Target": "目标",
      "CardId": "卡牌ID（可选）",
      "PotionId": "药水ID（可选）",
      "RelicId": "遗物ID（可选）",
      "IsPassive": false,
      "DamageAmount": 0,
      "BlockAmount": 0,
      "Metadata": {}
    }
  ]
}
```

### camelCase格式（也支持）
```json
{
  "battleId": "唯一战斗ID",
  "startTime": "战斗开始时间",
  "endTime": "战斗结束时间",
  "totalTurns": 总回合数,
  "actions": [
    {
      "turnNumber": 回合数,
      "timestamp": "时间戳",
      "type": "动作类型",
      "initiator": "发起者",
      "target": "目标",
      "cardId": "卡牌ID（可选）",
      "potionId": "药水ID（可选）",
      "relicId": "遗物ID（可选）",
      "isPassive": false,
      "damageAmount": 0,
      "blockAmount": 0,
      "metadata": {}
    }
  ]
}
```

### 支持的动作类型
- `PlayCard`: 出牌
- `UsePotion`: 使用药水
- `RelicTrigger`: 遗物触发
- `PassiveEffect`: 被动效果
- `EndTurn`: 结束回合
- `StartTurn`: 开始回合
- `EnemyAction`: 敌人动作
- `DamageDealt`: 造成伤害
- `DamageReceived`: 受到伤害
- `BlockGained`: 获得格挡
- `BlockLost`: 失去格挡
- `PowerGained`: 获得能力
- `PowerLost`: 失去能力
- `CardDrawn`: 抽牌
- `CardDiscarded`: 弃牌
- `CardExhausted`: 消耗牌

## 颜色编码
每个动作类型对应不同的颜色：
- **出牌**: 绿色 (#4CAF50)
- **使用药水**: 蓝色 (#2196F3)
- **遗物触发**: 橙色 (#FF9800)
- **敌人动作**: 红色 (#F44336)
- **回合开始/结束**: 灰色/棕色
- **伤害相关**: 深橙色/粉色
- **格挡相关**: 青色
- **其他动作**: 相应颜色

## 键盘快捷键
- **ESC**: 取消选中节点
- **Ctrl + R**: 重置布局
- **Delete**: 清除所有数据

## 开发说明

### 项目结构
```
WorldlineViewer/
├── index.html              # 主页面
├── style.css              # 样式文件
├── app.js                 # Vue.js主应用
├── TODO.md                # 开发计划
├── README.md              # 使用说明
├── assets/                # 静态资源
│   └── sample-battle.json # 示例数据文件
```

### 核心类说明
- **Node**: 节点类，表示一个战斗事件
- **Connection**: 连线类，连接两个节点
- **CanvasRenderer**: Canvas渲染引擎，处理绘制和交互
- **LayoutManager**: 布局管理器，计算节点位置

### 数据流
1. 用户上传JSON文件
2. 解析JSON数据并验证格式
3. 创建Node和Connection对象
4. 计算初始布局或从本地存储加载位置
5. 在Canvas上绘制节点和连线
6. 处理用户交互（点击、拖拽、缩放）

## 浏览器兼容性
- **推荐**: Chrome 90+, Firefox 88+, Edge 90+
- **要求**: 支持Canvas、ES6+、File API的现代浏览器
- **不支持**: IE浏览器

## 已知限制
1. 节点数量过多时可能影响性能（建议不超过500个节点）
2. 本地存储有大小限制（通常5MB）
3. 需要现代浏览器支持
4. 仅支持WorldlineTracker特定格式的JSON文件
5. 中文等Unicode字符需要浏览器正确支持UTF-8编码

## 故障排除

### 常见问题
1. **文件无法上传**
   - 检查文件是否为JSON格式
   - 检查文件大小是否过大
   - 检查浏览器控制台是否有错误

2. **节点不显示**
   - 检查JSON文件格式是否正确
   - 检查浏览器是否支持Canvas
   - 刷新页面重试

3. **拖拽功能失效**
   - 检查是否启用了JavaScript
   - 尝试在其他浏览器中测试
   - 清除浏览器缓存

4. **布局混乱**
   - 点击"重置布局"按钮
   - 清除浏览器本地存储
   - 重新上传文件

### 调试建议
1. 打开浏览器开发者工具（F12）
2. 查看控制台输出
3. 检查网络请求
4. 验证JSON文件格式

## 后续开发计划
参见 [TODO.md](TODO.md) 文件了解详细开发计划。

## 版本历史
- **v1.0.0** (2026-04-06): 初始版本，实现基础可视化功能

## 许可证
本项目基于MIT许可证开源。

## 贡献
欢迎提交Issue和Pull Request来改进这个项目。

## 联系
如有问题或建议，请通过项目Issue页面联系。