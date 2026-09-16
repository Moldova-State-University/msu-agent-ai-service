# Skill: USM Regulation Structure Recognition

This document is likely an institutional regulation of Universitatea de Stat din Moldova.

Rules:

1. If the document begins with a cover/header table containing:
   - "Universitatea de Stat din Moldova"
   - "Departamentul Managementul Calității"
   - "APROBAT"
   - "Senatul USM"
   - "Rector"
   - a regulation name beginning with "Regulamentul..."

   then this table is a document cover/header, not a normal content section.

2. Extract the regulation name from the cover/header table and use it as the only H1 document title.

Correct:

# Regulamentul instituțional privind organizarea și desfășurarea învățământului dual în cadrul studiilor superioare de licență (ciclul I)

Incorrect:

# I. PREAMBUL

1. Roman-numbered headings are main sections, not document titles.

Examples of roman-numbered main sections:

- I. PREAMBUL
- II. DISPOZIȚII GENERALE
- III. PLANIFICAREA ȘI ORGANIZAREA ÎNVĂȚĂMÂNTULUI DUAL
- IV. DISPOZIȚII FINALE

They must always be formatted as H2:

## I. PREAMBUL

## II. DISPOZIȚII GENERALE

## III. PLANIFICAREA ȘI ORGANIZAREA ÎNVĂȚĂMÂNTULUI DUAL

## IV. DISPOZIȚII FINALE

1. Never format roman-numbered sections as H1 if the document has a separate regulation title.

2. All roman-numbered sections must have the same heading level.

3. Do not keep the cover/header table as the first semantic section of the document. Use it only to detect metadata and document title.

4. Approval information such as approval date, protocol number, rector name, and issuing body is metadata. It must not become a document section.
