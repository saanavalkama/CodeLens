## Database — PostgreSQL

A relational database was the natural fit for this project's access patterns. The core 
retrieval operation — fetching individual chunks ranked by vector similarity — does not 
require returning whole nested documents. Despite the hierarchical data model 
(repositories → repository files → file chunks → graph nodes), reads are always 
targeted at individual rows rather than entire document trees, which makes the relational 
model more efficient than a document store. 

**pgvector over a dedicated vector database**
pgvector was chosen over a dedicated vector database (Pinecone, Qdrant, Weaviate) for 
operational simplicity. Keeping vector search inside PostgreSQL meant cascade deletes 
handled chunk cleanup automatically when files or repositories were removed, without 
needing to synchronize deletions across two separate systems. It also avoided adding 
another service to the infrastructure.

**Write characteristics**
The indexing pipeline is write-heavy by nature — when a repository is indexed, thousands 
of chunk rows, file content rows, and graph nodes are written in a short burst. A 
write-optimized store using an LSM-tree structure (e.g. Cassandra) could have reduced 
indexing time by handling these bursts more efficiently. However, indexing is already slow 
for several reasons (GitHub API fetching, embedding generation, graph extraction), and 
write performance was not the primary bottleneck. Read performance was prioritized instead, 
as the user-facing query path — conversation messages triggering RAG retrieval and an LLM 
call — is where latency is most visible. PostgreSQL handles bounded write bursts adequately 
while providing strong read performance with proper indexing.

**Summary**
A database optimized for write-heavy bursts with strong read performance would have been 
the theoretical sweet spot. PostgreSQL approximates this well enough for the access patterns 
of this application, while also providing relational integrity, cascade deletes, and vector 
search in a single system. Prior operational familiarity with PostgreSQL also reduced 
deployment and maintenance risk.

##What I would do differently if cost wasn't a constraint

The biggest limitation of the current implementation is that search is single-shot — one query, one retrieval, one answer. For broad architectural questions this often produces incomplete results.
With unlimited budget I would make the search agentic in three ways:

**Query decomposition**
broad questions would be split into sub-questions and answered independently before being synthesized into a final response. For example "how does messaging work in this codebase" would decompose into "where is the message published", "what consumes it", "how are failures handled."
**Hypothetical code embedding** 
when natural language queries produce poor results, the agent would generate a synthetic code example representing the expected answer and use that as the search query instead. Code embeddings match better against code than natural language does.
**Autonomous codebase traversal**
rather than relying on a single retrieval step, the agent would iteratively query the codebase, deciding what context to fetch next based on what it already knows. This handles multi-hop questions that single retrieval misses entirely.
The reason these aren't implemented is cost. Each agentic step requires an additional LLM call, and for a tool that developers would use frequently throughout the day, costs would compound quickly and make it impractical without enterprise-level infrastructure.