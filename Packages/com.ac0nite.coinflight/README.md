# Coin Flight System

Reusable Unity 6 + URP coin flight system. Modes: `UI -> UI` and `World -> UI`. Backend: PrimeTween. Zero-alloc hot path.

## Requirements

- Unity 6000.0+
- URP 17.x
- PrimeTween 1.4.0+ (via npm scoped registry)
- Unity.Mathematics 1.3.2+

## Install

This package is an embedded package at `Packages/com.ac0nite.coinflight`.

Add the following scoped registry to your project `Packages/manifest.json` to resolve PrimeTween:

```json
{
  "scopedRegistries": [
    {
      "name": "npm",
      "url": "https://registry.npmjs.org",
      "scopes": ["com.kyrylokuzyk"]
    }
  ]
}
```

## Status

Iteration 1 — scaffolding in progress. See `.windsurf/plans/coin-flight-mvp-iter1-1191d0.md` in the project root for the full plan.
