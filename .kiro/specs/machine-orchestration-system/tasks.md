# 实施计划：机器编排系统

## 概述

本实施计划将机器编排系统分解为离散的、可操作的任务。系统使用 .NET 10、C# 13 构建，遵循 Haskell 函数式编程哲学，强调类型安全、纯函数和副作用隔离。

## DSL 方法

**重要决策**：系统使用 **C# 代码直接构建 AST** 作为 DSL，而不是实现文本解析器。

**优势**：
- ✅ 无需实现 Lexer 和 Parser
- ✅ 完整的 IDE 支持（智能提示、重构、调试）
- ✅ 编译时类型检查
- ✅ 零学习成本（用户已熟悉 C#）
- ✅ 更简单的实现和维护

**示例**：
```csharp
// 用户直接用 C# 构建自动化逻辑
var automation = Ast.Create(
    new Statement.Sequence(Seq<Statement>(
        new Statement.Action(motorId, new PartAction.Motor(MotorAction.Home.Instance)),
        new Statement.WaitUntil(new Condition.SensorState(sensorId, true)),
        new Statement.Loop(Some<uint>(3), 
            new Statement.Action(motorId, new PartAction.Motor(new MotorAction.MoveTo(100, 50))))
    ))
);
```

## 技术栈

- 后端：.NET 10、C# 13、LanguageExt.Core、System.Reactive
- 前端：React 19、TypeScript、Three.js、Vite
- 测试：xUnit、FsCheck（基于属性的测试）

## 任务

- [x] 1. 设置项目结构和依赖项
  - 创建解决方案文件和项目结构
  - 添加 NuGet 包：LanguageExt.Core、System.Reactive、xUnit、FsCheck.Xunit
  - 使用 Vite、React 19、Three.js 设置前端项目
  - 配置构建和测试基础设施
  - _需求：19.1-19.5、27.1-27.5_

- [x] 2. 实现核心领域类型（Part、PartCategory、Actions）
  - [x] 2.1 创建包含四个分类的 PartCategory 代数数据类型
    - 将 MotorType、OutputType、InputType、StaticType 实现为密封记录
    - 使用和类型模式进行类型安全的分类
    - _需求：1.12-1.20_
  
  - [x] 2.2 实现 Part 类型和定义
    - 创建 PartId newtype 包装器
    - 实现 PartType 和类型（Motor、Actuator、Sensor、Static）
    - 实现 MotorType（LinearScrew、RotaryTable）
    - 实现 ActuatorType（Cylinder、Gripper、Suction、Indicator）
    - 实现 SensorType（Pressure、Micrometer、Scanner）
    - 实现 StaticType（Shaft、Bracket）
    - _需求：1.1-1.11_
  
  - [x] 2.3 实现传感器配置类型
    - 创建 CylinderSensorConfig 和类型（None、ExtendOnly、Both）
    - 创建 GripperSensorConfig 和 SuctionSensorConfig 记录
    - 使用 Option<T> 表示可选的传感器配置
    - _需求：1.6-1.9、11.6-11.8、28.1-28.10_
  
  - [x] 2.4 实现动作类型
    - 创建 MotorAction 和类型（MoveTo、RotateTo、Home、Stop）
    - 创建 ActuatorAction 和类型（Extend、Retract、Close、Open、Suction、Normal、On、Off）
    - 创建包装 Motor 和 Actuator 动作的 PartAction 和类型
    - _需求：4.1-4.6_
  
  - [x] 2.5 编写零件分类完整性的属性测试
    - **属性 1：零件分类完整性**
    - **验证：需求 1.12-1.15**
  
  - [x] 2.6 编写传感器配置类型安全的属性测试
    - **属性 23：传感器配置类型安全**
    - **验证：需求 1.6-1.9、11.6-11.8**

- [x] 3. 实现坐标系统和变换矩阵
  - [x] 3.1 创建包含位置和旋转的 Coordinate 类型
    - 使用 System.Numerics.Vector3 表示位置
    - 使用 System.Numerics.Quaternion 表示旋转
    - 实现 Identity 坐标
    - _需求：2.1-2.2_
  
  - [x] 3.2 实现 TransformationMatrix 类型
    - 包装 System.Numerics.Matrix4x4
    - 实现 Translation、Rotation、Scale 工厂方法
    - 实现矩阵组合的 Compose 方法
    - 实现坐标变换的 ApplyTo 方法
    - _需求：2.3-2.5_
  
  - [x] 3.3 实现 CoordinateSystem 静态类
    - 实现 CreateCoordinate 函数
    - 实现 ComposeCoordinates 函数（相对坐标转绝对坐标）
    - 实现 CreateTransformation 函数
    - 实现 ComposeTransformations 函数
    - 实现 ApplyToCoordinate 函数
    - _需求：2.1-2.10_
  
  - [x] 3.4 编写变换矩阵结合律的属性测试
    - **属性 3：变换矩阵结合律**
    - **验证：需求 2.3-2.5**
  
  - [x] 3.5 编写变换矩阵幺元的属性测试
    - **属性 4：变换矩阵幺元**
    - **验证：需求 2.3-2.5**
  
  - [x] 3.6 编写坐标组合边缘情况的单元测试
    - 测试幺元组合
    - 测试零向量处理
    - 测试四元数归一化

- [x] 4. 检查点 - 确保所有测试通过
  - 确保所有测试通过，如有问题请询问用户。

- [x] 5. 实现统一递归组合模型（ComposableEntity）
  - [x] 5.1 创建 EntityId newtype 和 ComposableEntity 代数数据类型
    - 实现 EntityId 包装器
    - 创建包含 Part 和 Composite 变体的 ComposableEntity 和类型
    - 实现 GetId 和 GetCoordinate 方法
    - _需求：3.1-3.3_
  
  - [x] 5.2 实现 ApplyTransformation 递归方法
    - 对 Part 坐标应用变换
    - 递归地对 Composite 子实体应用变换
    - 保持相对坐标不变
    - _需求：3.6-3.7、2.6_
  
  - [x] 5.3 实现 ComputeAbsoluteCoordinates 递归方法
    - 从根到叶递归计算绝对坐标
    - 组合父子相对坐标
    - 返回 (PartId, Coordinate) 对的序列
    - _需求：2.1-2.2、2.9-2.10_
  
  - [x] 5.4 实现带验证的 AddChild 方法
    - 添加带相对坐标的子实体
    - 返回 Either<CompositionError, ComposableEntity>
    - 验证组合约束
    - _需求：3.2、3.4_
  
  - [x] 5.5 创建 Component、Module、Machine 的类型别名
    - 实现 CreateComponent、CreateModule、CreateMachine 工厂函数
    - 全部返回带语义命名的 ComposableEntity.Composite
    - _需求：3.1_
  
  - [x] 5.6 编写相对坐标不变性的属性测试
    - **属性 5：相对坐标不变性**
    - **验证：需求 2.6**
  
  - [x] 5.7 编写递归变换传播的属性测试
    - **属性 6：递归变换传播**
    - **验证：需求 2.9-2.10、3.7**
  
  - [x] 5.8 编写组合操作结合律的属性测试
    - **属性 7：组合操作结合律**
    - **验证：需求 3.5**
  
  - [x] 5.9 编写递归组合深度的属性测试
    - **属性 8：递归组合深度和完整性**
    - **验证：需求 3.3、3.4**
  
  - [x] 5.10 编写绝对坐标计算的属性测试
    - **属性 9：绝对坐标计算正确性**
    - **验证：需求 2.1-2.2、2.9-2.10**

- [x] 6. 实现零件库组件
  - [x] 6.1 创建 IPartLibrary 接口和实现
    - 实现 GetAllParts 纯函数
    - 实现 GetPartsByCategory 纯函数
    - 实现返回 Option<Part> 的 GetPartById 纯函数
    - 使用不可变的 Seq<Part> 存储
    - _需求：1.1-1.2、1.14-1.15_
  
  - [x] 6.2 填充零件库初始零件
    - 添加电机零件（LinearScrew、RotaryTable）
    - 添加执行器零件（Cylinder、Gripper、Suction、Indicator）
    - 添加传感器零件（Pressure、Micrometer、Scanner）
    - 添加静态零件（Shaft、Bracket）
    - _需求：1.1、1.16-1.19_
  
  - [x] 6.3 编写零件分类查询一致性的属性测试
    - **属性 2：零件分类查询一致性**
    - **验证：需求 1.14-1.15**
  
  - [x] 6.4 编写零件库查询的单元测试
    - 测试每个分类的 GetPartsByCategory
    - 测试有效和无效 ID 的 GetPartById

- [x] 7. 实现组合引擎
  - [x] 7.1 创建 ICompositionEngine 接口
    - 定义 Compose 方法签名
    - 定义 ApplyTransformation 方法签名
    - 定义 ComputeAbsoluteCoordinates 方法签名
    - _需求：3.2、3.6-3.7_
  
  - [x] 7.2 使用纯函数实现 CompositionEngine
    - 使用 AddChild 方法实现 Compose
    - 实现委托给实体方法的 ApplyTransformation
    - 实现委托给实体方法的 ComputeAbsoluteCoordinates
    - 使用 Either<CompositionError, T> 处理组合错误
    - _需求：3.2-3.7_
  
  - [x] 7.3 编写组合错误情况的单元测试
    - 测试循环引用检测
    - 测试超过最大深度
    - 测试向叶节点添加子节点

- [x] 8. 检查点 - 确保所有测试通过
  - 确保所有测试通过，如有问题请询问用户。


- [x] 9. 实现 DSL AST 类型
  - [x] 9.1 创建 Statement 代数数据类型
    - 实现 Action、Wait、WaitUntil、Sequence、Parallel、Loop、If 变体
    - 使用密封记录模式表示和类型
    - _需求：8.1-8.4_
  
  - [x] 9.2 创建 Condition 代数数据类型
    - 实现 SensorState、StateSensor、SensorValue、And、Or、Not 变体
    - 实现 ComparisonOp 枚举
    - _需求：8.1-8.4、28.8_
  
  - [x] 9.3 创建包装 Seq<Statement> 的 Ast 记录
    - 简单的语句序列包装器
    - _需求：8.2_
  
  - [x] 9.4 编写 AST 构造的单元测试
    - 测试创建各种语句类型
    - 测试嵌套结构

- [x] 10. ~~实现 DSL 词法分析器~~ (已跳过 - 使用 C# 作为 DSL)
  - **注意**：用户将直接使用 C# 代码构建 AST，而不是解析基于文本的 DSL
  - 这消除了对词法分析器、解析器和美化打印器的需求
  - 优势：IDE 支持、类型安全、无解析错误、更易调试
  - [ ]* ~~10.1 创建 DSL 的 Token 类型~~
  - [ ]* ~~10.2 实现 Lexer 类~~
  - [ ]* ~~10.3 编写词法分析器的单元测试~~

- [x] 11. 实现 DSL 验证器（简化版）
  - [x] 11.1 创建 IDslValidator 接口
    - 定义返回 Either<ValidationError, Unit> 的 Validate 方法
    - 验证 AST 语义正确性（无需解析）
    - _需求：8.3、9.2_
  
  - [ ]* ~~11.2 实现递归下降解析器~~ (已跳过 - 使用 C# 作为 DSL)
  
  - [x] 11.3 实现语义验证器
    - 验证实体 ID 在机器中存在
    - 验证传感器引用有效
    - 验证动作与零件类型兼容
    - 验证执行器的状态传感器引用
    - 返回描述性的 ValidationError
    - _需求：8.3、9.2、28.8_
  
  - [ ]* ~~11.4 实现美化打印器~~ (已跳过 - 使用 C# 作为 DSL)
  
  - [ ]* ~~11.5 编写解析/打印往返的属性测试~~ (已跳过)
  
  - [ ]* ~~11.6 编写解析器错误处理的属性测试~~ (已跳过)
  
  - [x] 11.7 编写验证器的单元测试
    - 测试实体 ID 验证
    - 测试传感器引用验证
    - 测试动作兼容性验证
    - 测试多个错误收集

- [x] 12. 检查点 - 确保所有测试通过
  - 确保所有测试通过，如有问题请询问用户。


- [x] 13. 实现配置类型和验证
  - [x] 13.1 创建 PartConfig 代数数据类型
    - 实现 MotorConfig、ActuatorConfig、SensorConfig、Static 变体
    - 包含 BoardConnection、LimitSensors、StateSensorPorts 类型
    - 使用 Option<T> 表示可选配置
    - _需求：11.1-11.8_
  
  - [x] 13.2 创建 SensorConnection 代数数据类型
    - 实现 SerialSingle、SerialMultiple、Usb 变体
    - _需求：7.1-7.5_
  
  - [x] 13.3 创建 MachineConfig 和 ControlBoardConfig 类型
    - 实现 MachineConfig 记录
    - 实现 ControlBoardConfig 和类型（LeiSai、ZhengYunDong、Simulated）
    - 使用 JsonDerivedType 属性进行多态序列化
    - _需求：10.1-10.5、12.1-12.4_
  
  - [x] 13.4 实现 IConfigValidator 接口和实现
    - 验证传感器端口分配
    - 验证控制板兼容性
    - 验证电机配置
    - 验证执行器传感器配置
    - 返回 Either<ValidationError, Unit>
    - 收集多个验证错误
    - _需求：11.9-11.12、12.2-12.4_
  
  - [ ]* 13.5 编写配置验证完整性的属性测试
    - **属性 16：配置验证完整性**
    - **验证：需求 11.9-11.10**
  
  - [ ]* 13.6 编写验证错误描述性的属性测试
    - **属性 17：配置验证错误描述性**
    - **验证：需求 11.11-11.12**
  
  - [x] 13.7 编写特定验证场景的单元测试
    - 测试缺失传感器端口检测
    - 测试不兼容的控制板参数
    - 测试多个错误收集

- [x] 14. 实现配置序列化
  - [x] 14.1 创建 IConfigSerializer 接口
    - 定义返回 Either<SerializationError, string> 的 Serialize 方法
    - 定义返回 Either<DeserializationError, MachineConfig> 的 Deserialize 方法
    - _需求：23.1-23.2_
  
  - [x] 14.2 使用 System.Text.Json 实现 ConfigSerializer
    - 为多态类型配置 JSON 选项
    - 处理 Option<T> 序列化
    - 处理代数数据类型序列化
    - 失败时返回描述性错误
    - _需求：23.1-23.3_
  
  - [ ]* 14.3 编写序列化往返的属性测试
    - **属性 13：配置序列化往返**
    - **验证：需求 23.4**
  
  - [x] 14.4 编写反序列化错误处理的属性测试
    - **属性 14：配置反序列化错误处理**
    - **验证：需求 23.5**
  
  - [ ]* 14.5 编写序列化边缘情况的单元测试
    - 测试空配置
    - 测试深度嵌套结构
    - 测试损坏的 JSON 处理

- [x] 15. 实现配置持久化
  - [x] 15.1 创建 IConfigPersistence 接口
    - 定义返回 Task<Either<IoError, Unit>> 的 Save 方法
    - 定义返回 Task<Either<IoError, MachineConfig>> 的 Load 方法
    - _需求：23.1-23.2_
  
  - [x] 15.2 使用文件 I/O 实现 ConfigPersistence
    - 使用异步文件操作
    - 委托给 ConfigSerializer 进行序列化
    - 优雅地处理文件系统错误
    - 失败时返回描述性的 IoError
    - _需求：23.1-23.5_
  
  - [x] 15.3 编写持久化的集成测试
    - 测试保存和加载循环
    - 测试文件未找到处理
    - 测试权限错误

- [x] 16. 检查点 - 确保所有测试通过
  - 确保所有测试通过，如有问题请询问用户。


- [x] 17. 实现控制板抽象
  - [x] 17.1 创建 Command 代数数据类型
    - 实现 Motor、Actuator、ReadSensor、ReadStateSensor、EmergencyStop 变体
    - 创建 MotorId、ActuatorId、SensorId、StateSensorId 新类型
    - _需求：10.1-10.6_
  
  - [x] 17.2 创建 IControlBoard 接口
    - 定义返回 Task<Either<ControlBoardError, Unit>> 的 Initialize 方法
    - 定义 SendMotorCommand 方法
    - 定义 SendActuatorCommand 方法
    - 定义 ReadSensor 方法
    - 定义 ReadStateSensor 方法
    - 定义 EmergencyStop 方法
    - 定义返回 IObservable<ControlBoardState> 的 StateStream 属性
    - _需求：10.1-10.6、28.1-28.6_
  
  - [x] 17.3 创建 ControlBoardError 代数数据类型
    - 实现 ConnectionError、CommandFailed、NotInitialized 变体
    - _需求：24.1-24.6_
  
  - [x] 17.4 编写命令类型构造的单元测试
    - 测试创建各种命令类型
    - 测试新类型包装器

- [x] 18. 实现模拟控制板
  - [x] 18.1 实现 SimulatedControlBoard 类
    - 实现 IControlBoard 接口
    - 使用延迟模拟电机运动
    - 使用状态变化模拟执行器动作
    - 使用随机值模拟传感器读数
    - 使用可配置的延迟
    - 将状态变化发布到 StateStream
    - _需求：10.4、13.8-13.9_
  
  - [x] 18.2 实现模拟设备的状态跟踪
    - 跟踪电机位置
    - 跟踪执行器状态
    - 跟踪传感器值
    - 使用不可变状态更新
    - _需求：13.8-13.10_
  
  - [x] 18.3 编写模拟板的集成测试
    - 测试电机命令执行
    - 测试执行器命令执行
    - 测试传感器读取
    - 测试状态流更新

- [x] 19. 实现雷赛控制板
  - [x] 19.1 实现 LeiSaiBoard 类
    - 实现 IControlBoard 接口
    - 集成雷赛 SDK/API
    - 将命令映射到雷赛协议
    - 处理连接管理
    - 将状态变化发布到 StateStream
    - _需求：10.2_
  
  - [x] 19.2 实现错误处理和重试逻辑
    - 处理连接错误
    - 实现指数退避的命令重试
    - 返回描述性的 ControlBoardError
    - _需求：24.1-24.6_
  
  - [x] 19.3 使用模拟的雷赛 SDK 编写集成测试
    - 测试命令发送
    - 测试错误处理
    - 测试重试逻辑

- [x] 20. 实现正运动控制板
  - [x] 20.1 实现 ZhengYunDongBoard 类
    - 实现 IControlBoard 接口
    - 集成正运动 SDK/API
    - 将命令映射到正运动协议
    - 处理连接管理
    - 将状态变化发布到 StateStream
    - _需求：10.3_
  
  - [x] 20.2 实现错误处理和重试逻辑
    - 处理连接错误
    - 实现指数退避的命令重试
    - 返回描述性的 ControlBoardError
    - _需求：24.1-24.6_
  
  - [x] 20.3 使用模拟的正运动 SDK 编写集成测试
    - 测试命令发送
    - 测试错误处理
    - 测试重试逻辑

- [x] 21. 检查点 - 确保所有测试通过
  - 确保所有测试通过，如有问题请询问用户。


- [x] 22. 实现 DSL 解释器（纯函数部分）
  - [x] 22.1 创建 ExecutionState 不可变类型
    - 实现 ProgramCounter、MachineState、CallStack、Bindings 字段
    - 创建 PartState 和类型（Motor、Actuator、Sensor）
    - 创建 SensorReading 和类型
    - 创建 StackFrame 和 Value 类型
    - _需求：15.2_
  
  - [x] 22.2 创建 IDslInterpreter 接口
    - 定义返回 Either<ExecutionError, ExecutionState> 的 Step 方法
    - 定义返回 bool 的 IsComplete 方法
    - _需求：15.1-15.6_
  
  - [x] 22.3 使用纯状态转换实现 DslInterpreter
    - 为每种语句类型实现 Step（Action、Wait、Sequence 等）
    - 纯粹地评估条件
    - 不可变地更新执行状态
    - 为无效转换返回 ExecutionError
    - _需求：15.1-15.6_
  
  - [x] 22.4 编写状态转换确定性的属性测试
    - **属性 19：状态转换确定性**
    - **验证：需求 15.2**
  
  - [x] 22.5 编写状态转换不可变性的属性测试
    - **属性 20：状态转换不可变性**
    - **验证：需求 15.2**
  
  - [x] 22.6 编写每种语句类型的单元测试
    - 测试 Action 执行
    - 测试 Wait 计时
    - 测试 Sequence 顺序
    - 测试 Parallel 执行
    - 测试 Loop 迭代
    - 测试 If 分支

- [x] 23. 实现自动化执行器（副作用部分）
  - [x] 23.1 创建 IDslExecutor 接口
    - 定义返回 Task<Either<ExecutionError, Unit>> 的 ExecuteCommand 方法
    - 定义返回 IObservable<ExecutionState> 的 ExecutionStateStream 属性
    - _需求：15.1-15.6_
  
  - [x] 23.2 实现 AutomationExecutor 类
    - 集成 IDslInterpreter 进行纯状态转换
    - 集成 IControlBoard 进行命令执行
    - 将命令作为副作用执行
    - 使用 System.Reactive 将执行状态发布到流
    - 优雅地处理执行错误
    - 错误时实现紧急停止
    - _需求：15.1-15.6、24.1-24.6_
  
  - [x] 23.3 实现错误恢复和安全停止
    - 错误时停止所有电机
    - 将执行器设置为安全状态
    - 记录错误上下文
    - 返回描述性的 ExecutionError
    - _需求：24.1-24.6_
  
  - [x] 23.4 编写自动化执行的集成测试
    - 测试简单序列执行
    - 测试并行执行
    - 测试循环执行
    - 测试错误处理和安全停止
    - 测试状态流更新

- [ ] 24. 实现自动化逻辑存储
  - [ ] 24.1 创建 AutomationLogic 记录类型
    - 包含 LogicId、Name、Ast 字段
    - _需求：14.1-14.5_
  
  - [ ] 24.2 创建 IAutomationLogicManager 接口
    - 定义返回 Either<LogicError, IAutomationLogicManager> 的 AddLogic 方法
    - 定义返回 Option<AutomationLogic> 的 GetLogic 方法
    - 定义返回 Seq<LogicId> 的 ListLogics 方法
    - _需求：14.1-14.3_
  
  - [ ] 24.3 使用不可变存储实现 AutomationLogicManager
    - 使用 HashMap<LogicId, AutomationLogic> 存储
    - 实现逻辑管理的纯函数
    - _需求：14.1-14.5_
  
  - [ ] 24.4 实现逻辑序列化和持久化
    - 将 AutomationLogic 序列化为 JSON
    - 持久化到文件系统
    - 从文件系统加载
    - _需求：14.4-14.5_
  
  - [ ]* 24.5 编写逻辑序列化往返的属性测试
    - **属性 15：自动化逻辑序列化往返**
    - **验证：需求 14.5**
  
  - [ ]* 24.6 编写逻辑管理的单元测试
    - 测试添加逻辑
    - 测试检索逻辑
    - 测试列出逻辑

- [ ] 25. 检查点 - 确保所有测试通过
  - 确保所有测试通过，如有问题请询问用户。


- [ ] 26. 实现可视化状态映射
  - [ ] 26.1 创建 VisualState 类型
    - 实现包含 PartVisualState 映射和时间戳的 VisualState 记录
    - 实现包含坐标、动作、颜色的 PartVisualState 记录
    - 实现 PartVisualAction 和类型（MotorMoving、MotorIdle、ActuatorActive、ActuatorIdle）
    - _需求：13.1-13.2_
  
  - [ ] 26.2 创建 IStateMapper 接口
    - 定义 MapToVisualState 方法（纯函数）
    - 定义 ComputeAnimationFrame 方法（纯函数）
    - _需求：13.1-13.2、5.2_
  
  - [ ] 26.3 使用纯函数实现 StateMapper
    - 将 MachineState 映射到 VisualState
    - 使用线性插值计算动画帧
    - 统一支持虚拟和真实设备
    - _需求：13.1、13.10、5.2_
  
  - [ ]* 26.4 编写状态映射纯函数性和确定性的属性测试
    - **属性 21：状态映射纯函数性和确定性**
    - **验证：需求 13.1、13.10**
  
  - [ ]* 26.5 编写动画帧插值的属性测试
    - **属性 10：动画帧插值线性性**
    - **验证：需求 5.2**
  
  - [ ]* 26.6 编写状态映射的单元测试
    - 测试电机状态映射
    - 测试执行器状态映射
    - 测试动画帧计算

- [ ] 27. 实现可视化服务
  - [ ] 27.1 创建 IVisualizationService 接口
    - 定义返回 IObservable<VisualState> 的 VisualStateStream 属性
    - 定义 SetUpdateRate 方法
    - _需求：13.3-13.5_
  
  - [ ] 27.2 使用 System.Reactive 实现 VisualizationService
    - 订阅 ExecutionStateStream
    - 使用 StateMapper 将执行状态映射到可视状态
    - 以配置的速率发布可视状态（最低 10 FPS）
    - 使用 Observable.Sample 或 Observable.Throttle 进行速率限制
    - _需求：13.3-13.5_
  
  - [ ]* 27.3 编写可视化服务的集成测试
    - 测试状态流订阅
    - 测试更新速率配置
    - 测试最低 10 FPS 保证

- [ ] 28. 实现错误类型和处理
  - [ ] 28.1 创建所有错误代数数据类型
    - 实现 CompositionError（InvalidCoordinate、CircularReference、MaxDepthExceeded、CannotAddChildToLeaf）
    - 实现包含行、列、消息的 ParseError
    - 实现 ValidationError（MissingField、InvalidValue、MissingSensorPort、IncompatibleConfig、Multiple）
    - 实现 ExecutionError（HardwareError、Timeout、InvalidStateTransition、SensorError）
    - 实现 SerializationError 和 DeserializationError
    - 实现 IoError、SensorError、LogicError
    - _需求：24.1-24.6_
  
  - [ ] 28.2 创建 SystemError 顶层和类型
    - 在统一层次结构中包装所有错误类型
    - _需求：24.2-24.3_
  
  - [ ] 28.3 实现错误上下文和日志记录
    - 创建包含时间戳、状态、堆栈跟踪的 ErrorContext 记录
    - 实现结构化错误日志记录
    - _需求：24.4_
  
  - [ ]* 28.4 编写错误传播的属性测试
    - **属性 22：错误传播完整性**
    - **验证：需求 24.2-24.3**
  
  - [ ]* 28.5 编写错误处理的单元测试
    - 测试错误创建
    - 测试错误上下文捕获
    - 测试错误日志记录

- [ ] 29. 检查点 - 确保所有测试通过
  - 确保所有测试通过，如有问题请询问用户。


- [ ] 30. 实现后端应用层
  - [ ] 30.1 创建 MachineService 类
    - 实现机器组合操作
    - 集成 ICompositionEngine
    - 集成 IPartLibrary
    - 通过清晰的 API 暴露操作
    - _需求：1.1-1.2、3.1-3.7_
  
  - [ ] 30.2 创建 AutomationService 类
    - 实现自动化逻辑管理
    - 集成 IAutomationLogicManager
    - 集成 IDslExecutor
    - 暴露执行控制 API
    - _需求：14.1-14.5、15.1-15.6_
  
  - [ ] 30.3 创建 VisualizationService 集成
    - 集成 IVisualizationService
    - 通过 SignalR 暴露可视状态流
    - _需求：13.1-13.10_
  
  - [ ] 30.4 实现 Web API 控制器
    - 创建 PartLibraryController（GET 端点）
    - 创建 MachineController（CRUD 端点）
    - 创建 AutomationController（执行控制端点）
    - 创建 ConfigurationController（保存/加载端点）
    - 使用 ASP.NET Core 最小 API 或控制器
    - 返回包装在适当 HTTP 响应中的 Either<Error, T>
    - _需求：所有应用级需求_
  
  - [ ] 30.5 实现实时通信的 SignalR 集线器
    - 创建用于流式传输可视状态的 VisualizationHub
    - 创建用于流式传输执行状态的 ExecutionHub
    - 使用 System.Reactive 将可观察对象桥接到 SignalR
    - _需求：13.3-13.10、15.1-15.6_
  
  - [ ]* 30.6 编写 API 端点的集成测试
    - 测试零件库查询
    - 测试机器 CRUD 操作
    - 测试自动化执行
    - 测试配置保存/加载
  
  - [ ]* 30.7 编写 SignalR 集线器的集成测试
    - 测试可视状态流式传输
    - 测试执行状态流式传输
    - 测试连接处理

- [ ] 31. 实现前端项目结构
  - [ ] 31.1 设置 Vite + React 19 + TypeScript 项目
    - 初始化 Vite 项目
    - 配置严格模式的 TypeScript
    - 添加依赖：React 19、Three.js、@react-three/fiber、@react-three/drei
    - 添加依赖：framer-motion、@react-spring/three
    - 添加依赖：Tailwind CSS、lucide-react
    - 配置构建和开发服务器
    - _需求：前端技术栈_
  
  - [ ] 31.2 创建 TypeScript 类型定义
    - 定义与后端模型匹配的类型（Part、ComposableEntity、Coordinate 等）
    - 定义 API 响应类型
    - 定义 SignalR 消息类型
    - _需求：类型安全_
  
  - [ ] 31.3 实现 API 客户端服务
    - 创建基于 axios 的 API 客户端
    - 实现所有后端端点的方法
    - 优雅地处理错误
    - _需求：后端集成_
  
  - [ ] 31.4 实现 SignalR 客户端服务
    - 创建 SignalR 连接管理器
    - 订阅 VisualizationHub
    - 订阅 ExecutionHub
    - 处理重新连接
    - _需求：13.3-13.10、15.1-15.6_

- [ ] 32. 实现前端 3D 可视化
  - [ ] 32.1 使用 React Three Fiber 创建 Scene3D 组件
    - 设置包含相机、灯光、控制器的画布（Canvas）
    - 实现用于相机操作的轨道控制器（OrbitControls）
    - 添加网格和轴辅助工具
    - _需求：5.1-5.4、13.1-13.10_
  
  - [ ] 32.2 创建用于渲染零件的 Part3D 组件
    - 渲染不同类型的零件（电机、执行器、传感器、静态零件）
    - 使用适当的几何形状和材质
    - 应用坐标变换
    - 可视化零件动作（电机运动、执行器状态）
    - _需求：5.1-5.4、13.1-13.10_
  
  - [ ] 32.3 创建用于渲染可组合实体的 Machine3D 组件
    - 递归渲染 ComposableEntity 层级结构
    - 应用变换矩阵
    - 高亮选中的零件
    - _需求：3.1-3.7、5.1-5.4_
  
  - [ ] 32.4 实现动画系统
    - 实现电机运动动画
    - 实现执行器动作动画
    - 使用 @react-spring/three 实现平滑动画
    - 最低 10 FPS 的更新频率
    - _需求：5.1-5.4、13.3_
  
  - [ ] 32.5 集成实时状态更新
    - 订阅来自 SignalR 的可视状态流
    - 根据状态变化更新 3D 场景
    - 平滑处理状态转换
    - _需求：13.3-13.10_

- [ ] 33. 检查点 - 确保所有测试通过
  - 确保所有测试通过，如有问题请询问用户。


- [ ] 34. 实现前端 UI 组件
  - [ ] 34.1 创建零件库（PartLibrary）组件
    - 按分类显示零件
    - 实现分类筛选功能
    - 选中时显示零件详情
    - 支持拖放方式向机器添加零件
    - _需求：1.1-1.2、1.14-1.15_
  
  - [ ] 34.2 创建机器编辑器（MachineEditor）组件
    - 显示机器层级结构树
    - 允许添加/移除零件
    - 允许编辑零件坐标
    - 允许编辑零件配置
    - 与 3D 场景集成以提供视觉反馈
    - _需求：2.1-2.10、3.1-3.7、11.1-11.12_
  
  - [ ] 34.3 创建配置面板（ConfigurationPanel）组件
    - 显示零件配置表单
    - 验证配置输入
    - 显示验证错误
    - 支持电机、执行器、传感器配置
    - _需求：11.1-11.12、12.1-12.4_
  
  - [ ] 34.4 创建自动化面板（AutomationPanel）组件
    - 显示自动化逻辑列表
    - 允许创建/编辑 DSL 脚本
    - 提供 DSL 语法高亮
    - 显示带行列号的解析错误
    - 执行/停止自动化
    - _需求：8.1-8.5、14.1-14.5、15.1-15.6_
  
  - [ ] 34.5 创建控制面板（ControlPanel）组件
    - 选择控制板类型
    - 配置控制板参数
    - 显示连接状态
    - 紧急停止按钮
    - _需求：10.1-10.6、12.1-12.4_
  
  - [ ] 34.6 创建状态栏（StatusBar）组件
    - 显示执行状态
    - 显示错误和警告
    - 显示性能指标
    - _需求：13.1-13.10、26.1-26.5_

- [ ] 35. 实现前端状态管理
  - [ ] 35.1 创建用于机器状态的 React 钩子
    - 用于机器 CRUD 操作的 useMachine 钩子
    - 用于零件查询的 usePartLibrary 钩子
    - 用于自动化控制的 useAutomation 钩子
    - 用于可视状态订阅的 useVisualization 钩子
    - _需求：所有前端需求_
  
  - [ ] 35.2 实现错误处理和通知
    - 显示错误通知
    - 内联显示验证错误
    - 优雅地处理 API 错误
    - _需求：24.1-24.6_

- [ ] 36. 实现 FsCheck 自定义生成器
  - [ ] 36.1 为核心类型创建生成器
    - 坐标（Coordinate）生成器
    - 变换矩阵（TransformationMatrix）生成器
    - 可组合实体（ComposableEntity）生成器（带大小控制）
    - 零件（Part）生成器
    - 零件配置（PartConfig）生成器
    - _需求：27.1-27.5_
  
  - [ ] 36.2 为 DSL 类型创建生成器
    - 语句（Statement）生成器
    - 条件（Condition）生成器
    - 抽象语法树（Ast）生成器
    - _需求：27.1-27.5_
  
  - [ ] 36.3 为配置类型创建生成器
    - 机器配置（MachineConfig）生成器
    - 控制板配置（ControlBoardConfig）生成器
    - 自动化逻辑（AutomationLogic）生成器
    - _需求：27.1-27.5_
  
  - [ ]* 36.4 为生成器编写测试
    - 测试生成器生成有效值
    - 测试生成器遵循大小参数限制
    - 测试生成器覆盖率

- [ ] 37. 检查点 - 确保所有测试通过
  - 确保所有测试通过，如有问题请询问用户。


- [ ] 38. 实现剩余的基于属性的测试
  - [ ]* 38.1 为控制板参数类型安全编写属性测试
    - **属性 18：控制板参数类型安全**
    - **验证：需求 12.4**
    - 注：这主要由类型系统在编译时强制执行
  
  - [ ]* 38.2 验证所有 23 个属性都有对应的测试
    - 复查先前任务中实现的所有属性测试
    - 确保设计文档中的每个属性都被覆盖
    - 记录任何由编译时保证的属性

- [ ] 39. 实现传感器管理
  - [ ] 39.1 创建 ISensorReader 接口
    - 定义返回 Task<Either<SensorError, SensorReading>> 的 Read 方法
    - 定义返回 IObservable<SensorReading> 的 ReadingStream 方法
    - _需求：6.1-6.4、7.1-7.5_
  
  - [ ] 39.2 为不同连接类型实现传感器读取器
    - 实现 SerialSingleSensorReader
    - 实现 SerialMultipleSensorReader
    - 实现 UsbSensorReader
    - 优雅地处理 I/O 错误
    - 将读数发布到流
    - _需求：7.1-7.5_
  
  - [ ] 39.3 实现传感器配置验证
    - 验证串口配置
    - 验证 USB 设备配置
    - 返回描述性的 ValidationError
    - _需求：6.4、7.1-7.5_
  
  - [ ]* 39.4 为传感器读取器编写集成测试
    - 测试串口传感器读取（使用模拟端口）
    - 测试 USB 传感器读取（使用模拟设备）
    - 测试错误处理
    - 测试读取流

- [ ] 40. 实现性能监控
  - [ ] 40.1 创建性能指标类型
    - 定义周期时间（CycleTime）、命令延迟（CommandLatency）、帧率（FrameRate）指标
    - _需求：26.1-26.5_
  
  - [ ] 40.2 实现指标收集
    - 测量自动化步骤周期时间
    - 测量控制板命令延迟
    - 测量可视化帧率
    - 使用纯函数计算指标
    - 将收集逻辑隔离在副作用边界内
    - _需求：26.1-26.5_
  
  - [ ] 40.3 通过 API 和 UI 暴露指标
    - 为 Web API 添加指标端点
    - 在状态栏（StatusBar）组件中显示指标
    - _需求：26.4_
  
  - [ ]* 40.4 为指标计算编写单元测试
    - 测试周期时间计算
    - 测试延迟测量
    - 测试帧率计算

- [ ] 41. 实现并发安全
  - [ ] 41.1 复查共享状态的线程安全性
    - 识别所有可变共享状态
    - 确保使用适当的同步原语
    - 记录并发边界
    - _需求：25.1-25.5_
  
  - [ ] 41.2 添加并发测试
    - 测试并发机器修改
    - 测试并发自动化执行
    - 测试并发配置更新
    - _需求：25.1-25.5_

- [ ] 42. 检查点 - 确保所有测试通过
  - 确保所有测试通过，如有问题请询问用户。


- [ ] 43. 实现端到端集成和优化
  - [ ] 43.1 创建端到端集成测试
    - 测试完整工作流：创建机器 → 配置 → 创建自动化 → 执行 → 可视化
    - 测试配置保存/加载周期
    - 测试错误恢复场景
    - _需求：所有需求_
  
  - [ ] 43.2 实现全面的错误处理
    - 复查所有错误路径
    - 确保所有错误都具有描述性
    - 确保所有错误都被正确记录
    - 确保 UI 适当显示所有错误
    - _需求：24.1-24.6_
  
  - [ ] 43.3 优化性能
    - 分析关键路径性能
    - 必要时优化坐标计算
    - 必要时优化状态更新
    - 确保可视化保持最低 10 FPS
    - _需求：13.3、26.1-26.5_
  
  - [ ] 43.4 添加全面的日志记录
    - 记录所有状态转换
    - 记录所有命令执行
    - 记录所有带上下文的错误
    - 使用结构化日志
    - _需求：15.4、24.4_
  
  - [ ] 43.5 创建用户文档
    - 记录 DSL 语法和语义
    - 记录零件库和分类
    - 记录配置选项
    - 记录控制板设置
    - 提供示例自动化脚本
  
  - [ ] 43.6 创建开发者文档
    - 记录架构和设计决策
    - 记录纯函数边界
    - 记录副作用边界
    - 记录测试策略
    - 记录如何添加新零件类型
    - 记录如何添加新控制板
  
  - [ ] 43.7 复查代码质量
    - 确保所有代码遵循函数式编程原则
    - 确保所有公共 API 都有完善的文档
    - 确保所有错误类型都具有描述性
    - 运行静态分析（dotnet format、分析器）
    - _需求：19.1-19.5、20.1-20.5、21.1-21.5_

- [ ] 44. 最终检查点 - 全面测试和验证
  - 运行所有单元测试，确保纯函数覆盖率达 80% 以上
  - 运行所有属性测试，每个测试至少迭代 100 次
  - 运行所有集成测试
  - 运行端到端测试
  - 验证所有 23 个正确性属性都经过测试
  - 验证所有需求都已实现
  - 验证系统编译无错误或警告
  - 使用模拟控制板进行测试
  - 记录所有已知限制或未来工作计划
  - _需求：27.1-27.5、所有需求_

## 说明

- **DSL 实现**：使用 C# 代码直接构建 AST，而非基于文本的 DSL 解析。任务 10（词法分析器）和任务 11 的部分内容（解析器、美化打印器）已跳过。
- 标有 `*` 的任务为可选测试任务，为加快最小可行产品（MVP）开发可跳过，但强烈建议在生产环境中实现
- 标有 `~~删除线~~` 的任务因采用 C# 作为 DSL 的方案而跳过
- 每个任务都关联了特定的需求以确保可追溯性
- 检查点确保在整个实施过程中进行增量验证
- 属性测试验证设计文档中的通用正确性属性
- 单元测试验证特定示例和边缘情况
- 实现遵循严格的函数式编程原则，清晰分离纯函数和副作用
- 进入下一个主要阶段前，所有代码必须能无错误编译
- 需求 16-18（浮高机）标记为未来实现项，但其依赖项（零件、传感器、DSL、控制板）已完全实现

## 实施顺序依据

1. **基础先行（任务 1-8）**：核心类型、坐标系统和组合模型构成数学基础
2. **DSL AST 和验证（任务 9、11）**：定义 AST 类型和语义验证（无需解析 - 用户编写 C# 代码）
3. **配置（任务 13-16）**：配置类型、验证和持久化
4. **硬件抽象（任务 17-21）**：控制板和设备通信
5. **执行引擎（任务 22-25）**：解释和执行自动化逻辑
6. **可视化（任务 26-27）**：实时状态可视化
7. **错误处理（任务 28）**：全面的错误管理
8. **应用层（任务 29-35）**：后端 API 和前端 UI
9. **测试基础设施（任务 36-38）**：基于属性的测试生成器
10. **附加功能（任务 39-41）**：传感器、监控、并发
11. **集成和优化（任务 43-44）**：端到端测试和文档

此顺序确保每一层都构建在稳定的基础上，并通过频繁的检查点及早发现问题。

**注**：任务 10（词法分析器）和任务 11 的部分内容（解析器）已跳过，因为我们直接使用 C# 代码构建 AST。
