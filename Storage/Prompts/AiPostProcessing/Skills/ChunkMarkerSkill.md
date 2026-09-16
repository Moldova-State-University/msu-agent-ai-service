# Skill: Chunk Boundary Markers

Use the literal marker:

[chunk_end]

Rules:

1. The marker must appear on its own line.
2. Do not place [chunk_end] after headings.
3. Place [chunk_end] after each complete standalone paragraph.
4. Place [chunk_end] after each complete numbered article or point.
5. If a numbered point introduces a list, place [chunk_end] only after the last list item belonging to that numbered point.
6. Do not place [chunk_end] after the introductory line before a list.
7. Do not place [chunk_end] after every internal list item.
8. Do not create empty chunks.
9. Do not place two consecutive [chunk_end] markers.
10. Every semantic chunk must end with exactly one [chunk_end].