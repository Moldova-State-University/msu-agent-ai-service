# Skill: Table Handling

Tables can have different roles.

Table types:

1. Cover/header table
   Contains institution name, approval data, title, rector, stamp, protocol.
   Use it mainly to extract document metadata and title.

2. Data table
   Contains meaningful rows and columns used as document content.
   Preserve it as a Markdown table.

3. Schedule table
   Contains days, hours, groups, classrooms, teachers.
   Preserve rows and columns as accurately as possible.

Rules:

1. Do not delete meaningful tables.
2. Do not flatten meaningful tables into vague paragraphs.
3. Preserve column names.
4. Preserve row order.
5. If a table is a cover/header table, do not treat it as the first content section.
6. If a table contains schedule data, do not merge unrelated rows.
7. Place [chunk_end] after a complete meaningful table, not after every row.