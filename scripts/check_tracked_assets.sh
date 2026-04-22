#!/usr/bin/env bash
set -euo pipefail

required=(
  "Assets/SDAS/Samples/SevenChapters_200sqm.json"
  "Assets/SDAS/Tests/GeometryAndMappingTests.cs"
  "Assets/SDAS/Tests/SampleScenarioSolverTests.cs"
  "Assets/SDAS/Tests/SolverFallbackTests.cs"
  "CASE_SCENARIO_GUIDE.md"
  "UNITY_QUICKSTART.md"
)

echo "[SDAS] checking required tracked files..."

missing=0
for f in "${required[@]}"; do
  if git ls-files --error-unmatch "$f" >/dev/null 2>&1; then
    echo "  ✅ tracked: $f"
  else
    echo "  ❌ missing : $f"
    missing=1
  fi
done

if [[ $missing -ne 0 ]]; then
  echo "[SDAS] some required files are not tracked."
  exit 1
fi

echo "[SDAS] all required files are tracked in git index."
