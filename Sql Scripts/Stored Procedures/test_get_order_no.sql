DECLARE @order_no NVARCHAR(64)

EXEC dbo.usp_get_order_no @order_no OUTPUT

PRINT @order_no

