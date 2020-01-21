using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Musika.Models;
using Musika.Repository.Interface;
using System.Data.Entity.Validation;
using System.Diagnostics;


namespace Musika.Repository.UnitofWork
{
    public class UnitOfWork :IUnitOfWork
    {
        private MusikaEntities _db;
        private bool _IsTransaction = false;


        public bool IsTransaction
        {
            get { return _IsTransaction; }
        }

        public UnitOfWork()
        {
            _db = new MusikaEntities();
        }

        public void Dispose()
        {
          
        }

        public void StartTransaction()
        {
            _db.Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);
            _IsTransaction = true;
        }

        public void Commit()
        {
            try
            {
                _db.SaveChanges();
                _db.Database.CurrentTransaction.Commit();
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Debug.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Debug.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
        }

        public void RollBack()
        {
            _db.SaveChanges();
            _db.Database.CurrentTransaction.Rollback();
        }

        public MusikaEntities Db
        {
            get { return _db; }
        }

    }
}
