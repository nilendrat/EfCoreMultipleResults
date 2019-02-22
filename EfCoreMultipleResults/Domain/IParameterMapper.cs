using EfCoreMultipleResults.Utils;


namespace EfCoreMultipleResults.Domain
{
    public interface IParameterMapper
    {
        StoredProcedure Map<TSource>(TSource source);
    }
}
