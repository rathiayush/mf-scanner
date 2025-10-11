
--to delete duplicate in holding changes


WITH DuplicateCTE AS (
    SELECT 
        Id,                 -- assuming your table has a primary key column named Id
        FromDate,
        ToDate,
        ROW_NUMBER() OVER (PARTITION BY FromDate, ToDate ORDER BY Id) AS rn
    FROM HoldingChanges
    WHERE NewQuantity IS NULL
      AND NewWeight IS NULL
      AND NewValue IS NULL
)
select 
DELETE FROM DuplicateCTE
WHERE rn > 1;











-- Backfill PortfolioHolding.PortfolioSnapshotId where null
UPDATE ph
SET ph.PortfolioSnapshotId = ps.Id
FROM PortfolioHoldings ph
JOIN PortfolioFiles pf
  ON pf.Id = ph.PortfolioFileId
JOIN PortfolioSnapshots ps
  ON ps.MutualFundId = ph.MutualFundId
 AND ps.SnapshotDate = pf.ReportDate
WHERE ph.PortfolioSnapshotId IS NULL;