---
name: unity-test-assemblies
description: Assembly definitions for Unity tests. .asmdef setup, NUnit references. Use when setting up test assemblies in Unity.
---

# Assembly Definitions for Tests

## Purpose

Correctly set up Assembly Definitions for Unity tests.

## Checklist

- [ ] Create separate test assemblies for each module
- [ ] Use naming: `{Project}.{Module}.Tests.{Mode}`
- [ ] Set `includePlatforms: ["Editor"]` for Edit Mode tests
- [ ] Enable `overrideReferences: true` for NUnit
- [ ] Add `nunit.framework.dll` to precompiledReferences
- [ ] Set `autoReferenced: false` for all test assemblies

---

## Folder structure - P1

```
Assets/_Project/
├── Scripts/
│   └── Core/
│       ├── GameLogic.cs
│       └── Daifugo.Core.asmdef       # Production
│
└── Tests/
    ├── Editor/                        # Edit Mode tests
    │   └── Core/
    │       ├── GameLogicTests.cs
    │       └── Daifugo.Core.Tests.Editor.asmdef
    │
    └── Runtime/                       # Play Mode tests
        └── Integration/
            ├── IntegrationTests.cs
            └── Daifugo.Tests.Runtime.asmdef
```

---

## Edit Mode test assembly - P1

File: `Tests/Editor/Core/Daifugo.Core.Tests.Editor.asmdef`

```json
{
    "name": "Daifugo.Core.Tests.Editor",
    "references": [
        "Daifugo.Core",
        "Daifugo.Data"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": false,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

### Key settings

| Setting | Value | Purpose |
|---------|-------|---------|
| `includePlatforms` | `["Editor"]` | Exclude from builds |
| `overrideReferences` | `true` | Required for NUnit |
| `precompiledReferences` | `["nunit.framework.dll"]` | NUnit framework |
| `autoReferenced` | `false` | Prevent accidental dependencies |

---

## Play Mode test assembly - P1

File: `Tests/Runtime/Daifugo.Tests.Runtime.asmdef`

```json
{
    "name": "Daifugo.Tests.Runtime",
    "references": [
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner",
        "Daifugo.Core",
        "Daifugo.Data"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": false,
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"
    ],
    "versionDefines": [],
    "noEngineReferences": false
}
```

### Differences from Edit Mode

| Setting | Edit Mode | Play Mode |
|---------|-----------|-----------|
| `includePlatforms` | `["Editor"]` | `[]` (any platform) |
| `references` | Production assemblies | + UnityEngine/Editor.TestRunner |
| `defineConstraints` | `[]` | `["UNITY_INCLUDE_TESTS"]` |

---

## Creating test assembly (easy way) - P1

1. Open Test Runner (`Window > General > Test Runner`)
2. Select `EditMode` tab
3. Click "Create EditMode Test Assembly Folder"
4. Unity creates folder and asmdef with correct settings

---

## Common issues - P1

### Tests not appearing

**Problem:** Test Runner doesn't show tests

**Solution:**
```json
"overrideReferences": true,
"precompiledReferences": [
    "nunit.framework.dll"
]
```

### Compilation errors

**Problem:** `The type or namespace name 'NUnit' could not be found`

**Solution:** Ensure `overrideReferences: true` and add `nunit.framework.dll`

### Tests in build

**Problem:** Tests included in build

**Solution:**
```json
"includePlatforms": [
    "Editor"
]
```

---

## Quick checklist - P1

For each test assembly:

- [ ] Name follows pattern: `{Project}.{Module}.Tests.{Mode}`
- [ ] References production assemblies
- [ ] `overrideReferences: true`
- [ ] `precompiledReferences: ["nunit.framework.dll"]`
- [ ] `autoReferenced: false`
- [ ] Edit Mode: `includePlatforms: ["Editor"]`
- [ ] Play Mode: `UNITY_INCLUDE_TESTS` define constraint
- [ ] Tests appear in Test Runner
