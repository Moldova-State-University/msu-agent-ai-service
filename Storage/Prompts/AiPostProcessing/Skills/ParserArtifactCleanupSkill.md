# Skill: Parser Artifact Cleanup

The input may contain parser artifacts from PDF conversion.

Possible artifacts:
- literal "\n"
- excessive empty lines
- repeated page numbers
- broken words caused by line wrapping
- HTML line breaks such as <br/>
- table formatting artifacts
- duplicated headers or footers

Rules:

1. Remove literal "\n" artifacts when they are not part of the document text.
2. Remove standalone page numbers if they are clearly page numbers.
3. Remove repeated headers and footers only if they are parser artifacts.
4. Do not remove real document headings.
5. Do not join unrelated paragraphs.
6. Do not change the meaning of text.
7. Preserve real tables.
8. Preserve legal numbering.