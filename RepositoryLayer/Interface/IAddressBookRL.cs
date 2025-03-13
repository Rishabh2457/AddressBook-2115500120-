using ModelLayer;
using ModelLayer.DTO;
using System.Collections.Generic;

namespace RepositoryLayer.Interface
{
    public interface IAddressBookRL
    {
        List<AddressBook> GetAllContacts();
        AddressBook GetContactById(int id);
        AddressBook AddContact(AddressBook contact);
        AddressBook UpdateContact(int id, AddressBook contact);
        bool DeleteContact(int id);
    }
}
