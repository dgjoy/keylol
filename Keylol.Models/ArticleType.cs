namespace Keylol.Models
{
    public enum ArticleType
    {
        简评,
        评,
        研,
        谈,
        讯,
        档
    }

    public static class ArticleTypeExtensions
    {
        public static bool AllowVote(this ArticleType type)
        {
            return type == ArticleType.简评 || type == ArticleType.评;
        }
    }
}