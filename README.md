# Tang3cko Standards

## Purpose

Shared standards and Claude Code skills for Tang3cko projects. Add as a Git submodule to maintain consistency across repositories.

---

## Contents

### Standards

| Directory | Description |
|-----------|-------------|
| [documentation/](documentation/) | Documentation writing standards |
| [repository/](repository/) | Repository setup standards |
| [unity/](unity/) | Unity and C# coding standards |

### Claude Code skills

| Skill | Description |
|-------|-------------|
| [unity-csharp](skills/unity-csharp/) | Unity C# naming, architecture, performance |
| [mirror-networking](skills/mirror-networking/) | Mirror multiplayer patterns |
| [unity-ui](skills/unity-ui/) | UI Toolkit, uGUI, accessibility |
| [unity-testing](skills/unity-testing/) | NUnit, FIRST principles, test patterns |
| [documentation](skills/documentation/) | Technical writing standards |

---

## Installation

```bash
# Add as submodule
git submodule add https://github.com/tang3cko/coding-standards.git docs/standards

# Clone with submodules
git clone --recurse-submodules <your-project-url>

# Initialize submodules in existing clone
git submodule update --init --recursive
```

---

## Skills setup

Link skills to enable automatic context loading in Claude Code.

```bash
# Create skills directory
mkdir -p .claude/skills

# Link skills (adjust path based on submodule location)
ln -s ../../docs/standards/skills/unity-csharp .claude/skills/unity-csharp
ln -s ../../docs/standards/skills/documentation .claude/skills/documentation
```

See [repository/setup.md](repository/setup.md) for detailed setup instructions.

---

## Priority levels

Each standard uses priority levels to indicate importance:

- **P1 (Required)**: Must be followed
- **P2 (Recommended)**: Should be followed when possible
- **P3 (Optional)**: Nice-to-have

---

## Updates

```bash
git submodule update --remote docs/standards
```

---

## References

- [Google Developer Documentation Style Guide](https://developers.google.com/style)
- [Microsoft Writing Style Guide](https://learn.microsoft.com/en-us/style-guide/)
- [Unity Manual](https://docs.unity3d.com/Manual/)
- [C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [Claude Code Skills](https://docs.anthropic.com/en/docs/claude-code/skills)
