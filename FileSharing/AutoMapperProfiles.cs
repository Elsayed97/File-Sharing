using AutoMapper;
using FileSharing.Models;
using FileSharing.Data;

namespace FileSharingApp
{
    public class UploadProfile : Profile
    {
        public UploadProfile()
        {
            CreateMap<InputUpload, Uploads>()
                .ForMember(u => u.Id, op => op.Ignore())
                .ForMember(u => u.UploadDate, op => op.Ignore());

            CreateMap<Uploads, UploadViewModel>();
        }
    }

    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<ApplicationUser, UserViewModel>()
                .ForMember(u => u.HasPassword, op => op.MapFrom(u => u.PasswordHash != null));
        }
    }
}
