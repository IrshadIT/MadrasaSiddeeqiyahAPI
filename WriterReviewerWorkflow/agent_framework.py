from abc import ABC, abstractmethod
from dataclasses import dataclass
from typing import Any, Dict, Generic, Optional, TypeVar

T = TypeVar("T")

@dataclass
class AgentResult(Generic[T]):
    text: T
    metadata: Dict[str, Any] = None

    def __post_init__(self):
        if self.metadata is None:
            self.metadata = {}

class Agent(ABC, Generic[T]):
    """Base agent interface for multi-agent workflows."""

    @abstractmethod
    def run(self, input_data: Any) -> AgentResult[T]:
        raise NotImplementedError()

class Workflow(ABC, Generic[T]):
    """Base workflow that orchestrates agents."""

    def __init__(self, agents: Dict[str, Agent[Any]]):
        self.agents = agents
        self.trace: Dict[str, Any] = {}

    def execute(self, request: str) -> T:
        raise NotImplementedError()

    def record_trace(self, key: str, value: Any) -> None:
        self.trace[key] = value

    def get_trace(self) -> Dict[str, Any]:
        return self.trace
