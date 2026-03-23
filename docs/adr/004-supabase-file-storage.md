# ADR 004 — Supabase Storage for File Storage

## Status
Accepted

## Context
The Driver and Truck detail pages need to store uploaded files (documents, images, etc.) and serve them back on demand.

Railway — our hosting platform (see ADR-001) — runs the app in an ephemeral container. Any file written to the local filesystem is destroyed on every deployment, restart, or container recycle. Local filesystem storage is therefore not viable for user-uploaded files in production.

An external object store is required. The options considered were:

| Option | Notes |
|--------|-------|
| **Supabase Storage** | S3-compatible endpoint, free tier (500 MB storage, 2 GB egress/month), no credit card required |
| **Cloudflare R2** | S3-compatible, generous free tier, but requires a payment method on file even for free usage |
| **AWS S3** | Established standard but egress costs add up; overkill for a small internal tool |
| **PostgreSQL (bytea)** | No extra service needed, but Railway's free DB storage is too limited and Postgres is not designed for blob storage |
| **Local filesystem** | Not viable on Railway — wiped on every deploy |

Cloudflare R2 was the original choice but was ruled out because it requires a credit card for account verification even on the free tier. Supabase provides an equivalent free tier without that requirement.

## Decision

Use **Supabase Storage** as the object store for all uploaded files.

Supabase Storage exposes an S3-compatible endpoint, accessed via the AWS S3 SDK (`AWSSDK.S3`) with `ForcePathStyle = true`:

```
https://<project-ref>.storage.supabase.co/storage/v1/s3
```

A single `R2FileStorageService` (implementing `IFileStorageService`) wraps the S3 client. The app stores the object key in the `AttachedFile.StoragePath` database column and uses it for all subsequent read/delete operations. Uploads and downloads are proxied through the app server — the browser never communicates with Supabase directly.

Two buckets are maintained: `abba-fleet-mark-laptop` for local development and `abba-fleet-prod` for production. Credentials are supplied via environment variables and are never committed to source control.

## Consequences

- **Free tier is sufficient:** 500 MB storage, 2 GB egress/month — well within range for a ~5-user internal tool storing driver/truck documents
- **No credit card required:** Supabase free tier has no payment method requirement
- **Survivable deployments:** files are stored outside the Railway container and persist across deploys and restarts
- **S3-compatible:** the AWS SDK is reused as-is — no Supabase-specific client library needed
- **Credentials required for file ops:** the app uses lazy client creation so it starts without credentials, but any file operation will fail until the four env vars are set (`Supabase__S3Endpoint`, `Supabase__AccessKeyId`, `Supabase__SecretAccessKey`, `Supabase__BucketName`)
- **Downloads proxied through server:** Supabase response streams pass through the app server; presigned URLs would be more scalable for large files but are deferred as out of scope
- **No virus scanning:** files are stored without content inspection — acceptable for a trusted internal team but a known gap if the user base grows
