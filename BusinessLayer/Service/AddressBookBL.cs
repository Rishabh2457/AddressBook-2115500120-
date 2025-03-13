using BusinessLayer.Interface;
using ModelLayer;
using ModelLayer.DTO;
using RepositoryLayer.Interface;
using System.Collections.Generic;

namespace BusinessLayer.Service
{
    public class AddressBookBL : IAddressBookBL
    {
        private readonly IAddressBookRL _addressBookRL;

        public AddressBookBL(IAddressBookRL addressBookRL)
        {
            _addressBookRL = addressBookRL;
        }

        public List<AddressBook> GetAllContacts()
        {
            return _addressBookRL.GetAllContacts();
        }

        public AddressBook GetContactById(int id)
        {
            return _addressBookRL.GetContactById(id);
        }

        public AddressBook AddContact(AddressBook contact)
        {
            return _addressBookRL.AddContact(contact);
        }

        public AddressBook UpdateContact(int id, AddressBook contact)
        {
            return _addressBookRL.UpdateContact(id, contact);
        }

        public bool DeleteContact(int id)
        {
            return _addressBookRL.DeleteContact(id);
        }
    }
}
