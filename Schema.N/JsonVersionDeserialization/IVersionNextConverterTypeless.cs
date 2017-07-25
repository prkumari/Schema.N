using System;

namespace Schema.N
{
    public interface IVersionNextConverterTypeless
    {
        IDataVersionInfo ConvertToNext(IDataVersionInfo prevVersiondata);

        Type GetRawDataType();


        Type GetThisVersionType();
        

        Type GetNextVersionType();
    }
}
