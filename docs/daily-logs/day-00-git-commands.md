# Day 00 — Git Commands Reference
**Date:** March 5, 2026  
**Purpose:** Every git command used today, explained from scratch

---

## What is Git?

Git is a **version control system** — it takes snapshots of your code over time so you can:
- Go back to any previous version
- Work on multiple features in parallel (branches)
- Collaborate with others without overwriting each other's work
- Show employers a professional commit history

---

## Commands used today

---

### `git config`
**What it does:** Configures your Git identity. This name and email appear on every commit you make.

```bash
git config user.name "Ramiro Lopez"
git config user.email "rammirolopez@gmail.com"
```

> Without `--global` it only applies to the current repo.  
> With `--global` it applies to every repo on your machine.

**When to use it:** Once per machine (with `--global`) or once per repo (without it).

---

### `git clone`
**What it does:** Downloads a remote repository from GitHub to your local machine and connects them automatically.

```bash
git clone https://github.com/Ramiro671/jobtracker-pro.git
```

**What it creates:**
```
jobtracker-pro/        ← local folder
  .git/                ← hidden folder where Git stores everything
  README.md
  LICENSE
  .gitignore
```

**When to use it:** Once, when you first bring a remote repo to your machine.

---

### `git status`
**What it does:** Shows the current state of your working directory — which files are new, modified, or staged.

```bash
git status
```

**Possible outputs:**
- `Untracked files` → new files Git doesn't know about yet
- `Changes not staged` → files you modified but haven't prepared for commit
- `Changes to be committed` → files staged and ready to commit

**When to use it:** Before every `git add` and every `git commit` — it's your safety check.

---

### `git add`
**What it does:** Stages files — tells Git "include these changes in the next commit".

```bash
git add docs/                  # stages everything inside the docs/ folder
git add docs/daily-logs/       # stages a specific subfolder
git add .                      # stages ALL changes in the current directory
git add Program.cs             # stages a single file
```

> Staging is a two-step process by design: you choose exactly what goes into each commit.

**When to use it:** After creating or modifying files you want to save.

---

### `git commit`
**What it does:** Takes a permanent snapshot of the staged files and saves it to your local history with a message.

```bash
git commit -m "docs: add day-00 setup log and daily-logs structure"
```

**Commit message convention (Conventional Commits):**

| Prefix | When to use |
|--------|-------------|
| `feat:` | New feature added |
| `fix:` | Bug fixed |
| `docs:` | Documentation only |
| `chore:` | Tooling, config, setup |
| `refactor:` | Code restructured, no new feature |
| `test:` | Tests added or modified |
| `style:` | Formatting only, no logic change |

**Examples you'll use in this project:**
```bash
git commit -m "feat: add JobApplication entity with Clean Architecture"
git commit -m "fix: resolve JWT token expiration issue"
git commit -m "docs: update README with API endpoints"
git commit -m "chore: add Docker compose configuration"
git commit -m "test: add unit tests for JobApplicationService"
```

> ⚠️ Commit messages in English always — it's the professional standard.

---

### `git log`
**What it does:** Shows the commit history of the repository.

```bash
git log --oneline      # compact view, one line per commit
git log                # full view with author, date, message
```

**Output of `git log --oneline` today:**
```
44e37e5 docs: add day-00 setup log and daily-logs structure
96f6a36 docs: update README
9ad6517 Initial commit
```

Each 7-character code (`44e37e5`) is a **commit hash** — a unique ID for that snapshot.

**When to use it:** To review what was done, find a specific commit, or check that your push worked.

---

### `git push`
**What it does:** Uploads your local commits to the remote repository on GitHub.

```bash
git push origin main
```

- `origin` → the name Git gives to your remote (GitHub) by default
- `main` → the branch you're pushing to

**When to use it:** After committing, to sync your local work with GitHub.

---

### `git pull`
**What it does:** Downloads commits from the remote and merges them into your local branch.

```bash
git pull origin main
```

**When to use it:** Before starting work each day, to make sure you have the latest version.

---

### `git pull --rebase` ⭐ 
**What it does:** Downloads remote commits and replays your local commits **on top** of them, keeping a clean linear history.

```bash
git pull origin main --rebase
```

**Why `--rebase` instead of plain `git pull`:**

Without `--rebase`:
```
A --- B --- C (remote)
         \
          D (yours)
          ↓ merge creates extra commit
A --- B --- C --- M (merge commit, messy)
                /
               D
```

With `--rebase`:
```
A --- B --- C (remote)
                \
                 D (yours, replayed on top — clean)
```

**When to use it:** When `git push` is rejected because the remote has commits you don't have locally. This was exactly what happened today.

---

### `git version`
**What it does:** Shows the installed Git version.

```bash
git --version
# git version 2.x.x
```

---

## The workflow you'll use every day in this project

```bash
# 1. Start of the day — get latest changes
git pull origin main --rebase

# 2. Work on your code...

# 3. Check what changed
git status

# 4. Stage your changes
git add .

# 5. Commit with a meaningful message
git commit -m "feat: add JWT authentication to WebApi"

# 6. Push to GitHub
git push origin main
```

---

## What happened today — step by step

```bash
# Configured identity for this repo
git config user.name "Ramiro Lopez"
git config user.email "rammirolopez@gmail.com"

# Checked the initial commit from GitHub
git log --oneline
# 9ad6517 Initial commit

# Staged the new docs folder
git add docs/
# warning: LF will be replaced by CRLF  ← normal on Windows, not an error

# Created first real commit
git commit -m "docs: add day-00 setup log and daily-logs structure"
# [main 390145a] ...

# Push rejected because GitHub had README + LICENSE we didn't have locally
git push origin main
# ! [rejected] — remote contains work not in local

# Downloaded remote commits and replayed ours on top
git pull origin main --rebase
# Successfully rebased and updated refs/heads/main

# Push succeeded
git push origin main
# 96f6a36..44e37e5  main -> main ✅
```

---

## The LF/CRLF warning explained

```
warning: in the working copy of '...', LF will be replaced by CRLF
```

- **LF** (Line Feed `\n`) → line ending used on Linux/Mac
- **CRLF** (Carriage Return + Line Feed `\r\n`) → line ending used on Windows

Git on Windows converts automatically. This is **not an error** — just Git telling you it's normalizing line endings. Your `.gitignore` with the VisualStudio template already handles this correctly.

---

*JobTracker Pro · Git Reference · Day 00 · github.com/Ramiro671/jobtracker-pro*
