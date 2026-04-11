-- Quick insert helper for the FileMetaDatas table.
-- Replace the sample values before running it in psql, pgAdmin, DBeaver, or any SQL client.

INSERT INTO "FileMetaDatas" (
    "OriginalName",
    "Token",
    "Extension",
    "Size",
    "Password",
    "CreatedAt",
    "ExpirationDays",
    "Tags",
    "UserId"
)
VALUES (
    'invoice-2026.pdf',
    'b7f4c2a9d1e84f7b9f2a6d8c3e1f5a20',
    '.pdf',
    '245760',
    NULL,
    '2026-01-01 00:00:00+00',
    7,
    ARRAY['finance', 'archive'],
    1
)
RETURNING *;
