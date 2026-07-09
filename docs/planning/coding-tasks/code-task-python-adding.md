### Code task id: python_adding
1

### Code task title:
Adding two numbers in Python

### Default code
def add(a, b):
    # write python code here

### Example
add(5,5) should return 10

### Constraints
- The function must be called add
- It must return the answer
- Do not use input
- Do not only print the output/result

### How this should work internally
1. Backend gets task by id 1
2. AICodeEvaluationService creates a testing plan
3. The code runs using CodeExecutionService
4. AIFeedbackService generates feedbac
5. AIHallucinationService verifies the feedback
6. Progress is then updated through GameService (or potentially another state service)