using System;
using RepositoryLayer.Context;
using ModelLayer.Model;
using RepositoryLayer.Interface;
using Azure;

namespace RepositoryLayer.Service
{
    public class AddressBookRL : IAddressBookRL
    {
        private readonly AddressBookContext _context;
        //Constructor of class
        public AddressBookRL(AddressBookContext context)
        {
            _context = context;
        }
        /// <summary>
        /// method to get all the contacts from database
        /// </summary>
        /// <returns>List of contacts </returns>
        public List<AddressBook> GetAll()
        {
            return _context.AddressBookEntries.ToList<AddressBook>();
        }
        /// <summary>
        /// method to get the particular contact
        /// </summary>
        /// <param name="id">input id from user</param>
        /// <returns>Contact on Particular id</returns>
        public AddressBook? GetById(int id)
        {

            return _context.AddressBookEntries.Find(id);
        }
        /// <summary>
        /// method to add the contact in addressbook
        /// </summary>
        /// <param name="entry">Contact info to be added</param>
        /// <returns>Returns True or false if added or not</returns>
        public bool AddEntry(AddressBook contact)
        {
            contact.UserId = contact.UserId; 
            _context.AddressBookEntries.Add(contact);
            return _context.SaveChanges() > 0;
        }

        /// <summary>
        /// method to update the contact info on id
        /// </summary>
        /// <param name="id">id to be updated</param>
        /// <param name="updatedentry">Updated contact info from user</param>
        /// <returns>True or false if contact is updated</returns>
        public bool UpdateEntry(int id, AddressBook updatedentry)
        {
            var contact = _context.AddressBookEntries.Find(id);
            if (contact != null)
            {
                contact.Name = updatedentry.Name;
                contact.Email = updatedentry.Email;
                contact.Phone = updatedentry.Phone;
                _context.SaveChanges();
                return true;
            }

            return false;
        }
        /// <summary>
        /// delete the contact on particular id
        /// </summary>
        /// <param name="id">id to be deleted</param>
        /// <returns>Returns true or false if deleted or not</returns>
        public bool DeleteEntry(int id)
        {
            var contact = _context.AddressBookEntries.Find(id);
            if (contact != null)
            {
                _context.AddressBookEntries.Remove(contact);
                _context.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
