using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonVersionDeserialization
{
    public class DataVersionInfoUgly<TRawData> : IDataVersionInfo
    {
        public object RawDataObject => RawData;

        public TRawData RawData { get; set; }

        public object PocoObject { get; }
        public Type PocoType { get; set; }

        public DataVersionInfoUgly(TRawData rawData, object poco, Type pocoType)
        {
            RawData = rawData;
            PocoObject = poco;
            PocoType = pocoType;
        }

        public Type GetRawDataType()
        {
            return typeof(TRawData);
        }

        public Type GetPocoType()
        {
            return PocoType;
        }
    }
}
