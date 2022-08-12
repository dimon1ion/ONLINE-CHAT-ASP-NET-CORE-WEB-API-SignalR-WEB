using FluentValidation;
using MW_Chat_ASP_NET_CORE_WEB_API_SignalR.Models;

namespace MW_Chat_ASP_NET_CORE_WEB_API_SignalR.Validators
{
    public class UserValidator : AbstractValidator<User>
    {
        public UserValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is Empty")
                .Length(3, 20).WithMessage("The name can be from 3 to 20 characters");
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is Empty")
                .MinimumLength(8).WithMessage("Min 8 symbols")
                .Matches("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$").WithMessage("Wrong format \"Aa1$\"");
        }
    }
}
