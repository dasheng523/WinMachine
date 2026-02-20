# Machine Orchestration System

工业自动化机器编排系统 - 使用函数式编程原则构建的类型安全的机器编排平台。

## 技术栈

### 后端
- **.NET 10** - 运行时平台
- **C# 13** - 编程语言（实现 Haskell 函数式编程哲学）
- **LanguageExt.Core** - 函数式编程扩展（Option, Either, Seq 等）
- **System.Reactive** - 响应式编程（Rx.NET）
- **xUnit** - 单元测试框架
- **FsCheck** - 基于属性的测试

### 前端
- **React 19** - UI 框架
- **TypeScript** - 类型安全的 JavaScript
- **Vite** - 构建工具
- **Three.js** - 3D 渲染引擎
- **@react-three/fiber** - React Three.js 集成
- **@react-three/drei** - Three.js 辅助工具
- **Tailwind CSS** - 样式框架
- **framer-motion** - 动画库
- **@react-spring/three** - 3D 动画
- **lucide-react** - 图标库
- **@microsoft/signalr** - 实时通信
- **axios** - HTTP 客户端

## 项目结构

```
MachineOrchestration/
├── src/
│   ├── MachineOrchestration.Core/           # 核心领域模型（纯函数）
│   ├── MachineOrchestration.Dsl/            # DSL 解析器和解释器
│   ├── MachineOrchestration.ControlBoards/  # 控制板抽象和实现
│   ├── MachineOrchestration.Configuration/  # 配置管理
│   ├── MachineOrchestration.Automation/     # 自动化逻辑执行
│   ├── MachineOrchestration.Visualization/  # 可视化状态映射
│   └── MachineOrchestration.App/            # Web API 和 SignalR
├── tests/
│   └── MachineOrchestration.Tests/          # 单元测试和属性测试
├── machine-orchestration-front/             # React 前端应用
└── MachineOrchestration.sln                 # 解决方案文件
```

## 构建和运行

### 后端

```bash
# 构建所有项目
dotnet build

# 运行测试
dotnet test

# 运行 Web API
dotnet run --project src/MachineOrchestration.App
```

### 前端

```bash
cd machine-orchestration-front

# 安装依赖（已完成）
npm install

# 开发模式
npm run dev

# 构建生产版本
npm run build
```

## 设计原则

1. **类型安全优先** - 使用代数数据类型在编译时捕获错误
2. **纯函数核心** - 核心业务逻辑为纯函数，副作用隔离在边界层
3. **递归组合模型** - Part、Component、Module、Machine 统一为可组合实体
4. **不可变数据** - 所有数据结构默认不可变
5. **响应式编程** - 使用 IObservable<T> 处理异步事件流
6. **函数式扩展** - 使用 LanguageExt.Core 提供函数式类型

## 开发状态

✅ 项目结构已设置
✅ 依赖项已安装
✅ 构建基础设施已配置
✅ 测试基础设施已配置

下一步：实现核心领域类型（参见 tasks.md）
