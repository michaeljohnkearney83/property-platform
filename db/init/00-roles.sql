-- Local development only (Docker Compose). The app connects as a NON-superuser,
-- because Postgres superusers bypass row-level security even with FORCE.
CREATE ROLE app_user LOGIN PASSWORD 'app_dev_password';
CREATE DATABASE propertyplatform OWNER app_user;
\connect propertyplatform
CREATE EXTENSION IF NOT EXISTS vector;
CREATE EXTENSION IF NOT EXISTS pgcrypto;
