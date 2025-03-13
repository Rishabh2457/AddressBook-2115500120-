using Microsoft.EntityFrameworkCore;
using ModelLayer;
using ModelLayer.DTO;
using RepositoryLayer.Context;
using RepositoryLayer.Interface;
using System.Collections.Generic;
using System.Linq;

namespace RepositoryLayer.Service
{
    public class AddressBookRL : IAddressBookRL
    {
        private readonly AddressBookContext _context;

        public AddressBookRL(AddressBookContext context)
        {
            _context = context;
        }

        public List<AddressBook> GetAllContacts() => _context.AddressBookEntries.ToList();

        public AddressBook GetContactById(int id)
        {
            return _context.AddressBookEntries.Find(id);
        }

        public AddressBook AddContact(AddressBook contact)
        {
            _context.AddressBookEntries.Add(contact);
            _context.SaveChanges();
            return contact;
        }

        public AddressBook UpdateContact(int id, AddressBook contact)
        {
            var existingContact = _context.AddressBookEntries.Find(id);
            if (existingContact == null) return null;

            existingContact.Name = contact.Name;
            existingContact.Email = contact.Email;
            existingContact.Phone = contact.Phone;
            existingContact.Address = contact.Address;

            _context.SaveChanges();
            return existingContact;
        }

        public bool DeleteContact(int id)
        {
            var contact = _context.AddressBookEntries.Find(id);
            if (contact == null) return false;

            _context.AddressBookEntries.Remove(contact);
            _context.SaveChanges();
            return true;
        }
    }
}
