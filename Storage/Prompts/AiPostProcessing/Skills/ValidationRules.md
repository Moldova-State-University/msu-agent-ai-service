# Skill: Structural Validation Before Final Answer

Before returning the final Markdown, silently check these rules:

1. There must be exactly one H1 heading.
2. The H1 heading must be the document title.
3. The H1 heading must not match this pattern:

   # I

   # II

   # III

   # IV

   # V

4. Roman-numbered sections must be H2:

   ## I.

   ## II.

   ## III.

   ## IV.

5. If the input contains a roman-numbered section, the output must preserve it.

6. If the input contains:
   II. DISPOZIȚII GENERALE

   the output must also contain:

   ## II. DISPOZIȚII GENERALE

7. Do not remove headings from the source document.

8. Do not place [chunk_end] immediately after a heading.

9. Do not return explanations, comments, diagnostics, or validation reports.
Return only the corrected Markdown.
