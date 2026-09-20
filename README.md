# msu-agent-ai-service

AI service repository for MSU Agent project. Managed by Terraform.

A RAG agent that answers questions about USM regulations. Documents are split into
chunks, embedded with Ollama and stored in Qdrant; the chat model queries them
through a tool call.

## Setup

### 1. Configure the Ollama endpoint

Create a `.env` file in the repository root:

```
Ollama__Endpoint=http://85.120.14.163/ollama/
Ollama__ApiKey=<api key>
```

Save it as UTF-8 **without BOM** — otherwise the first variable name is read with an
invisible prefix and ignored. 

### 2. Start Qdrant

```bash
docker run -d --name qdrant -p 6333:6333 -p 6334:6334 -v qdrant_storage:/qdrant/storage qdrant/qdrant
```

### 3. Index the documents

```bash
cd QdrantLoad && dotnet run
```

Reads `Storage/DocumentPreparedChunks/*.json`, embeds every chunk and uploads it to
Qdrant. Takes 5–20 minutes and **prints nothing** — follow the progress in another
terminal:

```bash
curl http://localhost:6333/collections/regulations
```

`points_count` should reach ~697. Files already listed in
`Storage/State/processed-files-Qdrant.json` are skipped; clear that file to `[]` to
reindex everything.

### 4. Run the API

```bash
cd USMAgent.AIService.API && dotnet run --launch-profile http
```

Listens on `http://localhost:5027`.

## Endpoints

| Method | Path | Body | Description |
|---|---|---|---|
| GET | `/api/chat/models` | — | models available on the Ollama server |
| POST | `/api/regulations/search` | `"credite"` | semantic search over the regulations |
| POST | `/api/chat` | `"Câte credite?"` | ask the agent; answer returns in full, not streamed |

Request bodies are bare JSON strings, not objects — send `"credite"`, not
`{"query": "credite"}`, with `Content-Type: application/json`.

## Structure

```
USMAgent.Application/              use cases and ports (interfaces) — no external dependencies
  Abstractions/                    IChatAgent, ISearchRegulations, IRegulationSearchStore, ...
  Models/                          RegulationChunk, RegulationSearchResult, ...
  Options/                         IndexingOptions
  RegulationIndexer.cs             indexing use case
  SearchRegulations.cs             search use case

USMAgent.Infrastructure/           adapters implementing the ports
  AI/Ollama/                       chat agent, embeddings, agent tools
  VectorStore/Qdrant/              search and index stores
  FileSystem/                      prompts, JSON stores, file hashing
  DependencyInjection/             AddUsmAgent — composition root

USMAgent.AIService.API/            REST host
QdrantLoad/                        indexing console app
USMAgent.AIService.Hosting.Aspire/ Aspire host (needs the Aspire CLI to build)

Storage/
  Raw/                             source PDFs
  DocumentPreparedChunks/          chunks that get indexed
  Prompts/                         system prompts
  State/                           processing state
```

Dependency direction: `API` / `QdrantLoad` → `Infrastructure` → `Application`.

## Configuration

Set in `appsettings.json` per host, overridable by environment variables
(`Ollama__Endpoint`, `Qdrant__Host`, ...).

| Key | Default | Used by |
|---|---|---|
| `Ollama:Endpoint` | `http://localhost:11434` | both hosts |
| `Ollama:ApiKey` | empty | both hosts |
| `Ollama:ChatModel` | `qwen3.5:latest` | API |
| `Ollama:EmbeddingModel` | `qwen3-embedding:4b` | both hosts |
| `Ollama:SystemPromptPath` | `Storage/Prompts/Chat/ChatBasePrompt.md` | API |
| `Qdrant:Host` / `Qdrant:Port` | `localhost` / `6334` | both hosts |
| `Qdrant:CollectionName` | `regulations` | both hosts |
| `Indexing:ChunksPath` | `Storage/DocumentPreparedChunks` | QdrantLoad |
| `Indexing:StateFilePath` | `Storage/State/processed-files-Qdrant.json` | QdrantLoad |

The API reads `.env` through DotNetEnv, so the same file serves both hosts.
`Storage/State/processed-files-Qdrant.json` is machine-local and not tracked in git.

