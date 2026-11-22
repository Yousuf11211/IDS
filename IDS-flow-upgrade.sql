BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'AckFlagCounts');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [AckFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'BwdAckFlagCounts');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [BwdAckFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'BwdBulkStateCount');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [BwdBulkStateCount] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'BwdFinFlagCounts');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [BwdFinFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'BwdInitWinBytes');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [BwdInitWinBytes] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'BwdPacketsCount');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [BwdPacketsCount] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'BwdRstFlagCounts');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [BwdRstFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'BwdSynFlagCounts');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [BwdSynFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'BwdUrgFlagCounts');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var8 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [BwdUrgFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'CwrFlagCounts');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var9 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [CwrFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var10 sysname;
    SELECT @var10 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'DstPort');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var10 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [DstPort] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var11 sysname;
    SELECT @var11 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'EceFlagCounts');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var11 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [EceFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var12 sysname;
    SELECT @var12 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FinFlagCounts');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var12 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FinFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var13 sysname;
    SELECT @var13 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdAckFlagCounts');
    IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var13 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdAckFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var14 sysname;
    SELECT @var14 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdFinFlagCounts');
    IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var14 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdFinFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var15 sysname;
    SELECT @var15 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdInitWinBytes');
    IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var15 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdInitWinBytes] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var16 sysname;
    SELECT @var16 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdPacketsCount');
    IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var16 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdPacketsCount] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var17 sysname;
    SELECT @var17 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdPshFlagCounts');
    IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var17 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdPshFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var18 sysname;
    SELECT @var18 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdRstFlagCounts');
    IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var18 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdRstFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var19 sysname;
    SELECT @var19 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdSynFlagCounts');
    IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var19 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdSynFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var20 sysname;
    SELECT @var20 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdUrgFlagCounts');
    IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var20 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdUrgFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var21 sysname;
    SELECT @var21 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'PacketsCount');
    IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var21 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [PacketsCount] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var22 sysname;
    SELECT @var22 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'PshFlagCounts');
    IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var22 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [PshFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var23 sysname;
    SELECT @var23 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'RstFlagCounts');
    IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var23 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [RstFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var24 sysname;
    SELECT @var24 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'SubflowBwdPackets');
    IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var24 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [SubflowBwdPackets] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var25 sysname;
    SELECT @var25 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'SubflowFwdPackets');
    IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var25 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [SubflowFwdPackets] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var26 sysname;
    SELECT @var26 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'SynFlagCounts');
    IF @var26 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var26 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [SynFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var27 sysname;
    SELECT @var27 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'UrgFlagCounts');
    IF @var27 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var27 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [UrgFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var28 sysname;
    SELECT @var28 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'BwdTotalHeaderBytes');
    IF @var28 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var28 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [BwdTotalHeaderBytes] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var29 sysname;
    SELECT @var29 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'BwdTotalPayloadBytes');
    IF @var29 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var29 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [BwdTotalPayloadBytes] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var30 sysname;
    SELECT @var30 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdTotalHeaderBytes');
    IF @var30 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var30 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdTotalHeaderBytes] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var31 sysname;
    SELECT @var31 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdTotalPayloadBytes');
    IF @var31 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var31 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdTotalPayloadBytes] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var32 sysname;
    SELECT @var32 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'SubflowBwdBytes');
    IF @var32 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var32 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [SubflowBwdBytes] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var33 sysname;
    SELECT @var33 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'SubflowFwdBytes');
    IF @var33 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var33 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [SubflowFwdBytes] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var34 sysname;
    SELECT @var34 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'TotalHeaderBytes');
    IF @var34 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var34 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [TotalHeaderBytes] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var35 sysname;
    SELECT @var35 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'TotalPayloadBytes');
    IF @var35 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var35 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [TotalPayloadBytes] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var36 sysname;
    SELECT @var36 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'AckFlagPercentageInTotal');
    IF @var36 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var36 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [AckFlagPercentageInTotal] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var37 sysname;
    SELECT @var37 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'ActiveCov');
    IF @var37 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var37 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [ActiveCov] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var38 sysname;
    SELECT @var38 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'ActiveMax');
    IF @var38 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var38 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [ActiveMax] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var39 sysname;
    SELECT @var39 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'ActiveMean');
    IF @var39 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var39 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [ActiveMean] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var40 sysname;
    SELECT @var40 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'ActiveMedian');
    IF @var40 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var40 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [ActiveMedian] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var41 sysname;
    SELECT @var41 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'ActiveMin');
    IF @var41 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var41 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [ActiveMin] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var42 sysname;
    SELECT @var42 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'ActiveMode');
    IF @var42 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var42 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [ActiveMode] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var43 sysname;
    SELECT @var43 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'ActiveSkewness');
    IF @var43 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var43 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [ActiveSkewness] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var44 sysname;
    SELECT @var44 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'ActiveStd');
    IF @var44 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var44 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [ActiveStd] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var45 sysname;
    SELECT @var45 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'ActiveVariance');
    IF @var45 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var45 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [ActiveVariance] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var46 sysname;
    SELECT @var46 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'AvgBwdBulkRate');
    IF @var46 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var46 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [AvgBwdBulkRate] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var47 sysname;
    SELECT @var47 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'AvgSegmentSize');
    IF @var47 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var47 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [AvgSegmentSize] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var48 sysname;
    SELECT @var48 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'BwdAckFlagPercentageInBwdPackets');
    IF @var48 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var48 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [BwdAckFlagPercentageInBwdPackets] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var49 sysname;
    SELECT @var49 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'BwdAckFlagPercentageInTotal');
    IF @var49 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var49 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [BwdAckFlagPercentageInTotal] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var50 sysname;
    SELECT @var50 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'BwdBulkDuration');
    IF @var50 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var50 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [BwdBulkDuration] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var51 sysname;
    SELECT @var51 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'BwdBytesRate');
    IF @var51 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var51 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [BwdBytesRate] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var52 sysname;
    SELECT @var52 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'BwdMeanHeaderBytes');
    IF @var52 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var52 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [BwdMeanHeaderBytes] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var53 sysname;
    SELECT @var53 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'BwdPacketsIatMax');
    IF @var53 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var53 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [BwdPacketsIatMax] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var54 sysname;
    SELECT @var54 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'BwdPacketsIatMean');
    IF @var54 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var54 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [BwdPacketsIatMean] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var55 sysname;
    SELECT @var55 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'BwdPacketsIatMedian');
    IF @var55 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var55 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [BwdPacketsIatMedian] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var56 sysname;
    SELECT @var56 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'BwdPacketsIatMin');
    IF @var56 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var56 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [BwdPacketsIatMin] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var57 sysname;
    SELECT @var57 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'BwdPacketsIatMode');
    IF @var57 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var57 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [BwdPacketsIatMode] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var58 sysname;
    SELECT @var58 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'BwdPacketsIatStd');
    IF @var58 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var58 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [BwdPacketsIatStd] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var59 sysname;
    SELECT @var59 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'BwdPacketsIatTotal');
    IF @var59 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var59 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [BwdPacketsIatTotal] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var60 sysname;
    SELECT @var60 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'BwdPacketsIatVariance');
    IF @var60 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var60 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [BwdPacketsIatVariance] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var61 sysname;
    SELECT @var61 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'BwdPacketsRate');
    IF @var61 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var61 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [BwdPacketsRate] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var62 sysname;
    SELECT @var62 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'BwdPayloadBytesMax');
    IF @var62 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var62 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [BwdPayloadBytesMax] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var63 sysname;
    SELECT @var63 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'BwdPayloadBytesMedian');
    IF @var63 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var63 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [BwdPayloadBytesMedian] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var64 sysname;
    SELECT @var64 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'BwdPayloadBytesStd');
    IF @var64 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var64 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [BwdPayloadBytesStd] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var65 sysname;
    SELECT @var65 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'BytesRate');
    IF @var65 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var65 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [BytesRate] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var66 sysname;
    SELECT @var66 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'CovBwdHeaderBytesDeltaLen');
    IF @var66 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var66 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [CovBwdHeaderBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var67 sysname;
    SELECT @var67 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'CovBwdPacketsDeltaLen');
    IF @var67 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var67 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [CovBwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var68 sysname;
    SELECT @var68 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'CovBwdPacketsDeltaTime');
    IF @var68 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var68 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [CovBwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var69 sysname;
    SELECT @var69 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'CovFwdHeaderBytesDeltaLen');
    IF @var69 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var69 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [CovFwdHeaderBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var70 sysname;
    SELECT @var70 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'CovFwdPacketsDeltaLen');
    IF @var70 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var70 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [CovFwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var71 sysname;
    SELECT @var71 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'CovFwdPacketsDeltaTime');
    IF @var71 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var71 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [CovFwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var72 sysname;
    SELECT @var72 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'CovHeaderBytesDeltaLen');
    IF @var72 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var72 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [CovHeaderBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var73 sysname;
    SELECT @var73 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'CovPacketsDeltaLen');
    IF @var73 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var73 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [CovPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var74 sysname;
    SELECT @var74 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'DeltaStart');
    IF @var74 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var74 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [DeltaStart] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var75 sysname;
    SELECT @var75 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'DownUpRate');
    IF @var75 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var75 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [DownUpRate] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var76 sysname;
    SELECT @var76 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'Duration');
    IF @var76 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var76 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [Duration] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var77 sysname;
    SELECT @var77 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdAckFlagPercentageInFwdPackets');
    IF @var77 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var77 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdAckFlagPercentageInFwdPackets] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var78 sysname;
    SELECT @var78 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdAckFlagPercentageInTotal');
    IF @var78 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var78 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdAckFlagPercentageInTotal] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var79 sysname;
    SELECT @var79 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdAvgSegmentSize');
    IF @var79 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var79 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdAvgSegmentSize] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var80 sysname;
    SELECT @var80 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdBytesRate');
    IF @var80 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var80 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdBytesRate] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var81 sysname;
    SELECT @var81 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdMeanHeaderBytes');
    IF @var81 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var81 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdMeanHeaderBytes] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var82 sysname;
    SELECT @var82 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdPacketsIatCov');
    IF @var82 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var82 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdPacketsIatCov] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var83 sysname;
    SELECT @var83 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdPacketsIatMax');
    IF @var83 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var83 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdPacketsIatMax] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var84 sysname;
    SELECT @var84 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdPacketsIatMean');
    IF @var84 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var84 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdPacketsIatMean] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var85 sysname;
    SELECT @var85 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdPacketsIatMedian');
    IF @var85 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var85 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdPacketsIatMedian] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var86 sysname;
    SELECT @var86 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdPacketsIatMin');
    IF @var86 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var86 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdPacketsIatMin] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var87 sysname;
    SELECT @var87 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdPacketsIatMode');
    IF @var87 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var87 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdPacketsIatMode] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var88 sysname;
    SELECT @var88 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdPacketsIatSkewness');
    IF @var88 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var88 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdPacketsIatSkewness] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var89 sysname;
    SELECT @var89 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdPacketsIatStd');
    IF @var89 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var89 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdPacketsIatStd] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var90 sysname;
    SELECT @var90 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdPacketsIatTotal');
    IF @var90 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var90 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdPacketsIatTotal] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var91 sysname;
    SELECT @var91 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdPacketsIatVariance');
    IF @var91 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var91 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdPacketsIatVariance] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var92 sysname;
    SELECT @var92 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdPacketsRate');
    IF @var92 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var92 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdPacketsRate] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var93 sysname;
    SELECT @var93 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdPayloadBytesCov');
    IF @var93 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var93 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdPayloadBytesCov] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var94 sysname;
    SELECT @var94 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdPayloadBytesMax');
    IF @var94 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var94 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdPayloadBytesMax] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var95 sysname;
    SELECT @var95 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdPayloadBytesMedian');
    IF @var95 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var95 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdPayloadBytesMedian] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var96 sysname;
    SELECT @var96 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdPayloadBytesSkewness');
    IF @var96 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var96 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdPayloadBytesSkewness] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var97 sysname;
    SELECT @var97 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'FwdPayloadBytesStd');
    IF @var97 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var97 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [FwdPayloadBytesStd] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var98 sysname;
    SELECT @var98 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'HandshakeDuration');
    IF @var98 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var98 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [HandshakeDuration] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var99 sysname;
    SELECT @var99 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'IdleCov');
    IF @var99 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var99 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [IdleCov] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var100 sysname;
    SELECT @var100 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'IdleMax');
    IF @var100 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var100 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [IdleMax] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var101 sysname;
    SELECT @var101 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'IdleMean');
    IF @var101 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var101 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [IdleMean] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var102 sysname;
    SELECT @var102 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'IdleMedian');
    IF @var102 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var102 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [IdleMedian] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var103 sysname;
    SELECT @var103 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'IdleMin');
    IF @var103 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var103 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [IdleMin] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var104 sysname;
    SELECT @var104 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'IdleMode');
    IF @var104 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var104 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [IdleMode] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var105 sysname;
    SELECT @var105 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'IdleSkewness');
    IF @var105 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var105 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [IdleSkewness] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var106 sysname;
    SELECT @var106 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'IdleStd');
    IF @var106 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var106 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [IdleStd] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var107 sysname;
    SELECT @var107 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'IdleVariance');
    IF @var107 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var107 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [IdleVariance] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var108 sysname;
    SELECT @var108 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'MaxBwdPacketsDeltaLen');
    IF @var108 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var108 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [MaxBwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var109 sysname;
    SELECT @var109 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'MaxBwdPacketsDeltaTime');
    IF @var109 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var109 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [MaxBwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var110 sysname;
    SELECT @var110 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'MaxFwdPacketsDeltaTime');
    IF @var110 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var110 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [MaxFwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var111 sysname;
    SELECT @var111 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'MaxHeaderBytesDeltaLen');
    IF @var111 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var111 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [MaxHeaderBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var112 sysname;
    SELECT @var112 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'MaxPayloadBytesDeltaLen');
    IF @var112 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var112 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [MaxPayloadBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var113 sysname;
    SELECT @var113 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'MeanBwdHeaderBytesDeltaLen');
    IF @var113 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var113 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [MeanBwdHeaderBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var114 sysname;
    SELECT @var114 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'MeanBwdPacketsDeltaLen');
    IF @var114 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var114 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [MeanBwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var115 sysname;
    SELECT @var115 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'MeanBwdPacketsDeltaTime');
    IF @var115 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var115 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [MeanBwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var116 sysname;
    SELECT @var116 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'MeanFwdHeaderBytesDeltaLen');
    IF @var116 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var116 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [MeanFwdHeaderBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var117 sysname;
    SELECT @var117 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'MeanFwdPacketsDeltaLen');
    IF @var117 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var117 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [MeanFwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var118 sysname;
    SELECT @var118 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'MeanFwdPacketsDeltaTime');
    IF @var118 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var118 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [MeanFwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var119 sysname;
    SELECT @var119 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'MeanHeaderBytes');
    IF @var119 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var119 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [MeanHeaderBytes] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var120 sysname;
    SELECT @var120 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'MeanHeaderBytesDeltaLen');
    IF @var120 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var120 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [MeanHeaderBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var121 sysname;
    SELECT @var121 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'MeanPacketsDeltaLen');
    IF @var121 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var121 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [MeanPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var122 sysname;
    SELECT @var122 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'MeanPacketsDeltaTime');
    IF @var122 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var122 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [MeanPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var123 sysname;
    SELECT @var123 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'MeanPayloadBytesDeltaLen');
    IF @var123 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var123 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [MeanPayloadBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var124 sysname;
    SELECT @var124 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'MedianBwdPacketsDeltaLen');
    IF @var124 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var124 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [MedianBwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var125 sysname;
    SELECT @var125 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'MedianFwdPacketsDeltaLen');
    IF @var125 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var125 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [MedianFwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var126 sysname;
    SELECT @var126 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'MedianFwdPacketsDeltaTime');
    IF @var126 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var126 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [MedianFwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var127 sysname;
    SELECT @var127 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'MedianPacketsDeltaLen');
    IF @var127 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var127 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [MedianPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var128 sysname;
    SELECT @var128 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'MinFwdPacketsDeltaTime');
    IF @var128 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var128 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [MinFwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var129 sysname;
    SELECT @var129 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'MinPayloadBytesDeltaLen');
    IF @var129 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var129 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [MinPayloadBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var130 sysname;
    SELECT @var130 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'ModeBwdPacketsDeltaLen');
    IF @var130 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var130 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [ModeBwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var131 sysname;
    SELECT @var131 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'ModeFwdPacketsDeltaLen');
    IF @var131 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var131 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [ModeFwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var132 sysname;
    SELECT @var132 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'ModeFwdPacketsDeltaTime');
    IF @var132 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var132 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [ModeFwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var133 sysname;
    SELECT @var133 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'ModeFwdPayloadBytesDeltaLen');
    IF @var133 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var133 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [ModeFwdPayloadBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var134 sysname;
    SELECT @var134 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'ModePacketsDeltaLen');
    IF @var134 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var134 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [ModePacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var135 sysname;
    SELECT @var135 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'ModePacketsDeltaTime');
    IF @var135 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var135 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [ModePacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var136 sysname;
    SELECT @var136 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'ModePayloadBytesDeltaLen');
    IF @var136 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var136 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [ModePayloadBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var137 sysname;
    SELECT @var137 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'PacketIatMax');
    IF @var137 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var137 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [PacketIatMax] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var138 sysname;
    SELECT @var138 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'PacketIatMin');
    IF @var138 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var138 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [PacketIatMin] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var139 sysname;
    SELECT @var139 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'PacketIatStd');
    IF @var139 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var139 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [PacketIatStd] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var140 sysname;
    SELECT @var140 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'PacketIatTotal');
    IF @var140 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var140 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [PacketIatTotal] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var141 sysname;
    SELECT @var141 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'PacketsIatCov');
    IF @var141 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var141 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [PacketsIatCov] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var142 sysname;
    SELECT @var142 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'PacketsIatMean');
    IF @var142 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var142 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [PacketsIatMean] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var143 sysname;
    SELECT @var143 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'PacketsIatMedian');
    IF @var143 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var143 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [PacketsIatMedian] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var144 sysname;
    SELECT @var144 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'PacketsIatMode');
    IF @var144 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var144 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [PacketsIatMode] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var145 sysname;
    SELECT @var145 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'PacketsIatVariance');
    IF @var145 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var145 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [PacketsIatVariance] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var146 sysname;
    SELECT @var146 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'PacketsRate');
    IF @var146 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var146 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [PacketsRate] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var147 sysname;
    SELECT @var147 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'PayloadBytesCov');
    IF @var147 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var147 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [PayloadBytesCov] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var148 sysname;
    SELECT @var148 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'PayloadBytesMax');
    IF @var148 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var148 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [PayloadBytesMax] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var149 sysname;
    SELECT @var149 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'PayloadBytesMean');
    IF @var149 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var149 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [PayloadBytesMean] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var150 sysname;
    SELECT @var150 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'PayloadBytesMedian');
    IF @var150 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var150 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [PayloadBytesMedian] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var151 sysname;
    SELECT @var151 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'PayloadBytesSkewness');
    IF @var151 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var151 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [PayloadBytesSkewness] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var152 sysname;
    SELECT @var152 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'PayloadBytesStd');
    IF @var152 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var152 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [PayloadBytesStd] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var153 sysname;
    SELECT @var153 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'PayloadBytesVariance');
    IF @var153 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var153 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [PayloadBytesVariance] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var154 sysname;
    SELECT @var154 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'RstFlagPercentageInTotal');
    IF @var154 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var154 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [RstFlagPercentageInTotal] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var155 sysname;
    SELECT @var155 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'SkewnessBwdPacketsDeltaLen');
    IF @var155 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var155 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [SkewnessBwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var156 sysname;
    SELECT @var156 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'SkewnessBwdPacketsDeltaTime');
    IF @var156 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var156 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [SkewnessBwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var157 sysname;
    SELECT @var157 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'SkewnessFwdPacketsDeltaLen');
    IF @var157 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var157 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [SkewnessFwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var158 sysname;
    SELECT @var158 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'SkewnessFwdPacketsDeltaTime');
    IF @var158 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var158 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [SkewnessFwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var159 sysname;
    SELECT @var159 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'SkewnessHeaderBytes');
    IF @var159 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var159 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [SkewnessHeaderBytes] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var160 sysname;
    SELECT @var160 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'SkewnessPacketsDeltaLen');
    IF @var160 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var160 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [SkewnessPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var161 sysname;
    SELECT @var161 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'SkewnessPacketsDeltaTime');
    IF @var161 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var161 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [SkewnessPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var162 sysname;
    SELECT @var162 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'StdFwdPacketsDeltaTime');
    IF @var162 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var162 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [StdFwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var163 sysname;
    SELECT @var163 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'StdHeaderBytes');
    IF @var163 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var163 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [StdHeaderBytes] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var164 sysname;
    SELECT @var164 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'StdPacketsDeltaTime');
    IF @var164 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var164 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [StdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var165 sysname;
    SELECT @var165 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'SynFlagPercentageInTotal');
    IF @var165 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var165 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [SynFlagPercentageInTotal] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var166 sysname;
    SELECT @var166 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'VarianceHeaderBytes');
    IF @var166 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var166 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [VarianceHeaderBytes] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var167 sysname;
    SELECT @var167 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RawPackets]') AND [c].[name] = N'VariancePacketsDeltaTime');
    IF @var167 IS NOT NULL EXEC(N'ALTER TABLE [RawPackets] DROP CONSTRAINT [' + @var167 + '];');
    ALTER TABLE [RawPackets] ALTER COLUMN [VariancePacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    ALTER TABLE [RawPackets] ADD [DeltaStartIncompleteHandshake] bit NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var168 sysname;
    SELECT @var168 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'AckFlagCounts');
    IF @var168 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var168 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [AckFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var169 sysname;
    SELECT @var169 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'BwdAckFlagCounts');
    IF @var169 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var169 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [BwdAckFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var170 sysname;
    SELECT @var170 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'BwdBulkStateCount');
    IF @var170 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var170 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [BwdBulkStateCount] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var171 sysname;
    SELECT @var171 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'BwdFinFlagCounts');
    IF @var171 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var171 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [BwdFinFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var172 sysname;
    SELECT @var172 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'BwdInitWinBytes');
    IF @var172 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var172 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [BwdInitWinBytes] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var173 sysname;
    SELECT @var173 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'BwdPacketsCount');
    IF @var173 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var173 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [BwdPacketsCount] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var174 sysname;
    SELECT @var174 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'BwdRstFlagCounts');
    IF @var174 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var174 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [BwdRstFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var175 sysname;
    SELECT @var175 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'BwdSynFlagCounts');
    IF @var175 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var175 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [BwdSynFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var176 sysname;
    SELECT @var176 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'BwdUrgFlagCounts');
    IF @var176 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var176 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [BwdUrgFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var177 sysname;
    SELECT @var177 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'CwrFlagCounts');
    IF @var177 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var177 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [CwrFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var178 sysname;
    SELECT @var178 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'DstPort');
    IF @var178 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var178 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [DstPort] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var179 sysname;
    SELECT @var179 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'EceFlagCounts');
    IF @var179 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var179 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [EceFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var180 sysname;
    SELECT @var180 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FinFlagCounts');
    IF @var180 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var180 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FinFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var181 sysname;
    SELECT @var181 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdAckFlagCounts');
    IF @var181 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var181 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdAckFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var182 sysname;
    SELECT @var182 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdFinFlagCounts');
    IF @var182 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var182 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdFinFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var183 sysname;
    SELECT @var183 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdInitWinBytes');
    IF @var183 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var183 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdInitWinBytes] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var184 sysname;
    SELECT @var184 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdPacketsCount');
    IF @var184 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var184 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdPacketsCount] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var185 sysname;
    SELECT @var185 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdPshFlagCounts');
    IF @var185 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var185 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdPshFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var186 sysname;
    SELECT @var186 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdRstFlagCounts');
    IF @var186 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var186 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdRstFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var187 sysname;
    SELECT @var187 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdSynFlagCounts');
    IF @var187 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var187 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdSynFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var188 sysname;
    SELECT @var188 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdUrgFlagCounts');
    IF @var188 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var188 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdUrgFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var189 sysname;
    SELECT @var189 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'PacketsCount');
    IF @var189 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var189 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [PacketsCount] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var190 sysname;
    SELECT @var190 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'PshFlagCounts');
    IF @var190 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var190 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [PshFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var191 sysname;
    SELECT @var191 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'RstFlagCounts');
    IF @var191 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var191 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [RstFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var192 sysname;
    SELECT @var192 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'SubflowBwdPackets');
    IF @var192 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var192 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [SubflowBwdPackets] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var193 sysname;
    SELECT @var193 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'SubflowFwdPackets');
    IF @var193 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var193 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [SubflowFwdPackets] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var194 sysname;
    SELECT @var194 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'SynFlagCounts');
    IF @var194 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var194 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [SynFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var195 sysname;
    SELECT @var195 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'UrgFlagCounts');
    IF @var195 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var195 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [UrgFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var196 sysname;
    SELECT @var196 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'BwdTotalHeaderBytes');
    IF @var196 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var196 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [BwdTotalHeaderBytes] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var197 sysname;
    SELECT @var197 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'BwdTotalPayloadBytes');
    IF @var197 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var197 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [BwdTotalPayloadBytes] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var198 sysname;
    SELECT @var198 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdTotalHeaderBytes');
    IF @var198 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var198 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdTotalHeaderBytes] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var199 sysname;
    SELECT @var199 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdTotalPayloadBytes');
    IF @var199 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var199 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdTotalPayloadBytes] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var200 sysname;
    SELECT @var200 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'SubflowBwdBytes');
    IF @var200 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var200 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [SubflowBwdBytes] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var201 sysname;
    SELECT @var201 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'SubflowFwdBytes');
    IF @var201 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var201 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [SubflowFwdBytes] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var202 sysname;
    SELECT @var202 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'TotalHeaderBytes');
    IF @var202 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var202 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [TotalHeaderBytes] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var203 sysname;
    SELECT @var203 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'TotalPayloadBytes');
    IF @var203 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var203 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [TotalPayloadBytes] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var204 sysname;
    SELECT @var204 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'AckFlagPercentageInTotal');
    IF @var204 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var204 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [AckFlagPercentageInTotal] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var205 sysname;
    SELECT @var205 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'ActiveCov');
    IF @var205 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var205 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [ActiveCov] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var206 sysname;
    SELECT @var206 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'ActiveMax');
    IF @var206 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var206 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [ActiveMax] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var207 sysname;
    SELECT @var207 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'ActiveMean');
    IF @var207 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var207 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [ActiveMean] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var208 sysname;
    SELECT @var208 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'ActiveMedian');
    IF @var208 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var208 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [ActiveMedian] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var209 sysname;
    SELECT @var209 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'ActiveMin');
    IF @var209 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var209 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [ActiveMin] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var210 sysname;
    SELECT @var210 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'ActiveMode');
    IF @var210 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var210 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [ActiveMode] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var211 sysname;
    SELECT @var211 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'ActiveSkewness');
    IF @var211 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var211 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [ActiveSkewness] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var212 sysname;
    SELECT @var212 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'ActiveStd');
    IF @var212 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var212 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [ActiveStd] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var213 sysname;
    SELECT @var213 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'ActiveVariance');
    IF @var213 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var213 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [ActiveVariance] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var214 sysname;
    SELECT @var214 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'AvgBwdBulkRate');
    IF @var214 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var214 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [AvgBwdBulkRate] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var215 sysname;
    SELECT @var215 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'AvgSegmentSize');
    IF @var215 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var215 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [AvgSegmentSize] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var216 sysname;
    SELECT @var216 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'BwdAckFlagPercentageInBwdPackets');
    IF @var216 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var216 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [BwdAckFlagPercentageInBwdPackets] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var217 sysname;
    SELECT @var217 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'BwdAckFlagPercentageInTotal');
    IF @var217 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var217 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [BwdAckFlagPercentageInTotal] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var218 sysname;
    SELECT @var218 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'BwdBulkDuration');
    IF @var218 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var218 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [BwdBulkDuration] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var219 sysname;
    SELECT @var219 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'BwdBytesRate');
    IF @var219 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var219 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [BwdBytesRate] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var220 sysname;
    SELECT @var220 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'BwdMeanHeaderBytes');
    IF @var220 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var220 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [BwdMeanHeaderBytes] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var221 sysname;
    SELECT @var221 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'BwdPacketsIatMax');
    IF @var221 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var221 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [BwdPacketsIatMax] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var222 sysname;
    SELECT @var222 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'BwdPacketsIatMean');
    IF @var222 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var222 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [BwdPacketsIatMean] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var223 sysname;
    SELECT @var223 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'BwdPacketsIatMedian');
    IF @var223 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var223 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [BwdPacketsIatMedian] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var224 sysname;
    SELECT @var224 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'BwdPacketsIatMin');
    IF @var224 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var224 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [BwdPacketsIatMin] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var225 sysname;
    SELECT @var225 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'BwdPacketsIatMode');
    IF @var225 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var225 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [BwdPacketsIatMode] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var226 sysname;
    SELECT @var226 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'BwdPacketsIatStd');
    IF @var226 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var226 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [BwdPacketsIatStd] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var227 sysname;
    SELECT @var227 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'BwdPacketsIatTotal');
    IF @var227 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var227 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [BwdPacketsIatTotal] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var228 sysname;
    SELECT @var228 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'BwdPacketsIatVariance');
    IF @var228 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var228 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [BwdPacketsIatVariance] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var229 sysname;
    SELECT @var229 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'BwdPacketsRate');
    IF @var229 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var229 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [BwdPacketsRate] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var230 sysname;
    SELECT @var230 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'BwdPayloadBytesMax');
    IF @var230 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var230 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [BwdPayloadBytesMax] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var231 sysname;
    SELECT @var231 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'BwdPayloadBytesMedian');
    IF @var231 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var231 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [BwdPayloadBytesMedian] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var232 sysname;
    SELECT @var232 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'BwdPayloadBytesStd');
    IF @var232 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var232 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [BwdPayloadBytesStd] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var233 sysname;
    SELECT @var233 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'BytesRate');
    IF @var233 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var233 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [BytesRate] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var234 sysname;
    SELECT @var234 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'CovBwdHeaderBytesDeltaLen');
    IF @var234 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var234 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [CovBwdHeaderBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var235 sysname;
    SELECT @var235 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'CovBwdPacketsDeltaLen');
    IF @var235 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var235 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [CovBwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var236 sysname;
    SELECT @var236 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'CovBwdPacketsDeltaTime');
    IF @var236 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var236 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [CovBwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var237 sysname;
    SELECT @var237 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'CovFwdHeaderBytesDeltaLen');
    IF @var237 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var237 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [CovFwdHeaderBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var238 sysname;
    SELECT @var238 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'CovFwdPacketsDeltaLen');
    IF @var238 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var238 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [CovFwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var239 sysname;
    SELECT @var239 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'CovFwdPacketsDeltaTime');
    IF @var239 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var239 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [CovFwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var240 sysname;
    SELECT @var240 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'CovHeaderBytesDeltaLen');
    IF @var240 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var240 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [CovHeaderBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var241 sysname;
    SELECT @var241 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'CovPacketsDeltaLen');
    IF @var241 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var241 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [CovPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var242 sysname;
    SELECT @var242 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'DeltaStart');
    IF @var242 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var242 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [DeltaStart] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var243 sysname;
    SELECT @var243 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'DownUpRate');
    IF @var243 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var243 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [DownUpRate] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var244 sysname;
    SELECT @var244 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'Duration');
    IF @var244 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var244 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [Duration] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var245 sysname;
    SELECT @var245 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdAckFlagPercentageInFwdPackets');
    IF @var245 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var245 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdAckFlagPercentageInFwdPackets] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var246 sysname;
    SELECT @var246 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdAckFlagPercentageInTotal');
    IF @var246 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var246 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdAckFlagPercentageInTotal] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var247 sysname;
    SELECT @var247 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdAvgSegmentSize');
    IF @var247 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var247 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdAvgSegmentSize] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var248 sysname;
    SELECT @var248 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdBytesRate');
    IF @var248 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var248 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdBytesRate] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var249 sysname;
    SELECT @var249 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdMeanHeaderBytes');
    IF @var249 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var249 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdMeanHeaderBytes] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var250 sysname;
    SELECT @var250 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdPacketsIatCov');
    IF @var250 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var250 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdPacketsIatCov] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var251 sysname;
    SELECT @var251 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdPacketsIatMax');
    IF @var251 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var251 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdPacketsIatMax] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var252 sysname;
    SELECT @var252 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdPacketsIatMean');
    IF @var252 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var252 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdPacketsIatMean] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var253 sysname;
    SELECT @var253 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdPacketsIatMedian');
    IF @var253 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var253 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdPacketsIatMedian] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var254 sysname;
    SELECT @var254 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdPacketsIatMin');
    IF @var254 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var254 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdPacketsIatMin] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var255 sysname;
    SELECT @var255 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdPacketsIatMode');
    IF @var255 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var255 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdPacketsIatMode] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var256 sysname;
    SELECT @var256 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdPacketsIatSkewness');
    IF @var256 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var256 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdPacketsIatSkewness] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var257 sysname;
    SELECT @var257 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdPacketsIatStd');
    IF @var257 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var257 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdPacketsIatStd] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var258 sysname;
    SELECT @var258 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdPacketsIatTotal');
    IF @var258 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var258 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdPacketsIatTotal] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var259 sysname;
    SELECT @var259 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdPacketsIatVariance');
    IF @var259 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var259 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdPacketsIatVariance] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var260 sysname;
    SELECT @var260 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdPacketsRate');
    IF @var260 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var260 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdPacketsRate] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var261 sysname;
    SELECT @var261 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdPayloadBytesCov');
    IF @var261 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var261 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdPayloadBytesCov] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var262 sysname;
    SELECT @var262 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdPayloadBytesMax');
    IF @var262 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var262 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdPayloadBytesMax] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var263 sysname;
    SELECT @var263 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdPayloadBytesMedian');
    IF @var263 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var263 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdPayloadBytesMedian] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var264 sysname;
    SELECT @var264 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdPayloadBytesSkewness');
    IF @var264 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var264 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdPayloadBytesSkewness] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var265 sysname;
    SELECT @var265 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'FwdPayloadBytesStd');
    IF @var265 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var265 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [FwdPayloadBytesStd] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var266 sysname;
    SELECT @var266 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'HandshakeDuration');
    IF @var266 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var266 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [HandshakeDuration] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var267 sysname;
    SELECT @var267 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'IdleCov');
    IF @var267 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var267 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [IdleCov] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var268 sysname;
    SELECT @var268 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'IdleMax');
    IF @var268 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var268 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [IdleMax] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var269 sysname;
    SELECT @var269 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'IdleMean');
    IF @var269 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var269 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [IdleMean] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var270 sysname;
    SELECT @var270 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'IdleMedian');
    IF @var270 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var270 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [IdleMedian] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var271 sysname;
    SELECT @var271 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'IdleMin');
    IF @var271 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var271 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [IdleMin] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var272 sysname;
    SELECT @var272 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'IdleMode');
    IF @var272 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var272 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [IdleMode] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var273 sysname;
    SELECT @var273 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'IdleSkewness');
    IF @var273 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var273 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [IdleSkewness] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var274 sysname;
    SELECT @var274 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'IdleStd');
    IF @var274 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var274 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [IdleStd] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var275 sysname;
    SELECT @var275 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'IdleVariance');
    IF @var275 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var275 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [IdleVariance] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var276 sysname;
    SELECT @var276 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'MaxBwdPacketsDeltaLen');
    IF @var276 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var276 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [MaxBwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var277 sysname;
    SELECT @var277 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'MaxBwdPacketsDeltaTime');
    IF @var277 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var277 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [MaxBwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var278 sysname;
    SELECT @var278 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'MaxFwdPacketsDeltaTime');
    IF @var278 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var278 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [MaxFwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var279 sysname;
    SELECT @var279 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'MaxHeaderBytesDeltaLen');
    IF @var279 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var279 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [MaxHeaderBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var280 sysname;
    SELECT @var280 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'MaxPayloadBytesDeltaLen');
    IF @var280 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var280 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [MaxPayloadBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var281 sysname;
    SELECT @var281 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'MeanBwdHeaderBytesDeltaLen');
    IF @var281 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var281 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [MeanBwdHeaderBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var282 sysname;
    SELECT @var282 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'MeanBwdPacketsDeltaLen');
    IF @var282 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var282 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [MeanBwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var283 sysname;
    SELECT @var283 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'MeanBwdPacketsDeltaTime');
    IF @var283 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var283 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [MeanBwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var284 sysname;
    SELECT @var284 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'MeanFwdHeaderBytesDeltaLen');
    IF @var284 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var284 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [MeanFwdHeaderBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var285 sysname;
    SELECT @var285 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'MeanFwdPacketsDeltaLen');
    IF @var285 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var285 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [MeanFwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var286 sysname;
    SELECT @var286 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'MeanFwdPacketsDeltaTime');
    IF @var286 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var286 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [MeanFwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var287 sysname;
    SELECT @var287 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'MeanHeaderBytes');
    IF @var287 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var287 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [MeanHeaderBytes] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var288 sysname;
    SELECT @var288 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'MeanHeaderBytesDeltaLen');
    IF @var288 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var288 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [MeanHeaderBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var289 sysname;
    SELECT @var289 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'MeanPacketsDeltaLen');
    IF @var289 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var289 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [MeanPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var290 sysname;
    SELECT @var290 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'MeanPacketsDeltaTime');
    IF @var290 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var290 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [MeanPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var291 sysname;
    SELECT @var291 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'MeanPayloadBytesDeltaLen');
    IF @var291 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var291 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [MeanPayloadBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var292 sysname;
    SELECT @var292 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'MedianBwdPacketsDeltaLen');
    IF @var292 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var292 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [MedianBwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var293 sysname;
    SELECT @var293 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'MedianFwdPacketsDeltaLen');
    IF @var293 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var293 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [MedianFwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var294 sysname;
    SELECT @var294 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'MedianFwdPacketsDeltaTime');
    IF @var294 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var294 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [MedianFwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var295 sysname;
    SELECT @var295 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'MedianPacketsDeltaLen');
    IF @var295 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var295 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [MedianPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var296 sysname;
    SELECT @var296 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'MinFwdPacketsDeltaTime');
    IF @var296 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var296 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [MinFwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var297 sysname;
    SELECT @var297 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'MinPayloadBytesDeltaLen');
    IF @var297 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var297 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [MinPayloadBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var298 sysname;
    SELECT @var298 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'ModeBwdPacketsDeltaLen');
    IF @var298 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var298 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [ModeBwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var299 sysname;
    SELECT @var299 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'ModeFwdPacketsDeltaLen');
    IF @var299 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var299 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [ModeFwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var300 sysname;
    SELECT @var300 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'ModeFwdPacketsDeltaTime');
    IF @var300 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var300 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [ModeFwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var301 sysname;
    SELECT @var301 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'ModeFwdPayloadBytesDeltaLen');
    IF @var301 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var301 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [ModeFwdPayloadBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var302 sysname;
    SELECT @var302 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'ModePacketsDeltaLen');
    IF @var302 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var302 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [ModePacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var303 sysname;
    SELECT @var303 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'ModePacketsDeltaTime');
    IF @var303 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var303 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [ModePacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var304 sysname;
    SELECT @var304 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'ModePayloadBytesDeltaLen');
    IF @var304 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var304 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [ModePayloadBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var305 sysname;
    SELECT @var305 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'PacketIatMax');
    IF @var305 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var305 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [PacketIatMax] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var306 sysname;
    SELECT @var306 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'PacketIatMin');
    IF @var306 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var306 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [PacketIatMin] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var307 sysname;
    SELECT @var307 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'PacketIatStd');
    IF @var307 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var307 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [PacketIatStd] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var308 sysname;
    SELECT @var308 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'PacketIatTotal');
    IF @var308 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var308 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [PacketIatTotal] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var309 sysname;
    SELECT @var309 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'PacketsIatCov');
    IF @var309 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var309 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [PacketsIatCov] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var310 sysname;
    SELECT @var310 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'PacketsIatMean');
    IF @var310 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var310 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [PacketsIatMean] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var311 sysname;
    SELECT @var311 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'PacketsIatMedian');
    IF @var311 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var311 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [PacketsIatMedian] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var312 sysname;
    SELECT @var312 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'PacketsIatMode');
    IF @var312 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var312 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [PacketsIatMode] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var313 sysname;
    SELECT @var313 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'PacketsIatVariance');
    IF @var313 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var313 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [PacketsIatVariance] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var314 sysname;
    SELECT @var314 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'PacketsRate');
    IF @var314 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var314 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [PacketsRate] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var315 sysname;
    SELECT @var315 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'PayloadBytesCov');
    IF @var315 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var315 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [PayloadBytesCov] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var316 sysname;
    SELECT @var316 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'PayloadBytesMax');
    IF @var316 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var316 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [PayloadBytesMax] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var317 sysname;
    SELECT @var317 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'PayloadBytesMean');
    IF @var317 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var317 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [PayloadBytesMean] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var318 sysname;
    SELECT @var318 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'PayloadBytesMedian');
    IF @var318 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var318 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [PayloadBytesMedian] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var319 sysname;
    SELECT @var319 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'PayloadBytesSkewness');
    IF @var319 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var319 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [PayloadBytesSkewness] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var320 sysname;
    SELECT @var320 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'PayloadBytesStd');
    IF @var320 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var320 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [PayloadBytesStd] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var321 sysname;
    SELECT @var321 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'PayloadBytesVariance');
    IF @var321 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var321 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [PayloadBytesVariance] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var322 sysname;
    SELECT @var322 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'RstFlagPercentageInTotal');
    IF @var322 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var322 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [RstFlagPercentageInTotal] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var323 sysname;
    SELECT @var323 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'SkewnessBwdPacketsDeltaLen');
    IF @var323 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var323 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [SkewnessBwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var324 sysname;
    SELECT @var324 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'SkewnessBwdPacketsDeltaTime');
    IF @var324 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var324 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [SkewnessBwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var325 sysname;
    SELECT @var325 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'SkewnessFwdPacketsDeltaLen');
    IF @var325 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var325 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [SkewnessFwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var326 sysname;
    SELECT @var326 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'SkewnessFwdPacketsDeltaTime');
    IF @var326 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var326 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [SkewnessFwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var327 sysname;
    SELECT @var327 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'SkewnessHeaderBytes');
    IF @var327 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var327 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [SkewnessHeaderBytes] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var328 sysname;
    SELECT @var328 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'SkewnessPacketsDeltaLen');
    IF @var328 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var328 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [SkewnessPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var329 sysname;
    SELECT @var329 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'SkewnessPacketsDeltaTime');
    IF @var329 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var329 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [SkewnessPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var330 sysname;
    SELECT @var330 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'StdFwdPacketsDeltaTime');
    IF @var330 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var330 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [StdFwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var331 sysname;
    SELECT @var331 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'StdHeaderBytes');
    IF @var331 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var331 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [StdHeaderBytes] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var332 sysname;
    SELECT @var332 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'StdPacketsDeltaTime');
    IF @var332 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var332 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [StdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var333 sysname;
    SELECT @var333 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'SynFlagPercentageInTotal');
    IF @var333 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var333 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [SynFlagPercentageInTotal] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var334 sysname;
    SELECT @var334 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'VarianceHeaderBytes');
    IF @var334 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var334 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [VarianceHeaderBytes] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var335 sysname;
    SELECT @var335 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Benign_Table]') AND [c].[name] = N'VariancePacketsDeltaTime');
    IF @var335 IS NOT NULL EXEC(N'ALTER TABLE [Benign_Table] DROP CONSTRAINT [' + @var335 + '];');
    ALTER TABLE [Benign_Table] ALTER COLUMN [VariancePacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    ALTER TABLE [Benign_Table] ADD [DeltaStartIncompleteHandshake] bit NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var336 sysname;
    SELECT @var336 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'AckFlagCounts');
    IF @var336 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var336 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [AckFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var337 sysname;
    SELECT @var337 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'BwdAckFlagCounts');
    IF @var337 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var337 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [BwdAckFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var338 sysname;
    SELECT @var338 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'BwdBulkStateCount');
    IF @var338 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var338 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [BwdBulkStateCount] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var339 sysname;
    SELECT @var339 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'BwdFinFlagCounts');
    IF @var339 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var339 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [BwdFinFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var340 sysname;
    SELECT @var340 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'BwdInitWinBytes');
    IF @var340 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var340 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [BwdInitWinBytes] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var341 sysname;
    SELECT @var341 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'BwdPacketsCount');
    IF @var341 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var341 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [BwdPacketsCount] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var342 sysname;
    SELECT @var342 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'BwdRstFlagCounts');
    IF @var342 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var342 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [BwdRstFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var343 sysname;
    SELECT @var343 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'BwdSynFlagCounts');
    IF @var343 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var343 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [BwdSynFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var344 sysname;
    SELECT @var344 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'BwdUrgFlagCounts');
    IF @var344 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var344 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [BwdUrgFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var345 sysname;
    SELECT @var345 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'CwrFlagCounts');
    IF @var345 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var345 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [CwrFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var346 sysname;
    SELECT @var346 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'DstPort');
    IF @var346 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var346 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [DstPort] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var347 sysname;
    SELECT @var347 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'EceFlagCounts');
    IF @var347 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var347 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [EceFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var348 sysname;
    SELECT @var348 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FinFlagCounts');
    IF @var348 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var348 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FinFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var349 sysname;
    SELECT @var349 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdAckFlagCounts');
    IF @var349 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var349 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdAckFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var350 sysname;
    SELECT @var350 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdFinFlagCounts');
    IF @var350 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var350 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdFinFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var351 sysname;
    SELECT @var351 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdInitWinBytes');
    IF @var351 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var351 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdInitWinBytes] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var352 sysname;
    SELECT @var352 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdPacketsCount');
    IF @var352 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var352 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdPacketsCount] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var353 sysname;
    SELECT @var353 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdPshFlagCounts');
    IF @var353 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var353 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdPshFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var354 sysname;
    SELECT @var354 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdRstFlagCounts');
    IF @var354 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var354 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdRstFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var355 sysname;
    SELECT @var355 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdSynFlagCounts');
    IF @var355 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var355 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdSynFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var356 sysname;
    SELECT @var356 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdUrgFlagCounts');
    IF @var356 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var356 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdUrgFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var357 sysname;
    SELECT @var357 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'PacketsCount');
    IF @var357 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var357 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [PacketsCount] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var358 sysname;
    SELECT @var358 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'PshFlagCounts');
    IF @var358 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var358 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [PshFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var359 sysname;
    SELECT @var359 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'RstFlagCounts');
    IF @var359 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var359 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [RstFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var360 sysname;
    SELECT @var360 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'SubflowBwdPackets');
    IF @var360 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var360 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [SubflowBwdPackets] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var361 sysname;
    SELECT @var361 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'SubflowFwdPackets');
    IF @var361 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var361 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [SubflowFwdPackets] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var362 sysname;
    SELECT @var362 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'SynFlagCounts');
    IF @var362 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var362 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [SynFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var363 sysname;
    SELECT @var363 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'UrgFlagCounts');
    IF @var363 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var363 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [UrgFlagCounts] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var364 sysname;
    SELECT @var364 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'BwdTotalHeaderBytes');
    IF @var364 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var364 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [BwdTotalHeaderBytes] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var365 sysname;
    SELECT @var365 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'BwdTotalPayloadBytes');
    IF @var365 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var365 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [BwdTotalPayloadBytes] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var366 sysname;
    SELECT @var366 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdTotalHeaderBytes');
    IF @var366 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var366 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdTotalHeaderBytes] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var367 sysname;
    SELECT @var367 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdTotalPayloadBytes');
    IF @var367 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var367 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdTotalPayloadBytes] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var368 sysname;
    SELECT @var368 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'SubflowBwdBytes');
    IF @var368 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var368 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [SubflowBwdBytes] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var369 sysname;
    SELECT @var369 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'SubflowFwdBytes');
    IF @var369 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var369 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [SubflowFwdBytes] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var370 sysname;
    SELECT @var370 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'TotalHeaderBytes');
    IF @var370 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var370 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [TotalHeaderBytes] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var371 sysname;
    SELECT @var371 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'TotalPayloadBytes');
    IF @var371 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var371 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [TotalPayloadBytes] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var372 sysname;
    SELECT @var372 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'AckFlagPercentageInTotal');
    IF @var372 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var372 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [AckFlagPercentageInTotal] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var373 sysname;
    SELECT @var373 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'ActiveCov');
    IF @var373 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var373 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [ActiveCov] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var374 sysname;
    SELECT @var374 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'ActiveMax');
    IF @var374 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var374 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [ActiveMax] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var375 sysname;
    SELECT @var375 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'ActiveMean');
    IF @var375 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var375 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [ActiveMean] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var376 sysname;
    SELECT @var376 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'ActiveMedian');
    IF @var376 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var376 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [ActiveMedian] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var377 sysname;
    SELECT @var377 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'ActiveMin');
    IF @var377 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var377 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [ActiveMin] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var378 sysname;
    SELECT @var378 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'ActiveMode');
    IF @var378 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var378 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [ActiveMode] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var379 sysname;
    SELECT @var379 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'ActiveSkewness');
    IF @var379 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var379 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [ActiveSkewness] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var380 sysname;
    SELECT @var380 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'ActiveStd');
    IF @var380 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var380 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [ActiveStd] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var381 sysname;
    SELECT @var381 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'ActiveVariance');
    IF @var381 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var381 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [ActiveVariance] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var382 sysname;
    SELECT @var382 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'AvgBwdBulkRate');
    IF @var382 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var382 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [AvgBwdBulkRate] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var383 sysname;
    SELECT @var383 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'AvgSegmentSize');
    IF @var383 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var383 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [AvgSegmentSize] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var384 sysname;
    SELECT @var384 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'BwdAckFlagPercentageInBwdPackets');
    IF @var384 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var384 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [BwdAckFlagPercentageInBwdPackets] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var385 sysname;
    SELECT @var385 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'BwdAckFlagPercentageInTotal');
    IF @var385 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var385 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [BwdAckFlagPercentageInTotal] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var386 sysname;
    SELECT @var386 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'BwdBulkDuration');
    IF @var386 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var386 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [BwdBulkDuration] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var387 sysname;
    SELECT @var387 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'BwdBytesRate');
    IF @var387 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var387 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [BwdBytesRate] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var388 sysname;
    SELECT @var388 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'BwdMeanHeaderBytes');
    IF @var388 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var388 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [BwdMeanHeaderBytes] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var389 sysname;
    SELECT @var389 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'BwdPacketsIatMax');
    IF @var389 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var389 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [BwdPacketsIatMax] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var390 sysname;
    SELECT @var390 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'BwdPacketsIatMean');
    IF @var390 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var390 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [BwdPacketsIatMean] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var391 sysname;
    SELECT @var391 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'BwdPacketsIatMedian');
    IF @var391 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var391 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [BwdPacketsIatMedian] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var392 sysname;
    SELECT @var392 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'BwdPacketsIatMin');
    IF @var392 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var392 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [BwdPacketsIatMin] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var393 sysname;
    SELECT @var393 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'BwdPacketsIatMode');
    IF @var393 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var393 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [BwdPacketsIatMode] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var394 sysname;
    SELECT @var394 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'BwdPacketsIatStd');
    IF @var394 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var394 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [BwdPacketsIatStd] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var395 sysname;
    SELECT @var395 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'BwdPacketsIatTotal');
    IF @var395 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var395 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [BwdPacketsIatTotal] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var396 sysname;
    SELECT @var396 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'BwdPacketsIatVariance');
    IF @var396 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var396 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [BwdPacketsIatVariance] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var397 sysname;
    SELECT @var397 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'BwdPacketsRate');
    IF @var397 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var397 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [BwdPacketsRate] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var398 sysname;
    SELECT @var398 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'BwdPayloadBytesMax');
    IF @var398 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var398 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [BwdPayloadBytesMax] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var399 sysname;
    SELECT @var399 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'BwdPayloadBytesMedian');
    IF @var399 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var399 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [BwdPayloadBytesMedian] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var400 sysname;
    SELECT @var400 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'BwdPayloadBytesStd');
    IF @var400 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var400 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [BwdPayloadBytesStd] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var401 sysname;
    SELECT @var401 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'BytesRate');
    IF @var401 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var401 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [BytesRate] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var402 sysname;
    SELECT @var402 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'CovBwdHeaderBytesDeltaLen');
    IF @var402 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var402 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [CovBwdHeaderBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var403 sysname;
    SELECT @var403 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'CovBwdPacketsDeltaLen');
    IF @var403 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var403 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [CovBwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var404 sysname;
    SELECT @var404 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'CovBwdPacketsDeltaTime');
    IF @var404 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var404 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [CovBwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var405 sysname;
    SELECT @var405 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'CovFwdHeaderBytesDeltaLen');
    IF @var405 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var405 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [CovFwdHeaderBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var406 sysname;
    SELECT @var406 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'CovFwdPacketsDeltaLen');
    IF @var406 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var406 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [CovFwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var407 sysname;
    SELECT @var407 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'CovFwdPacketsDeltaTime');
    IF @var407 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var407 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [CovFwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var408 sysname;
    SELECT @var408 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'CovHeaderBytesDeltaLen');
    IF @var408 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var408 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [CovHeaderBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var409 sysname;
    SELECT @var409 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'CovPacketsDeltaLen');
    IF @var409 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var409 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [CovPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var410 sysname;
    SELECT @var410 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'DeltaStart');
    IF @var410 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var410 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [DeltaStart] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var411 sysname;
    SELECT @var411 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'DownUpRate');
    IF @var411 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var411 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [DownUpRate] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var412 sysname;
    SELECT @var412 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'Duration');
    IF @var412 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var412 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [Duration] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var413 sysname;
    SELECT @var413 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdAckFlagPercentageInFwdPackets');
    IF @var413 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var413 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdAckFlagPercentageInFwdPackets] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var414 sysname;
    SELECT @var414 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdAckFlagPercentageInTotal');
    IF @var414 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var414 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdAckFlagPercentageInTotal] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var415 sysname;
    SELECT @var415 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdAvgSegmentSize');
    IF @var415 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var415 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdAvgSegmentSize] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var416 sysname;
    SELECT @var416 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdBytesRate');
    IF @var416 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var416 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdBytesRate] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var417 sysname;
    SELECT @var417 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdMeanHeaderBytes');
    IF @var417 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var417 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdMeanHeaderBytes] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var418 sysname;
    SELECT @var418 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdPacketsIatCov');
    IF @var418 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var418 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdPacketsIatCov] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var419 sysname;
    SELECT @var419 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdPacketsIatMax');
    IF @var419 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var419 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdPacketsIatMax] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var420 sysname;
    SELECT @var420 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdPacketsIatMean');
    IF @var420 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var420 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdPacketsIatMean] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var421 sysname;
    SELECT @var421 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdPacketsIatMedian');
    IF @var421 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var421 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdPacketsIatMedian] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var422 sysname;
    SELECT @var422 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdPacketsIatMin');
    IF @var422 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var422 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdPacketsIatMin] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var423 sysname;
    SELECT @var423 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdPacketsIatMode');
    IF @var423 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var423 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdPacketsIatMode] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var424 sysname;
    SELECT @var424 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdPacketsIatSkewness');
    IF @var424 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var424 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdPacketsIatSkewness] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var425 sysname;
    SELECT @var425 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdPacketsIatStd');
    IF @var425 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var425 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdPacketsIatStd] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var426 sysname;
    SELECT @var426 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdPacketsIatTotal');
    IF @var426 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var426 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdPacketsIatTotal] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var427 sysname;
    SELECT @var427 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdPacketsIatVariance');
    IF @var427 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var427 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdPacketsIatVariance] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var428 sysname;
    SELECT @var428 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdPacketsRate');
    IF @var428 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var428 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdPacketsRate] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var429 sysname;
    SELECT @var429 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdPayloadBytesCov');
    IF @var429 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var429 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdPayloadBytesCov] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var430 sysname;
    SELECT @var430 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdPayloadBytesMax');
    IF @var430 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var430 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdPayloadBytesMax] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var431 sysname;
    SELECT @var431 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdPayloadBytesMedian');
    IF @var431 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var431 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdPayloadBytesMedian] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var432 sysname;
    SELECT @var432 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdPayloadBytesSkewness');
    IF @var432 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var432 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdPayloadBytesSkewness] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var433 sysname;
    SELECT @var433 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'FwdPayloadBytesStd');
    IF @var433 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var433 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [FwdPayloadBytesStd] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var434 sysname;
    SELECT @var434 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'HandshakeDuration');
    IF @var434 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var434 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [HandshakeDuration] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var435 sysname;
    SELECT @var435 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'IdleCov');
    IF @var435 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var435 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [IdleCov] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var436 sysname;
    SELECT @var436 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'IdleMax');
    IF @var436 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var436 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [IdleMax] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var437 sysname;
    SELECT @var437 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'IdleMean');
    IF @var437 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var437 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [IdleMean] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var438 sysname;
    SELECT @var438 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'IdleMedian');
    IF @var438 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var438 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [IdleMedian] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var439 sysname;
    SELECT @var439 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'IdleMin');
    IF @var439 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var439 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [IdleMin] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var440 sysname;
    SELECT @var440 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'IdleMode');
    IF @var440 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var440 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [IdleMode] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var441 sysname;
    SELECT @var441 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'IdleSkewness');
    IF @var441 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var441 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [IdleSkewness] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var442 sysname;
    SELECT @var442 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'IdleStd');
    IF @var442 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var442 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [IdleStd] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var443 sysname;
    SELECT @var443 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'IdleVariance');
    IF @var443 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var443 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [IdleVariance] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var444 sysname;
    SELECT @var444 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'MaxBwdPacketsDeltaLen');
    IF @var444 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var444 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [MaxBwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var445 sysname;
    SELECT @var445 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'MaxBwdPacketsDeltaTime');
    IF @var445 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var445 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [MaxBwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var446 sysname;
    SELECT @var446 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'MaxFwdPacketsDeltaTime');
    IF @var446 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var446 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [MaxFwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var447 sysname;
    SELECT @var447 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'MaxHeaderBytesDeltaLen');
    IF @var447 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var447 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [MaxHeaderBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var448 sysname;
    SELECT @var448 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'MaxPayloadBytesDeltaLen');
    IF @var448 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var448 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [MaxPayloadBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var449 sysname;
    SELECT @var449 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'MeanBwdHeaderBytesDeltaLen');
    IF @var449 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var449 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [MeanBwdHeaderBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var450 sysname;
    SELECT @var450 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'MeanBwdPacketsDeltaLen');
    IF @var450 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var450 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [MeanBwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var451 sysname;
    SELECT @var451 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'MeanBwdPacketsDeltaTime');
    IF @var451 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var451 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [MeanBwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var452 sysname;
    SELECT @var452 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'MeanFwdHeaderBytesDeltaLen');
    IF @var452 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var452 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [MeanFwdHeaderBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var453 sysname;
    SELECT @var453 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'MeanFwdPacketsDeltaLen');
    IF @var453 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var453 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [MeanFwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var454 sysname;
    SELECT @var454 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'MeanFwdPacketsDeltaTime');
    IF @var454 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var454 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [MeanFwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var455 sysname;
    SELECT @var455 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'MeanHeaderBytes');
    IF @var455 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var455 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [MeanHeaderBytes] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var456 sysname;
    SELECT @var456 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'MeanHeaderBytesDeltaLen');
    IF @var456 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var456 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [MeanHeaderBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var457 sysname;
    SELECT @var457 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'MeanPacketsDeltaLen');
    IF @var457 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var457 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [MeanPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var458 sysname;
    SELECT @var458 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'MeanPacketsDeltaTime');
    IF @var458 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var458 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [MeanPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var459 sysname;
    SELECT @var459 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'MeanPayloadBytesDeltaLen');
    IF @var459 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var459 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [MeanPayloadBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var460 sysname;
    SELECT @var460 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'MedianBwdPacketsDeltaLen');
    IF @var460 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var460 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [MedianBwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var461 sysname;
    SELECT @var461 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'MedianFwdPacketsDeltaLen');
    IF @var461 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var461 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [MedianFwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var462 sysname;
    SELECT @var462 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'MedianFwdPacketsDeltaTime');
    IF @var462 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var462 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [MedianFwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var463 sysname;
    SELECT @var463 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'MedianPacketsDeltaLen');
    IF @var463 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var463 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [MedianPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var464 sysname;
    SELECT @var464 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'MinFwdPacketsDeltaTime');
    IF @var464 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var464 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [MinFwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var465 sysname;
    SELECT @var465 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'MinPayloadBytesDeltaLen');
    IF @var465 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var465 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [MinPayloadBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var466 sysname;
    SELECT @var466 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'ModeBwdPacketsDeltaLen');
    IF @var466 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var466 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [ModeBwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var467 sysname;
    SELECT @var467 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'ModeFwdPacketsDeltaLen');
    IF @var467 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var467 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [ModeFwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var468 sysname;
    SELECT @var468 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'ModeFwdPacketsDeltaTime');
    IF @var468 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var468 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [ModeFwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var469 sysname;
    SELECT @var469 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'ModeFwdPayloadBytesDeltaLen');
    IF @var469 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var469 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [ModeFwdPayloadBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var470 sysname;
    SELECT @var470 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'ModePacketsDeltaLen');
    IF @var470 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var470 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [ModePacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var471 sysname;
    SELECT @var471 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'ModePacketsDeltaTime');
    IF @var471 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var471 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [ModePacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var472 sysname;
    SELECT @var472 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'ModePayloadBytesDeltaLen');
    IF @var472 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var472 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [ModePayloadBytesDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var473 sysname;
    SELECT @var473 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'PacketIatMax');
    IF @var473 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var473 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [PacketIatMax] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var474 sysname;
    SELECT @var474 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'PacketIatMin');
    IF @var474 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var474 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [PacketIatMin] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var475 sysname;
    SELECT @var475 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'PacketIatStd');
    IF @var475 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var475 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [PacketIatStd] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var476 sysname;
    SELECT @var476 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'PacketIatTotal');
    IF @var476 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var476 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [PacketIatTotal] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var477 sysname;
    SELECT @var477 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'PacketsIatCov');
    IF @var477 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var477 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [PacketsIatCov] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var478 sysname;
    SELECT @var478 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'PacketsIatMean');
    IF @var478 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var478 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [PacketsIatMean] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var479 sysname;
    SELECT @var479 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'PacketsIatMedian');
    IF @var479 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var479 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [PacketsIatMedian] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var480 sysname;
    SELECT @var480 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'PacketsIatMode');
    IF @var480 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var480 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [PacketsIatMode] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var481 sysname;
    SELECT @var481 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'PacketsIatVariance');
    IF @var481 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var481 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [PacketsIatVariance] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var482 sysname;
    SELECT @var482 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'PacketsRate');
    IF @var482 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var482 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [PacketsRate] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var483 sysname;
    SELECT @var483 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'PayloadBytesCov');
    IF @var483 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var483 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [PayloadBytesCov] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var484 sysname;
    SELECT @var484 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'PayloadBytesMax');
    IF @var484 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var484 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [PayloadBytesMax] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var485 sysname;
    SELECT @var485 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'PayloadBytesMean');
    IF @var485 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var485 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [PayloadBytesMean] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var486 sysname;
    SELECT @var486 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'PayloadBytesMedian');
    IF @var486 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var486 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [PayloadBytesMedian] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var487 sysname;
    SELECT @var487 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'PayloadBytesSkewness');
    IF @var487 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var487 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [PayloadBytesSkewness] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var488 sysname;
    SELECT @var488 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'PayloadBytesStd');
    IF @var488 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var488 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [PayloadBytesStd] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var489 sysname;
    SELECT @var489 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'PayloadBytesVariance');
    IF @var489 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var489 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [PayloadBytesVariance] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var490 sysname;
    SELECT @var490 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'RstFlagPercentageInTotal');
    IF @var490 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var490 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [RstFlagPercentageInTotal] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var491 sysname;
    SELECT @var491 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'SkewnessBwdPacketsDeltaLen');
    IF @var491 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var491 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [SkewnessBwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var492 sysname;
    SELECT @var492 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'SkewnessBwdPacketsDeltaTime');
    IF @var492 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var492 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [SkewnessBwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var493 sysname;
    SELECT @var493 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'SkewnessFwdPacketsDeltaLen');
    IF @var493 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var493 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [SkewnessFwdPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var494 sysname;
    SELECT @var494 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'SkewnessFwdPacketsDeltaTime');
    IF @var494 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var494 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [SkewnessFwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var495 sysname;
    SELECT @var495 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'SkewnessHeaderBytes');
    IF @var495 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var495 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [SkewnessHeaderBytes] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var496 sysname;
    SELECT @var496 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'SkewnessPacketsDeltaLen');
    IF @var496 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var496 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [SkewnessPacketsDeltaLen] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var497 sysname;
    SELECT @var497 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'SkewnessPacketsDeltaTime');
    IF @var497 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var497 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [SkewnessPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var498 sysname;
    SELECT @var498 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'StdFwdPacketsDeltaTime');
    IF @var498 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var498 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [StdFwdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var499 sysname;
    SELECT @var499 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'StdHeaderBytes');
    IF @var499 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var499 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [StdHeaderBytes] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var500 sysname;
    SELECT @var500 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'StdPacketsDeltaTime');
    IF @var500 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var500 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [StdPacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var501 sysname;
    SELECT @var501 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'SynFlagPercentageInTotal');
    IF @var501 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var501 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [SynFlagPercentageInTotal] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var502 sysname;
    SELECT @var502 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'VarianceHeaderBytes');
    IF @var502 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var502 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [VarianceHeaderBytes] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    DECLARE @var503 sysname;
    SELECT @var503 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attack_Table]') AND [c].[name] = N'VariancePacketsDeltaTime');
    IF @var503 IS NOT NULL EXEC(N'ALTER TABLE [Attack_Table] DROP CONSTRAINT [' + @var503 + '];');
    ALTER TABLE [Attack_Table] ALTER COLUMN [VariancePacketsDeltaTime] float NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    ALTER TABLE [Attack_Table] ADD [DeltaStartIncompleteHandshake] bit NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261122151153_SupportNullableFlowInputs'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261122151153_SupportNullableFlowInputs', N'8.0.21');
END;
GO

COMMIT;
GO

