using Celeste;
using System;
using System.Linq;

namespace NoMathExpectation.Celeste.ThinkTwiceBeforeRetry
{
    public static class PlayerExtension
    {
        public static bool HasImportantCollectible(this Player player)
        {
            return player.Leader.Followers.Any(follower =>
            {
                var entity = follower.Entity;
                if (entity is Strawberry strawberry)
                {
                    return strawberry.Golden;
                }
                if (Type.GetType("Celeste.Mod.CollabUtils2.Entities.SilverBerry").IsInstanceOfType(entity))
                {
                    return true;
                }
                return false;
            });
        }
    }
}
