using Celeste;
using System.Linq;

namespace NoMathExpectation.Celeste.ThinkTwiceBeforeRetry
{
    public static class PlayerExtension
    {
        public static bool HasImportantCollectible(this Player player)
        {
            return player.Leader.Followers.Any(follower =>
            {
                var entity = follower?.Entity;
                if (entity == null)
                {
                    return false;
                }
                if (entity is Strawberry strawberry)
                {
                    return strawberry.Golden;
                }
                return false;
            });
        }
    }
}
