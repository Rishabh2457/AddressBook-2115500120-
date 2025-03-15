using System;
using ModelLayer.Model;
namespace RepositoryLayer.Interface
{
    public interface IAddressBookRL
    {
        AddressBook? GetById(int id);
        public List<AddressBook> GetAll();
        bool AddEntry(AddressBook entry);
        bool UpdateEntry(int id, AddressBook updatedentry);
        bool DeleteEntry(int id);

    }
}
