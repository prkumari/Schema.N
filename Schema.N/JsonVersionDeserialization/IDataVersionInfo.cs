using System;
using System.Security.Cryptography.X509Certificates;

namespace JsonVersionDeserialization
{
    public interface IDataVersionInfo
    {
        object RawDataObject { get; }

        object PocoObject { get; }

        Type GetRawDataType();

        Type GetPocoType();
    }
}
