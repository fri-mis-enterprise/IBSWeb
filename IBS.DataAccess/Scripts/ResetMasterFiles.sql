-- Back up ibs_gl_dev and stop application writes before running this entire script.
-- Clears all public tables except the user/setup tables listed below and EF migration history.
-- Cleared tables' owned sequences restart at their configured starting values (normally 1).
-- Preserved tables and their sequences are unchanged. Other schemas are not modified.
BEGIN;

SET LOCAL lock_timeout = '10s';

DO $reset$
DECLARE
    tables_to_clear text;
BEGIN
    IF current_database() <> 'ibs_gl_dev' THEN
        RAISE EXCEPTION 'Expected database ibs_gl_dev, connected to %', current_database();
    END IF;

    SELECT string_agg(format('ONLY %I.%I', schemaname, tablename), ', ' ORDER BY tablename)
    INTO tables_to_clear
    FROM pg_tables
    WHERE schemaname = 'public'
        AND tablename NOT LIKE 'AspNet%'
        AND tablename <> ALL (ARRAY[
            '__EFMigrationsHistory',
            'app_settings',
            'companies',
            'department_accesses',
            'products',
            'filpride_terms',
            'notifications',
            'user_notifications',
            'hub_connections'
        ]);

    IF tables_to_clear IS NULL THEN
        RAISE NOTICE 'No business tables found to clear.';
        RETURN;
    END IF;

    RAISE NOTICE 'Clearing tables: %', tables_to_clear;

    -- RESTRICT stops the entire reset if a preserved or other-schema table references a target.
    EXECUTE 'TRUNCATE TABLE ' || tables_to_clear || ' RESTART IDENTITY RESTRICT';
END;
$reset$;

COMMIT;
