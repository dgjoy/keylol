namespace Keylol.FontGarage.Table
{
    public interface IDeepCopyable
    {
        object DeepCopy();
    }

    public interface IOpenTypeFontTable : IOpenTypeFontSerializable, IDeepCopyable
    {
        string Tag { get; }
    }
}