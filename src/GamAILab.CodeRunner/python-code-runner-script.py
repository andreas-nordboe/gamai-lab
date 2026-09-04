import contextlib
import json
import importlib.util
import io
import traceback
import sys
import os
from pathlib import Path

WORKSPACE = Path(os.environ.get("CODE_RUNNER_WORKSPACE", "/workspace"))
SUBMISSION_PATH = WORKSPACE / "code_submission.py"
LOCAL_TEST_PATH = WORKSPACE / "tests.json"

def load_learner_code():
    loc = importlib.util.spec_from_file_location("learner_code", SUBMISSION_PATH)

    if loc is None or loc.loader is None:
        raise ImportError("Could not find learner code")
    
    module = importlib.util.module_from_spec(loc)
    loc.loader.exec_module(module)

    return module

# old implementation that only works with numbers
def run_learner_code(learner_code, test):
    output = {
        "name": test["name"],
        "passed": False,
        "expectedResult": test["expectedResult"],
        "actualResult": None,
        "error": None
    }
    
    
    try:
        function = getattr(learner_code, test["functionName"])
    
        if not callable(function):
            raise TypeError(
                f"{test['functionName']} failed"
            )
    
        arguments = [
            parse_value(argument)
            for argument in test["arguments"]
        ]
    
        expected_result = parse_value({ "type": test["expectedResultType"],  "value": test["expectedResult"]  })
        actual_result = function(*arguments)
        output["actualResult"] = actual_result
        output["passed"] = actual_result == expected_result
    
    except Exception as exception:
        output["error"] = (
            f"{type(exception).__name__}: {exception}"
        )
    
    return output
    
# new test runner that supports more test types (11.08.2026)
def run_test(learner_code, test):
    type = test["testType"]
    
    if type == "standardInputOutput":
        return run_standard_input_output_test(test)
    
    if type == "functionReturn":
        return run_learner_code(learner_code, test)
    
    if type == "standardOutput":
        return run_standard_output_test(test)
    
    if type == "expectedException":
        return run_exception_test(learner_code, test)
        
    raise ValueError(f"The test type: '{type}' is not valid")
        
# parses new value types, not only numbers/ints like before
def parse_value(value):
    type = value["type"]
    value = value["value"] # cache
    
    if type == "string":
           return value
    
    if type == "null":
           return None
   
    if type == "boolean":
       if value.lower() == "true":
           return True

       if value.lower() == "false":
           return False

       raise ValueError(f"Invalid boolean: {value}")

    if type == "number":
       numbr = float(value)
       return int(numbr) if numbr.is_integer() else numbr

    if type == "json":
       return json.loads(value)

    raise ValueError(f"{type} is unsupported")
   
def execute_script(standard_input=None):
    standard_output = io.StringIO()
    standard_error = io.StringIO()

    input_text = "\n".join(standard_input or [])
    if input_text:
        input_text += "\n"

    stdin_old = sys.stdin

    try:
        sys.stdin = io.StringIO(input_text)

        with open(SUBMISSION_PATH, encoding="utf-8") as learner_file:
            source = learner_file.read()

        with (contextlib.redirect_stdout(standard_output), contextlib.redirect_stderr(standard_error)):
            exec( compile(source, SUBMISSION_PATH, "exec"), {"__name__": "__main__"} )

    finally:
        sys.stdin = stdin_old

    return standard_output.getvalue(), standard_error.getvalue()
    
# print type tasks
def run_standard_output_test(test):
    output = {
        "name": test["name"],
        "passed": False,
        "expectedResult": test["expectedResult"],
        "actualResult": None,
        "error": None
    }

    try:
        actual_output, _ = execute_script()
        expected_output = test["expectedResult"]
        
        # fixes weird issues with model evaluation
        actual_normalised = actual_output.rstrip("\r\n")
        expected_normalised = expected_output.rstrip("\r\n")
        
        output["actualResult"] = actual_normalised
        output["passed"] = actual_normalised == expected_normalised

    except Exception as exception:
        output["error"] = (
            f"{type(exception).__name__}: {exception}"
        )
    return output
    
# input and print
def run_standard_input_output_test(test):
    output = {
        "name": test["name"],
        "passed": False,
        "expectedResult": test["expectedResult"],
        "actualResult": None,
        "error": None
    }

    try:
        actual_output, _ = execute_script(
            test.get("standardInput", [])
        )
        expected_output = test["expectedResult"]
        
        # fixes weird issues with model evaluation
        actual_normalised = actual_output.rstrip("\r\n")
        expected_normalised = expected_output.rstrip("\r\n")

        output["actualResult"] = actual_normalised
        output["passed"] = actual_normalised == expected_normalised

    except Exception as exception:
        output["error"] = (
            f"{type(exception).__name__}: {exception}"
        )

    return output
    
# exception
def run_exception_test(learner_code, test):
    output = {
        "name": test["name"],
        "passed": False,
        "expectedResult": test["exception"],
        "actualResult": None,
        "error": None
    }

    try:
        function = getattr(learner_code,  test["functionName"])

        if not callable(function):
            raise TypeError(
                f"{test['functionName']} failed"
            )

        arguments = [
            parse_value(argument)
            for argument in test["arguments"]
        ]

        try:
            function(*arguments)
            output["actualResult"] = "No exception"

        except Exception as exception:
            actual_exception = type(exception).__name__

            output["actualResult"] = actual_exception
            output["passed"] = (
                actual_exception == test["exception"]
            )

    except Exception as exception:
        output["error"] = (
            f"{type(exception).__name__}: {exception}"
        )

    return output

def main():
    with open(LOCAL_TEST_PATH, encoding="utf-8") as test_file:
        tests = json.load(test_file)
        standard_output = io.StringIO()
        standard_error = io.StringIO()
        output = []
        fatal_error = None

        with contextlib.redirect_stdout(standard_output), contextlib.redirect_stderr(standard_error):
            try:
                learner_code = None
                
                # there was an issue that prevented prints from working during code exectuion (that has no tests)
                if not tests:
                    exec_standard_output, execution_standard_error = execute_script()
                    
                    standard_output.write(exec_standard_output)
                    standard_error.write(execution_standard_error)
                
                for test in tests:
                    test_type = test["testType"]
    
                    if test_type in ("functionReturn", "expectedException"):
                        if learner_code is None:
                            learner_code = load_learner_code()
                    output.append(run_test(learner_code, test))
            
            except BaseException as exception:
                fatal_error = f"{type(exception).__name__}: {exception}"

    print (json.dumps({"didComplete": fatal_error is None,  "standardOutput": standard_output.getvalue(),  "standardError": standard_error.getvalue(), "fatalError": fatal_error, "testOutputs": output
    }))

if __name__ == "__main__":
    main()