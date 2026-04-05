# 项目构建和测试命令

## 构建命令
```powershell
# 构建项目
dotnet build

# 清理构建
dotnet clean

# 发布项目
dotnet publish -c Release
```

## 测试命令
```powershell
# 运行所有测试
dotnet test

# 运行特定测试项目
dotnet test --project Tests/WorldlineTracker.Tests.csproj

# 带详细输出的测试
dotnet test --verbosity normal
```

## 代码质量检查
```powershell
# 代码格式化
dotnet format

# 静态代码分析
dotnet analyze

# 检查代码风格
dotnet format --verify-no-changes
```

## Godot相关命令
```powershell
# 导出PCK文件
# 需要在project.godot中配置导出预设
godot --headless --export-pack "BasicExport" "output.pck"

# 运行Godot编辑器
godot
```

## 开发工作流
```powershell
# 1. 构建项目
dotnet build

# 2. 运行测试
dotnet test

# 3. 格式化代码
dotnet format

# 4. 提交更改
git add .
git commit -m "描述更改内容"
git push origin <branch>
```

## 环境配置
```powershell
# 恢复NuGet包
dotnet restore

# 更新依赖
dotnet add package <package-name>

# 列出项目依赖
dotnet list package
```

## 注意事项
1. 所有命令在PowerShell中执行
2. 确保在项目根目录执行命令
3. 构建前检查依赖是否完整
4. 测试通过后再提交代码

---
**最后更新**: 2025-04-05  
**维护者**: CrimmyP