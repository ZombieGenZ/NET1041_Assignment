using Assignment.Models;

namespace Assignment.Utilities
{
    public static class EarnPoint
    {
        public static void IncreaseAccumulatedPoint(ApplicationDbContext context, long point, long userId)
        {
            var user = context.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return;
            }

            user.TotalAccumulatedPoints += point;

            if (user.TotalAccumulatedPoints >= 1000 && user.TotalAccumulatedPoints < 5000)
            {
                user.Rank = UserRankEnum.Copper;
            }
            else if (user.TotalAccumulatedPoints >= 5000 && user.TotalAccumulatedPoints < 15000)
            {
                user.Rank = UserRankEnum.Silver;
            }
            else if (user.TotalAccumulatedPoints >= 15000 && user.TotalAccumulatedPoints < 50000)
            {
                user.Rank = UserRankEnum.Gold;
            }
            else if (user.TotalAccumulatedPoints >= 50000)
            {
                user.Rank = UserRankEnum.Gold;
            }

            context.Users.Update(user);
            context.SaveChanges();
        }
    }
}
