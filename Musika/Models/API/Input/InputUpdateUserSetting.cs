
using Musika.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Musika.Models.API.Input
{
    public class InputUpdateUserSetting
    {
        [Required]
        public long UserID { get; set; }

        [Required]
        public EUserSettings SettingKey { get; set; }

        public bool? SettingValue { get; set; }

        [Range(0, 1)]
        public int NotificationCount { get; set; }
    }
}