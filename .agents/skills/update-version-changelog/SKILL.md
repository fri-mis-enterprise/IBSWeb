---
name: update-version-changelog
description: Update the application version and CHANGELOG.md in IBSWeb and its company copies from actual Git changes. Use when the user asks to bump the version, prepare release notes, or update the version and changelog, including requests based on current HEAD or local changes versus remote.
---

# Update Version and Changelog

Update release metadata in the active IBSWeb repository. A short request such as "update the version and changelog" is sufficient; do not require the user to repeat file paths or Git commands.

## Inspect the release scope

- Read the repository's AGENTS.md and applicable .editorconfig. Preserve existing staged and unstaged work, including edits already made to release files.
- Confirm the active repository, branch, exact HEAD, working-tree status, latest changelog entry, and canonical version. In IBSWeb the release files are root CHANGELOG.md and the Version property in IBSWeb/IBSWeb.csproj; verify these in the active checkout rather than assuming a previous version or company copy.
- Honor the user's scope: current HEAD excludes uncommitted work; uncommitted changes includes the index and working tree; local versus remote compares the current branch with its actual upstream and includes local edits when requested. Fetch that upstream before claiming the comparison is current. If fetching fails, disclose the stale reference.
- Without an explicit range, identify changes not yet represented by the latest changelog using local history and current staged/unstaged edits. Use git log --first-parent --date=short and git log -- CHANGELOG.md IBSWeb/IBSWeb.csproj to locate the last documented release boundary, then inspect its diff to HEAD plus relevant local changes. Check relevant untracked source files too. A recent changelog edit alone does not prove all earlier work is documented.
- Inspect the actual changed code and merge diffs to establish behavior; commit titles alone are insufficient. Ask only when a materially ambiguous baseline cannot be resolved from repository evidence or the conversation.

## Update the two release files

- Document implemented changes within that scope. Write concise descriptions of observable behavior, using the existing changelog layout and categories. State breaking changes and required operator actions explicitly.
- Preserve earlier release notes. Avoid duplicate entries and speculative benefits, unsupported claims, secrets, and unrelated history. When the user requests changes per date, group them by the date they reached the target branch, using first-parent integration history.
- Use a version explicitly requested by the user. Otherwise follow SemVer and repository conventions: patch for fixes, minor for compatible features, major for breaking public or operational contracts. Explain the selected bump briefly in the final response.
- If the current in-progress release already represents the requested work, complete or correct that entry and synchronize the project version without another automatic bump. If nothing new needs documenting and both versions agree, report that no update is needed.
- Follow the existing heading format, currently ## [vX.Y.Z] - YYYY-MM-DD, and use the user's local date for a new release. Keep the project Version value and release heading consistent.
- Limit edits to CHANGELOG.md and the canonical version declaration unless an existing release mechanism requires another file. Do not update package versions or generated files. Preserve unrelated dirty changes within either release file.
- Commit, tag, push, and deploy only when separately requested.

## Verify and report

- Inspect the complete resulting diff, distinguishing pre-existing edits from this task. Run git diff --check and confirm encoding, line endings, indentation, and final newlines against .editorconfig.
- Read the effective version with dotnet msbuild IBSWeb/IBSWeb.csproj -getProperty:Version and verify that it matches the latest release heading. Run the repository's required validation, normally dotnet build "Integrated Business System.sln". If the build cannot run, report that limitation accurately.
- Report the old and new version (or unchanged version), the release scope, files changed, and validation results concisely. Do not claim runtime behavior was tested when only metadata or compilation was checked.
