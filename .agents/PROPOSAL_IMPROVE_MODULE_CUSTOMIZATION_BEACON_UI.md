# Proposal: Improve Building-Level Module Customization Beacon UX

## 0. Cover Page

- **Title:** Improve Beacon Configuration in Building-Level Module Customization
- **Repository:** `shpaass/yafc-ce`
- **Author:** Community proposal draft
- **Date:** 2026-02-24
- **Status:** Draft for discussion
- **Target Area:** Production Table → Building-level Module Customization (`Module customization` dialog)
- **Priority:** Correctness > Readability > Maintainability > Performance

### Executive Summary

The current building-level module customization flow asks users to enter **the number of modules in beacons**, not **the number of beacons affecting a building**. This differs from the global module autofill configuration (which uses _Beacons per building_), and can be confusing in practical and edge scenarios.

This proposal introduces an explicit, optional per-building beacon count input while preserving existing behavior for old projects.

---

## 1. Why this change

### 1.1 UX clarity and mental model

In Factorio terms, many players reason in:

- _How many beacons affect this building?_
- _What module(s) do those beacons contain?_

Current building-level customization instead asks for module counts and derives beacon count indirectly. This is technically valid, but less intuitive and inconsistent with the global settings screen.

### 1.2 Consistency with global configuration

Global autofill already uses:

- Beacon entity
- Beacon module
- **Beacons per building**

Building-level customization currently does not expose the same primary control, making per-building overrides harder to reason about.

### 1.3 Edge / extreme cases are hard to express

The current model cannot naturally represent or communicate scenarios such as:

- intentionally sparse beacon filling,
- hand-tuned beacon count with non-trivial module distribution,
- explicit beacon-count targets for balancing profile/effect scaling.

Even when outcomes can be approximated via module totals, user intent is not explicit.

---

## 2. Usage scenarios

### 2.1 Different beacon entities per building group

Users may want one row to use a specific beacon type/quality and a specific number of affecting beacons, while another row uses a different setup.

### 2.2 Beacons not fully filled

In practical setups, users may intentionally avoid filling every beacon slot (cost, transition state, staged upgrades). They still want a clear, explicit beacon-count setting.

### 2.3 Late-game tuning with profile effects

Beacon profile scaling depends on beacon count. Users tuning exact throughput/power behavior need direct beacon-count control rather than inferring via module totals.

---

## 3. Before vs After

| Aspect                                   | Current behavior                              | Proposed behavior                                                 |
| ---------------------------------------- | --------------------------------------------- | ----------------------------------------------------------------- |
| Main beacon control in building-level UI | Number of modules in beacons                  | **Explicit beacons affecting this building** + module composition |
| Relation to global config                | Different mental model                        | Same primary concept (`beacons per building`)                     |
| Data semantics                           | Beacon count always derived from module total | Beacon count can be explicit (optional override)                  |
| Compatibility for old projects           | N/A                                           | Old projects continue to work via fallback behavior               |

### User-facing expectation after change

Users can set:

1. Which beacon affects this building,
2. How many beacons affect this building,
3. What modules are inside those beacons,
   with clear validation and feedback.

---

## 4. Proposed solution

### 4.1 What to change

#### A) Model (required)

Add an optional field to building-level module template data, e.g.:

- `beaconCountOverride` (nullable integer)

Scope:

- `ModuleTemplate`
- `ModuleTemplateBuilder`
- related calculation paths that currently derive beacon count only from module totals

Behavior:

- If `beaconCountOverride` is set, use it as effective beacon count.
- If not set, keep current logic (derive from module total and beacon module slots).

#### B) Computation (required)

Define a single effective count rule used everywhere:

- `effectiveBeaconCount = beaconCountOverride ?? derivedCount`

Apply this rule consistently in:

- solver-related module effects,
- current-effects preview in customization UI,
- displays/tooltips that show beacon count.

#### C) UI (required)

In building-level `Module customization` dialog:

- Add editable field: **Beacons affecting this building**
- Keep existing beacon module list controls
- Add capacity guidance/validation, e.g.:
  - Total beacon module entries should not exceed `effectiveBeaconCount * beacon.moduleSlots`

#### D) Localization and tests (required)

- Add/adjust locale strings
- Update serialization guard tests
- Add or update model tests for:
  - fallback behavior,
  - explicit override behavior,
  - validation boundaries.

### 4.2 What not to change

- Do **not** change global module autofill semantics (`ModuleFillerParameters`), except keeping conceptual alignment.
- Do **not** remove current module-list capability (it is more expressive than global single-module beacon fill).
- Do **not** force migration of existing saves.

### 4.3 Why model change is necessary (and UI-only is not enough)

A UI-only transformation cannot fully represent two independent degrees of freedom (beacon count and module distribution) using old data shape alone. Without a model field, user intent is either lossy or non-persistent.

### 4.4 Can we avoid model changes?

Short answer: not if correctness is the top priority.

Possible alternatives (not recommended):

- Runtime-only temporary beacon count (lost on reopen)
- Implicit conversion hacks from beacon count to module totals (cannot represent all valid intents)

Both weaken correctness and/or persistence.

---

## 5. Community Q&A / expected concerns

### Q1) Is this backward compatible with existing project files?

**Yes (planned).**
Old saves without the new field load normally. New code falls back to current derived behavior when no override is present.

### Q2) If users open a new save in an old YAFC version, will it corrupt saves?

Expected behavior: old versions should ignore unknown extra fields and use old derivation logic. This may change computed behavior for rows using new override data, but should not create unrecoverable file corruption.

### Q3) Is the change large/risky?

**Moderate scope, controlled risk.**
Core risk is consistency (UI preview vs solver vs display). Mitigation: centralize effective beacon-count logic and cover with targeted tests.

### Q4) Why not keep old save format unchanged and only transform in UI/calculation?

Because that approach cannot reliably persist user intent for independent beacon count control. It introduces lossy states and increases hidden inconsistency risk.

### Q5) How many PRs?

Suggested split:

1. **PR-1 (Model + calculation + tests):** add optional override field, effective-count logic, serialization/test updates.
2. **PR-2 (UI + localization):** building-level dialog controls and validation messaging.
3. **PR-3 (polish):** docs, tooltip refinements, minor UX adjustments.

### Q6) Relationship with global settings?

Global settings remain default behavior. Building-level customization is a row-level override. The two become conceptually aligned (both expose beacon count directly) while keeping current precedence rules.

### Q7) Main risks and mitigations

- **Risk:** inconsistent count usage across code paths
  - **Mitigation:** one effective-count helper path + tests
- **Risk:** ambiguous validation UX
  - **Mitigation:** explicit capacity hint and clear error/warning behavior
- **Risk:** downgrade behavior confusion
  - **Mitigation:** document that old versions ignore new override and may recalculate using legacy derivation

---

## Proposed acceptance criteria

1. Users can explicitly set per-building beacon count in module customization.
2. Old project files load with unchanged behavior when override is absent.
3. Current-effects preview and solver use the same effective beacon count.
4. Validation prevents impossible states (or clearly warns and normalizes behavior).
5. Global autofill behavior remains unchanged.

---

## Request for feedback

Please review especially:

1. Whether explicit beacon count should be hard-validated vs warning-only,
2. Preferred downgrade messaging strategy,
3. PR split and sequencing preferences for maintainers.
