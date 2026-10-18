from typing import Dict

from agent_framework import Workflow
from agents import WriterAgent, ReviewerAgent

class WriterReviewerWorkflow(Workflow[str]):
    """A two-agent workflow that creates and refines content."""

    def __init__(self) -> None:
        agents = {
            "writer": WriterAgent(),
            "reviewer": ReviewerAgent(),
        }
        super().__init__(agents)

    def execute(self, request: str) -> str:
        self.record_trace("request", request)

        writer_result = self.agents["writer"].run(request)
        self.record_trace("writer_output", writer_result.text)

        reviewer_result = self.agents["reviewer"].run(writer_result.text)
        self.record_trace("reviewer_feedback", reviewer_result.text)

        refined_text = reviewer_result.metadata.get("refined_text")
        if not refined_text:
            refined_text = writer_result.text

        self.record_trace("final_output", refined_text)
        return refined_text
