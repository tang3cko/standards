# Game Concept Design Frameworks - Reference

Comprehensive reference for organizing game concepts from initial idea to actionable design document.
Covers established frameworks, step-by-step methodologies, templates, and player-experience-centered approaches.

---

## 1. Overview of Established Frameworks

### 1.1 Elevator Pitch

A single sentence (or short paragraph) that captures the essence of a game concept.
Purpose: communicate the core idea in under 30 seconds to anyone -- team members, publishers, investors, or players.

**Format:**

```
[Game Title] is a [genre] game where the player [core action/role]
in a [setting/world] to [primary goal],
featuring [unique hook/differentiator].
```

**Evaluation criteria:**

- Can a stranger understand what the game IS?
- Does it convey what makes the game UNIQUE?
- Does it hint at why the game is FUN?

### 1.2 One-Sheet (One-Page Design Document)

A single-page overview that serves as the executive summary of a game concept. Used to align teams, pitch to stakeholders, and crystallize vision early.

**Standard sections:**

| Section | Content |
|---------|---------|
| Title and Tagline | Game name + catchy phrase summarizing the experience |
| Elevator Pitch | 2-3 sentence core concept |
| Genre and Platform | Game category + target platforms |
| Target Audience | Ideal player demographics and psychographics |
| Design Pillars | 3-5 words/phrases defining the core experience |
| Core Loop | Visual or textual diagram of the primary gameplay cycle |
| Key Mechanics | 3-5 main player actions and systems |
| USP (Unique Selling Points) | What sets this game apart from competitors |
| Art Style / Visual Direction | Brief description with reference images or keywords |
| Comparable Titles | 2-3 reference games (X meets Y format) |

### 1.3 Design Pillars (Pillars of Design)

Foundational words or short phrases that encapsulate the intended player experience. All design decisions must serve at least one pillar. Recommended count: 3-5.

**Characteristics of good pillars:**

- Focus on player FEELINGS and EXPERIENCES, not mechanics
- Each pillar has a concrete answer for HOW it will be achieved
- Short enough to memorize; specific enough to filter decisions
- Can be used as a litmus test: "Does this feature serve a pillar?"

**Real-world examples:**

| Game | Pillars |
|------|---------|
| The Last of Us | Story-driven, Stealth-focused, Exploration |
| Breath of the Wild | Exploration, Traversal freedom, Discovery |
| Hades | Narrative through death, Fluid combat, "One more run" |

**Anti-patterns:**

- Vague pillars: "Make it fun" or "High quality"
- Too many pillars: more than 5 dilutes focus
- Pillar without execution plan: no concrete design answer

### 1.4 MDA Framework (Mechanics-Dynamics-Aesthetics)

Formal analytical framework by Hunicke, LeBlanc, and Zubek (2004).

**Three layers:**

| Layer | Definition | Perspective |
|-------|-----------|-------------|
| Mechanics | Rules, algorithms, data structures, base components | Designer builds these |
| Dynamics | Runtime behaviors emerging from player + mechanics interaction | Emerges from play |
| Aesthetics | Emotional responses evoked in the player | Player experiences these |

**Key insight:** Designers work Mechanics -> Dynamics -> Aesthetics (bottom-up), but players experience Aesthetics -> Dynamics -> Mechanics (top-down). Effective concept design starts from the desired Aesthetics and works backward.

**Eight aesthetic categories (LeBlanc):**

1. Sensation -- game as sense-pleasure
2. Fantasy -- game as make-believe
3. Narrative -- game as drama
4. Challenge -- game as obstacle course
5. Fellowship -- game as social framework
6. Discovery -- game as uncharted territory
7. Expression -- game as self-discovery
8. Submission -- game as pastime

### 1.5 The Elemental Tetrad (Jesse Schell)

Four equally important elements that compose any game:

1. **Mechanics** -- procedures, rules, goals
2. **Story** -- sequence of events that unfolds
3. **Aesthetics** -- how the game looks, sounds, smells, tastes, feels
4. **Technology** -- materials and interactions that make the game possible

All four elements support the player EXPERIENCE, which is the ultimate design target. "The game is not the experience. The game enables the experience."

### 1.6 Core-Focus-Power Methodology

Three-step design methodology by Adrian Novell:

| Step | Purpose | Question |
|------|---------|----------|
| Core | Define the central intent -- emotion, mechanic, message, or business objective | "What is the ONE thing this game must deliver?" |
| Focus | Every feature must point toward the Core; discard what does not serve it | "Does this feature serve the Core?" |
| Power | At least one feature goes beyond comfort zone with bold, fresh execution | "How can we take this to the extreme?" |

---

## 2. Concept Organization Steps (Stage-by-Stage)

### Stage 1: Ideation and Core Definition

**Goal:** Capture the raw idea and distill it to its essence.

**Actions:**

1. Write a free-form description of the game (1-2 paragraphs)
2. Identify the single most important player action (the "verb")
3. Define the Core emotion or experience the game must deliver
4. Draft the elevator pitch
5. List 3-5 comparable titles and what you borrow/differ from each

**Output:** Elevator pitch + core verb + target emotion

### Stage 2: Core Loop and Mechanics Definition

**Goal:** Define the repeatable cycle of actions that forms the gameplay backbone.

**Actions:**

1. Diagram the core loop as a cycle: Action -> Feedback -> Reward -> Motivation -> Action
2. Identify primary mechanics (the "verbs" the player performs)
3. Define secondary loops that feed into or branch from the core loop
4. Establish progression systems (how the loop evolves over time)
5. Validate: does every loop element serve a design pillar?

**Core loop pattern:**

```
[Player Action] -> [System Response/Feedback] -> [Reward/Progression] -> [New Challenge/Motivation] -> (repeat)

Example (Roguelike):
Explore -> Encounter enemies -> Fight/Flee -> Gain loot/XP -> Upgrade -> Explore deeper -> (repeat)
```

**Output:** Core loop diagram + mechanics list + progression outline

### Stage 3: Target Audience and Player Motivation

**Goal:** Define who will play this game and why.

**Actions:**

1. Define demographics: age range, platform preference, play session length
2. Define psychographics: motivations, play style preferences, genre familiarity
3. Map to Bartle player types (or similar taxonomy):
   - **Achievers** -- motivated by goals, points, mastery
   - **Explorers** -- motivated by discovery, secrets, world knowledge
   - **Socializers** -- motivated by interaction, community, cooperation
   - **Killers** -- motivated by competition, dominance, PvP
4. Identify primary and secondary player types the game serves
5. Validate core loop against target audience expectations

**Output:** Player persona(s) + motivation mapping + audience validation

### Stage 4: Differentiation and Unique Value Proposition

**Goal:** Articulate what makes this game different and worth playing.

**Actions:**

1. Conduct competitive analysis of 3-5 comparable titles
2. Identify gaps: what do competitors NOT offer?
3. Define USP (Unique Selling Proposition) in one sentence
4. Map unique features to design pillars
5. Validate: does the USP resonate with the target audience?

**Competitive analysis template:**

| Comparable Title | Genre | Strengths | Weaknesses | Our Differentiator |
|-----------------|-------|-----------|------------|-------------------|
| [Game A] | | | | |
| [Game B] | | | | |
| [Game C] | | | | |

**Output:** USP statement + competitive positioning + differentiator list

### Stage 5: Experience Goals and Emotional Design

**Goal:** Define the emotional journey the player should experience.

**Actions:**

1. Define the player fantasy: "What does the player GET TO BE or DO?"
2. Map desired emotions across the play session (tension curve)
3. Identify key "moments" the game must deliver (first 5 minutes, boss fights, story reveals)
4. Validate against MDA aesthetics: which aesthetic categories does this game prioritize?
5. Define "feel" targets for core mechanics (responsive, weighty, floaty, snappy)

**Emotional arc template:**

```
Opening:     [Curiosity / Wonder / Excitement]
Early game:  [Empowerment / Learning / Discovery]
Mid game:    [Challenge / Tension / Investment]
Climax:      [Triumph / Revelation / Catharsis]
End game:    [Satisfaction / Nostalgia / Motivation to replay]
```

**Output:** Player fantasy statement + emotional arc + feel targets + priority MDA aesthetics

---

## 3. GDD (Game Design Document) - Initial Section Structure

The GDD is a living document that evolves throughout development. The initial sections form the concept-phase GDD, built from the outputs of Stages 1-5 above.

### Recommended Initial GDD Structure

```
1. Executive Summary
   1.1 Elevator Pitch
   1.2 Design Pillars (3-5)
   1.3 Genre and Platform
   1.4 Target Audience
   1.5 Comparable Titles

2. Gameplay
   2.1 Core Loop (diagram + description)
   2.2 Primary Mechanics
   2.3 Secondary Mechanics
   2.4 Progression Systems
   2.5 Win/Lose Conditions
   2.6 Game Flow (session structure)

3. Player Experience
   3.1 Player Fantasy
   3.2 Emotional Goals / Feeling Targets
   3.3 Target MDA Aesthetics
   3.4 Key Moments

4. World and Setting
   4.1 World Overview
   4.2 Narrative Premise (if applicable)
   4.3 Key Characters (if applicable)
   4.4 Lore Summary

5. Art and Audio Direction
   5.1 Visual Style (with references/moodboard keywords)
   5.2 Color Palette Direction
   5.3 Audio Direction / Mood
   5.4 UI Style Direction

6. Technical Overview
   6.1 Target Platform(s)
   6.2 Engine / Technology
   6.3 Key Technical Constraints
   6.4 Multiplayer / Online Requirements

7. Scope and Constraints
   7.1 MVP Feature Set
   7.2 Stretch Goals
   7.3 Non-Goals (explicit exclusions)
   7.4 Known Risks

8. Market and Differentiation
   8.1 Competitive Analysis
   8.2 Unique Selling Proposition
   8.3 Monetization Model (if applicable)
```

---

## 4. MCPMarket Game Concept Designer - 5-Stage Workflow Reference

The MCPMarket Game Concept Designer skill provides a five-step workflow to transform abstract game ideas into concrete, programmable design documents. Its core philosophy shifts focus from vague aesthetics to specific player actions and "decision-making weight."

### Reconstructed 5-Stage Workflow

Based on the skill's stated goals and methodology:

| Stage | Name | Focus |
|-------|------|-------|
| 1 | Idea Extraction | Extract and verbalize the raw game concept from the user's description |
| 2 | Core Loop Definition | Define the specific player actions that form the core gameplay cycle |
| 3 | Decision-Making Weight | Identify where meaningful player choices occur and what gives them weight |
| 4 | Scope and Constraint Setting | Set boundaries, define MVP, establish what the game is NOT |
| 5 | Design Blueprint Output | Produce a structured, programmable design document ready for implementation |

### Key Principles from MCPMarket Approach

1. **Player actions over aesthetics** -- Define what the player DOES before how the game LOOKS
2. **Decision-making weight** -- Every player choice should have meaningful consequences
3. **Programmable output** -- The final document should be concrete enough to start coding from
4. **Scope discipline** -- Explicit constraints prevent feature creep from concept phase
5. **Fun factor validation** -- Validate that the concept is enjoyable BEFORE writing code

---

## 5. Player-Experience-Centered Concept Organization

### 5.1 Start from the Player Fantasy

The most effective concept organization begins not with mechanics or technology, but with the player experience.

**Guiding questions:**

- What does the player GET TO BE? (Role/Identity)
- What does the player GET TO DO? (Core verbs/actions)
- How should the player FEEL? (Emotional targets)
- What STORY does the player tell themselves? (Internal narrative)
- What makes the player COME BACK? (Retention hook)

### 5.2 Experience-First Design Process

```
1. Define Player Fantasy
   "In this game, you are a [role] who [core action] in [world]"

2. Identify Target Emotions (pick 2-3 primary)
   Curiosity, Mastery, Wonder, Tension, Triumph, Connection, Creativity, Power

3. Design Mechanics to Evoke Those Emotions
   For each target emotion, identify mechanics that create it:
   - Curiosity -> procedural generation, hidden areas, lore fragments
   - Mastery -> skill-based combat, speedrun potential, difficulty scaling
   - Wonder -> vast vistas, unexpected discoveries, emergent behaviors

4. Build Core Loop Around the Strongest Mechanic-Emotion Pair
   The core loop should repeatedly deliver the PRIMARY emotion

5. Validate with the "Monday Morning Test"
   "Would the target player choose this game over alternatives on a Monday morning?"
```

### 5.3 Jesse Schell's Lens Questions (Selected for Concept Phase)

From "The Art of Game Design: A Book of Lenses" -- key questions for concept validation:

| Lens | Question |
|------|----------|
| Essential Experience | What experience do I want the player to have? |
| Surprise | What will surprise the player? |
| Fun | What parts are fun? Why? Can I make them more fun? |
| Curiosity | What questions does the game put into the player's mind? |
| Problem Solving | What problems does the game ask the player to solve? |
| Freedom | When does my player feel free? When constrained? |
| Needs | Which player needs does my game satisfy? (Maslow, competence, autonomy, relatedness) |
| Flow | Is there a balance of challenge vs. ability that creates flow? |

---

## 6. Templates

### Template A: Quick Concept Card (5 minutes)

```markdown
# [Game Title]

**Pitch:** [One sentence]
**Genre:** [Genre + Sub-genre]
**Platform:** [Target platform(s)]
**Reference:** [Game X] meets [Game Y]

**Core Verb:** [The ONE thing the player does most]
**Core Emotion:** [The ONE feeling the game must deliver]
**Core Loop:** [Action] -> [Feedback] -> [Reward] -> [Motivation]

**Pillars:**
1. [Pillar 1]
2. [Pillar 2]
3. [Pillar 3]

**Why play THIS game?** [USP in one sentence]
```

### Template B: Concept One-Sheet (30 minutes)

```markdown
# [Game Title]
*[Tagline]*

## Elevator Pitch
[2-3 sentences capturing the full concept]

## Design Pillars
1. **[Pillar 1]** -- [One sentence explaining how this manifests in gameplay]
2. **[Pillar 2]** -- [One sentence explaining how this manifests in gameplay]
3. **[Pillar 3]** -- [One sentence explaining how this manifests in gameplay]

## Core Loop
[Diagram or step-by-step description of the primary gameplay cycle]

## Player Fantasy
"In this game, you are [role]. You [core actions] in [world] because [motivation]."

## Target Audience
- **Primary:** [Demographics + psychographics]
- **Player types:** [Bartle types or motivation categories]
- **Session length:** [Typical play session]

## Key Mechanics
1. [Mechanic 1] -- [Brief description]
2. [Mechanic 2] -- [Brief description]
3. [Mechanic 3] -- [Brief description]

## Unique Selling Points
- [USP 1]
- [USP 2]
- [USP 3]

## Visual Direction
[Art style keywords + 2-3 reference titles or images]

## Comparable Titles
| Title | What We Borrow | How We Differ |
|-------|---------------|---------------|
| [Game A] | | |
| [Game B] | | |

## Non-Goals
- [What this game is NOT]
- [Features explicitly excluded]

## Target Emotions
[Opening] -> [Early game] -> [Mid game] -> [Climax] -> [End game]
```

### Template C: Concept Validation Checklist

```markdown
## Concept Validation Checklist

### Clarity
- [ ] Can I explain the game in one sentence?
- [ ] Does the pitch convey what makes it unique?
- [ ] Can a non-gamer understand the basic appeal?

### Core Loop
- [ ] Is the core loop defined as a clear cycle?
- [ ] Does every loop iteration deliver the core emotion?
- [ ] Does the loop evolve/deepen over time?

### Player Experience
- [ ] Is the player fantasy clearly defined?
- [ ] Are target emotions identified (2-3 primary)?
- [ ] Do mechanics serve the emotional goals?

### Differentiation
- [ ] Is the USP articulated in one sentence?
- [ ] Does competitive analysis show a clear gap?
- [ ] Would the target audience choose this over alternatives?

### Scope
- [ ] Is the MVP feature set defined?
- [ ] Are non-goals explicitly stated?
- [ ] Is the scope realistic for the team/timeline?

### Pillars
- [ ] Are 3-5 pillars defined?
- [ ] Does each pillar have a concrete execution plan?
- [ ] Can every feature be justified by at least one pillar?
```

---

## 7. Worked Example

### Example: "Tidecaller" -- Concept Card

```markdown
# Tidecaller

**Pitch:** A rhythm-action roguelike where you command ocean tides to protect
a coastal village from eldritch sea creatures.
**Genre:** Rhythm-Action Roguelike
**Platform:** PC, Nintendo Switch
**Reference:** Crypt of the NecroDancer meets Slay the Spire

**Core Verb:** Conduct (rhythmic tide commands)
**Core Emotion:** Flow state with mounting tension
**Core Loop:** Scout threat -> Compose tide pattern -> Execute on beat -> Harvest debris -> Upgrade village

**Pillars:**
1. Rhythmic mastery -- every action syncs to music
2. Escalating dread -- each night brings stranger horrors
3. Village attachment -- you care about what you protect

**Why play THIS game?** The only roguelike where your rhythm accuracy
directly shapes the battlefield, turning music into a weapon.
```

### Example: "Tidecaller" -- Design Pillar Validation

| Feature Idea | Serves Pillar? | Decision |
|-------------|---------------|----------|
| Beat-synced wave controls | Rhythmic mastery | KEEP - Core mechanic |
| Village building between waves | Village attachment | KEEP - Emotional investment |
| Creeping fog each night | Escalating dread | KEEP - Atmosphere |
| PvP multiplayer mode | None | CUT - Does not serve any pillar |
| Unlockable cosmetic hats | None directly | DEFER - Low priority |

### Example: "Tidecaller" -- MDA Mapping

| Target Aesthetic | Mechanic That Delivers It |
|-----------------|--------------------------|
| Challenge | Beat accuracy determines wave strength |
| Fantasy | Player as mythic tide-caller, conducting the ocean |
| Narrative | Village evolves based on player performance |
| Sensation | Synchronized audiovisual feedback on beat hits |

---

## 8. Sources and Further Reading

- Hunicke, LeBlanc, Zubek -- "MDA: A Formal Approach to Game Design and Game Research" (2004)
- Jesse Schell -- "The Art of Game Design: A Book of Lenses" (3rd Edition)
- Scott Rogers -- "Level Up! The Guide to Great Video Game Design" (One-Sheet methodology)
- Adrian Novell -- "Core, Focus & Power: A Game Design Methodology" (Gamasutra/Game Developer)
- Richard Bartle -- "Hearts, Clubs, Diamonds, Spades: Players Who Suit MUDs" (1996)
- MCPMarket -- Game Concept Designer Skill (5-stage workflow for programmable design documents)
