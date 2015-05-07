using System.Collections.Generic;
using System.Data.Entity.Migrations;
using Keylol.DAL;
using Keylol.Models;
using Microsoft.AspNet.Identity;

namespace Keylol.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<KeylolDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(KeylolDbContext context)
        {
            var hasher = new PasswordHasher();
            var profilePoint = new ProfilePoint
            {
                OwnedPieces = new List<Piece>()
            };
            var normalPoint = new NormalPoint
            {
                Name = "²âÊÔ¾Ýµã",
                AlternativeName = "Test Point",
                UrlFriendlyName = "test-point",
                Type = NormalPointType.Game
            };
            var user = new KeylolUser
            {
                UserName = "stackia",
                PasswordHash = hasher.HashPassword("test"),
                Email = "jsq2627@gmail.com",
                ProfilePoint = profilePoint,
                SubscribedPoints = new List<Point> {normalPoint},
                ModeratedPoints = new List<NormalPoint> {normalPoint}
            };
            var articleType = new ArticleType
            {
                Category = ArticleTypeCategory.Topic,
                Name = "²âÊÔ"
            };
            var article = new Article
            {
                Title = "²âÊÔÎÄÕÂ",
                Content = "¹þ¹þ¹þ¹þ¹þ",
                Type = articleType,
                AttachedPoints = new List<Point> {profilePoint, normalPoint}
            };
            profilePoint.OwnedPieces.Add(article);
            context.Users.Add(user);
            context.SaveChanges();
        }
    }
}