#!/usr/bin/env bash
set -euo pipefail

current_branch=$(git branch --show-current)

echo "[SDAS] current local branch: ${current_branch}"

if ! git remote | grep -q .; then
  echo "[SDAS] no git remote configured in this environment."
  echo "[SDAS] push cannot be performed from here."
  exit 0
fi

for r in $(git remote); do
  echo "[SDAS] remote: $r -> $(git remote get-url "$r")"
done

tracking=$(git rev-parse --abbrev-ref --symbolic-full-name "@{u}" 2>/dev/null || true)
if [[ -z "$tracking" ]]; then
  echo "[SDAS] current branch has no upstream tracking branch configured."
  exit 0
fi

echo "[SDAS] upstream: $tracking"
ahead_behind=$(git rev-list --left-right --count "$tracking"...HEAD)
behind=$(echo "$ahead_behind" | awk '{print $1}')
ahead=$(echo "$ahead_behind" | awk '{print $2}')

echo "[SDAS] ahead: $ahead, behind: $behind"
if [[ "$ahead" -gt 0 ]]; then
  echo "[SDAS] local commits are ahead; run: git push"
else
  echo "[SDAS] no local commits pending push."
fi
