# ASP.NET Core Web API (Backend)

## Endpoints:
- CodeTaskEndpoint.cs (get coding task)
- CodeSubmissionEndpoint.cs (learner submits code and evalute using AI)
- ProgressEndpoint.cs (this could also be called GameEndpoint, but it might be better to introduce separation early, even for gamification elements)
- PersonaSimulationEndpoint.cs
- Monitoring

Later deliverables (rough ideas):
POST /api/persona-simulation-runs
GET  /api/monitoringdashboard/summary

## Services:
- AuthenticationService.cs (potentially also UserService.cs for RBAC)
- SubmissionService.cs
- CodeExecutionService.cs
- AICodeEvaluationService.cs
- FeedbackService.cs (I might remove this and handle this inside the AICodeEvaluationService instead)
- GameService.cs (as project expands I might separate this logic into different services/implement microservices or handle this in a different container/API)
- PersonaSimulationService.cs
- EngagementService.cs
- HallucinationCheckerService.cs
- EducatorMonitoringService.cs
- AnalyticsService.cs (potentially logging service)


## Models:
User.cs
- UserId
- Username
- Email (since I'm not testing on real students username is probably okay but I want the platform to be realistic)
- Role
- CreatedAt
- LastLoggedIn (maybe)

### UserRole.cs 
(thinking enum or class containing a list/array if more details/id/metadata might be necessary later)
- Learner (student/AI learner persona)
- Educator (monitoring dasboard, if time)
- Admin (seeded root user managing RBAC is probably okay)
- Researcher/Developer (for analytics)

### CodeTask.cs
- CodeTaskId
- Title
- Description
- DefaultCode
- Examples (how a correct code answers could look like, arrays could help with accuracy)
- Constraints (instructions on what the code execution should look like)
- Difficulty (I want adaptive learning but difficulty initially could help guide the learners a bit)
- Timestamps (CreatedAt, maybe also UpdatedAt for accidental tampering safety)

### CodeSubmission.cs
- CodeSubmissionId
- UserId / User (depending on how I'm fetching later)
- CodeTaskId
- CodeSubmission
- Attempts
- Timestamps (last submit, initial submit, maybe frequencies)

### ExcecutionResult.cs
- ExecutionResultId
- CodeSubmission (either code submission task id, nested class or both)
- 
    
### AIEvaluationPlan.cs
- EvaluationPlanId
- CodeTaskId
- Criteria (possibly list/array)
- CommonMistakes
- GeneratedTests ()
- FeedbackInstructions
- ModelUsed
- Timestamps (CreatedAt/InitiatedAt/TimeToPlan)

### GameProgress.cs
- 

### Data:
- GamAILabDbContext.cs 
(for database/ORM, I consider using SQLite or PostgreSQL, although SQLite might be more than sufficient for this initally as the MVP will be pretty lightweight)

## Frontend UI

Pages:
- Dashboard/Home (Shows daily summary)
- Learning path / Progress / Subbmission Summary 
- Code task UI ()
- Achievements / Rewards
- Onboarding (if time)