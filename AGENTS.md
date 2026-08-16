# Repository Instructions

## Linear is required

All development work in this repository must be tied to Linear before code
changes begin.

Do not use the Linear MCP server or any Linear connector/MCP tooling in this
repository. Linear access must always go through the Linear HTTP GraphQL API
using an API key from the local environment.

Before reading or changing Linear issues, check for `LINEAR_API_KEY` in this
order:

1. The process environment.
2. `.env.local` in the repository root.
3. `.env` in the repository root.

If the API key is missing, stop and ask the human to configure it before
continuing. Do not fall back to MCP.

Use the Linear GraphQL endpoint:

```text
https://api.linear.app/graphql
```

For a personal Linear API key, send the key directly in the `Authorization`
header. Do not prefix it with `Bearer`. Also send `Content-Type:
application/json`.

Never print, log, commit, or include the API key in issue content, comments,
source code, patches, command output, or documentation.

## Linear workspace scope

All Linear work for this repository must remain inside:

- Team: `TCC` (key `TCC`).
- Project: `PrograMago`.

Create, read, update, and comment only on issues that belong to this team and
project. Do not access or modify issues, projects, cycles, or settings from any
other Linear team while working in this repository.
