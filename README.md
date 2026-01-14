# Tang3cko Standards

## Purpose

Shared coding standards and Claude Code skills for Tang3cko projects.

---

## Contents

### AI Agent Skills

| Skill | Description |
|-------|-------------|
| [unity](skills/unity/) | Unity C# standards, ScriptableObject architecture (Tang3cko.ReactiveSO), testing, UI |
| [documentation](skills/documentation/) | Technical writing standards |
| [repository-setup](skills/repository-setup/) | Repository setup and configuration |

### Other

| Directory | Description |
|-----------|-------------|
| [unity/networking/mirror/](unity/networking/mirror/) | Mirror networking reference (legacy) |

---

## TODO

- [ ] Mirror networking: Convert to skill or remove (currently legacy reference only)

---

## Installation

In Claude Code, use the `/plugin` command:

```
/plugin add https://github.com/tang3cko/coding-standards
```

See [Claude Code Plugins Documentation](https://code.claude.com/docs/en/plugins) for details.

---

## Priority levels

Each standard uses priority levels to indicate importance:

- **P1 (Required)**: Must be followed
- **P2 (Recommended)**: Should be followed when possible
- **P3 (Optional)**: Nice-to-have

---

## References

- [Tang3cko.ReactiveSO](https://github.com/tang3cko/ReactiveSO) - ScriptableObject-based reactive architecture
- [Unity Manual](https://docs.unity3d.com/Manual/)
- [C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [Claude Code Skills](https://docs.anthropic.com/en/docs/claude-code/skills)
