# Copilot Instructions

## Project Guidelines
- UsersService now injects ILogger<UsersService> and IMapper (mapping changed to AutoMapper). Preference for scoped ConnectionContext factory to be added; record change. ConnectionContext class was added and IDatabaseFactory.CreateDatabase signature changed to accept ConnectionContext. BaseDbManager now requires ConnectionContext and IDatabaseFactory together. Removed System.Drawing.Common from some csproj. Confidence:0.8