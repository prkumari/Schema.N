using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonVersionDeserialization
{
    public interface IVersionNextConverterTypeless
    {
        IDataVersionInfo ConvertToNext(IDataVersionInfo prevVersiondata);

        Type GetRawDataType();


        Type GetThisVersionType();
        

        Type GetNextVersionType();
    }
}
