using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using iSportsRecruiting.Shared.DTO;
using iSportsRecruiting.Shared.Utils;
using iSportsRecruiting.Shared.Utils.Enums;

namespace iSportsRecruiting.Shared.Models
{
    public class SocialMediaModel
    {
        public int Id { get; set; }
        public int User_id { get; set; }
        [UrlAttribute]
        public string Instagram { get; set; }
        [UrlAttribute]
        [SocialMediaUrlAttribute(SocialMedia.Facebook)]
        public string Facebook { get; set; }
        [UrlAttribute]
        public string Twitter { get; set; }
        
    }
    public static class SocialMediaDTOExtension
    {
        public static SocialMediaModel ToModel(this SocialMediaDTO socialMedia)
        {
            return new SocialMediaModel
            {
                Id = socialMedia.Id,
                User_id = socialMedia.User_id,
                Instagram = socialMedia.Instagram,
                Facebook = socialMedia.Facebook,
                Twitter = socialMedia.Twitter
            };
        }
    }
}
