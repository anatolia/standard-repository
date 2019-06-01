using System.Data;
using System.Data.Common;

namespace StandardRepository.Models
{
    public class DbParameterInfo
    {
        public string Name { get; }
        public object Value { get; }
        public DbType DbType { get; }
        public int? Size { get; set; }
        public byte? Precision { get; set; }
        public byte? Scale { get; set; }

        public DbParameterInfo(string name, object value, DbType dbType,
            int? size = null, byte? precision = null, byte? scale = null)
        {
            Name = name;
            Value = value;
            DbType = dbType;
            Size = size;
            Precision = precision;
            Scale = scale;
        }

        public T CreateParameter<T>() where T : DbParameter, new()
        {
            var parameter = new T();
            ApplyToParameter(parameter);
            return parameter;
        }

        public void ApplyToParameter<T>(T parameter) where T : DbParameter
        {
            parameter.ParameterName = Name;
            parameter.Value = Value;
            parameter.DbType = DbType;

            if (Size.HasValue)
            {
                parameter.Size = Size.Value;
            }

            if (Precision.HasValue)
            {
                parameter.Precision = Precision.Value;
            }

            if (Scale.HasValue)
            {
                parameter.Scale = Scale.Value;
            }
        }
    }
}