# Assembly definitions for tests

## Purpose

This document defines how to correctly set up Assembly Definitions for Unity tests to enable proper test execution and compilation.

## Checklist

- [ ] Create separate test assemblies for each module
- [ ] Use consistent naming: `{Project}.{Module}.Tests.{Mode}`
- [ ] Set `includePlatforms: ["Editor"]` for Edit Mode tests
- [ ] Enable `overrideReferences: true` for NUnit
- [ ] Add `nunit.framework.dll` to precompiledReferences
- [ ] Set `autoReferenced: false` for all test assemblies
- [ ] Reference only necessary production assemblies

---

## Why assembly definitions - P1

Unity Test Framework requires assembly definitions to:
- Reference NUnit framework
- Separate test code from production code
- Exclude tests from builds
- Control compilation dependencies
- Speed up compilation

---

## Recommended folder structure - P1

```
Assets/_Project/
├── Scripts/
│   ├── Core/
│   │   ├── GameLogic.cs
│   │   ├── CardValidator.cs
│   │   └── Daifugo.Core.asmdef          # Production assembly
│   │
│   ├── Data/
│   │   ├── CardSO.cs
│   │   ├── PlayerHandSO.cs
│   │   └── Daifugo.Data.asmdef          # Production assembly
│   │
│   └── UI/
│       ├── GameScreenUI.cs
│       └── Daifugo.UI.asmdef            # Production assembly
│
└── Tests/
    ├── Editor/                           # Edit Mode tests
    │   ├── Core/
    │   │   ├── GameLogicTests.cs
    │   │   ├── CardValidatorTests.cs
    │   │   └── Daifugo.Core.Tests.Editor.asmdef
    │   │
    │   ├── Data/
    │   │   ├── CardSOTests.cs
    │   │   └── Daifugo.Data.Tests.Editor.asmdef
    │   │
    │   └── UI/
    │       ├── GameScreenUITests.cs
    │       └── Daifugo.UI.Tests.Editor.asmdef
    │
    └── Runtime/                          # Play Mode tests
        └── Integration/
            ├── GameManagerIntegrationTests.cs
            └── Daifugo.Integration.Tests.Runtime.asmdef
```

---

## Creating test assembly (easy way) - P1

### Using Test Runner window

1. Open Test Runner (`Window > General > Test Runner`)
2. Select `EditMode` tab
3. Click "Create EditMode Test Assembly Folder"
4. Unity creates:
   - `Tests/Editor/` folder
   - Assembly definition with correct settings

Result: Automatic setup with correct references.

---

## Edit Mode test assembly - P1

### Template

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

### Key settings explained

#### `name`

```json
"name": "Daifugo.Core.Tests.Editor"
```

- Assembly name
- Pattern: `{Project}.{Module}.Tests.{Mode}`

#### `references`

```json
"references": [
    "Daifugo.Core",    # Production code
    "Daifugo.Data"     # Dependencies
]
```

- Production assemblies to test
- Add all dependencies your tests need

#### `includePlatforms`

```json
"includePlatforms": ["Editor"]
```

- Critical: Exclude from builds
- Tests only run in Editor
- Reduces build size

#### `overrideReferences`

```json
"overrideReferences": true
```

- Required for NUnit
- Allows manual reference to precompiled DLLs

#### `precompiledReferences`

```json
"precompiledReferences": [
    "nunit.framework.dll"
]
```

- Required: NUnit framework reference
- Unity provides this DLL

#### `autoReferenced`

```json
"autoReferenced": false
```

- Tests are not auto-referenced by other assemblies
- Prevents accidental dependencies

---

## Play Mode test assembly - P1

### Template

File: `Tests/Runtime/Daifugo.Integration.Tests.Runtime.asmdef`

```json
{
    "name": "Daifugo.Integration.Tests.Runtime",
    "references": [
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner",
        "Daifugo.Core",
        "Daifugo.Data",
        "Daifugo.UI"
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

#### No `includePlatforms` restriction

```json
"includePlatforms": []  // Can run on any platform
```

- Can run in Editor + Build targets
- For integration testing on devices

#### Additional references

```json
"references": [
    "UnityEngine.TestRunner",    # Unity Test Framework (Play Mode)
    "UnityEditor.TestRunner",    # Editor integration
    "Daifugo.Core",              # Your assemblies
    // ...
]
```

#### Define constraint

```json
"defineConstraints": [
    "UNITY_INCLUDE_TESTS"
]
```

- Conditional compilation
- Code only included when tests are enabled

---

## Manual setup steps - P1

### Step 1: Create production assembly

1. Right-click on `Scripts/Core` folder
2. Select "Create > Assembly Definition"
3. Name it `Daifugo.Core`

### Step 2: Create test assembly

1. Right-click on `Tests/Editor/Core` folder
2. Select "Create > Assembly Definition"
3. Name it `Daifugo.Core.Tests.Editor`
4. Configure settings (see template above)

### Step 3: Add references

1. Click test assembly definition in Inspector
2. Find "Assembly Definition References"
3. Click `+` button
4. Select `Daifugo.Core` (production assembly)
5. Click "Apply"

### Step 4: Configure platform

1. In Inspector, find "Platforms"
2. Check "Editor" only
3. Uncheck all others
4. Click "Apply"

### Step 5: Override references

1. In Inspector, check "Override References"
2. "Precompiled References" section appears
3. Add `nunit.framework.dll`
4. Click "Apply"

---

## Verification - P1

### Check settings

- [ ] `includePlatforms: ["Editor"]` for Edit Mode
- [ ] `overrideReferences: true`
- [ ] `precompiledReferences: ["nunit.framework.dll"]`
- [ ] `references` includes production assemblies
- [ ] `autoReferenced: false`

### Run tests

1. Open Test Runner (`Window > General > Test Runner`)
2. Select `EditMode` tab
3. Tests should appear
4. Click "Run All"

If tests do not appear:
- Check assembly references
- Check NUnit reference
- Reimport assembly definition

---

## Common issues - P1

### Tests not appearing

Problem: Test Runner does not show tests

Solution:
1. Check `precompiledReferences: ["nunit.framework.dll"]`
2. Check `overrideReferences: true`
3. Reimport assembly definition (right-click → Reimport)

### Compilation errors

Problem: `The type or namespace name 'NUnit' could not be found`

Solution:
```json
"overrideReferences": true,
"precompiledReferences": [
    "nunit.framework.dll"  // ← Add this
]
```

### Tests in build

Problem: Tests included in build

Solution:
```json
"includePlatforms": [
    "Editor"  // ← Only Editor
]
```

---

## Best practices - P1

### Do

**Separate test assemblies by module:**

```
Daifugo.Core.Tests.Editor
Daifugo.Data.Tests.Editor
Daifugo.UI.Tests.Editor
```

**Use consistent naming:**

```
{Project}.{Module}.Tests.{Mode}
```

**Set includePlatforms for Edit Mode:**

```json
"includePlatforms": ["Editor"]
```

**Reference only what you need:**

```json
"references": [
    "Daifugo.Core",  // Only this module
    "Daifugo.Data"   // And its dependencies
]
```

### Do not

**Do not forget includePlatforms:**

```json
// ❌ BAD - tests in build
"includePlatforms": []

// ✅ GOOD
"includePlatforms": ["Editor"]
```

**Do not auto-reference tests:**

```json
// ❌ BAD
"autoReferenced": true

// ✅ GOOD
"autoReferenced": false
```

**Do not reference everything:**

```json
// ❌ BAD - unnecessary dependencies
"references": [
    "Daifugo.Core",
    "Daifugo.Data",
    "Daifugo.UI",
    "Daifugo.Audio",    // Not needed
    "Daifugo.Network"   // Not needed
]
```

---

## Quick checklist - P1

### For each test assembly

- [ ] Name follows pattern: `{Project}.{Module}.Tests.{Mode}`
- [ ] References production assemblies
- [ ] `overrideReferences: true`
- [ ] `precompiledReferences: ["nunit.framework.dll"]`
- [ ] `autoReferenced: false`
- [ ] Edit Mode: `includePlatforms: ["Editor"]`
- [ ] Play Mode: `UNITY_INCLUDE_TESTS` define constraint
- [ ] Tests appear in Test Runner
- [ ] Tests run successfully

---

## Summary - P1

| Setting | Edit Mode | Play Mode |
|---------|-----------|-----------|
| `includePlatforms` | `["Editor"]` | `[]` |
| `references` | Production assemblies | + UnityEngine/Editor.TestRunner |
| `defineConstraints` | `[]` | `["UNITY_INCLUDE_TESTS"]` |
| `overrideReferences` | `true` | `true` |
| `precompiledReferences` | `["nunit.framework.dll"]` | `["nunit.framework.dll"]` |
| `autoReferenced` | `false` | `false` |

---

## References

- [Unity Assembly Definition Files](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html)
- [Unity Test Framework Manual](https://docs.unity3d.com/Packages/com.unity.test-framework@latest)
