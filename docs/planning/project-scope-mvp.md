# Project Scope MVP

The purpose of this document is to define what must be included in the MVP to reduce risks of overengineering.

- Learner opens one Python code task (maybe combining strings or for loops), which will be expanded upon later (possibly automated task generation for specific topics)
- Learner submits code
- The backend stores the submission attempt
- The AI LLM model generates an evaluation plan (criteria, generated tests, common mistakes, hint and feedback response instructions)
- Docker test runner executes the submitted Python code
- The AI LLM model evaluates the task context and execution outcome (execution logs should suffice as evidence)
- The hallucination checker (LLM model again) verifies the feedback
- The frontend displays the results, including feedback, progress and hints (student request/during struggles)