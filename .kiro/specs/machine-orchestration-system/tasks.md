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

- [x] 6. Implement Part Library component
  - [x] 6.1 Create IPartLibrary interface and implementation
    - Implement GetAllParts pure function
    - Implement GetPartsByCategory pure function
    - Implement GetPartById pure function returning Option<Part>
    - Use immutable Seq<Part> for storage
    - _Requirements: 1.1-1.2, 1.14-1.15_
  
  - [x] 6.2 Populate part library with initial parts
    - Add motor parts (LinearScrew, RotaryTable)
    - Add actuator parts (Cylinder, Gripper, Suction, Indicator)
    - Add sensor parts (Pressure, Micrometer, Scanner)
    - Add static parts (Shaft, Bracket)
    - _Requirements: 1.1, 1.16-1.19_
  
  - [x] 6.3 Write property test for part category query consistency
    - **Property 2: 零件分类查询一致性**
    - **Validates: Requirements 1.14-1.15**
  
  - [x] 6.4 Write unit tests for part library queries
    - Test GetPartsByCategory for each category
    - Test GetPartById with valid and invalid IDs

- [x] 7. Implement composition engine
  - [x] 7.1 Create ICompositionEngine interface
    - Define Compose method signature
    - Define ApplyTransformation method signature
    - Define ComputeAbsoluteCoordinates method signature
    - _Requirements: 3.2, 3.6-3.7_
  
  - [x] 7.2 Implement CompositionEngine with pure functions
    - Implement Compose using AddChild method
    - Implement ApplyTransformation delegating to entity method
    - Implement ComputeAbsoluteCoordinates delegating to entity method
    - Handle composition errors with Either<CompositionError, T>
    - _Requirements: 3.2-3.7_
  
  - [x] 7.3 Write unit tests for composition error cases
    - Test circular reference detection
    - Test max depth exceeded
    - Test adding child to leaf node

- [x] 8. Checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.


- [x] 9. Implement DSL AST types
  - [x] 9.1 Create Statement algebraic data type
    - Implement Action, Wait, WaitUntil, Sequence, Parallel, Loop, If variants
    - Use sealed record pattern for sum type
    - _Requirements: 8.1-8.4_
  
  - [x] 9.2 Create Condition algebraic data type
    - Implement SensorState, StateSensor, SensorValue, And, Or, Not variants
    - Implement ComparisonOp enum
    - _Requirements: 8.1-8.4, 28.8_
  
  - [x] 9.3 Create Ast record wrapping Seq<Statement>
    - Simple wrapper for statement sequence
    - _Requirements: 8.2_
  
  - [x] 9.4 Write unit tests for AST construction
    - Test creating various statement types
    - Test nested structures

- [ ] 10. ~~Implement DSL Lexer~~ (SKIPPED - Using C# as DSL)
  - **Note**: User will directly construct AST using C# code instead of parsing text-based DSL
  - This eliminates the need for Lexer, Parser, and Pretty Printer
  - Benefits: IDE support, type safety, no parsing errors, easier debugging
  - [ ]* ~~10.1 Create Token types for DSL~~
  - [ ]* ~~10.2 Implement Lexer class~~
  - [ ]* ~~10.3 Write unit tests for lexer~~

- [ ] 11. Implement DSL Validator (Simplified)
  - [ ] 11.1 Create IDslValidator interface
    - Define Validate method returning Either<ValidationError, Unit>
    - Validate AST semantic correctness (no parsing needed)
    - _Requirements: 8.3, 9.2_
  
  - [ ]* ~~11.2 Implement recursive descent parser~~ (SKIPPED - Using C# as DSL)
  
  - [ ] 11.3 Implement semantic validator
    - Validate entity IDs exist in machine
    - Validate sensor references are valid
    - Validate action compatibility with part types
    - Validate state sensor references for actuators
    - Return descriptive ValidationError
    - _Requirements: 8.3, 9.2, 28.8_
  
  - [ ]* ~~11.4 Implement pretty printer~~ (SKIPPED - Using C# as DSL)
  
  - [ ]* ~~11.5 Write property test for parse/print round-trip~~ (SKIPPED)
  
  - [ ]* ~~11.6 Write property test for parser error handling~~ (SKIPPED)
  
  - [ ]* 11.7 Write unit tests for validator
    - Test entity ID validation
    - Test sensor reference validation
    - Test action compatibility validation
    - Test multiple error collection

- [ ] 12. Checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.


- [ ] 13. Implement configuration types and validation
  - [ ] 13.1 Create PartConfig algebraic data type
    - Implement MotorConfig, ActuatorConfig, SensorConfig, Static variants
    - Include BoardConnection, LimitSensors, StateSensorPorts types
    - Use Option<T> for optional configurations
    - _Requirements: 11.1-11.8_
  
  - [ ] 13.2 Create SensorConnection algebraic data type
    - Implement SerialSingle, SerialMultiple, Usb variants
    - _Requirements: 7.1-7.5_
  
  - [ ] 13.3 Create MachineConfig and ControlBoardConfig types
    - Implement MachineConfig record
    - Implement ControlBoardConfig sum type (LeiSai, ZhengYunDong, Simulated)
    - Use JsonDerivedType attributes for polymorphic serialization
    - _Requirements: 10.1-10.5, 12.1-12.4_
  
  - [ ] 13.4 Implement IConfigValidator interface and implementation
    - Validate sensor port assignments
    - Validate control board compatibility
    - Validate motor configurations
    - Validate actuator sensor configurations
    - Return Either<ValidationError, Unit>
    - Collect multiple validation errors
    - _Requirements: 11.9-11.12, 12.2-12.4_
  
  - [ ]* 13.5 Write property test for configuration validation completeness
    - **Property 16: 配置验证完整性**
    - **Validates: Requirements 11.9-11.10**
  
  - [ ]* 13.6 Write property test for validation error descriptiveness
    - **Property 17: 配置验证错误描述性**
    - **Validates: Requirements 11.11-11.12**
  
  - [ ]* 13.7 Write unit tests for specific validation scenarios
    - Test missing sensor port detection
    - Test incompatible control board parameters
    - Test multiple error collection

- [ ] 14. Implement configuration serialization
  - [ ] 14.1 Create IConfigSerializer interface
    - Define Serialize method returning Either<SerializationError, string>
    - Define Deserialize method returning Either<DeserializationError, MachineConfig>
    - _Requirements: 23.1-23.2_
  
  - [ ] 14.2 Implement ConfigSerializer using System.Text.Json
    - Configure JSON options for polymorphic types
    - Handle Option<T> serialization
    - Handle algebraic data types serialization
    - Return descriptive errors on failure
    - _Requirements: 23.1-23.3_
  
  - [ ]* 14.3 Write property test for serialization round-trip
    - **Property 13: 配置序列化往返**
    - **Validates: Requirements 23.4**
  
  - [ ]* 14.4 Write property test for deserialization error handling
    - **Property 14: 配置反序列化错误处理**
    - **Validates: Requirements 23.5**
  
  - [ ]* 14.5 Write unit tests for serialization edge cases
    - Test empty configurations
    - Test deeply nested structures
    - Test corrupted JSON handling

- [ ] 15. Implement configuration persistence
  - [ ] 15.1 Create IConfigPersistence interface
    - Define Save method returning Task<Either<IoError, Unit>>
    - Define Load method returning Task<Either<IoError, MachineConfig>>
    - _Requirements: 23.1-23.2_
  
  - [ ] 15.2 Implement ConfigPersistence with file I/O
    - Use async file operations
    - Delegate to ConfigSerializer for serialization
    - Handle file system errors gracefully
    - Return descriptive IoError on failure
    - _Requirements: 23.1-23.5_
  
  - [ ]* 15.3 Write integration tests for persistence
    - Test save and load cycle
    - Test file not found handling
    - Test permission errors

- [ ] 16. Checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.


- [ ] 17. Implement control board abstraction
  - [ ] 17.1 Create Command algebraic data type
    - Implement Motor, Actuator, ReadSensor, ReadStateSensor, EmergencyStop variants
    - Create MotorId, ActuatorId, SensorId, StateSensorId newtypes
    - _Requirements: 10.1-10.6_
  
  - [ ] 17.2 Create IControlBoard interface
    - Define Initialize method returning Task<Either<ControlBoardError, Unit>>
    - Define SendMotorCommand method
    - Define SendActuatorCommand method
    - Define ReadSensor method
    - Define ReadStateSensor method
    - Define EmergencyStop method
    - Define StateStream property returning IObservable<ControlBoardState>
    - _Requirements: 10.1-10.6, 28.1-28.6_
  
  - [ ] 17.3 Create ControlBoardError algebraic data type
    - Implement ConnectionError, CommandFailed, NotInitialized variants
    - _Requirements: 24.1-24.6_
  
  - [ ]* 17.4 Write unit tests for command type construction
    - Test creating various command types
    - Test newtype wrappers

- [ ] 18. Implement simulated control board
  - [ ] 18.1 Implement SimulatedControlBoard class
    - Implement IControlBoard interface
    - Simulate motor movements with delays
    - Simulate actuator actions with state changes
    - Simulate sensor readings with random values
    - Use configurable latency
    - Publish state changes to StateStream
    - _Requirements: 10.4, 13.8-13.9_
  
  - [ ] 18.2 Implement state tracking for simulated devices
    - Track motor positions
    - Track actuator states
    - Track sensor values
    - Use immutable state updates
    - _Requirements: 13.8-13.10_
  
  - [ ]* 18.3 Write integration tests for simulated board
    - Test motor command execution
    - Test actuator command execution
    - Test sensor reading
    - Test state stream updates

- [ ] 19. Implement LeiSai control board
  - [ ] 19.1 Implement LeiSaiBoard class
    - Implement IControlBoard interface
    - Integrate with LeiSai SDK/API
    - Map commands to LeiSai protocol
    - Handle connection management
    - Publish state changes to StateStream
    - _Requirements: 10.2_
  
  - [ ] 19.2 Implement error handling and retry logic
    - Handle connection errors
    - Implement command retry with exponential backoff
    - Return descriptive ControlBoardError
    - _Requirements: 24.1-24.6_
  
  - [ ]* 19.3 Write integration tests with mocked LeiSai SDK
    - Test command sending
    - Test error handling
    - Test retry logic

- [ ] 20. Implement ZhengYunDong control board
  - [ ] 20.1 Implement ZhengYunDongBoard class
    - Implement IControlBoard interface
    - Integrate with ZhengYunDong SDK/API
    - Map commands to ZhengYunDong protocol
    - Handle connection management
    - Publish state changes to StateStream
    - _Requirements: 10.3_
  
  - [ ] 20.2 Implement error handling and retry logic
    - Handle connection errors
    - Implement command retry with exponential backoff
    - Return descriptive ControlBoardError
    - _Requirements: 24.1-24.6_
  
  - [ ]* 20.3 Write integration tests with mocked ZhengYunDong SDK
    - Test command sending
    - Test error handling
    - Test retry logic

- [ ] 21. Checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.


- [ ] 22. Implement DSL interpreter (pure function part)
  - [ ] 22.1 Create ExecutionState immutable type
    - Implement ProgramCounter, MachineState, CallStack, Bindings fields
    - Create PartState sum type (Motor, Actuator, Sensor)
    - Create SensorReading sum type
    - Create StackFrame and Value types
    - _Requirements: 15.2_
  
  - [ ] 22.2 Create IDslInterpreter interface
    - Define Step method returning Either<ExecutionError, ExecutionState>
    - Define IsComplete method returning bool
    - _Requirements: 15.1-15.6_
  
  - [ ] 22.3 Implement DslInterpreter with pure state transitions
    - Implement Step for each statement type (Action, Wait, Sequence, etc.)
    - Evaluate conditions purely
    - Update execution state immutably
    - Return ExecutionError for invalid transitions
    - _Requirements: 15.1-15.6_
  
  - [ ]* 22.4 Write property test for state transition determinism
    - **Property 19: 状态转换确定性**
    - **Validates: Requirements 15.2**
  
  - [ ]* 22.5 Write property test for state transition immutability
    - **Property 20: 状态转换不可变性**
    - **Validates: Requirements 15.2**
  
  - [ ]* 22.6 Write unit tests for each statement type
    - Test Action execution
    - Test Wait timing
    - Test Sequence ordering
    - Test Parallel execution
    - Test Loop iteration
    - Test If branching

- [ ] 23. Implement automation executor (side effect part)
  - [ ] 23.1 Create IDslExecutor interface
    - Define ExecuteCommand method returning Task<Either<ExecutionError, Unit>>
    - Define ExecutionStateStream property returning IObservable<ExecutionState>
    - _Requirements: 15.1-15.6_
  
  - [ ] 23.2 Implement AutomationExecutor class
    - Integrate IDslInterpreter for pure state transitions
    - Integrate IControlBoard for command execution
    - Execute commands as side effects
    - Publish execution state to stream using System.Reactive
    - Handle execution errors gracefully
    - Implement emergency stop on error
    - _Requirements: 15.1-15.6, 24.1-24.6_
  
  - [ ] 23.3 Implement error recovery and safe stop
    - Stop all motors on error
    - Set actuators to safe state
    - Log error context
    - Return descriptive ExecutionError
    - _Requirements: 24.1-24.6_
  
  - [ ]* 23.4 Write integration tests for automation execution
    - Test simple sequence execution
    - Test parallel execution
    - Test loop execution
    - Test error handling and safe stop
    - Test state stream updates

- [ ] 24. Implement automation logic storage
  - [ ] 24.1 Create AutomationLogic record type
    - Include LogicId, Name, Ast fields
    - _Requirements: 14.1-14.5_
  
  - [ ] 24.2 Create IAutomationLogicManager interface
    - Define AddLogic method returning Either<LogicError, IAutomationLogicManager>
    - Define GetLogic method returning Option<AutomationLogic>
    - Define ListLogics method returning Seq<LogicId>
    - _Requirements: 14.1-14.3_
  
  - [ ] 24.3 Implement AutomationLogicManager with immutable storage
    - Use HashMap<LogicId, AutomationLogic> for storage
    - Implement pure functions for logic management
    - _Requirements: 14.1-14.5_
  
  - [ ] 24.4 Implement logic serialization and persistence
    - Serialize AutomationLogic to JSON
    - Persist to file system
    - Load from file system
    - _Requirements: 14.4-14.5_
  
  - [ ]* 24.5 Write property test for logic serialization round-trip
    - **Property 15: 自动化逻辑序列化往返**
    - **Validates: Requirements 14.5**
  
  - [ ]* 24.6 Write unit tests for logic management
    - Test adding logic
    - Test retrieving logic
    - Test listing logics

- [ ] 25. Checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.


- [ ] 26. Implement visualization state mapping
  - [ ] 26.1 Create VisualState types
    - Implement VisualState record with PartVisualState map and timestamp
    - Implement PartVisualState record with coordinate, action, color
    - Implement PartVisualAction sum type (MotorMoving, MotorIdle, ActuatorActive, ActuatorIdle)
    - _Requirements: 13.1-13.2_
  
  - [ ] 26.2 Create IStateMapper interface
    - Define MapToVisualState method (pure function)
    - Define ComputeAnimationFrame method (pure function)
    - _Requirements: 13.1-13.2, 5.2_
  
  - [ ] 26.3 Implement StateMapper with pure functions
    - Map MachineState to VisualState
    - Compute animation frames with linear interpolation
    - Support both virtual and real devices uniformly
    - _Requirements: 13.1, 13.10, 5.2_
  
  - [ ]* 26.4 Write property test for state mapping purity and determinism
    - **Property 21: 状态映射纯函数性和确定性**
    - **Validates: Requirements 13.1, 13.10**
  
  - [ ]* 26.5 Write property test for animation frame interpolation
    - **Property 10: 动画帧插值线性性**
    - **Validates: Requirements 5.2**
  
  - [ ]* 26.6 Write unit tests for state mapping
    - Test motor state mapping
    - Test actuator state mapping
    - Test animation frame calculation

- [ ] 27. Implement visualization service
  - [ ] 27.1 Create IVisualizationService interface
    - Define VisualStateStream property returning IObservable<VisualState>
    - Define SetUpdateRate method
    - _Requirements: 13.3-13.5_
  
  - [ ] 27.2 Implement VisualizationService with System.Reactive
    - Subscribe to ExecutionStateStream
    - Map execution state to visual state using StateMapper
    - Publish visual state at configured rate (minimum 10 FPS)
    - Use Observable.Sample or Observable.Throttle for rate limiting
    - _Requirements: 13.3-13.5_
  
  - [ ]* 27.3 Write integration tests for visualization service
    - Test state stream subscription
    - Test update rate configuration
    - Test minimum 10 FPS guarantee

- [ ] 28. Implement error types and handling
  - [ ] 28.1 Create all error algebraic data types
    - Implement CompositionError (InvalidCoordinate, CircularReference, MaxDepthExceeded, CannotAddChildToLeaf)
    - Implement ParseError with line, column, message
    - Implement ValidationError (MissingField, InvalidValue, MissingSensorPort, IncompatibleConfig, Multiple)
    - Implement ExecutionError (HardwareError, Timeout, InvalidStateTransition, SensorError)
    - Implement SerializationError and DeserializationError
    - Implement IoError, SensorError, LogicError
    - _Requirements: 24.1-24.6_
  
  - [ ] 28.2 Create SystemError top-level sum type
    - Wrap all error types in unified hierarchy
    - _Requirements: 24.2-24.3_
  
  - [ ] 28.3 Implement error context and logging
    - Create ErrorContext record with timestamp, state, stack trace
    - Implement structured error logging
    - _Requirements: 24.4_
  
  - [ ]* 28.4 Write property test for error propagation
    - **Property 22: 错误传播完整性**
    - **Validates: Requirements 24.2-24.3**
  
  - [ ]* 28.5 Write unit tests for error handling
    - Test error creation
    - Test error context capture
    - Test error logging

- [ ] 29. Checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.


- [ ] 30. Implement backend application layer
  - [ ] 30.1 Create MachineService class
    - Implement machine composition operations
    - Integrate ICompositionEngine
    - Integrate IPartLibrary
    - Expose operations via clean API
    - _Requirements: 1.1-1.2, 3.1-3.7_
  
  - [ ] 30.2 Create AutomationService class
    - Implement automation logic management
    - Integrate IAutomationLogicManager
    - Integrate IDslExecutor
    - Expose execution control API
    - _Requirements: 14.1-14.5, 15.1-15.6_
  
  - [ ] 30.3 Create VisualizationService integration
    - Integrate IVisualizationService
    - Expose visual state stream via SignalR
    - _Requirements: 13.1-13.10_
  
  - [ ] 30.4 Implement Web API controllers
    - Create PartLibraryController (GET endpoints)
    - Create MachineController (CRUD endpoints)
    - Create AutomationController (execution control endpoints)
    - Create ConfigurationController (save/load endpoints)
    - Use ASP.NET Core minimal APIs or controllers
    - Return Either<Error, T> wrapped in appropriate HTTP responses
    - _Requirements: All application-level requirements_
  
  - [ ] 30.5 Implement SignalR hubs for real-time communication
    - Create VisualizationHub for streaming visual state
    - Create ExecutionHub for streaming execution state
    - Use System.Reactive to bridge observables to SignalR
    - _Requirements: 13.3-13.10, 15.1-15.6_
  
  - [ ]* 30.6 Write integration tests for API endpoints
    - Test part library queries
    - Test machine CRUD operations
    - Test automation execution
    - Test configuration save/load
  
  - [ ]* 30.7 Write integration tests for SignalR hubs
    - Test visual state streaming
    - Test execution state streaming
    - Test connection handling

- [ ] 31. Implement frontend project structure
  - [ ] 31.1 Set up Vite + React 19 + TypeScript project
    - Initialize Vite project
    - Configure TypeScript with strict mode
    - Add dependencies: React 19, Three.js, @react-three/fiber, @react-three/drei
    - Add dependencies: framer-motion, @react-spring/three
    - Add dependencies: Tailwind CSS, lucide-react
    - Configure build and dev server
    - _Requirements: Frontend technology stack_
  
  - [ ] 31.2 Create TypeScript type definitions
    - Define types matching backend models (Part, ComposableEntity, Coordinate, etc.)
    - Define API response types
    - Define SignalR message types
    - _Requirements: Type safety_
  
  - [ ] 31.3 Implement API client service
    - Create axios-based API client
    - Implement methods for all backend endpoints
    - Handle errors gracefully
    - _Requirements: Backend integration_
  
  - [ ] 31.4 Implement SignalR client service
    - Create SignalR connection manager
    - Subscribe to VisualizationHub
    - Subscribe to ExecutionHub
    - Handle reconnection
    - _Requirements: 13.3-13.10, 15.1-15.6_

- [ ] 32. Implement frontend 3D visualization
  - [ ] 32.1 Create Scene3D component with React Three Fiber
    - Set up Canvas with camera, lights, controls
    - Implement OrbitControls for camera manipulation
    - Add grid and axes helpers
    - _Requirements: 5.1-5.4, 13.1-13.10_
  
  - [ ] 32.2 Create Part3D component for rendering parts
    - Render different part types (motors, actuators, sensors, static)
    - Use appropriate geometries and materials
    - Apply coordinate transformations
    - Visualize part actions (motor movement, actuator state)
    - _Requirements: 5.1-5.4, 13.1-13.10_
  
  - [ ] 32.3 Create Machine3D component for rendering composable entities
    - Recursively render ComposableEntity hierarchy
    - Apply transformation matrices
    - Highlight selected parts
    - _Requirements: 3.1-3.7, 5.1-5.4_
  
  - [ ] 32.4 Implement animation system
    - Animate motor movements
    - Animate actuator actions
    - Use @react-spring/three for smooth animations
    - Update at minimum 10 FPS
    - _Requirements: 5.1-5.4, 13.3_
  
  - [ ] 32.5 Integrate real-time state updates
    - Subscribe to visual state stream from SignalR
    - Update 3D scene based on state changes
    - Handle state transitions smoothly
    - _Requirements: 13.3-13.10_

- [ ] 33. Checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.


- [ ] 34. Implement frontend UI components
  - [ ] 34.1 Create PartLibrary component
    - Display parts organized by category
    - Implement category filtering
    - Show part details on selection
    - Drag-and-drop support for adding parts to machine
    - _Requirements: 1.1-1.2, 1.14-1.15_
  
  - [ ] 34.2 Create MachineEditor component
    - Display machine hierarchy tree
    - Allow adding/removing parts
    - Allow editing part coordinates
    - Allow editing part configurations
    - Integrate with 3D scene for visual feedback
    - _Requirements: 2.1-2.10, 3.1-3.7, 11.1-11.12_
  
  - [ ] 34.3 Create ConfigurationPanel component
    - Display part configuration forms
    - Validate configuration inputs
    - Show validation errors
    - Support motor, actuator, sensor configurations
    - _Requirements: 11.1-11.12, 12.1-12.4_
  
  - [ ] 34.4 Create AutomationPanel component
    - Display list of automation logics
    - Allow creating/editing DSL scripts
    - Provide DSL syntax highlighting
    - Show parse errors with line/column
    - Execute/stop automation
    - _Requirements: 8.1-8.5, 14.1-14.5, 15.1-15.6_
  
  - [ ] 34.5 Create ControlPanel component
    - Select control board type
    - Configure control board parameters
    - Show connection status
    - Emergency stop button
    - _Requirements: 10.1-10.6, 12.1-12.4_
  
  - [ ] 34.6 Create StatusBar component
    - Show execution state
    - Show errors and warnings
    - Show performance metrics
    - _Requirements: 13.1-13.10, 26.1-26.5_

- [ ] 35. Implement frontend state management
  - [ ] 35.1 Create React hooks for machine state
    - useMachine hook for machine CRUD operations
    - usePartLibrary hook for part queries
    - useAutomation hook for automation control
    - useVisualization hook for visual state subscription
    - _Requirements: All frontend requirements_
  
  - [ ] 35.2 Implement error handling and notifications
    - Display error notifications
    - Show validation errors inline
    - Handle API errors gracefully
    - _Requirements: 24.1-24.6_

- [ ] 36. Implement FsCheck custom generators
  - [ ] 36.1 Create generators for core types
    - Coordinate generator
    - TransformationMatrix generator
    - ComposableEntity generator (with size control)
    - Part generator
    - PartConfig generator
    - _Requirements: 27.1-27.5_
  
  - [ ] 36.2 Create generators for DSL types
    - Statement generator
    - Condition generator
    - Ast generator
    - _Requirements: 27.1-27.5_
  
  - [ ] 36.3 Create generators for configuration types
    - MachineConfig generator
    - ControlBoardConfig generator
    - AutomationLogic generator
    - _Requirements: 27.1-27.5_
  
  - [ ]* 36.4 Write tests for generators
    - Test generator produces valid values
    - Test generator respects size parameter
    - Test generator coverage

- [ ] 37. Checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.


- [ ] 38. Implement remaining property-based tests
  - [ ]* 38.1 Write property test for control board parameter type safety
    - **Property 18: 控制板参数类型安全**
    - **Validates: Requirements 12.4**
    - Note: This is primarily enforced by the type system at compile time
  
  - [ ]* 38.2 Verify all 23 properties have corresponding tests
    - Review all property tests implemented in previous tasks
    - Ensure each property from design document is covered
    - Document any properties that are compile-time guarantees

- [ ] 39. Implement sensor management
  - [ ] 39.1 Create ISensorReader interface
    - Define Read method returning Task<Either<SensorError, SensorReading>>
    - Define ReadingStream method returning IObservable<SensorReading>
    - _Requirements: 6.1-6.4, 7.1-7.5_
  
  - [ ] 39.2 Implement sensor readers for different connection types
    - Implement SerialSingleSensorReader
    - Implement SerialMultipleSensorReader
    - Implement UsbSensorReader
    - Handle I/O errors gracefully
    - Publish readings to stream
    - _Requirements: 7.1-7.5_
  
  - [ ] 39.3 Implement sensor configuration validation
    - Validate serial port configurations
    - Validate USB device configurations
    - Return descriptive ValidationError
    - _Requirements: 6.4, 7.1-7.5_
  
  - [ ]* 39.4 Write integration tests for sensor readers
    - Test serial sensor reading (with mocked port)
    - Test USB sensor reading (with mocked device)
    - Test error handling
    - Test reading stream

- [ ] 40. Implement performance monitoring
  - [ ] 40.1 Create performance metrics types
    - Define CycleTime, CommandLatency, FrameRate metrics
    - _Requirements: 26.1-26.5_
  
  - [ ] 40.2 Implement metrics collection
    - Measure automation step cycle time
    - Measure control board command latency
    - Measure visualization frame rate
    - Use pure functions for metric calculation
    - Isolate collection in side effect boundary
    - _Requirements: 26.1-26.5_
  
  - [ ] 40.3 Expose metrics via API and UI
    - Add metrics endpoint to Web API
    - Display metrics in StatusBar component
    - _Requirements: 26.4_
  
  - [ ]* 40.4 Write unit tests for metrics calculation
    - Test cycle time calculation
    - Test latency measurement
    - Test frame rate calculation

- [ ] 41. Implement concurrency safety
  - [ ] 41.1 Review shared state for thread safety
    - Identify all mutable shared state
    - Ensure proper synchronization primitives
    - Document concurrency boundaries
    - _Requirements: 25.1-25.5_
  
  - [ ] 41.2 Add concurrency tests
    - Test concurrent machine modifications
    - Test concurrent automation execution
    - Test concurrent configuration updates
    - _Requirements: 25.1-25.5_

- [ ] 42. Checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.


- [ ] 43. Implement end-to-end integration and polish
  - [ ] 43.1 Create end-to-end integration tests
    - Test complete workflow: create machine → configure → create automation → execute → visualize
    - Test configuration save/load cycle
    - Test error recovery scenarios
    - _Requirements: All requirements_
  
  - [ ] 43.2 Implement comprehensive error handling
    - Review all error paths
    - Ensure all errors are descriptive
    - Ensure all errors are properly logged
    - Ensure UI displays all errors appropriately
    - _Requirements: 24.1-24.6_
  
  - [ ] 43.3 Optimize performance
    - Profile critical paths
    - Optimize coordinate calculations if needed
    - Optimize state updates if needed
    - Ensure visualization maintains minimum 10 FPS
    - _Requirements: 13.3, 26.1-26.5_
  
  - [ ] 43.4 Add comprehensive logging
    - Log all state transitions
    - Log all command executions
    - Log all errors with context
    - Use structured logging
    - _Requirements: 15.4, 24.4_
  
  - [ ] 43.5 Create user documentation
    - Document DSL syntax and semantics
    - Document part library and categories
    - Document configuration options
    - Document control board setup
    - Provide example automation scripts
  
  - [ ] 43.6 Create developer documentation
    - Document architecture and design decisions
    - Document pure function boundaries
    - Document side effect boundaries
    - Document testing strategy
    - Document how to add new part types
    - Document how to add new control boards
  
  - [ ] 43.7 Review code quality
    - Ensure all code follows functional programming principles
    - Ensure all public APIs are well-documented
    - Ensure all error types are descriptive
    - Run static analysis (dotnet format, analyzers)
    - _Requirements: 19.1-19.5, 20.1-20.5, 21.1-21.5_

- [ ] 44. Final checkpoint - Comprehensive testing and validation
  - Run all unit tests and ensure 80%+ coverage for pure functions
  - Run all property tests with 100+ iterations each
  - Run all integration tests
  - Run end-to-end tests
  - Verify all 23 correctness properties are tested
  - Verify all requirements are implemented
  - Verify system compiles with no errors or warnings
  - Test with simulated control board
  - Document any known limitations or future work
  - _Requirements: 27.1-27.5, All requirements_

## Notes

- **DSL Implementation**: Using C# code to directly construct AST instead of text-based DSL parsing. Tasks 10 (Lexer) and parts of Task 11 (Parser, Pretty Printer) are skipped.
- Tasks marked with `*` are optional testing tasks and can be skipped for faster MVP, but are highly recommended for production quality
- Tasks marked with `~~strikethrough~~` are skipped due to the C#-as-DSL approach
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation throughout implementation
- Property tests validate universal correctness properties from the design document
- Unit tests validate specific examples and edge cases
- The implementation follows strict functional programming principles with clear separation between pure functions and side effects
- All code must compile without errors before proceeding to next major phase
- Requirements 16-18 (浮高机) are marked as future implementation, but their dependencies (parts, sensors, DSL, control boards) are fully implemented

## Implementation Order Rationale

1. **Foundation First (Tasks 1-8)**: Core types, coordinate system, and composition model form the mathematical foundation
2. **DSL AST and Validation (Tasks 9, 11)**: Define AST types and semantic validation (no parsing needed - users write C# code)
3. **Configuration (Tasks 13-16)**: Configuration types, validation, and persistence
4. **Hardware Abstraction (Tasks 17-21)**: Control boards and device communication
5. **Execution Engine (Tasks 22-25)**: Interpret and execute automation logic
6. **Visualization (Tasks 26-27)**: Real-time state visualization
7. **Error Handling (Task 28)**: Comprehensive error management
8. **Application Layer (Tasks 29-35)**: Backend API and frontend UI
9. **Testing Infrastructure (Tasks 36-38)**: Property-based test generators
10. **Additional Features (Tasks 39-41)**: Sensors, monitoring, concurrency
11. **Integration and Polish (Tasks 43-44)**: End-to-end testing and documentation

This order ensures that each layer builds on stable foundations, with frequent checkpoints to catch issues early.

**Note**: Task 10 (Lexer) and parts of Task 11 (Parser) are skipped because we use C# code directly to construct AST.

