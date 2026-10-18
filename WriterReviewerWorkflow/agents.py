from agent_framework import Agent, AgentResult

class WriterAgent(Agent[str]):
    """Writer agent creates the first draft based on the user's request."""

    def run(self, input_data: str) -> AgentResult[str]:
        prompt = input_data.strip()
        draft = self._create_initial_content(prompt)
        return AgentResult(text=draft, metadata={"phase": "writer", "request": prompt})

    def _create_initial_content(self, prompt: str) -> str:
        if not prompt:
            return "The writer could not create content because no request was provided."

        return (
            f"Based on the request: {prompt}\n\n"
            "Here is an initial draft of the content:\n\n"
            "1. Introduction\n"
            "Provide a short context and purpose for the topic.\n\n"
            "2. Key points\n"
            "Organize the main ideas clearly and concisely.\n\n"
            "3. Conclusion\n"
            "Summarize the takeaway and next steps.\n\n"
            "This draft is intended to capture the main idea and provide a foundation for further refinement."
        )

class ReviewerAgent(Agent[str]):
    """Reviewer agent provides concise, actionable feedback and improves the draft."""

    def run(self, input_data: str) -> AgentResult[str]:
        original = input_data.strip()
        feedback = self._review_content(original)
        refined = self._refine_content(original, feedback)
        return AgentResult(
            text=feedback,
            metadata={"phase": "reviewer", "refined_text": refined}
        )

    def _review_content(self, content: str) -> str:
        return (
            "Reviewer feedback:\n"
            "- Clarify the introduction with a stronger opening sentence.\n"
            "- Turn the key points into a cohesive structure rather than a list of notes.\n"
            "- Add concrete examples or outcomes where appropriate.\n"
            "- Make the conclusion more action-oriented and concise.\n"
        )

    def _refine_content(self, content: str, feedback: str) -> str:
        return (
            "Refined Content:\n\n"
            "Introduction:\n"
            "This content begins with a clear purpose and explains why the topic matters.\n\n"
            "Key Points:\n"
            "The main ideas are organized into a strong narrative, with each point clearly connected to the overall objective.\n\n"
            "Conclusion:\n"
            "The final section summarizes the most important insight and provides a direct next step for the reader.\n\n"
            "Overall, the revised content is easier to follow and more useful for the intended audience."
        )
