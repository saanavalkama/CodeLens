# CodeLens — RAG & Semantic Search Evaluation

*18-question retrieval evaluation · findings & scores*

## Method

18 natural-language questions were run against a live CodeLens instance over an
indexed repository, grouped into five categories: direct lookup, semantic /
natural-language, multi-hop, edge / negative, and multi-turn. Each answer was
scored 1–5 on **both correctness and source attribution** — a correct answer
citing the wrong file scores lower than a correct answer with the right source.
Negative queries (asking about features that do not exist) were scored on
whether the system correctly declined rather than fabricated.

**Scoring rule:** a 5 requires a correct answer plus at least one correct,
sufficient source. The system is not penalised for failing to retrieve
additional context the question did not require.

## Headline result

**76 / 90 (84%), average 4.22 / 5.** By the project's own scoring threshold this
sits in the "good" band: the pipeline is solid for localized retrieval, and the
measurable gap is concentrated in structural and cross-boundary questions —
precisely the area the planned v2 graph layer is designed to address. 

| Category | Score | Avg | Read |
|---|---|---|---|
| 1 — Direct Lookup | 18 / 20 | 4.5 | Strong. Only miss was a named-entity conflation (Q-01). |
| 2 — Semantic / NL | 15 / 20 | 3.75 | Localized answers scored 5; distributed/cross-system scored 2–3. |
| 3 — Multi-hop | 15 / 20 | 3.75 | Answer-locality predicted score better than question difficulty. |
| 4 — Edge / Negative | 15 / 15 | 5.0 | Perfect. Never fabricated, even when baited with adjacent code. |
| 5 — Multi-turn | 13 / 15 | 4.33 | History threading 5/5; only loss (Q-16) was a chunking issue. |
| **Total** | **76 / 90** | **4.22** | **84% — solid pipeline, gap is structural/cross-boundary.** |

> The category averages are less informative than the per-question spread.
> Categories 2 and 3 share an average of 3.75, but in both the variance is the
> story: localized questions scored 5 while cross-boundary questions scored
> 1–2. The mean hides what the pattern reveals.

## Key findings

### 1. Retrieval quality tracks answer-locality, not question difficulty

The strongest predictor of score was not the category but whether the answer to
the specific question lived in a single coherent chunk. A multi-hop question
whose answer was localized to one file (Q-10, crash recovery) scored 5, while a
semantically simple question whose answer spanned a system boundary (Q-08, the
.NET↔Python job queue) scored 2. In Category 3 the score band was predicted in
advance from this principle for all four questions and held each time.

- Localized, well-named, single-chunk answers → 4–5, regardless of category.
- Answers requiring assembly across a file or system boundary → 1–2.
- What matters is the locality of the *answer to the question asked*, not the
  locality of the underlying feature (Q-11 confirmed the distinction).

### 2. The unsafe failure mode is bounded and recognizable

When the system had retrieved partial, related material and was asked to
complete a chain, it bridged the gap with confident inference rather than
disclosing the gap (Q-08 invented the .NET producer with "typically XADD"; Q-09
stitched a hypothetical trace across a missing layer). However, it did **not**
fabricate features, entities, or policies that do not exist — even when tempting
adjacent code was present (Q-13 PaymentController, Q-14 real-time collaboration,
Q-15 OpenAI retry policy all correctly declined).

- **Danger zone:** "trace / complete the full path of X" where X is real but distributed.
- **Safe:** "does X exist / how does X work" where X does not exist — clean refusal every time.

### 3. The model's confidence is not a reliable trust signal

Identical hedging language ("should", "depending on", "typically") appeared both
when the answer was wrong and incomplete (Q-08) and when it was fully correct
(Q-10, verified against source). Reliability could not be read from the answer's
tone — only from verification against the code. For a code-intelligence tool
this is a meaningful limitation: output must be checked regardless of how
confident it reads.

### 4. Chunking granularity caps retrieval on long methods

Q-16 returned an accurate description of a service class but cut off mid-method,
because a long C# method had been split across chunks and only the opening half
was retrieved. This is a chunking-quality issue independent of the retrieval
model. It also reinforces the v2 plan from a second angle: the Roslyn
integration already scoped for the graph layer provides exact method boundaries,
which would resolve this split — so the v2 work pays off twice, once for
structural queries and once for chunk quality.

## Per-question results

| ID | Category | Query | Score | Result |
|---|---|---|:---:|---|
| Q-01 | Direct | POST method / SearchController | 3 | Answered FastAPI router, labelled it SearchController; missed .NET SearchController. Named-entity conflation. |
| Q-02 | Direct | Messages table name | 5 | Correct + authoritative source (`ToTable`). Set the scoring rule. |
| Q-03 | Direct | Embedding model | 5 | Exact model string, authoritative instantiation line. Clean needle retrieval. |
| Q-04 | Direct | Where key is decrypted | 5 | Correct method + line; conceptual lookup landed precisely. |
| Q-05 | Semantic | Where auth is handled | 3 | Right sources, wrong construction: conflated AuthController / GitHubAuthController. |
| Q-06 | Semantic | Refresh race protection | 5 | Correct single-flight promise-mutex; localized to one chunk. |
| Q-07 | Semantic | File chunking entry point | 5 | Correct two-tier strategy (tree-sitter + fixed-size fallback). |
| Q-08 | Semantic | Job queue .NET ↔ Python | 2 | Found Python consumer only; invented .NET producer ("typically XADD"). |
| Q-09 | Multi-hop | Full FE→DB→back trace | 1 | Partial .NET side, mislabeled controller, never reached search subsystem. |
| Q-10 | Multi-hop | Crash recovery | 5 | Verified against code: ACK-after-success, no-ACK-on-crash → re-process. |
| Q-11 | Multi-hop | BYOK key → LLM flow | 4 | Core key→client→LLM chain correct; .NET entry half generic but peripheral. |
| Q-12 | Multi-hop | Conversation ↔ Message | 5 | One-to-many, all three sources (nav prop, FK, EF config). |
| Q-13 | Edge | PaymentController (n/a) | 5 | Correct refusal — no fabrication of non-existent controller. |
| Q-14 | Edge | Real-time collab (n/a) | 5 | Refused despite tempting adjacent infra (Redis, multi-user). |
| Q-15 | Edge | OpenAI retry policy (n/a) | 5 | Didn't invent retry/backoff from surrounding error-handling code. |
| Q-16 | Multi-turn | Describe MessageService | 3 | Accurate but cut off mid-method. Root cause: chunking split long C# method. |
| Q-17 | Multi-turn | Its dependencies | 5 | History threading confirmed: resolved "it", listed 3 deps consistently. |
| Q-18 | Multi-turn | Can deps be swapped? | 5 | Two-turn reach + correct DI reasoning + silently corrected non-existent name. |

## Implications for v2

The evaluation supports the planned graph layer with specific, evidence-backed
motivation rather than intuition:

- Structural / named-entity queries (Q-01, Q-05) and cross-boundary traces
  (Q-08, Q-09) are the consistent failures — exactly the queries a Roslyn /
  tree-sitter call graph answers that flat vector search cannot.
- The graph layer also lets the system know whether a connective path exists,
  which would let it disclose gaps instead of inferring across them — addressing
  the unsafe failure mode in Finding 2.
- Roslyn method boundaries additionally fix the C# chunking split in Finding 4,
  improving retrieval even before the graph is queried.

Equally important, the evaluation establishes where the current system is
already trustworthy: direct factual lookups, "how does X work" on localized
features, negative queries, and multi-turn context. Those need no verification
caveat. The cross-boundary trace is the one shape to treat with caution.

---

*Baseline snapshot. Scores reflect v1 as evaluated; re-running after the C#
chunking fix and the v2 graph layer will produce a comparison point.*
