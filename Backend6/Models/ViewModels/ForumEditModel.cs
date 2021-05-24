using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Backend6.Models.ViewModels
{
    public class ForumEditModel
    {
        public Guid ForumCategoryId { get; set; } 

        [Required]
        [MaxLength(200)]
        public String Name { get; set; }

        public String Description { get; set; }
    }
}
