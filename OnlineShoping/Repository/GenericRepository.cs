using OnlineShoping.DAL;
using OnlineShoping.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace OnlineShoping.Repository
{
    public class GenericRepository<Tbl_Entity> : IRepository<Tbl_Entity> where Tbl_Entity : class
    {
        DbSet<Tbl_Entity> _dbset;
        private dbMyOnlineShoppingEntities _DBEntity;

        public GenericRepository(dbMyOnlineShoppingEntities DBEntity)
        {
            _DBEntity = DBEntity; 
            _dbset = DBEntity.Set<Tbl_Entity> ();
        }

       
        public void Add(Tbl_Entity entity)
        {
            _dbset.Add(entity);
            _DBEntity.SaveChanges();
        }

       

        public int GetAllRecordCount()
        {
            return _dbset.Count();
        }

        public IEnumerable<Tbl_Entity> GetAllRecords()
        {
            return _dbset.ToList();
        }

        public IQueryable<Tbl_Entity> GetAllRecordsIQueryable()
        {
            return _dbset;
        }

        public Tbl_Entity GetFirstorDefault(int recordId)
        {
            return _dbset.Find(recordId);
        }

        public Tbl_Entity GetFirstOrDefaultByParameter(Expression<Func<Tbl_Entity, bool>> wherePredict)
        {
            return _dbset.Where(wherePredict).FirstOrDefault();
        }

        public IEnumerable<Tbl_Entity> GetListParameter(Expression<Func<Tbl_Entity, bool>> wherePredict)
        {
            return _dbset.Where(wherePredict).ToList();
        }

        public IEnumerable<Tbl_Entity> GetProduct()
        {
            return _dbset.ToList();
        }



        public IEnumerable<Tbl_Entity> GetRecordsToShow(int pageNo, int CurrentPage, Expression<Func<Tbl_Entity, bool>> wherePredict, Expression<Func<Tbl_Entity, int>> orderByPredict)
        {
            if(wherePredict != null)
            {
                return _dbset.OrderBy(orderByPredict).Where(wherePredict).ToList();
            }
            else
            {
                return _dbset.OrderBy(orderByPredict).ToList();
            }
        }

        public IEnumerable<Tbl_Entity> GetResultBySqlprocedure(string query, params object[] parameters)
        {
         if(parameters != null)
            {
                return _DBEntity.Database.SqlQuery<Tbl_Entity>(query, parameters).ToList();
            }
         else
            {
                return _DBEntity.Database.SqlQuery<Tbl_Entity>(query).ToList();
            }
        }

        public void InactiveAndDeleteMarkBywheeClause(Expression<Func<Tbl_Entity, bool>> wherePredict, Action<Tbl_Entity> ForEachPredict)
        {
            _dbset.Where(wherePredict).ToList().ForEach(ForEachPredict);
        }

        public void Remove(Tbl_Entity entity)
        {
            if (_DBEntity.Entry(entity).State == EntityState.Detached)
                _dbset.Attach(entity);
            _dbset.Remove(entity);
            _DBEntity.SaveChanges();
        }

        public void RemoveByWhereClause(Expression<Func<Tbl_Entity, bool>> wherePredict)
        {
            Tbl_Entity entity = _dbset.Where(wherePredict).FirstOrDefault();
            Remove(entity);
            _DBEntity.SaveChanges();
        }

        public void RemoveRangeByWhereClause(Expression<Func<Tbl_Entity, bool>> wherePredict)
        {
            List<Tbl_Entity> entity = _dbset.Where(wherePredict).ToList();
            foreach(var ent in entity)
            {
                Remove(ent);
            }
        }

        public void Update(Tbl_Entity entity)
        {
            _dbset.Attach(entity);
            _DBEntity.Entry(entity).State = EntityState.Modified;
            _DBEntity.SaveChanges();
        }

        public void UpdateByWhereClause(Expression<Func<Tbl_Entity, bool>> wherePredict, Action<Tbl_Entity> ForEachPredict)
        {
            _dbset.Where(wherePredict).ToList().ForEach(ForEachPredict);
        }

      
    }
}