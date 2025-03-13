using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ModelLayer.DTO;

namespace RepositoryLayer.Context
{
    public class AddressBookContext:DbContext
    {
        public AddressBookContext(DbContextOptions<AddressBookContext> options) : base(options) { }

        public DbSet<AddressBook> AddressBookEntries { get; set; }

    }
}
