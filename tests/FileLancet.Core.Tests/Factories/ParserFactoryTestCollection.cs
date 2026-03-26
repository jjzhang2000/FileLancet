namespace FileLancet.Core.Tests.Factories;

/// <summary>
/// 解析器工厂测试集合 - 禁用并行执行以避免状态冲突
/// </summary>
[CollectionDefinition("ParserFactory Tests", DisableParallelization = true)]
public class ParserFactoryTestCollection : ICollectionFixture<object>
{
    // 此类仅用于定义集合属性
}
