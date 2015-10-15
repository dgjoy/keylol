using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.Models.DTO
{
    public class ArticleTypeDTO
    {
        public ArticleTypeDTO(ArticleType articleType)
        {
            Id = articleType.Id;
            Name = articleType.Name;
            Description = articleType.Description;
            AllowVote = articleType.AllowVote;
        }

        public string Id { get; set; }
        
        public string Name { get; set; }
        
        public string Description { get; set; }

        public bool AllowVote { get; set; }

    }
}
