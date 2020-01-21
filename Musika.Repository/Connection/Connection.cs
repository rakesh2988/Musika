using Musika.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Musika.Repository.Connection
{
    public static class Connection
    {
        public static MusikaEntities GetContext()
        {
            return new MusikaEntities();
        }
    }
}
