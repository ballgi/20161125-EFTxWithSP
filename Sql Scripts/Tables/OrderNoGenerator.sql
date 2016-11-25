CREATE TABLE [dbo].[OrderNoGenerator]
(
	[OrderDate] [DATE]	NOT NULL,
    [OrderSeq]  [INT]	NOT NULL,	-- 最後序號
    CONSTRAINT [PK_OrderNoGenerator] PRIMARY KEY CLUSTERED ([OrderDate] ASC)
);
GO