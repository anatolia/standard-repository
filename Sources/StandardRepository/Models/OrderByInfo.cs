using System;
using System.Linq.Expressions;

using StandardRepository.Models.Entities;

namespace StandardRepository.Models
{
    public class OrderByInfo<T> where T : BaseEntity
    {
        public Expression<Func<T, object>> OrderByColumn { get; }
        public bool IsAscending { get; }

        public OrderByInfo(Expression<Func<T, object>> orderByColumn, bool isAscending = true)
        {
            OrderByColumn = orderByColumn;
            IsAscending = isAscending;
        }
    }
}