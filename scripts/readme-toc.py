"""Regenerates the "Contents" menu in README.md from its ## and ### headings.

Usage:  python scripts/readme-toc.py
The menu lives between the <!-- toc --> and <!-- /toc --> markers.
Anchors follow GitHub's rules: lowercase, punctuation removed, spaces -> hyphens.
"""
import re
from pathlib import Path

README = Path(__file__).resolve().parent.parent / "README.md"
START, END = "<!-- toc -->", "<!-- /toc -->"


def slug(text: str, seen: dict[str, int]) -> str:
    s = text.strip().lower()
    s = re.sub(r"[^\w\- ]", "", s)  # drop punctuation (keeps letters, digits, _, -, space)
    s = s.replace(" ", "-")
    n = seen.get(s, 0)
    seen[s] = n + 1
    return s if n == 0 else f"{s}-{n}"


def main() -> None:
    text = README.read_text(encoding="utf-8")
    before, rest = text.split(START, 1)
    _, after = rest.split(END, 1)

    seen: dict[str, int] = {}
    lines = []
    in_code = False
    for line in after.splitlines():
        if line.startswith("```"):
            in_code = not in_code
        if in_code:
            continue
        m = re.match(r"^(##|###) (.+)$", line)
        if not m:
            continue
        level, title = m.groups()
        indent = "" if level == "##" else "   "
        label = title.replace("`", "")
        lines.append(f"{indent}- [{label}](#{slug(title, seen)})")

    menu = "\n".join(["", "**Contents**", "", *lines, ""])
    README.write_text(before + START + "\n" + menu + "\n" + END + after, encoding="utf-8")
    print(f"README contents updated ({len(lines)} entries)")


if __name__ == "__main__":
    main()
