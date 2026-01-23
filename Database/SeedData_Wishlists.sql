-- Seed data for Wishlists table
-- Each user can have multiple outfits in their wishlist
-- No duplicates allowed (same user + outfit combination)

-- User 3's wishlist (5 different outfits)
INSERT INTO [dbo].[Wishlists] ([UserId], [OutfitId], [AddedAt]) VALUES (3, 1, '2024-01-15 10:30:00');
INSERT INTO [dbo].[Wishlists] ([UserId], [OutfitId], [AddedAt]) VALUES (3, 2, '2024-01-16 14:20:00');
INSERT INTO [dbo].[Wishlists] ([UserId], [OutfitId], [AddedAt]) VALUES (3, 3, '2024-01-17 09:15:00');
INSERT INTO [dbo].[Wishlists] ([UserId], [OutfitId], [AddedAt]) VALUES (3, 5, '2024-01-18 16:45:00');
INSERT INTO [dbo].[Wishlists] ([UserId], [OutfitId], [AddedAt]) VALUES (3, 7, '2024-01-19 11:30:00');

-- User 4's wishlist (3 outfits)
INSERT INTO [dbo].[Wishlists] ([UserId], [OutfitId], [AddedAt]) VALUES (4, 2, '2024-01-20 13:00:00');
INSERT INTO [dbo].[Wishlists] ([UserId], [OutfitId], [AddedAt]) VALUES (4, 4, '2024-01-21 10:30:00');
INSERT INTO [dbo].[Wishlists] ([UserId], [OutfitId], [AddedAt]) VALUES (4, 6, '2024-01-22 15:45:00');

-- User 5's wishlist (2 outfits)
INSERT INTO [dbo].[Wishlists] ([UserId], [OutfitId], [AddedAt]) VALUES (5, 1, '2024-01-23 09:20:00');
INSERT INTO [dbo].[Wishlists] ([UserId], [OutfitId], [AddedAt]) VALUES (5, 3, '2024-01-24 14:10:00');

-- Verify the data
SELECT W.WishlistId, W.UserId, U.FullName, W.OutfitId, O.Name as OutfitName, W.AddedAt
FROM Wishlists W
INNER JOIN Users U ON W.UserId = U.UserId
INNER JOIN Outfits O ON W.OutfitId = O.OutfitId
ORDER BY W.UserId, W.AddedAt;
