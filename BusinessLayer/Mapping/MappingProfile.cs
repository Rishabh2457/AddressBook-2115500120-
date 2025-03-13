using AutoMapper;
using ModelLayer;
using ModelLayer.DTO;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<AddressBook, AddressBookDTO>().ReverseMap();
    }
}
