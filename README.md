# WebAdminSystem

Full-stack administrative web application built for managing the digital content and business operations of a small service company.

## Overview

WebAdminSystem combines a public-facing business website with an administrative area for managing products, services, users, quotations, reports and site content. The project demonstrates server-side development with ASP.NET MVC, database integration with Entity Framework and SQL Server, authentication, role-oriented administration and responsive web interfaces.

## Tech Stack

- C#
- ASP.NET MVC 5
- .NET Framework 4.8
- Entity Framework 6
- SQL Server
- Razor Views
- HTML, CSS and JavaScript
- Bootstrap
- BCrypt password hashing
- QuestPDF reporting

## Main Features

- Public business website and editable content
- Product and service management
- Quotation workflows
- Administrative dashboard
- User and role management
- Password hashing with BCrypt
- User activation and deactivation
- Reporting and PDF-related functionality
- SQL Server persistence through Entity Framework

## Project Structure

The application follows the traditional ASP.NET MVC architecture:

- `Controllers/` handles application and business workflows.
- `Models/` contains Entity Framework/database models and application models.
- `Views/` contains Razor views for the public site and administrative interface.
- `Content/` contains styles and visual assets.
- `Scripts/` contains client-side dependencies and behavior.
- `sql/` contains database scripts used by the project.

## Local Setup

### Requirements

- Windows
- Visual Studio with ASP.NET/.NET Framework development tools
- .NET Framework 4.8
- SQL Server / SQL Server Express

### Run locally

1. Clone the repository.
2. Restore the NuGet packages.
3. Create/restore the SQL Server database using the scripts in `sql/`.
4. Configure the `CSMMYMEntities` connection string in `CSMMYM/Web.config` for your local SQL Server instance.
5. Open `CSMMYM.sln` in Visual Studio.
6. Build and run the application using IIS Express.

> The connection string currently included in the development configuration uses Windows integrated authentication and is intended for local development. Production deployments should provide environment-specific database configuration rather than relying on a developer-machine connection string.

## Security Notes

New user passwords are hashed with BCrypt. The sign-in flow also supports migrating legacy plaintext passwords to BCrypt after a successful login. Before exposing the administrative application publicly, authorization rules, production configuration, database credentials and deployment settings should be reviewed and hardened.

## Portfolio Status

This repository is being prepared as a portfolio project. Current work focuses on security review, configuration cleanup, UI polish and deployment readiness.

## Author

Daniela Acevedo
