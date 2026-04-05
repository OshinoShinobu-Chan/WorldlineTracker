---
name: sts2-mod-dev
description: 《杀戮尖塔2》模组开发技能，包含构建、测试和部署命令
license: MIT
compatibility: opencode
metadata:
  audience: mod-developers
  workflow: sts2-modding
  platform: windows
---

## 技能概述

本技能为《杀戮尖塔2》模组开发提供标准化的构建、测试和部署工作流。专为使用C#、Godot引擎和BaseLib的模组开发设计。

## 核心功能

### 构建命令
```powershell
# 构建项目
dotnet build

# 清理构建
dotnet clean

# 发布项目
dotnet publish -c Release
```

### 测试命令
```powershell
# 运行所有测试
dotnet test

# 运行特定测试项目
dotnet test --project Tests/WorldlineTracker.Tests.csproj

# 带详细输出的测试
dotnet test --verbosity normal
```

### 代码质量检查
```powershell
# 代码格式化
dotnet format

# 静态代码分析
dotnet analyze

# 检查代码风格
dotnet format --verify-no-changes
```

### Godot相关命令
```powershell
# 导出PCK文件
# 需要在project.godot中配置导出预设
godot --headless --export-pack "BasicExport" "output.pck"

# 运行Godot编辑器
godot
```

## 开发工作流

### 标准开发流程
1. **构建项目**: `dotnet build`
2. **运行测试**: `dotnet test`
3. **格式化代码**: `dotnet format`
4. **提交更改**: 遵循AGENTS.md中的git规则

### 环境配置
```powershell
# 恢复NuGet包
dotnet restore

# 更新依赖
dotnet add package <package-name>

# 列出项目依赖
dotnet list package
```

## 使用场景

### 何时使用本技能
- 开始新的模组开发会话时
- 需要构建或测试模组时
- 准备部署模组时
- 设置开发环境时

### 注意事项
1. 所有命令在PowerShell中执行
2. 确保在项目根目录执行命令
3. 构建前检查依赖是否完整
4. 测试通过后再提交代码
5. 遵循Windows环境下的PowerShell语法

## 技能集成

本技能与以下项目文件协同工作：
- `AGENTS.md`: 开发规则和流程
- `TODO.md`: 功能实现计划
- `README.md`: 项目文档
- `opencode.json`: 权限配置

## 故障排除

### 常见问题
1. **构建失败**: 检查依赖包是否完整恢复
2. **测试失败**: 查看测试输出详情
3. **Godot导出失败**: 确认导出预设配置正确
4. **权限问题**: 确保有足够的文件系统权限

### 调试建议
- 使用`dotnet build --verbosity detailed`获取详细构建信息
- 检查项目文件中的路径配置
- 验证Godot版本与项目兼容性

## 版本兼容性

- **Godot**: 4.5.1+
- **.NET**: 9.0+
- **BaseLib**: 0.2.6+
- **操作系统**: Windows 10/11

---

**最后更新**: 2025-04-05  
**维护者**: CrimmyP