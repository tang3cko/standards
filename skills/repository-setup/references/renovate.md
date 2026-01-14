# Renovate Setup

## Purpose

Configure automated dependency updates with Renovate.

---

## Configuration - P2

### Create config file

Create `.github/renovate.json5`:

```json5
{
  $schema: 'https://docs.renovatebot.com/renovate-schema.json',
  extends: [
    'config:best-practices',
  ],
  timezone: 'Asia/Tokyo',
  schedule: ['before 9am on Monday'],
  automerge: true,
  automergeType: 'pr',
  matchUpdateTypes: ['minor', 'patch'],
}
```

### Enable Renovate

1. Install [Renovate GitHub App](https://github.com/apps/renovate)
2. Grant access to your repository
3. Renovate will create an onboarding PR

---

## Configuration options - P2

### Schedule

```json5
// Weekly on Monday morning (JST)
schedule: ['before 9am on Monday'],

// Daily
schedule: ['before 9am every weekday'],

// Monthly
schedule: ['before 9am on the first day of the month'],
```

### Automerge

```json5
// Automerge minor and patch updates
automerge: true,
automergeType: 'pr',
matchUpdateTypes: ['minor', 'patch'],
```

### Major updates

Major updates require manual review by default. This is intentional - breaking changes need human verification.

---

## Best practices - P2

### Review major updates

- Read changelogs before merging
- Check for breaking changes
- Test in development environment first

### Monitor PRs

- Renovate creates separate PRs for each update
- Failed CI blocks automerge
- Review dependency dashboard for overview

### Lock file maintenance

Renovate automatically updates lock files (package-lock.json, yarn.lock) to keep dependencies current.

---

## References

- [Renovate Documentation](https://docs.renovatebot.com/)
- [Renovate Configuration Options](https://docs.renovatebot.com/configuration-options/)
- [Renovate GitHub App](https://github.com/apps/renovate)
