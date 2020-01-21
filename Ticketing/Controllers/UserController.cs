using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Web.Http;

using Musika.Models;
using Musika.Areas.Admin.AdminModels.Input;
using Musika.Repository;
using Musika.Library.Utilities;
using Musika.Repository.Interface;
using Musika.Repository.UnitofWork;
using Musika.Repository.GRepository;


namespace Ticketing.Controllers
{
    [System.Web.Http.Route("api/[controller]")]
    public class UserController : Controller
    {
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("RegisterTicketingUser")]
        public bool RegisterTicketingUser([System.Web.Http.FromBody]TicketingUsers users)
        {
            Musika.Controllers.API.TicketingAPIController myController = new Musika.Controllers.API.TicketingAPIController();
            myController.RegisterTicketingUser(users);
            return true;
        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("AuthenticateUser")]
        public bool AuthenticateUser(TicketingUsers user)
        {
            Musika.Controllers.API.TicketingAPIController myController = new Musika.Controllers.API.TicketingAPIController();
            myController.AuthenticateUser(user);
            return true;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("RetreivePassword")]
        public string RetreivePassword(TicketingUsers user)
        {
            string pwd = string.Empty;
            
            Musika.Controllers.API.TicketingAPIController myController = new Musika.Controllers.API.TicketingAPIController();
            myController.RetreivePassword(user);
            return pwd;
        }
    }
}
