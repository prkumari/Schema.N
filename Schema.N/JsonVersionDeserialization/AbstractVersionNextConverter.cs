namespace JsonVersionDeserialization
{
    using System;

    public class AbstractVersionNextConverter<TRawData, TPocoThis, TPocoNext>  : IVersionNextConverterTypeless
    {
        public Func<DataVersionInfo<TRawData, TPocoThis>, DataVersionInfo<TRawData, TPocoNext>> ConversionFunc { get; set; }

        public AbstractVersionNextConverter(
            Func<DataVersionInfo<TRawData, TPocoThis>, DataVersionInfo<TRawData, TPocoNext>> conversionFunc)
        {
            ConversionFunc = conversionFunc;
        }

        public IDataVersionInfo ConvertToNext(IDataVersionInfo prevVersiondata)
        {
            if (prevVersiondata == null)
            {
                throw new ArgumentNullException(nameof(prevVersiondata));
            }

            if (prevVersiondata.GetRawDataType() != GetRawDataType() ||
                prevVersiondata.GetPocoType() != GetThisVersionType())
            {
                throw new ArgumentException("Cannot convert if the data version info types do not match the converter types.");
            }
            var convertedPrev = new DataVersionInfo<TRawData, TPocoThis>((TRawData) prevVersiondata.RawDataObject,
                (TPocoThis) prevVersiondata.PocoObject);
            return ConversionFunc(convertedPrev);
        }

        public Type GetRawDataType()
        {
            return typeof(TRawData);
        }

        public Type GetThisVersionType()
        {
            return typeof(TPocoThis);
        }

        public Type GetNextVersionType()
        {
            return typeof(TPocoNext);
        }
    }
}
