using System;

namespace Schema.N
{
    public interface IDataVersionInfo
    {
        object RawDataObject { get; }

        object PocoObject { get; }

        Type GetRawDataType();

        Type GetPocoType();
    }
}
