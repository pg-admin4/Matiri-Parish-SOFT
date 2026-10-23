#!/usr/bin/env pwsh
# One-line PowerShell script to apply EF Core migrations for API and MAUI (run from repo root)

dotnet ef database update -p MatiriParish.Core -s MatiriParish.Api --context MatiriParish.Core.Data.MatiriParishDbContext ; dotnet ef database update -p MatiriParish.Core -s MauiApp1 --context MatiriParish.Core.Data.MatiriParishDbContext
