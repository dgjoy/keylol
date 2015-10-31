using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.Models.ViewModels
{
    public class NormalPointVM {
        [Required(AllowEmptyStrings = true)]
        public string BackgroundImage { get; set; }
        
        public NormalPointType Type { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string AvatarImage { get; set; }

        [Required]
        public string IdCode { get; set; }

        [Required]
        public string ChineseName { get; set; }

        [Required]
        public string EnglishName { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string EnglishAliases { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string ChineseAliases { get; set; }
        
        [Required]
        public List<string> AssociatedPointsId { get; set; }

        public string StoreLink { get; set; }
    }
}
