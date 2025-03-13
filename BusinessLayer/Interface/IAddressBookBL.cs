using ModelLayer;
using ModelLayer.DTO;
using System.Collections.Generic;

namespace BusinessLayer.Interface
{
    public interface IAddressBookBL
    {
        List<AddressBook> GetAllContacts();
        AddressBook GetContactById(int id);
        AddressBook AddContact(AddressBookDTO contact);
        AddressBook UpdateContact(int id, AddressBookDTO contact);
        bool DeleteContact(int id);
    }
}
