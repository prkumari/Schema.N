using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schema.N
{
    public class CustomVersionNextConverter<TRawData, TPocoThis, TPocoNext> :
        AbstractVersionNextConverter<TRawData, TPocoThis, TPocoNext>
    {
        public Func<DataVersionInfo<TRawData, TPocoThis>, DataVersionInfo<TRawData, TPocoNext>> ConversionFunc { get; set; }

        public CustomVersionNextConverter(
            Func<DataVersionInfo<TRawData, TPocoThis>, DataVersionInfo<TRawData, TPocoNext>> conversionFunc)
        {
            ConversionFunc = conversionFunc;
        }

        protected override DataVersionInfo<TRawData, TPocoNext> ConvertToNext(DataVersionInfo<TRawData, TPocoThis> convertedPrev)
        {
            return ConversionFunc(convertedPrev);
        }
    }
}
