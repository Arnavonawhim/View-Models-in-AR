Perfect 👍 Here’s a ready-to-use **`SETUP.md`** (or you can paste it in your repo’s `README.md`). It explains to your collaborator how to clone and open the Unity project step by step.

---

# 🚀 Project Setup Guide (Unity + GitHub)

This guide explains how to set up the project on your machine and start contributing.

---

## 1. Prerequisites

* **Git** installed → [Download here](https://git-scm.com/downloads)
* **Git LFS** installed (if not already):

  ```bash
  git lfs install
  ```
* **Unity Hub** installed → [Download here](https://unity.com/download)
* Unity Editor version: **⚠️ Use the version in `ProjectSettings/ProjectVersion.txt`**
  (Unity Hub will prompt you if you don’t have it installed.)

---

## 2. Clone the Repository

Open a terminal (or Git Bash on Windows):

```bash
git clone https://github.com/Arnavonawhim/View-Models-in-AR.git
cd View-Models-in-AR
```

If Git LFS is used, also run:

```bash
git lfs pull
```

---

## 3. Open the Project in Unity Hub

1. Open **Unity Hub**
2. Click **Add → Add project from disk**
3. Select the cloned folder
4. Open it with the Unity version specified in `ProjectVersion.txt`

---

## 4. Unity Project Settings (should already be set)

* **Edit → Project Settings → Editor**

  * Version Control: **Visible Meta Files**
  * Asset Serialization: **Force Text**

---

## 5. Workflow

### Get latest changes before working:

```bash
git pull origin main
```

### Work on your feature:

* Create a new branch (recommended):

  ```bash
  git checkout -b feature/my-feature
  ```

### Save, stage, commit:

```bash
git add .
git commit -m "Describe your changes"
```

### Push your branch:

```bash
git push origin feature/my-feature
```

### Open a Pull Request (PR) on GitHub:

* Go to the repo on GitHub → click **Compare & pull request** → assign reviewers → merge into `main`.

---

## 6. Notes

* Always pull latest changes before starting new work.
* Use **branches** for features/bugfixes to avoid conflicts.
* If merge conflicts happen in `.unity` or `.prefab` files, Unity Smart Merge (YAMLMerge) may auto-resolve them; otherwise resolve manually in Unity.
* Do **not** commit the `Library/`, `Temp/`, or `Builds/` folders (already ignored by `.gitignore`).

---

✅ That’s it — you should now have the project running locally and be able to collaborate smoothly!

---

Would you like me to also add a **pre-written `.gitignore` and `.gitattributes`** snippet into this `SETUP.md`, so your collaborator never accidentally commits Unity’s `Library/` or forgets about LFS-tracked files?
