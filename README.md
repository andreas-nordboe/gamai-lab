# GamAI Lab
This demonstrates the final software artefact submission for the 'AI-Assistive Gamified Code Training using Simulated AI Persona Evaluation' MSc Applied Computer Science project.

## Software Artefact Key Features
-  Code evaluation pipeline that uses LLM model to generate evaluation plans
- Code management system
- AI-task generation
- TODO

## High-level system Architecuture Design
![System architecture](docs/architecture/system-architecture.png)


## How to Setup and Run the Project

Docker code execution image
1. Clone this repo using 'git clone https://github.com/andreas-nordboe/gamai-lab.git'
2. Setup as shown in 'Configure the Project Runtime' below, it is crucial that JWT secret key is set before contiunuing to next step.
3. Make sure the Docker daemon is running in the background (using for example Docker Desktop)
4. Open a console application and cd into the project folder (Terminal on Mac or CMD/Powershell on Windows) 
5. run 'docker compose up --build' to run a fresh build. It may take a while to start up GamAILab the first time, depending on the system hardware and internet connection speed as it requires downloading the configured LLM using Ollama. 
6. Once the backend has successfully started, navigate to http://localhost:5123/login and log in using the configured admin superuser credentials.
7. The frontend enables TODO

Note: Make sure these ports: 5123 for the frontend and 5270 for the Web API are not occupied, which can otherwise be changed by editing the docker compose file.

## Configure the Project Runtime
1. Open the .env file
2. Change the following attributes 
3. Re-run GamAILab using 'docker compose up' to apply these changes.

## Configruation Parameters
- **ROOT_ADMIN_EMAIL** 
    - email used for logging into the admin superuser, which is created during initial startup
- **ROOT_ADMIN_PASSWORD** 
    - password used for logging into the admin superuser.
- **JWT_SECRET_KEY** 
    -this **must** be set and at least 256 bytes long (either manually typed using random characters, using OpenSSL or an Online generator) 
- **JWT_EXPIRY_TIME_IN_MINS** 
    - defines user session duration across the applicaiton before it expires
- **OLLAMA_MODEL** 
    - defaults to gemma4 for lower hardware requirements and more efficient responses. However, using a code-specialised model, such as Qwen2.5-Coder-7B-Instruct is highly recommended to improve evaluation accuracy (at the cost of performance and higher hardware requirements) as mentioned in the report.
- **SEED__EXAMPLE_CODE_TASKS** 
    - set this to either `true` or `false` to load code tasks `` file found inside `GamAILab.WebAPI\SeededAppData\CodeTasks` directory
- **SEED__AI_PERSONAS** 
    - set this to either `true` or `false` to load code tasks from the `` file found inside `GamAILab.WebAPI\SeededAppData\AIPersonas` directory
- **CODE_EXECUTIONS_MAX_CONCURRENT_EXECUTIONS** 
    - defines the maximum amount of concurrent code executions (defaults to 50)
- **HALLUCINATION_CONSISTENCY_THRESHOLD** 
    - the threshold required for (defaults to 1.0, lower numbers, such as 0.8 reduces hallucination checker accuracy, however, code tasks with small claim counts should NOT use lower numbers as the consistency checker will likely fail)
- **HALLUCINATION_USE_VERIFIED_CODE_EVALUATIONS** 
    - set this to either `true` or `false` to toggle historical code evaluation context for hallucination checker
- **HALLUCINATION_MAX_VERIFIED_CODE_EVALUATIONS**   
    - set this to the maximum number of histrical verified code evaluations to retrieve at random per  hallucination checker process (if any)


## Improving AI Hallucination Checker Accuracy by Veriying Histroical Code Submissions
To improve TODO


## Backend Service Components
![Backend](docs/architecture/backend-services.png)


## Database Tables
![Database tables](docs/architecture/database-tables.png)

