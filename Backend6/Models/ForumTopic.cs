﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Backend6.Models
{
    public class ForumTopic
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ForumId { get; set; }

        public Forum Forum { get; set; }

        [Required]
        public String CreatorId { get; set; }

        public ApplicationUser Creator { get; set; }

        [Required]
        [MaxLength(200)]
        public String Name { get; set; }

        public DateTime Created { get; set; }

        public ICollection<ForumMessage> ForumMessages { get; set; }

    }
}
