# Compile-time schemas

The TSV files below `Schemas/table/` retain only field names and synthetic type-inference rows.

Every non-empty data cell must be exactly `0` or `sample`. No item names, stage definitions, rewards, artwork paths, descriptions, or other gameplay rows belong here. The safety verifier enforces this rule.

The table source generator maps these schema paths back to their expected `table/...` runtime paths. Actual runtime tables remain untracked under `Resources/table/`.
