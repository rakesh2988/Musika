using Musika.Models;
using Musika.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Musika.Repository.GRepository
{
//---------------- Sample code to use this repository
    //_unitOfWork.StartTransaction();

    //               //user
    //               GenericRepository<Users> _users = new GenericRepository<Users>(_unitOfWork);
    //               entity = _users.Repository.Get(p => p.Email == input.Email && p.Password == _password);
    //                entity.ModifiedDate = DateTime.Now;
    //               _users.Repository.Update(entity);

    //               //
    //               Models.Question _entityQuestion = null;
    //               GenericRepository<Question> _Question = new GenericRepository<Question>(_unitOfWork);
    //               _entityQuestion = _Question.Repository.Get(p => p.QuestionID == 7);
    //               _entityQuestion.ModifiedDate = DateTime.Now;
    //               _Question.Repository.Update(_entityQuestion);


    //           _unitOfWork.Commit();

  

    public class GenericRepository<T> where T : class
    {
       private  BaseRepository<T> _Repository;
       private readonly IUnitOfWork _unitOfWork;

       public GenericRepository(IUnitOfWork IUnitOfWork)
       {
           _unitOfWork = IUnitOfWork;
       }

      


       public BaseRepository<T> Repository
        {
            get
            {
                
                if (_Repository == null)
                {
                    _Repository = new BaseRepository<T>(_unitOfWork);
                }
               return _Repository;
            }
        }
    }
}
