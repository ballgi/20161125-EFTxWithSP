CREATE PROCEDURE dbo.usp_get_order_no
(		@po_order_no		nvarchar(64)		OUTPUT
)
AS

DECLARE     @today  date	= dbo.ufn_today()
DECLARE		@seqs	table	(seq int)
DECLARE		@seq	int

SET NOCOUNT ON;

UPDATE G
SET G.OrderSeq = G.OrderSeq + 1
OUTPUT INSERTED.OrderSeq INTO @seqs
FROM [dbo].[OrderNoGenerator] G
WHERE G.OrderDate = @today;

IF @@ROWCOUNT = 0
  INSERT INTO [dbo].[OrderNoGenerator] (
      OrderDate
    , OrderSeq
  ) OUTPUT INSERTED.OrderSeq INTO @seqs
  VALUES (
      @today
    , 1
  )

SELECT TOP 1 @seq = seq FROM @seqs
SELECT @po_order_no = FORMAT(@today, 'yyyyMMdd') + '-' + FORMAT(@seq, '0000');

SET NOCOUNT OFF;

GO
