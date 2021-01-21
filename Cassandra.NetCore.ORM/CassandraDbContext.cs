using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Cassandra.Mapping;
using Cassandra.NetCore.ORM.Helpers;
using Mapper = Cassandra.Mapping.Mapper;

namespace Cassandra.NetCore.ORM
{
    public class CassandraDbContext : IDisposable, ICassandraDbContext
    {
        private ISession _session;
        private IMapper _mapper;
        private static Cluster _cluster;
        private int _currentBatchSize = 0;
        private object _batchLock = new object();
        private BatchStatement _currentBatch = new BatchStatement();
        private string _keySpaceName;
        public int BatchSize { get; set; } = 50;
        public bool UseBatching { get; set; } = false;


        public CassandraDbContext(string userName, string password, string cassandraContactPoint, int cassandraPort, string keySpaceName)
        {
            // Connect to cassandra cluster  (Cassandra API on Azure Cosmos DB supports only TLSv1.2)
            var options = new Cassandra.SSLOptions(SslProtocols.Tls12, true, ValidateServerCertificate);

            options.SetHostNameResolver((ipAddress) => cassandraContactPoint);
            _cluster = Cluster
                .Builder()
                .WithCredentials(userName, password)
                .WithPort(cassandraPort)
                .AddContactPoint(cassandraContactPoint)
                .WithSSL(options)
                .Build();
            CreateKeySpace(keySpaceName).Wait();
            _session = _cluster.ConnectAsync(keySpaceName).Result;
             _mapper = new Mapper(_session);

             _keySpaceName = keySpaceName;
        }
        public static bool ValidateServerCertificate
        (
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors
        )
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);
            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        }

        public void Dispose()
        {
            lock (_currentBatch)
            {
                if (_currentBatchSize > 0)
                    _session.Execute(_currentBatch);
            }

            _session.Dispose();
        }

        private async Task CreateKeySpace(string keySpace)
        {
            _session = await _cluster.ConnectAsync();
            await _session.ExecuteAsync(new SimpleStatement($"CREATE KEYSPACE IF NOT EXISTS {keySpace} WITH REPLICATION = {{ 'class' : 'NetworkTopologyStrategy', 'datacenter1' : 1 }};"));
        }
        public void Insert<T>(T data)
        {
            try
            {

                 _mapper.Insert<T>(data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task InsertAsync<T>(T data)
        {
            try
            {
            
                await _mapper.InsertAsync<T>(data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        public void InsertIfNotExists<T>(T data)
        {
            try
            {
                _mapper.InsertIfNotExists<T>(data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        public async Task InsertIfNotExistsAsync<T>(T data)
        {
            try
            {
                await _mapper.InsertIfNotExistsAsync<T>(data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        //public async Task UpdateAsync<T>(Expression<Func<T,T>> valueToUpdate, Expression<Func<T, bool>> whereClause)
        //{
        //    try
        //    {

        //        await _mapper.UpdateAsync<T>("0");
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //        throw;
        //    }
        //}


        public IEnumerable<T> Select<T>(Expression<Func<T, bool>> predicate = null)
        {

            try
            {
                var tableName = typeof(T).ExtractTableName<T>();
                var selectQuery = $"select * from {tableName}";
                var queryStatement = QueryBuilder.EvaluateQuery(predicate);

                if (predicate != null)
                {
                    selectQuery = $"select * from {tableName} where {queryStatement.Statement}";
                }

                var output = _mapper.Fetch<T>(selectQuery, queryStatement?.Values);

                return output;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<IEnumerable<T>> SelectAsync<T>(Expression<Func<T, bool>> predicate = null)
        {
            try
            {
                var tableName = typeof(T).ExtractTableName<T>();
                var selectQuery = $"select * from {tableName}";

                var queryStatement = QueryBuilder.EvaluateQuery(predicate);

                if (predicate != null)
                {
                    selectQuery = $"select * from {tableName} where {queryStatement.Statement}";
                }
                var output = await _mapper.FetchAsync<T>(selectQuery, queryStatement?.Values);

                return output;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate = null)
        {
            var tableName = typeof(T).ExtractTableName<T>();
            var selectQuery = $"select * from {tableName}";
            var queryStatement = QueryBuilder.EvaluateQuery(predicate);

            if (predicate != null)
            {
                selectQuery = $"select * from {tableName} where {queryStatement.Statement}";
            }
            return _mapper.FirstOrDefault<T>(selectQuery, queryStatement?.Values);
        }

        public async Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate = null)
        {
            var tableName = typeof(T).ExtractTableName<T>();
            var selectQuery = $"select * from {tableName}";
            var queryStatement = QueryBuilder.EvaluateQuery(predicate);

            if (predicate != null)
            {
                selectQuery = $"select * from {tableName} where {queryStatement.Statement}";
            }
            return await _mapper.FirstOrDefaultAsync<T>(selectQuery, queryStatement?.Values);
        }

        public double Average<T, TNumericModel>(Expression<Func<T, TNumericModel>> propertyExpression, Expression<Func<T, bool>> predicate = null)
        {
            try
            {
                var columnName = QueryBuilder.EvaluatePropertyName(propertyExpression);

                var tableName = typeof(T).ExtractTableName<T>();

                var selectQuery = $"select avg({columnName}) from {tableName}";
                var statement = new SimpleStatement(selectQuery);
                if (predicate != null)
                {
                    var queryStatement = QueryBuilder.EvaluateQuery(predicate);
                    selectQuery = $"select avg({columnName}) from {tableName} where {queryStatement.Statement}";
                    statement = new SimpleStatement(selectQuery, queryStatement.Values);
                }
                var rows = _session.Execute(statement);
                var avg = Convert.ToDouble(rows.First()[0]);

                return avg;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        public async Task<double> AverageAsync<T, TNumericModel>(Expression<Func<T, bool>> predicate, Expression<Func<T, TNumericModel>> propertyExpression)
        {
            try
            {
                var columnName = QueryBuilder.EvaluatePropertyName(propertyExpression);

                var queryStatement = QueryBuilder.EvaluateQuery(predicate);
                var tableName = typeof(T).ExtractTableName<T>();
                var selectQuery = $"select avg({columnName}) from {tableName} where {queryStatement.Statement}";

                var statement = new SimpleStatement(selectQuery, queryStatement.Values);
                var rows = await _session.ExecuteAsync(statement);

                var avg = Convert.ToDouble(rows.First()[0]);

                return avg;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public double Sum<T, TNumericModel>(Expression<Func<T, bool>> predicate, Expression<Func<T, TNumericModel>> propertyExpression)
        {
            try
            {
                var columnName = QueryBuilder.EvaluatePropertyName(propertyExpression);

                var queryStatement = QueryBuilder.EvaluateQuery(predicate);
                var tableName = typeof(T).ExtractTableName<T>();
                var selectQuery = $"select sum({columnName}) from {tableName} where {queryStatement.Statement}";

                var statement = new SimpleStatement(selectQuery, queryStatement.Values);
                var rows = _session.Execute(statement);

                var sum = Convert.ToDouble(rows.First()[0]);

                return sum;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<double> SumAsync<T, TNumericModel>(Expression<Func<T, bool>> predicate, Expression<Func<T, TNumericModel>> propertyExpression)
        {
            try
            {
                var columnName = QueryBuilder.EvaluatePropertyName(propertyExpression);

                var queryStatement = QueryBuilder.EvaluateQuery(predicate);
                var tableName = typeof(T).ExtractTableName<T>();
                var selectQuery = $"select sum({columnName}) from {tableName} where {queryStatement.Statement}";

                var statement = new SimpleStatement(selectQuery, queryStatement.Values);
                var rows = await _session.ExecuteAsync(statement);

                var sum = Convert.ToDouble(rows.First()[0]);

                return sum;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public double Min<T, TNumericModel>(Expression<Func<T, bool>> predicate, Expression<Func<T, TNumericModel>> propertyExpression)
        {
            try
            {
                var columnName = QueryBuilder.EvaluatePropertyName(propertyExpression);

                var queryStatement = QueryBuilder.EvaluateQuery(predicate);
                var tableName = typeof(T).ExtractTableName<T>();
                var selectQuery = $"select min({columnName}) from {tableName} where {queryStatement.Statement}";

                var statement = new SimpleStatement(selectQuery, queryStatement.Values);
                var rows = _session.Execute(statement);
                var sum = Convert.ToDouble(rows.First()[0]);
                return sum;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        public async Task<double> MinAsync<T, TNumericModel>(Expression<Func<T, bool>> predicate, Expression<Func<T, TNumericModel>> propertyExpression)
        {
            try
            {
                var columnName = QueryBuilder.EvaluatePropertyName(propertyExpression);
                var queryStatement = QueryBuilder.EvaluateQuery(predicate);
                var tableName = typeof(T).ExtractTableName<T>();
                var selectQuery = $"select min({columnName}) from {tableName} where {queryStatement.Statement}";

                var statement = new SimpleStatement(selectQuery, queryStatement.Values);
                var rows = await _session.ExecuteAsync(statement);

                var sum = Convert.ToDouble(rows.First()[0]);
                return sum;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }
        public double Max<T, TNumericModel>(Expression<Func<T, bool>> predicate, Expression<Func<T, TNumericModel>> propertyExpression)
        {
            try
            {
                var columnName = QueryBuilder.EvaluatePropertyName(propertyExpression);

                var queryStatement = QueryBuilder.EvaluateQuery(predicate);
                var tableName = typeof(T).ExtractTableName<T>();
                var selectQuery = $"select max({columnName}) from {tableName} where {queryStatement.Statement}";

                var statement = new SimpleStatement(selectQuery, queryStatement.Values);
                var rows = _session.Execute(statement);

                var sum = Convert.ToDouble(rows.First()[0]);

                return sum;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        public async Task<double> MaxAsync<T, TNumericModel>(Expression<Func<T, bool>> predicate, Expression<Func<T, TNumericModel>> propertyExpression)
        {
            try
            {
                var columnName = QueryBuilder.EvaluatePropertyName(propertyExpression);

                var queryStatement = QueryBuilder.EvaluateQuery(predicate);
                var tableName = typeof(T).ExtractTableName<T>();
                var selectQuery = $"select max({columnName}) from {tableName} where {queryStatement.Statement}";

                var statement = new SimpleStatement(selectQuery, queryStatement.Values);
                var rows = await _session.ExecuteAsync(statement);

                var sum = Convert.ToDouble(rows.First()[0]);

                return sum;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        public T SingleOrDefault<T>(Expression<Func<T, bool>> predicate)
        {
            return Select(predicate).SingleOrDefault();
        }

        //public T SingleOrDefaultAsync<T>(Expression<Func<T, bool>> predicate)
        //{
        //    return SelectAsync(predicate).SingleOrDefault();
        //}



        public void AddOrUpdate<T>(T entity)
        {
            var insertStatment = CreateAddStatement(entity);

            if (UseBatching)
            {
                lock (_batchLock)
                {
                    _currentBatch.Add(insertStatment);
                    ++_currentBatchSize;
                    if (_currentBatchSize == BatchSize)
                    {
                        _session.Execute(_currentBatch);
                        _currentBatchSize = 0;
                        _currentBatch = new BatchStatement();
                    }
                }
            }
            else
            {
                _session.Execute(insertStatment);
            }
        }

        public async Task AddOrUpdateAsync<T>(T entity)
        {
            var insertStatment = CreateAddStatement(entity);

            if (UseBatching)
            {
                lock (_batchLock)
                {
                    _currentBatch.Add(insertStatment);
                    ++_currentBatchSize;
                    if (_currentBatchSize == BatchSize)
                    {
                        _session.Execute(_currentBatch); // due to page lock we can't able to use async method here.
                        _currentBatchSize = 0;
                        _currentBatch = new BatchStatement();
                    }
                }
            }
            else
            {
                await _session.ExecuteAsync(insertStatment);
            }
        }

        private Statement CreateAddStatement<T>(T entity)
        {
            var tableName = typeof(T).ExtractTableName<T>();

            // We are interested only in the properties we are not ignoring
            var properties = entity.GetType().GetCassandraRelevantProperties();
            var propertiesNames = properties.Select(p => p.GetColumnNameMapping()).ToArray();
            var parametersSignals = properties.Select(p => "?").ToArray();
            var propertiesValues = properties.Select(p => p.GetValue(entity)).ToArray();
            var insertCql = $"insert into {tableName}({string.Join(",", propertiesNames)}) values ({string.Join(",", parametersSignals)})";
            var insertStatment = new SimpleStatement(insertCql, propertiesValues);

            return insertStatment;
        }

        private SimpleStatement CreateClusterStatement<T>(T entity)
        {
            try
            {
                var tableName = typeof(T).ExtractTableName<T>();

                // We are interested only in the properties we are not ignoring
                var properties = entity.GetType().GetCassandraRelevantProperties();
                var propertiesNames = properties.Select(p => p.GetColumnNameAndPrimaryKeyMapping()).ToArray();

                var createCql = $"CREATE TABLE IF NOT EXISTS {_keySpaceName}.{tableName} ({string.Join(",", propertiesNames)})";


                var insertStatment = new SimpleStatement(createCql);

                return insertStatment;

               

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        public async Task CreateClusterAsync<T>() where T : class, new() 
        {
            try
            {
                var query = CreateClusterStatement(new T());
                await _session.ExecuteAsync(query);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
          
            
        }
    }
}
