# Security policy

This publicly visible source is intended for a private research or testing environment. Do not expose the SDK, game listener, MongoDB, or any companion redirector directly to the public internet.

## Required operating rules

- Treat every committed file and Actions log as permanently public; never use the repository for deployment secrets or runtime material.
- Keep MongoDB on loopback and use a dedicated, non-production database.
- Leave SDK and game listeners on loopback unless an isolated network design has been reviewed.
- Supply only operator-controlled runtime-data endpoints through `ASCNET_PRIMARY_CDNS` and `ASCNET_SECONDARY_CDNS`; never use a third party's production CDN without authorization.
- Never commit runtime resources, certificates, account caches, tokens, saves, logs, packet captures, or generated deployment output.
- Do not paste secrets or unredacted logs into issues, commits, CI output, or chat.
- Run `tools/verify-source-snapshot.ps1` before every commit and push.
- Run `tools/verify-runtime-data.ps1` before every deployment publish.

## Incident response

If a credential or private runtime artifact is committed, revoke or rotate it first. Then remove the reachable Git history and coordinate repository/fork cleanup. Rewriting or deleting a branch does not erase copies already cloned by another party.

Report suspected exposure privately to the repository owner. Do not open a public issue containing evidence.
