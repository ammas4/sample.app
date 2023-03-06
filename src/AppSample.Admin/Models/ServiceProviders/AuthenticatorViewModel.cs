using AppSample.Domain.Models.ServiceProviders;
using System.ComponentModel.DataAnnotations;

namespace AppSample.Admin.Models.ServiceProviders
{
    public class AuthenticatorViewModel
    {
        public int Id { get; set; }
        public int ServiceProviderId { get; set; }
        public AuthenticatorType Type { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Необходимо целое неотрицательное значение")]
        public int OrderLevel1 { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Необходимо целое неотрицательное значение")]
        public int OrderLevel2 { get; set; }

        [Required(ErrorMessage = "Необходимо значение в формате hh:mm:ss")]
        public TimeSpan? NextChainStartDelay { get; set; }
    }
}
