namespace Blog.Migrations
{
    using Blog.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Blog.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "Blog.Models.ApplicationDbContext";
        }

        protected override void Seed(Blog.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
            var Accounts = SeededAccount.CreateList();

            foreach (var account in Accounts)
            {
                //RoleManager, used to manage roles
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

                //UserManager, used to manage users
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                //Add a role if it doesn't exist.
                if (!context.Roles.Any(p => p.Name == account.Role))
                {
                    var roleToAdd = new IdentityRole(account.Role);
                    roleManager.Create(roleToAdd);
                }

                //Creating the user with this role
                ApplicationUser userToCreate;

                if (!context.Users.Any(p => p.UserName == account.UserName))
                {
                    userToCreate = new ApplicationUser();
                    userToCreate.UserName = account.UserName;
                    userToCreate.Email = account.UserName;

                    userManager.Create(userToCreate, account.Password);
                }
                else
                {
                    userToCreate = context.Users.First(p => p.UserName == account.UserName);
                }

                //Make sure the user is on its role
                if (!userManager.IsInRole(userToCreate.Id, account.Role))
                {
                    userManager.AddToRole(userToCreate.Id, account.Role);
                }
            }   
        }
    }
}
