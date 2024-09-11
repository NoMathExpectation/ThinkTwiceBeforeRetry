using Celeste;
using Celeste.Mod;
using MonoMod.Utils;
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
                if (entity is IStrawberry)
                {
                    var data = DynamicData.For(entity);
                    bool? golden = false;
                    var result = data.TryGet("Golden", out golden);
                    return result && (golden == true);
                }
                return false;
            });
        }
    }
}
