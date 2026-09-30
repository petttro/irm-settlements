-- 1. основной
CREATE INDEX IF NOT EXISTS ix_reports_main
    ON "settlements"."Reports" ("ServiceCompanySapId", "Status", "CreatedAt" DESC);

CREATE INDEX IF NOT EXISTS ix_reports_createddate
    ON "settlements"."Reports" ("CreatedAt" DESC);

-- 2. поиск
CREATE EXTENSION IF NOT EXISTS pg_trgm;

CREATE INDEX IF NOT EXISTS ix_reportitems_searchtext_trgm
    ON "settlements"."ReportItems"
        USING GIN ("SearchText" gin_trgm_ops);

CREATE INDEX IF NOT EXISTS ix_servicecompany_name_trgm
    ON "settlements"."ServiceCompanies"
        USING GIN ("Name" gin_trgm_ops);

CREATE INDEX IF NOT EXISTS ix_servicecenter_name_trgm
    ON "settlements"."ServiceCenters"
        USING GIN ("Name" gin_trgm_ops);
