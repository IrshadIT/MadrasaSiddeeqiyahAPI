# Writer-Reviewer Workflow

A lightweight multi-agent workflow app that uses a Writer agent and a Reviewer agent to collaborate on content creation.

## Workflow

1. **Writer Agent** receives a user request and generates an initial draft.
2. **Reviewer Agent** reviews the draft and provides concise, actionable feedback.
3. The workflow returns the final refined content in plain text.

## Run

Use Python 3.14 or later:

```bash
C:/Python314/python.exe main.py
```

Then type your request when prompted.

## Files

- `agent_framework.py` — base agent and workflow abstractions
- `agents.py` — writer and reviewer agent implementations
- `workflow.py` — orchestration of the two-agent collaboration
- `main.py` — CLI entrypoint
