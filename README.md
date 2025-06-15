# lib-common

## Description

This repo contains the source code for the `WordList.Common` shared library, which is used by the lambdas forming the Word List application's backend processing chain for updating word scores.

This library contains functions for sending and receiving messages on the known queues, logging and OpenAI access.

## Environment Variables

Environment variables for message queues are only used when sending to the queue:

| Variable Name      | Description                                        |
|--------------------|----------------------------------------------------|
| UPLOAD_SOURCE_CHUNKS_QUEUE_URL | URL of the upload-source-chunks queue. |
| PROCESS_SOURCE_CHUNK_QUEUE_URL | URL of the process-source-chunk queue. |
| QUERY_WORDS_QUEUE_URL          | URL of the query-words queue.          |
| UPDATE_BATCH_QUEUE_URL         | URL of the update-batch queue.         |
| UPDATE_WORDS_QUEUE_URL         | URL of the update-words queue.         |

## Common Packages

This library is published on GitHub.  To be able to import it, you'll need to use the following command:

```
dotnet nuget add source --username <your-username> --password <github-PAT> --store-password-in-clear-text --name github "https://nuget.pkg.github.com/word-list/index.json"
```