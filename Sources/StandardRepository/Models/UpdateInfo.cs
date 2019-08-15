using System;
using System.Linq.Expressions;

using StandardRepository.Models.Entities;

namespace StandardRepository.Models
{
    public class UpdateInfo<T> where T : BaseEntity
    {
        public Expression<Func<T, object>> UpdateColumn { get; }
        public object Value { get; }


        public UpdateInfo(Expression<Func<T, object>> updateColumn, object value)
        {
            UpdateColumn = updateColumn;
            Value = value;
        }
    }
}