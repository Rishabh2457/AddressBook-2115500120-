using AutoMapper;
using BusinessLayer.Interface;
using ModelLayer;
using ModelLayer.DTO;
using RepositoryLayer.Interface;
using System.Collections.Generic;

namespace BusinessLayer.Service
{
    public class AddressBookBL : IAddressBookBL
    {
        private readonly IAddressBookRL _repository;
        private readonly IMapper _mapper;

        public AddressBookBL(IAddressBookRL repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public List<AddressBook> GetAllContacts()
        {
            return _repository.GetAllContacts();
        }

        public AddressBook GetContactById(int id)
        {
            return _repository.GetContactById(id);
        }

        public AddressBook AddContact(AddressBookDTO contactDTO)
        {
            var contact = _mapper.Map<AddressBook>(contactDTO);
            return _repository.AddContact(contact);
        }

        public AddressBook UpdateContact(int id, AddressBookDTO contactDTO)
        {
            var updatedContact = _mapper.Map<AddressBook>(contactDTO);
            return _repository.UpdateContact(id, updatedContact);
        }

        public bool DeleteContact(int id)
        {
            return _repository.DeleteContact(id);
        }
    }
}
