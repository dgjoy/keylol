namespace Keylol.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ExposeForeignKeys : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Messages", name: "Receiver_Id", newName: "ReceiverId");
            RenameColumn(table: "dbo.Comments", name: "Commentator_Id", newName: "CommentatorId");
            RenameColumn(table: "dbo.Logs", name: "Editor_Id", newName: "EditorId");
            RenameColumn(table: "dbo.Likes", name: "Operator_Id", newName: "OperatorId");
            RenameColumn(table: "dbo.Logs", name: "User_Id", newName: "UserId");
            RenameColumn(table: "dbo.Messages", name: "Sender_Id", newName: "SenderId");
            RenameColumn(table: "dbo.Messages", name: "Sender_Id1", newName: "SenderId1");
            RenameColumn(table: "dbo.KeylolUsers", name: "SteamBot_Id", newName: "SteamBotId");
            RenameColumn(table: "dbo.Comments", name: "Article_Id", newName: "ArticleId");
            RenameColumn(table: "dbo.Likes", name: "Comment_Id", newName: "CommentId");
            RenameColumn(table: "dbo.Messages", name: "Comment_Id1", newName: "CommentId2");
            RenameColumn(table: "dbo.Messages", name: "Comment_Id", newName: "CommentId");
            RenameColumn(table: "dbo.Messages", name: "Comment_Id2", newName: "CommentId1");
            RenameColumn(table: "dbo.Messages", name: "Target_Id1", newName: "TargetId1");
            RenameColumn(table: "dbo.Entries", name: "Principal_Id", newName: "PrincipalId");
            RenameColumn(table: "dbo.Logs", name: "Article_Id", newName: "ArticleId");
            RenameColumn(table: "dbo.Likes", name: "Article_Id", newName: "ArticleId");
            RenameColumn(table: "dbo.Entries", name: "RecommendedArticle_Id", newName: "RecommendedArticleId");
            RenameColumn(table: "dbo.Messages", name: "Article_Id1", newName: "ArticleId2");
            RenameColumn(table: "dbo.Messages", name: "Article_Id2", newName: "ArticleId3");
            RenameColumn(table: "dbo.Messages", name: "Article_Id", newName: "ArticleId1");
            RenameColumn(table: "dbo.Messages", name: "Article_Id3", newName: "ArticleId4");
            RenameColumn(table: "dbo.Messages", name: "Article_Id4", newName: "ArticleId");
            RenameColumn(table: "dbo.Messages", name: "Article_Id5", newName: "ArticleId5");
            RenameColumn(table: "dbo.Messages", name: "Target_Id", newName: "TargetId");
            RenameColumn(table: "dbo.Entries", name: "Type_Id", newName: "TypeId");
            RenameColumn(table: "dbo.Entries", name: "VoteForPoint_Id", newName: "VoteForPointId");
            RenameColumn(table: "dbo.Messages", name: "Point_Id", newName: "PointId");
            RenameColumn(table: "dbo.SteamBindingTokens", name: "Bot_Id", newName: "BotId");
            RenameIndex(table: "dbo.Messages", name: "IX_Receiver_Id", newName: "IX_ReceiverId");
            RenameIndex(table: "dbo.Messages", name: "IX_Sender_Id", newName: "IX_SenderId");
            RenameIndex(table: "dbo.Messages", name: "IX_Article_Id4", newName: "IX_ArticleId");
            RenameIndex(table: "dbo.Messages", name: "IX_Point_Id", newName: "IX_PointId");
            RenameIndex(table: "dbo.Messages", name: "IX_Article_Id", newName: "IX_ArticleId1");
            RenameIndex(table: "dbo.Messages", name: "IX_Comment_Id", newName: "IX_CommentId");
            RenameIndex(table: "dbo.Messages", name: "IX_Article_Id1", newName: "IX_ArticleId2");
            RenameIndex(table: "dbo.Messages", name: "IX_Article_Id2", newName: "IX_ArticleId3");
            RenameIndex(table: "dbo.Messages", name: "IX_Article_Id3", newName: "IX_ArticleId4");
            RenameIndex(table: "dbo.Messages", name: "IX_Article_Id5", newName: "IX_ArticleId5");
            RenameIndex(table: "dbo.Messages", name: "IX_Comment_Id2", newName: "IX_CommentId1");
            RenameIndex(table: "dbo.Messages", name: "IX_Target_Id", newName: "IX_TargetId");
            RenameIndex(table: "dbo.Messages", name: "IX_Comment_Id1", newName: "IX_CommentId2");
            RenameIndex(table: "dbo.Messages", name: "IX_Target_Id1", newName: "IX_TargetId1");
            RenameIndex(table: "dbo.Messages", name: "IX_Sender_Id1", newName: "IX_SenderId1");
            RenameIndex(table: "dbo.KeylolUsers", name: "IX_SteamBot_Id", newName: "IX_SteamBotId");
            RenameIndex(table: "dbo.Comments", name: "IX_Commentator_Id", newName: "IX_CommentatorId");
            RenameIndex(table: "dbo.Comments", name: "IX_Article_Id", newName: "IX_ArticleId");
            RenameIndex(table: "dbo.Entries", name: "IX_Principal_Id", newName: "IX_PrincipalId");
            RenameIndex(table: "dbo.Entries", name: "IX_Type_Id", newName: "IX_TypeId");
            RenameIndex(table: "dbo.Entries", name: "IX_RecommendedArticle_Id", newName: "IX_RecommendedArticleId");
            RenameIndex(table: "dbo.Entries", name: "IX_VoteForPoint_Id", newName: "IX_VoteForPointId");
            RenameIndex(table: "dbo.Logs", name: "IX_Article_Id", newName: "IX_ArticleId");
            RenameIndex(table: "dbo.Logs", name: "IX_Editor_Id", newName: "IX_EditorId");
            RenameIndex(table: "dbo.Logs", name: "IX_User_Id", newName: "IX_UserId");
            RenameIndex(table: "dbo.Likes", name: "IX_Operator_Id", newName: "IX_OperatorId");
            RenameIndex(table: "dbo.Likes", name: "IX_Article_Id", newName: "IX_ArticleId");
            RenameIndex(table: "dbo.Likes", name: "IX_Comment_Id", newName: "IX_CommentId");
            RenameIndex(table: "dbo.SteamBindingTokens", name: "IX_Bot_Id", newName: "IX_BotId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.SteamBindingTokens", name: "IX_BotId", newName: "IX_Bot_Id");
            RenameIndex(table: "dbo.Likes", name: "IX_CommentId", newName: "IX_Comment_Id");
            RenameIndex(table: "dbo.Likes", name: "IX_ArticleId", newName: "IX_Article_Id");
            RenameIndex(table: "dbo.Likes", name: "IX_OperatorId", newName: "IX_Operator_Id");
            RenameIndex(table: "dbo.Logs", name: "IX_UserId", newName: "IX_User_Id");
            RenameIndex(table: "dbo.Logs", name: "IX_EditorId", newName: "IX_Editor_Id");
            RenameIndex(table: "dbo.Logs", name: "IX_ArticleId", newName: "IX_Article_Id");
            RenameIndex(table: "dbo.Entries", name: "IX_VoteForPointId", newName: "IX_VoteForPoint_Id");
            RenameIndex(table: "dbo.Entries", name: "IX_RecommendedArticleId", newName: "IX_RecommendedArticle_Id");
            RenameIndex(table: "dbo.Entries", name: "IX_TypeId", newName: "IX_Type_Id");
            RenameIndex(table: "dbo.Entries", name: "IX_PrincipalId", newName: "IX_Principal_Id");
            RenameIndex(table: "dbo.Comments", name: "IX_ArticleId", newName: "IX_Article_Id");
            RenameIndex(table: "dbo.Comments", name: "IX_CommentatorId", newName: "IX_Commentator_Id");
            RenameIndex(table: "dbo.KeylolUsers", name: "IX_SteamBotId", newName: "IX_SteamBot_Id");
            RenameIndex(table: "dbo.Messages", name: "IX_SenderId1", newName: "IX_Sender_Id1");
            RenameIndex(table: "dbo.Messages", name: "IX_TargetId1", newName: "IX_Target_Id1");
            RenameIndex(table: "dbo.Messages", name: "IX_CommentId2", newName: "IX_Comment_Id1");
            RenameIndex(table: "dbo.Messages", name: "IX_TargetId", newName: "IX_Target_Id");
            RenameIndex(table: "dbo.Messages", name: "IX_CommentId1", newName: "IX_Comment_Id2");
            RenameIndex(table: "dbo.Messages", name: "IX_ArticleId5", newName: "IX_Article_Id5");
            RenameIndex(table: "dbo.Messages", name: "IX_ArticleId4", newName: "IX_Article_Id3");
            RenameIndex(table: "dbo.Messages", name: "IX_ArticleId3", newName: "IX_Article_Id2");
            RenameIndex(table: "dbo.Messages", name: "IX_ArticleId2", newName: "IX_Article_Id1");
            RenameIndex(table: "dbo.Messages", name: "IX_CommentId", newName: "IX_Comment_Id");
            RenameIndex(table: "dbo.Messages", name: "IX_ArticleId1", newName: "IX_Article_Id");
            RenameIndex(table: "dbo.Messages", name: "IX_PointId", newName: "IX_Point_Id");
            RenameIndex(table: "dbo.Messages", name: "IX_ArticleId", newName: "IX_Article_Id4");
            RenameIndex(table: "dbo.Messages", name: "IX_SenderId", newName: "IX_Sender_Id");
            RenameIndex(table: "dbo.Messages", name: "IX_ReceiverId", newName: "IX_Receiver_Id");
            RenameColumn(table: "dbo.SteamBindingTokens", name: "BotId", newName: "Bot_Id");
            RenameColumn(table: "dbo.Messages", name: "PointId", newName: "Point_Id");
            RenameColumn(table: "dbo.Entries", name: "VoteForPointId", newName: "VoteForPoint_Id");
            RenameColumn(table: "dbo.Entries", name: "TypeId", newName: "Type_Id");
            RenameColumn(table: "dbo.Messages", name: "TargetId", newName: "Target_Id");
            RenameColumn(table: "dbo.Messages", name: "ArticleId5", newName: "Article_Id5");
            RenameColumn(table: "dbo.Messages", name: "ArticleId", newName: "Article_Id4");
            RenameColumn(table: "dbo.Messages", name: "ArticleId4", newName: "Article_Id3");
            RenameColumn(table: "dbo.Messages", name: "ArticleId1", newName: "Article_Id");
            RenameColumn(table: "dbo.Messages", name: "ArticleId3", newName: "Article_Id2");
            RenameColumn(table: "dbo.Messages", name: "ArticleId2", newName: "Article_Id1");
            RenameColumn(table: "dbo.Entries", name: "RecommendedArticleId", newName: "RecommendedArticle_Id");
            RenameColumn(table: "dbo.Likes", name: "ArticleId", newName: "Article_Id");
            RenameColumn(table: "dbo.Logs", name: "ArticleId", newName: "Article_Id");
            RenameColumn(table: "dbo.Entries", name: "PrincipalId", newName: "Principal_Id");
            RenameColumn(table: "dbo.Messages", name: "TargetId1", newName: "Target_Id1");
            RenameColumn(table: "dbo.Messages", name: "CommentId1", newName: "Comment_Id2");
            RenameColumn(table: "dbo.Messages", name: "CommentId", newName: "Comment_Id");
            RenameColumn(table: "dbo.Messages", name: "CommentId2", newName: "Comment_Id1");
            RenameColumn(table: "dbo.Likes", name: "CommentId", newName: "Comment_Id");
            RenameColumn(table: "dbo.Comments", name: "ArticleId", newName: "Article_Id");
            RenameColumn(table: "dbo.KeylolUsers", name: "SteamBotId", newName: "SteamBot_Id");
            RenameColumn(table: "dbo.Messages", name: "SenderId1", newName: "Sender_Id1");
            RenameColumn(table: "dbo.Messages", name: "SenderId", newName: "Sender_Id");
            RenameColumn(table: "dbo.Logs", name: "UserId", newName: "User_Id");
            RenameColumn(table: "dbo.Likes", name: "OperatorId", newName: "Operator_Id");
            RenameColumn(table: "dbo.Logs", name: "EditorId", newName: "Editor_Id");
            RenameColumn(table: "dbo.Comments", name: "CommentatorId", newName: "Commentator_Id");
            RenameColumn(table: "dbo.Messages", name: "ReceiverId", newName: "Receiver_Id");
        }
    }
}
