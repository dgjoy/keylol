using System.Data.Entity.Migrations;
using Keylol.Models.DAL;

namespace Keylol.Models.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<KeylolDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(KeylolDbContext context)
        {
//            context.ArticleTypes.AddOrUpdate(type => type.Name, new ArticleType
//            {
//                Name = "简评",
//                Description = "简评、刺猬、简评、刺猬",
//                AllowVote = true
//            });
//            context.ArticleTypes.AddOrUpdate(type => type.Name,
//                new ArticleType()
//                {
//                    Name = "评",
//                    Description = "感悟、心得、体验、报告",
//                    AllowVote = true
//                },
//                new ArticleType()
//                {
//                    Name = "研",
//                    Description = "攻略、技术、成就、教程"
//                },
//                new ArticleType()
//                {
//                    Name = "讯",
//                    Description = "新闻、购物、更新、竞技"
//                },
//                new ArticleType()
//                {
//                    Name = "谈",
//                    Description = "聊天、灌水、吐槽、杂文"
//                },
//                new ArticleType()
//                {
//                    Name = "档",
//                    Description = "声画、模组、插件、汉化"
//                });
//
//            var credentials =
//                @"keylol_bot_1 YrXF9LGfHkTJYW8HE4GE8YpJ|keylol_bot_2 NWG8SUTuXKGBkK7g4dXHCWdU|keylol_bot_3 EXc897fp5cg2akUpzazwRCk2|keylol_bot_4 LNFLNCvmSmr2EJHqRNHjpNVt|keylol_bot_5 AVw9sFHWQuZ9jx4xc8cwA5ny"
//                    .Split('|').Select(s =>
//                    {
//                        var parts = s.Split(' ');
//                        return new
//                        {
//                            UserName = parts[0],
//                            Password = parts[1]
//                        };
//                    });
//
//            foreach (var credential in credentials)
//            {
//                context.SteamBots.AddOrUpdate(bot => bot.SteamUserName, new Models.SteamBot
//                {
//                    SteamUserName = credential.UserName,
//                    SteamPassword = credential.Password,
//                    FriendUpperLimit = 200
//                });
//            }


            // 测试用机器人
//            context.SteamBots.AddOrUpdate(bot => bot.SteamUserName, new Models.SteamBot
//            {
//                SteamUserName = "keylol_bot_test_1",
//                SteamPassword = "fvZyELZWpW8SEy4UXaysz2HQ",
//                FriendUpperLimit = 200
//            });
//            context.SteamBots.AddOrUpdate(bot => bot.SteamUserName, new Models.SteamBot
//            {
//                SteamUserName = "keylol_bot_test_2",
//                SteamPassword = "bGGDWcAXCz2KhmmZ7sywwNGg",
//                FriendUpperLimit = 200
//            });
        }
    }
}