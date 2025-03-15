using AutoMapper;
using ModelLayer;
using ModelLayer.DTO;
using ModelLayer.Model;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, RegisterDTO>().ReverseMap();
        CreateMap<AddressBook, AddressBookDTO>().ReverseMap();
    }
}
