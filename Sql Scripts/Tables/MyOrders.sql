CREATE TABLE [dbo].[MyOrders](
	[OrderNo] [nvarchar](16) NOT NULL,
	[ShipName] [nvarchar](16) NOT NULL,
	[ShipAddress] [nvarchar](64) NOT NULL,
	[TotalAmt] [decimal](10, 0) NOT NULL,
	CONSTRAINT [PK_MyOrders] PRIMARY KEY CLUSTERED ([OrderNo] ASC)
);

GO