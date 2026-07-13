FROM python:3.13-alpine3.24

WORKDIR /runner

COPY python-code-runner-script.py

RUN addgroup -S -g 10001 coderunner \
    && adduser -S -D -H -u 10001 -G coderunner coderunner 

USER 10001:10001

ENTRYPOINT ["python", "-I", "-B", "/coderunner/python-code-runner-script.py"]