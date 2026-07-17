# Controlled 4.6 deployment

The deployment workflow deliberately separates source from runtime material. A source checkout alone is not playable.

## Prerequisites

- .NET 8 SDK for building, or use the self-contained publish produced by the script.
- A separately installed MongoDB instance bound to loopback.
- A private runtime-data directory that the operator is legally permitted to use.
- Two operator-controlled HTTP(S) runtime-data endpoints, supplied through `ASCNET_PRIMARY_CDNS` and `ASCNET_SECONDARY_CDNS`.
- A client-side routing arrangement supplied and maintained outside this repository.

The runtime-data root must contain at least:

```text
Configs/config.json
Configs/version_config.json
Configs/study_compatibility_4.6.0.json
Configs/table_compatibility_4.6.0.json
Data/
table/
table-4.6.0/
```

`config.json` must select application and study compatibility version `4.6.0`. The version response must be marked ready, and every versioned table must match the SHA-256 recorded by the compatibility manifest.

## Validate without publishing

```powershell
pwsh -File .\tools\verify-runtime-data.ps1 `
  -RuntimeDataRoot D:\private\AscNet-4.6-Resources
```

The verifier rejects executable files, databases, certificates, private keys, saves, and other package material that does not belong in a data-only input.

## Publish

Use a new, empty output directory:

```powershell
pwsh -File .\tools\publish-runtime.ps1 `
  -RuntimeDataRoot D:\private\AscNet-4.6-Resources `
  -OutputDirectory D:\deploy\AscNet-4.6 `
  -RuntimeIdentifier win-x64 `
  -AcknowledgeNoRedistribution
```

The publisher:

1. verifies the public source-snapshot boundary;
2. verifies the external 4.6 runtime-data contract and table hashes;
3. publishes a self-contained single-file server;
4. copies runtime data only into the local output;
5. removes retired-version notice/config selections from the generated output;
6. writes a deployment manifest with hashes and file counts.

The generated output may contain third-party runtime material supplied by the operator. Do not upload, sell, or redistribute it.

Before starting the generated server, set both CDN variables to endpoints you control and are authorized to use. There is deliberately no embedded production-CDN fallback:

```powershell
$env:ASCNET_PRIMARY_CDNS = 'https://private.example.invalid/runtime'
$env:ASCNET_SECONDARY_CDNS = 'https://private-backup.example.invalid/runtime'
```

## Network boundary

Default listeners are local only. A non-loopback SDK or game bind is rejected unless `ASCNET_ALLOW_REMOTE_BIND=1` is explicitly set. That switch is an acknowledgement, not a security hardening feature. Before using it, isolate the host with firewall/VPN rules and review authentication, rate limiting, logging, and database exposure.

## Status boundary

A successful source build proves compilation. A successful runtime-data check proves input integrity. A successful publish proves that a deployable directory was assembled. None of those proves lawful operation, production security, real-client connectivity, or gameplay correctness on a different host.
