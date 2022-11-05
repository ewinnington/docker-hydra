CREATE TABLE IF NOT EXISTS "events" (
  "id" serial primary key not null,
  "uuid" uuid NOT NULL,
  "type" text NOT NULL,
  "body" jsonb NOT NULL,
  "inserted_at" timestamp(6) NOT NULL DEFAULT statement_timestamp()
);