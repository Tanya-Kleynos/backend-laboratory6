﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Backend6.Models.ViewModels
{
    public class ForumMessageAttachmentEditModel
    {
        public IFormFile File { get; set; }
         
        [Required]
        public String FileName { get; set; }
    }
}
