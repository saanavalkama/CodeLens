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