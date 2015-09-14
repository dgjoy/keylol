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
            context.ArticleTypes.AddOrUpdate(type => type.Name,
                new ArticleType()
                {
                    Name = "评测",
                    Category = ArticleTypeCategory.Topic,
                    AllowVote = true
                },
                new ArticleType()
                {
                    Name = "感悟",
                    Category = ArticleTypeCategory.Topic,
                    AllowVote = true
                },
                new ArticleType()
                {
                    Name = "研讨",
                    Category = ArticleTypeCategory.Topic
                },
                new ArticleType()
                {
                    Name = "攻略",
                    Category = ArticleTypeCategory.Topic
                },
                new ArticleType()
                {
                    Name = "新闻",
                    Category = ArticleTypeCategory.Aging
                },
                new ArticleType()
                {
                    Name = "购物",
                    Category = ArticleTypeCategory.Aging
                },
                new ArticleType()
                {
                    Name = "杂谈",
                    Category = ArticleTypeCategory.Personal
                },
                new ArticleType()
                {
                    Name = "原声",
                    Category = ArticleTypeCategory.Resource
                },
                new ArticleType()
                {
                    Name = "原画",
                    Category = ArticleTypeCategory.Resource
                },
                new ArticleType()
                {
                    Name = "资料",
                    Category = ArticleTypeCategory.Resource
                },
                new ArticleType()
                {
                    Name = "汉化",
                    Category = ArticleTypeCategory.Resource
                },
                new ArticleType()
                {
                    Name = "模组",
                    Category = ArticleTypeCategory.Resource
                },
                new ArticleType()
                {
                    Name = "插件",
                    Category = ArticleTypeCategory.Resource
                });
//            var hasher = new PasswordHasher();
//            var profilePoint = new ProfilePoint
//            {
//                OwnedEntries = new List<Entry>()
//            };
//            var normalPoint = new NormalPoint
//            {
//                Name = "测试据点",
//                AlternativeName = "Test Point",
//                UrlFriendlyName = "test-point",
//                Type = NormalPointType.Game
//            };
//            var user = new KeylolUser
//            {
//                UserName = "stackia",
//                PasswordHash = hasher.HashPassword("test"),
//                Email = "jsq2627@gmail.com",
//                ProfilePoint = profilePoint,
//                SecurityStamp = "hahaha",
//                SubscribedPoints = new List<Point> {normalPoint},
//                ModeratedPoints = new List<NormalPoint> {normalPoint}
//            };
//            var articleType = new ArticleType
//            {
//                Category = ArticleTypeCategory.Topic,
//                Name = "测试",
//                UrlFriendlyName = "test"
//            };
//            var article = new Article
//            {
//                Title = "测试文章",
//                Content = "哈哈哈哈哈",
//                Type = articleType,
//                AttachedPoints = new List<Point> {profilePoint, normalPoint}
//            };
//            profilePoint.OwnedEntries.Add(article);
//            context.Users.Add(user);
//            context.SaveChanges();
        }
    }
}