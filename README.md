# AscNet 4.6 public source-available snapshot

This is a public, source-available snapshot of the AscNet compatibility work for client version `4.6.0`. It is not an official Punishing: Gray Raven project, not open-source software, and not a ready-to-run game-server bundle.

## Deliberate publication boundary

The repository contains server source and synthetic compile-time table schemas only. It excludes:

- game clients, launchers, proxies, certificates, databases, saves, logs, packet captures, and binaries;
- client-derived runtime tables, artwork, notices, captured responses, and account material;
- local paths, deployment reports, credentials, tokens, and one-click runtime bundles;
- retired compatibility configuration and the unrelated packet-capture parser.

`Schemas/table/` contains field names and synthetic `0` / `sample` rows solely for source generation. It is not runtime gameplay data.

## Access and redistribution

No open-source or redistribution license is granted. GitHub's terms permit users to view and fork public repositories; those platform rights do not authorize resale, paid hosting, off-platform redistribution, or bundling with third-party game material. See [RIGHTS-NOTICE.md](RIGHTS-NOTICE.md).

Public visibility means anyone can clone or fork the repository. The no-license notice is a legal boundary, not copy protection, and cannot technically prevent unauthorized redistribution or resale.

## Security defaults

- The SDK/HTTP host and game TCP listener default to loopback.
- Any non-loopback bind requires `ASCNET_ALLOW_REMOTE_BIND=1` as an explicit acknowledgement.
- HTTP request logs omit query strings.
- The runtime data pack is external, untracked, and validated before publishing.
- No third-party production CDN is embedded; the operator must provide controlled runtime-data endpoints explicitly.
- The source has not been hardened for an internet-facing production service.

## Build and source checks

.NET 8 is required.

```powershell
pwsh -File .\tools\verify-source-snapshot.ps1
dotnet build .\AscNet\AscNet.csproj -c Release
dotnet run --project .\AscNet.Test\AscNet.Test.csproj -c Release
```

## Controlled deployment

Deployment intentionally requires an operator-supplied, lawfully obtained 4.6 runtime-data directory. The repository never supplies that material.

```powershell
pwsh -File .\tools\publish-runtime.ps1 `
  -RuntimeDataRoot D:\private\AscNet-4.6-Resources `
  -OutputDirectory D:\deploy\AscNet-4.6 `
  -AcknowledgeNoRedistribution
```

Read [DEPLOYMENT.md](DEPLOYMENT.md) before using the publisher. It fails closed when the 4.6 version contract or table hashes do not match.

At runtime, `ASCNET_PRIMARY_CDNS` and `ASCNET_SECONDARY_CDNS` must point to HTTP(S) endpoints the operator controls and is authorized to use. The server will not fall back to a third-party production CDN.

## Rights and legal boundary

The upstream history does not provide an explicit license that this snapshot can safely extend. No new open-source license is asserted here. Game names, marks, data, client material, and other third-party rights remain with their respective owners. A disclaimer cannot legalize reverse engineering, private-server operation, or redistribution where applicable agreements or law prohibit it. Obtain qualified legal advice before sharing or operating this outside a private research environment.
