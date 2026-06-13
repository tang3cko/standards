---
name: unity-core
description: Write Unity C# code following Tang3cko core standards — naming, code organization, error handling, Input System, editor tooling. Use when writing or reviewing Unity C# code structure.
when_to_use: Triggers on "Unity", "C#", "naming", "null check", "InputReader", "namespace", "コード整理", "命名規則", "code style", "error handling".
model: sonnet
allowed-tools: Read Glob Grep Edit Write
---

# Unity Core Standards

Help users write, review, and refactor Unity C# code following Tang3cko core coding standards.

## Core Principles

1. **Naming consistency** - PascalCase for public, camelCase for private, PascalCase for constants (UPPER_SNAKE_CASE only for preprocessor symbols)
2. **Null safety** - Validate external references in OnValidate, use null-conditional operators
3. **Event-driven input** - InputReader pattern over direct InputAction references

## Workflow

`references/_core-rules.md` is auto-loaded with essential naming and structure rules. Identify the task type and follow the matching path.

### Writing new code

1. Load the references matching the code domain (naming, comments, error-handling, input, editor-tooling)
2. Generate code following the loaded standards
3. Verify naming, file structure order, and null safety

### Reviewing code

1. Check against naming conventions, code organization, and error handling
2. Note specific issues with line numbers
3. Categorize issues by priority (must fix, should fix, nice to have)

### A convention question

1. Load the reference matching the question domain
2. Explain with Bad/Good code examples where applicable

## Reference Files

| File | Use When |
|------|----------|
| references/_core-rules.md | Auto-loaded: Essential naming and structure rules |
| references/naming.md | Naming conventions in detail |
| references/code-organization.md | Directory structure, namespaces, Runtime/Editor boundary, SO Serializer |
| references/editor-tooling.md | Setup Editor pattern (`BuildAssets` / `WireScene`), idempotence, log suppression |
| references/comments.md | XML docs, Header/Tooltip attributes |
| references/error-handling.md | Null safety, OnValidate |
| references/input-system.md | InputReader pattern |
