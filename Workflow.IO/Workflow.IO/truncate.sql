DO $$ 
DECLARE 
    r RECORD; 
BEGIN 
    FOR r IN (
        SELECT tablename, schemaname 
        FROM pg_tables 
        WHERE schemaname NOT IN ('pg_catalog', 'information_schema') 
          AND tablename != '__EFMigrationsHistory'
    ) 
    LOOP 
        EXECUTE 'TRUNCATE TABLE ' || quote_ident(r.schemaname) || '.' || quote_ident(r.tablename) || ' CASCADE'; 
    END LOOP; 
END $$;
