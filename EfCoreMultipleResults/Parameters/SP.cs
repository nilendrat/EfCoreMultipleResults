namespace EfCoreMultipleResults.Parameters
{
    public abstract class SP
    {
        public virtual string GetName()
        {
            return GetType().Name;
        }
    }
}
