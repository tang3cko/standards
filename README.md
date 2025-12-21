# Coding standards

## Purpose

This repository contains shared coding standards for Tang3cko projects. Add it as a Git submodule to maintain consistency across repositories.

---

## Standards

| Directory | Description |
|-----------|-------------|
| [documentation/](documentation/) | Documentation writing standards |
| [repository/](repository/) | Repository setup standards |
| [unity/](unity/) | Unity and C# coding standards |

---

## Priority levels

Each standard uses priority levels to indicate importance:

- **P1 (Required)**: Must be followed
- **P2 (Recommended)**: Should be followed when possible
- **P3 (Optional)**: Nice-to-have

---

## Installation

```bash
# Add as submodule
git submodule add https://github.com/user/coding-standards.git docs/standards

# Clone with submodules
git clone --recurse-submodules <your-project-url>

# Initialize submodules in existing clone
git submodule update --init --recursive
```

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
