-- ============================================================
-- WISHLIST TABLE - ADD UNIQUE CONSTRAINT
-- ============================================================
-- Purpose: Prevent duplicate wishlist entries at database level
-- This ensures one user cannot add the same outfit multiple times
-- ============================================================

-- Check if constraint already exists
IF NOT EXISTS (
    SELECT 1 
FROM sys.indexes 
    WHERE name = 'UQ_Wishlist_User_Outfit' 
    AND object_id = OBJECT_ID('dbo.Wishlists')
)
BEGIN
    -- Create unique constraint on (UserId, OutfitId) combination
    CREATE UNIQUE INDEX UQ_Wishlist_User_Outfit 
    ON [dbo].[Wishlists] ([UserId], [OutfitId]);
    
    PRINT 'Unique constraint UQ_Wishlist_User_Outfit created successfully.';
END
ELSE
BEGIN
    PRINT 'Unique constraint UQ_Wishlist_User_Outfit already exists.';
END
GO

-- ============================================================
-- BENEFITS OF THIS CONSTRAINT:
-- ============================================================
-- 1. Database-level enforcement (can't be bypassed)
-- 2. Better data integrity
-- 3. Automatic prevention of duplicates
-- 4. Performance improvement on lookups
-- ============================================================

-- Test the constraint (should fail on second insert)
/*
-- This will succeed
INSERT INTO [dbo].[Wishlists] ([UserId], [OutfitId], [AddedAt]) 
VALUES (3, 99, GETDATE());

-- This will fail with error: "Cannot insert duplicate key"
INSERT INTO [dbo].[Wishlists] ([UserId], [OutfitId], [AddedAt]) 
VALUES (3, 99, GETDATE());
*/
