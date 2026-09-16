# Skill: Document Metadata Recognition

Some text belongs to document metadata, not to the main body.

Metadata examples:
- institution name
- issuing department
- approval body
- approval date
- protocol number
- rector name
- source file name
- version
- validity date

Rules:

1. Metadata must not become document sections.
2. Metadata must not be confused with document title.
3. Approval information must not be formatted as H1, H2, or H3.
4. If metadata is preserved in Markdown, place it under a separate metadata block before the main content.
5. The main content starts from the first real document section, such as "I. PREAMBUL".