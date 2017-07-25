using System;

namespace Schema.N
{
    public class DataVersionInfo<TRaw, TPoco> : IDataVersionInfo
    {
        public object RawDataObject => RawData;

        public object PocoObject => Poco;
        public Type GetRawDataType()
        {
            return typeof(TRaw);
        }

        public Type GetPocoType()
        {
            return typeof(TPoco);
        }

        public TRaw RawData { get; set; }

        public TPoco Poco { get; set; }

        public DataVersionInfo()
        {
        }

        public DataVersionInfo(TRaw raw, TPoco poco) : this()
        {
            RawData = raw;
            Poco = poco;
        }
    }
}
