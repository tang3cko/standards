---
name: unity-core
description: Write Unity C# code following Tang3cko core standards. Naming, code organization, error handling, Input System. Use when writing or reviewing Unity C# code structure. Triggers on "Unity", "C#", "naming", "null check", "InputReader", "namespace", "コード整理", "命名規則", "code style", "error handling".
model: sonnet
allowed-tools: Read, Glob, Grep, Edit, Write
---

# Unity Core Standards

Help users write, review, and refactor Unity C# code following Tang3cko core coding standards.

## Core Principles

1. **Naming consistency** - PascalCase for public, _camelCase for private, k_PascalCase for constants
2. **Null safety** - Validate external references in OnValidate, use null-conditional operators
3. **Event-driven input** - InputReader pattern over direct InputAction references

## When Invoked

### Step 1: Determine Task Type

- **Writing new code?** → Go to Step 2a
- **Reviewing existing code?** → Go to Step 2b
- **Question about conventions?** → Go to Step 2c

### Step 2a: Writing New Code

1. `_core-rules.md` is auto-loaded with essential P1 rules
2. Load specific references matching the code domain (naming, comments, error-handling, input)
3. Generate code following loaded standards
4. Verify naming, file structure order, and null safety

### Step 2b: Reviewing Code

1. `_core-rules.md` is auto-loaded with P1 rules
2. Check against naming conventions, code organization, error handling
3. Note specific issues with line numbers
4. Categorize issues by priority (P1 must fix, P2 should fix, P3 nice to have)

### Step 2c: Convention Question

1. Load the specific reference matching the question domain
2. Explain with code examples from references
3. Show Bad/Good patterns where applicable

## Reference Files

| File | Use When |
|------|----------|
| references/_core-rules.md | Auto-loaded: Essential naming and structure rules |
| references/naming.md | Naming conventions in detail |
| references/code-organization.md | Directory structure, namespaces |
| references/comments.md | XML docs, Header/Tooltip attributes |
| references/error-handling.md | Null safety, OnValidate |
| references/input-system.md | InputReader pattern |
