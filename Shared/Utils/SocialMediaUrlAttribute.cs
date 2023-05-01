using iSportsRecruiting.Shared.Utils.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSportsRecruiting.Shared.Utils
{
    public class SocialMediaUrlAttribute : ValidationAttribute
    {
        private SocialMedia _socialNetwork;
        public SocialMediaUrlAttribute(SocialMedia socialNetwork)
        {
            _socialNetwork = socialNetwork;
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null)
                return ValidationResult.Success;

            switch (_socialNetwork)
            {
                case SocialMedia.Facebook:
                    if (value.ToString().Contains("https://www.facebook.com/"))
                    {
                        return ValidationResult.Success;
                    }
                    else
                    {
                        return new ValidationResult("Please Correct (https://www.facebook.com/userexample)");
                    }                    
                case SocialMedia.Twitter:
                    if (value.ToString().Contains("https://www.twitter.com/"))
                    {
                        return ValidationResult.Success;
                    }
                    else
                    {
                        return new ValidationResult("Please Correct (https://www.twitter.com/userexample)");
                    }
                case SocialMedia.Instagram:
                    if (value.ToString().Contains("https://www.instagram.com/"))
                    {
                        return ValidationResult.Success;
                    }
                    else
                    {
                        return new ValidationResult("Please Correct (https://www.instagram.com/userexample)");
                    }
                default:
                    return ValidationResult.Success; 
            }
            
        }
    }
}
