namespace Keylol.Models
{
    public enum ArticleTypeNew
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
        public static bool AllowVote(this ArticleTypeNew type)
        {
            return type == ArticleTypeNew.简评 || type == ArticleTypeNew.评;
        }
    }
}