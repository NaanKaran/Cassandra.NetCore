using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Cassandra.NetCore.ORM
{
    public interface ICassandraDbContext
    {
        bool UseBatching { get; set; }
        void Insert<T>(T data);
        Task InsertAsync<T>(T data);
        void InsertIfNotExists<T>(T data);
        Task InsertIfNotExistsAsync<T>(T data);
        IEnumerable<T> Select<T>(Expression<Func<T, bool>> predicate = null);
        Task<IEnumerable<T>> SelectAsync<T>(Expression<Func<T, bool>> predicate = null);
        T FirstOrDefault<T>(Expression<Func<T, bool>> predicate = null);
        Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate = null);
        double Average<T, TNumericModel>(Expression<Func<T, TNumericModel>> propertyExpression, Expression<Func<T, bool>> predicate = null);
        Task<double> AverageAsync<T, TNumericModel>(Expression<Func<T, bool>> predicate, Expression<Func<T, TNumericModel>> propertyExpression);
        double Sum<T, TNumericModel>(Expression<Func<T, bool>> predicate, Expression<Func<T, TNumericModel>> propertyExpression);
        Task<double> SumAsync<T, TNumericModel>(Expression<Func<T, bool>> predicate, Expression<Func<T, TNumericModel>> propertyExpression);
        double Min<T, TNumericModel>(Expression<Func<T, bool>> predicate, Expression<Func<T, TNumericModel>> propertyExpression);
        Task<double> MinAsync<T, TNumericModel>(Expression<Func<T, bool>> predicate, Expression<Func<T, TNumericModel>> propertyExpression);
        double Max<T, TNumericModel>(Expression<Func<T, bool>> predicate, Expression<Func<T, TNumericModel>> propertyExpression);
        Task<double> MaxAsync<T, TNumericModel>(Expression<Func<T, bool>> predicate, Expression<Func<T, TNumericModel>> propertyExpression);
        T SingleOrDefault<T>(Expression<Func<T, bool>> predicate);
        void AddOrUpdate<T>(T entity);
        Task AddOrUpdateAsync<T>(T entity);

        Task CreateClusterAsync<T>() where T : class, new();
    }
}