using LanguageExt;
using MachineOrchestration.Core.Types;

namespace MachineOrchestration.Core.Domain;

/// <summary>
/// 零件库接口（纯函数）
/// 管理基础零件定义和分类
/// </summary>
public interface IPartLibrary
{
    /// <summary>获取所有零件</summary>
    Seq<Part> GetAllParts();
    
    /// <summary>按分类获取零件</summary>
    /// <param name="category">零件分类</param>
    /// <returns>属于该分类的所有零件</returns>
    Seq<Part> GetPartsByCategory(PartCategory category);
    
    /// <summary>根据 ID 获取零件</summary>
    /// <param name="id">零件 ID</param>
    /// <returns>零件（如果存在）</returns>
    Option<Part> GetPartById(PartId id);
}
