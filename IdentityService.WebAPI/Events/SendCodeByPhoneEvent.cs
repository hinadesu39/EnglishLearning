using MediatR;

namespace IdentityService.WebAPI.Events
{
    public record SendCodeByPhoneEvent(string PhoneNum, string token):INotification;

}
