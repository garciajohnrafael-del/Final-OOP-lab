# How to push OOP10FEXLab to GitHub (3 members)

Each member pushes **their own part**, so the commit history shows who did what.
Order: **MJ first → Raf → Member 3.**
Commands work in **Git Bash, PowerShell, or CMD**. Install Git first if needed: https://git-scm.com

---

## STEP 0 – Repo owner (MJ) creates the repo

1. github.com → **+** (top right) → **New repository**
2. Name: **OOP10FEXLab**. Leave README, .gitignore and license **unchecked** (the zip already has them).
3. Click **Create repository**.
4. **Settings → Collaborators → Add people** → add Raf's and Member 3's GitHub usernames.
5. Raf and Member 3 **accept the invite** from their email or github.com/notifications.

---

## STEP 1 – MJ pushes the base + Kiosk (everything except QueueMonitor)

Unzip `OOP10FEXLab.zip`, open a terminal **inside** the `OOP10FEXLab` folder (the one with `names.txt`):

```bash
git config --global user.name "MJ Full Name"
git config --global user.email "mj-github-email@example.com"

git init
git branch -M main
git add . ":!OOP10FEXLab/QueueMonitor"
git commit -m "Base project, data layer and Kiosk UI (MJ)"
git remote add origin https://github.com/<MJ-USERNAME>/OOP10FEXLab.git
git push -u origin main
```

A browser window pops up the first time → **Sign in with GitHub** → Authorize.

> The project will not build yet. Program.cs needs Raf's QueueMonitor files, which come in Step 2.

---

## STEP 2 – Raf pushes the Queue Monitor

```bash
git config --global user.name "Raf Full Name"
git config --global user.email "raf-github-email@example.com"

git clone https://github.com/<MJ-USERNAME>/OOP10FEXLab.git
cd OOP10FEXLab
```

Copy the **`QueueMonitor`** folder from the zip into `OOP10FEXLab\OOP10FEXLab\`. It goes beside the `Kiosk` folder.

```bash
git add OOP10FEXLab/QueueMonitor
git commit -m "Queue Monitor UI and queue service (Raf)"
git push
```

Now open `OOP10FEXLab.sln` and press F5. It should build and run.

---

## STEP 3 – Member 3 pushes names + QA

```bash
git clone https://github.com/<MJ-USERNAME>/OOP10FEXLab.git
cd OOP10FEXLab
```

Edit `names.txt` (real full names and section). Run the app and fill **Pass/Fail** in `TEST_CASES.md`.

```bash
git add names.txt TEST_CASES.md
git commit -m "Group names and QA test results"
git push
```

---

## Everyone, before any later change

```bash
git pull
# ...edit...
git add .
git commit -m "what you changed"
git push
```

## Common errors

| Error | Fix |
|---|---|
| `rejected … fetch first` | Run `git pull`, then `git push` again |
| `Permission denied / 403` | You haven't accepted the collaborator invite, or you are signed in to the wrong account |
| `remote origin already exists` | `git remote set-url origin https://github.com/<MJ-USERNAME>/OOP10FEXLab.git` |
| `src refspec main does not match` | You haven't committed yet. Run `git add` and `git commit` first |
| No Git installed? | On GitHub, go to the repo → **Add file → Upload files**, drag the folder, then **Commit changes** |

Submit the repo link: `https://github.com/<MJ-USERNAME>/OOP10FEXLab`
