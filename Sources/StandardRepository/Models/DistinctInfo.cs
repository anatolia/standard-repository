using System;
using System.Linq.Expressions;

using StandardRepository.Models.Entities;

namespace StandardRepository.Models
{
    public class DistinctInfo<T> where T : BaseEntity
    {
        public Expression<Func<T, object>> DistinctColumn { get; }
        public bool IsAscending { get; }

        public DistinctInfo(Expression<Func<T, object>> distinctColumn, bool isAscending = true)
        {
            DistinctColumn = distinctColumn;
            IsAscending = isAscending;
        }
    }
}