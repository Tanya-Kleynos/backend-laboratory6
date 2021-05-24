﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Backend6.Models
{
    public class Forum
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ForumCategoryId { get; set; }

        public ForumCategory ForumCategory { get; set; }

        [Required]
        [MaxLength(200)]
        public String Name { get; set; }

        public String Description { get; set; }
         
        public ICollection<ForumTopic> ForumTopics { get; set; }
    }
}
