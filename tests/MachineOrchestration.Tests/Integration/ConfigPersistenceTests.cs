using LanguageExt;
using MachineOrchestration.Configuration.Persistence;
using MachineOrchestration.Configuration.Serialization;
using MachineOrchestration.Configuration.Types;
using MachineOrchestration.Core.Types;
using Xunit;
using static LanguageExt.Prelude;

namespace MachineOrchestration.Tests.Integration;

/// <summary>
/// 配置持久化集成测试
/// </summary>
/// <remarks>
/// 测试配置的保存和加载功能，包括错误处理。
/// 验证：需求 23.1-23.5
/// </remarks>
public sealed class ConfigPersistenceTests : IDisposable
{
    private readonly IConfigPersistence _persistence;
    private readonly string _testDirectory;
    private readonly List<string> _testFiles;
    
    public ConfigPersistenceTests()
    {
        var serializer = new ConfigSerializer();
        _persistence = new ConfigPersistence(serializer);
        
        // 创建临时测试目录
        _testDirectory = Path.Combine(Path.GetTempPath(), $"ConfigPersistenceTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        
        _testFiles = new List<string>();
    }
    
    public void Dispose()
    {
        // 清理测试文件和目录
        try
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
        }
        catch
        {
            // 忽略清理错误
        }
    }
    
    private string GetTestFilePath(string fileName)
    {
        var path = Path.Combine(_testDirectory, fileName);
        _testFiles.Add(path);
        return path;
    }
    
    private MachineConfig CreateTestConfig()
    {
        var entityId = new EntityId(Guid.NewGuid());
        var partId = new PartId(Guid.NewGuid());
        
        var part = new Part(
            partId,
            "TestMotor",
            new PartType.Motor(new MotorType.LinearScrew(100f, 500f)),
            PartCategory.MotorType.Instance,
            new System.Numerics.Vector3(100, 50, 50));
        
        var motorConfig = new MotorConfig(
            100f,
            HomingMode.NegativeLimit,
            new BoardConnection(0),
            new LimitSensors(Option<SensorPort>.None, Option<SensorPort>.None));
        
        var partConfig = new PartConfig.Motor(motorConfig);
        
        var machine = new ComposableEntity.Part(
            entityId,
            part,
            Coordinate.Identity,
            partConfig);
        
        var controlBoard = new ControlBoardConfig.Simulated(100);
        
        return MachineConfig.Empty(machine, controlBoard);
    }
    
    /// <summary>
    /// 测试：保存和加载循环
    /// </summary>
    /// <remarks>
    /// 验证配置可以成功保存到文件并重新加载，且数据保持一致。
    /// 验证：需求 23.1-23.2, 23.4（往返属性）
    /// </remarks>
    [Fact]
    public async Task Save_And_Load_RoundTrip_Success()
    {
        // Arrange
        var config = CreateTestConfig();
        var filePath = GetTestFilePath("test_config.json");
        
        // Act - Save
        var saveResult = await _persistence.Save(config, filePath);
        
        // Assert - Save succeeded
        Assert.True(saveResult.IsRight, "Save should succeed");
        Assert.True(File.Exists(filePath), "File should exist after save");
        
        // Act - Load
        var loadResult = await _persistence.Load(filePath);
        
        // Assert - Load succeeded
        if (loadResult.IsLeft)
        {
            var error = loadResult.LeftAsEnumerable().First();
            Assert.Fail($"Load failed with error: {error}");
        }
        
        Assert.True(loadResult.IsRight, "Load should succeed");
        
        var loadedConfig = loadResult.RightAsEnumerable().First();
        
        // Assert - Data integrity (basic checks)
        Assert.NotNull(loadedConfig);
        Assert.NotNull(loadedConfig.Machine);
        Assert.NotNull(loadedConfig.ControlBoard);
        Assert.IsType<ControlBoardConfig.Simulated>(loadedConfig.ControlBoard);
        
        var simulatedBoard = (ControlBoardConfig.Simulated)loadedConfig.ControlBoard;
        Assert.Equal(100, simulatedBoard.LatencyMs);
    }
    
    /// <summary>
    /// 测试：文件未找到处理
    /// </summary>
    /// <remarks>
    /// 验证加载不存在的文件时返回 FileNotFound 错误。
    /// 验证：需求 23.2, 24.1-24.6
    /// </remarks>
    [Fact]
    public async Task Load_NonExistentFile_ReturnsFileNotFoundError()
    {
        // Arrange
        var filePath = GetTestFilePath("non_existent_file.json");
        
        // Act
        var result = await _persistence.Load(filePath);
        
        // Assert
        Assert.True(result.IsLeft, "Load should fail for non-existent file");
        
        var error = result.LeftAsEnumerable().First();
        Assert.IsType<IoError.FileNotFound>(error);
        
        var fileNotFoundError = (IoError.FileNotFound)error;
        Assert.Equal(filePath, fileNotFoundError.FilePath);
    }
    
    /// <summary>
    /// 测试：保存到不存在的目录（自动创建）
    /// </summary>
    /// <remarks>
    /// 验证保存到不存在的目录时，系统会自动创建目录。
    /// 验证：需求 23.1
    /// </remarks>
    [Fact]
    public async Task Save_ToNonExistentDirectory_CreatesDirectory()
    {
        // Arrange
        var config = CreateTestConfig();
        var subDirectory = Path.Combine(_testDirectory, "subdir1", "subdir2");
        var filePath = Path.Combine(subDirectory, "config.json");
        _testFiles.Add(filePath);
        
        // Act
        var result = await _persistence.Save(config, filePath);
        
        // Assert
        Assert.True(result.IsRight, "Save should succeed and create directories");
        Assert.True(Directory.Exists(subDirectory), "Subdirectories should be created");
        Assert.True(File.Exists(filePath), "File should exist");
    }
    
    /// <summary>
    /// 测试：保存空配置
    /// </summary>
    /// <remarks>
    /// 验证可以保存和加载空的自动化逻辑集合。
    /// 验证：需求 23.1-23.2
    /// </remarks>
    [Fact]
    public async Task Save_And_Load_EmptyAutomationLogics_Success()
    {
        // Arrange
        var config = CreateTestConfig(); // 已经是空的自动化逻辑
        var filePath = GetTestFilePath("empty_config.json");
        
        // Act
        var saveResult = await _persistence.Save(config, filePath);
        var loadResult = await _persistence.Load(filePath);
        
        // Assert
        Assert.True(saveResult.IsRight, "Save should succeed");
        Assert.True(loadResult.IsRight, "Load should succeed");
        
        var loadedConfig = loadResult.RightAsEnumerable().First();
        Assert.Empty(loadedConfig.AutomationLogics);
    }
    
    /// <summary>
    /// 测试：覆盖现有文件
    /// </summary>
    /// <remarks>
    /// 验证可以覆盖现有的配置文件。
    /// 验证：需求 23.1
    /// </remarks>
    [Fact]
    public async Task Save_OverwriteExistingFile_Success()
    {
        // Arrange
        var config1 = CreateTestConfig();
        var config2 = CreateTestConfig();
        var filePath = GetTestFilePath("overwrite_test.json");
        
        // Act - Save first config
        var saveResult1 = await _persistence.Save(config1, filePath);
        Assert.True(saveResult1.IsRight, "First save should succeed");
        
        var fileInfo1 = new FileInfo(filePath);
        var size1 = fileInfo1.Length;
        
        // Wait a bit to ensure different timestamp
        await Task.Delay(100);
        
        // Act - Save second config (overwrite)
        var saveResult2 = await _persistence.Save(config2, filePath);
        
        // Assert
        Assert.True(saveResult2.IsRight, "Second save should succeed");
        Assert.True(File.Exists(filePath), "File should still exist");
        
        // Verify file was actually overwritten
        var fileInfo2 = new FileInfo(filePath);
        Assert.True(fileInfo2.LastWriteTime > fileInfo1.LastWriteTime, 
            "File should have newer timestamp");
    }
    
    /// <summary>
    /// 测试：无效路径处理
    /// </summary>
    /// <remarks>
    /// 验证使用无效路径时返回适当的错误。
    /// 验证：需求 24.1-24.6
    /// </remarks>
    [Fact]
    public async Task Save_InvalidPath_ReturnsError()
    {
        // Arrange
        var config = CreateTestConfig();
        var invalidPath = new string('a', 300); // 路径过长
        
        // Act
        var result = await _persistence.Save(config, invalidPath);
        
        // Assert
        Assert.True(result.IsLeft, "Save should fail for invalid path");
        
        var error = result.LeftAsEnumerable().First();
        // 可能是 PathTooLong 或其他 IoError
        Assert.True(
            error is IoError.PathTooLong || 
            error is IoError.IoOperationFailed,
            "Should return appropriate I/O error");
    }
    
    /// <summary>
    /// 测试：加载损坏的 JSON 文件
    /// </summary>
    /// <remarks>
    /// 验证加载损坏的配置文件时返回反序列化错误。
    /// 验证：需求 23.5, 24.1-24.6
    /// </remarks>
    [Fact]
    public async Task Load_CorruptedJson_ReturnsDeserializationError()
    {
        // Arrange
        var filePath = GetTestFilePath("corrupted.json");
        await File.WriteAllTextAsync(filePath, "{ invalid json content ][");
        
        // Act
        var result = await _persistence.Load(filePath);
        
        // Assert
        Assert.True(result.IsLeft, "Load should fail for corrupted JSON");
        
        var error = result.LeftAsEnumerable().First();
        Assert.IsType<IoError.IoOperationFailed>(error);
        
        var ioError = (IoError.IoOperationFailed)error;
        Assert.Contains("Deserialization failed", ioError.Message);
    }
    
    /// <summary>
    /// 测试：保存 null 配置抛出异常
    /// </summary>
    /// <remarks>
    /// 验证传入 null 配置时抛出 ArgumentNullException。
    /// 验证：需求 23.1
    /// </remarks>
    [Fact]
    public async Task Save_NullConfig_ThrowsArgumentNullException()
    {
        // Arrange
        var filePath = GetTestFilePath("null_test.json");
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _persistence.Save(null!, filePath));
    }
    
    /// <summary>
    /// 测试：空文件路径抛出异常
    /// </summary>
    /// <remarks>
    /// 验证传入空文件路径时抛出 ArgumentException。
    /// 验证：需求 23.1-23.2
    /// </remarks>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Save_EmptyFilePath_ThrowsArgumentException(string? filePath)
    {
        // Arrange
        var config = CreateTestConfig();
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _persistence.Save(config, filePath!));
    }
    
    /// <summary>
    /// 测试：加载空文件路径抛出异常
    /// </summary>
    /// <remarks>
    /// 验证传入空文件路径时抛出 ArgumentException。
    /// 验证：需求 23.2
    /// </remarks>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Load_EmptyFilePath_ThrowsArgumentException(string? filePath)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _persistence.Load(filePath!));
    }
}
