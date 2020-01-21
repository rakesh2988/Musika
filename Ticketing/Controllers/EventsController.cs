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
    public class EventsController : Controller
    {
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("RegisterTicketingUser")]
        public bool CreateEvent([System.Web.Http.FromBody]TicketingEventsNewModel events)
        {
            Musika.Controllers.API.TicketingAPIController myController = new Musika.Controllers.API.TicketingAPIController();
            myController.UpdateTicketingEvent(events);
            return true;
        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("GetTicketingEvents")]
        public string GetTicketingEvents()
        {
            Musika.Controllers.API.TicketingAPIController myController = new Musika.Controllers.API.TicketingAPIController();
            string result = string.Empty;
            result = myController.GetTicketingEvents().ToString();
            return result;
        }
    }    
}