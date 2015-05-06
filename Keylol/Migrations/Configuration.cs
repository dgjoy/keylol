using System.Collections.Generic;
using Keylol.DAL;
using Keylol.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Keylol.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<KeylolDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(KeylolDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
            //var hasher = new PasswordHasher();
            //var profilePoint = new ProfilePoint();
            //var user = new KeylolUser
            //{
            //    UserName = "stackia",
            //    PasswordHash = hasher.HashPassword("test"),
            //    Email = "jsq2627@gmail.com",
            //    ProfilePoint = profilePoint
            //};
            //var normalPoint = new NormalPoint
            //{
            //    Name = "≤‚ ‘æ›µ„",
            //    AlternativeName = "Test Point",
            //    UrlFriendlyName = "test-point",
            //    Type = NormalPointType.Game,
            //    Subscribers = new List<KeylolUser>()
            //};
            //context.ProfilePoints.Add(profilePoint);
            //context.Users.Add(user);
            //normalPoint.Subscribers.Add(user);
            //context.NormalPoints.Add(normalPoint);
            //context.SaveChanges();
        }
    }
}
