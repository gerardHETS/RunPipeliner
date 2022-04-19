using Microsoft.EntityFrameworkCore;
using RunPipeliner.DataAccess.EntityModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace RunPipeliner.DataAccess
{
	public class AccountDbContext : DbContext
	{
		public AccountDbContext()
		{
		}

		public AccountDbContext(DbContextOptions<AccountDbContext> options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{

		}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseMySQL(@"server=192.168.0.52;userid=amasters;pwd=Smackster1;port=3306;database=lablynk;sslmode=none;");

        public DbSet<Account> Accounts { get; set; }
		public DbSet<Contact> Contacts { get; set; }
		public DbSet<Order> Orders { get; set; }

	}
}

