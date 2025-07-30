using Assignment.Models;

namespace Assignment.Utilities
{
    public static class EarnPoint
    {
        public static void IncreaseAccumulatedPoint(ApplicationDbContext context, double point, long userId)
        {
            var user = context.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return;
            }

            user.TotalAccumulatedPoints += point;
            user.AccumulatedPoints += point;

            UserRankEnum oldRank = user.Rank;

            if (user.TotalAccumulatedPoints >= 1000000 && user.TotalAccumulatedPoints < 5000000)
            {
                user.Rank = UserRankEnum.Copper;
            }
            else if (user.TotalAccumulatedPoints >= 5000000 && user.TotalAccumulatedPoints < 15000000)
            {
                user.Rank = UserRankEnum.Silver;
            }
            else if (user.TotalAccumulatedPoints >= 15000000 && user.TotalAccumulatedPoints < 50000000)
            {
                user.Rank = UserRankEnum.Gold;
            }
            else if (user.TotalAccumulatedPoints >= 50000000)
            {
                user.Rank = UserRankEnum.Diamond;
            }

            if (user.Rank != oldRank)
            {
                if (oldRank == UserRankEnum.None && user.Rank == UserRankEnum.Copper)
                {
                    context.Vouchers.Add(new Vouchers()
                    {
                        Code = RandomStringGenerator.GenerateRandomAlphanumericString(),
                        Name = "Mã giảm giá độc quyền dành cho khách hàng hạng đồng",
                        Description = "Mã giảm giá độc quyền dành cho khách hàng hạng đồng",
                        Type = VoucherTypeEnum.Private,
                        UserId = userId,
                        Discount = 20,
                        DiscountType = DiscountTypeEnum.Percent,
                        Quantity = 5,
                        StartTime = DateTime.Now,
                        IsLifeTime = true,
                        MinimumRequirements = 0,
                        UnlimitedPercentageDiscount = true
                    });
                }
                if (oldRank == UserRankEnum.Copper && user.Rank < UserRankEnum.Silver)
                {
                    context.Vouchers.Add(new Vouchers()
                    {
                        Code = RandomStringGenerator.GenerateRandomAlphanumericString(),
                        Name = "Mã giảm giá độc quyền dành cho khách hàng hạng bạc",
                        Description = "Mã giảm giá độc quyền dành cho khách hàng hạng bạc",
                        Type = VoucherTypeEnum.Private,
                        UserId = userId,
                        Discount = 30,
                        DiscountType = DiscountTypeEnum.Percent,
                        Quantity = 5,
                        StartTime = DateTime.Now,
                        IsLifeTime = true,
                        MinimumRequirements = 0,
                        UnlimitedPercentageDiscount = true
                    });
                }
                if (oldRank == UserRankEnum.Silver && user.Rank == UserRankEnum.Gold)
                {
                    context.Vouchers.Add(new Vouchers()
                    {
                        Code = RandomStringGenerator.GenerateRandomAlphanumericString(),
                        Name = "Mã giảm giá độc quyền dành cho khách hàng hạng vàng",
                        Description = "Mã giảm giá độc quyền dành cho khách hàng hạng vàng",
                        Type = VoucherTypeEnum.Private,
                        UserId = userId,
                        Discount = 40,
                        DiscountType = DiscountTypeEnum.Percent,
                        Quantity = 5,
                        StartTime = DateTime.Now,
                        IsLifeTime = true,
                        MinimumRequirements = 0,
                        UnlimitedPercentageDiscount = true
                    });
                }
                if (oldRank == UserRankEnum.Gold && user.Rank == UserRankEnum.Diamond)
                {
                    context.Vouchers.Add(new Vouchers()
                    {
                        Code = RandomStringGenerator.GenerateRandomAlphanumericString(),
                        Name = "Mã giảm giá độc quyền dành cho khách hàng hạng kim cương",
                        Description = "Mã giảm giá độc quyền dành cho khách hàng hạng kim cương",
                        Type = VoucherTypeEnum.Private,
                        UserId = userId,
                        Discount = 50,
                        DiscountType = DiscountTypeEnum.Percent,
                        Quantity = 5,
                        StartTime = DateTime.Now,
                        IsLifeTime = true,
                        MinimumRequirements = 0,
                        UnlimitedPercentageDiscount = true
                    });
                }
            }

            context.Users.Update(user);
            context.SaveChanges();
        }
    }
}
