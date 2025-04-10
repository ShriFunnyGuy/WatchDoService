# Windows Services Monitoring & Alert System

## Overview

This system is a .NET Worker Service designed to be deployed on individual Windows servers. It monitors the health of specified Windows Services and sends email alerts when services are either stopped or missing.

## Features

- Monitors local Windows Services every 60 seconds (configurable)
- Sends alerts for newly stopped or missing services
- Avoids duplicate email notifications within the same day
- Admin-configurable service list via PDI system UI
- No recovery detection implemented (as per management directive)

## Architecture

- **Local Deployment:** Service is installed and runs on each server
- **Central Configuration:** Uses shared SQL table `admin_watchdogwinservices`
- **UI Integration:** Admins manage services using the PDI UI

## Configuration Table

**Table:** `admin_watchdogwinservices`

| Column Name        | Type         | Description                        |
|--------------------|--------------|------------------------------------|
| WatchDogWinServiceID | int        | Primary key                        |
| ServiceName        | string       | Name of the Windows Service        |
| ServerName         | string       | Host server name                   |
| ServiceStatus      | string?      | Optional                           |
| CreatedById        | int          | Admin who created the entry        |
| CreatedDate        | datetime     | Date of entry creation             |
| ModifyById         | int?         | Admin who modified the entry       |
| ModifyDate         | datetime?    | Date of modification               |

## Monitoring Logic

- Services are checked every `CheckIntervalSeconds` (default: 60)
- Alert conditions:
  - A new service becomes stopped or missing
  - Still down services on a new day

## Alert Conditions

| Scenario                             | Email Sent? |
|--------------------------------------|-------------|
| A new service goes down              | ✅ Yes      |
| Service remains down                 | ❌ No       |
| Same service fails again on same day| ✅ Yes      |
| Service recovers                     | ❌ No       |
| Issue continues next day             | ✅ Yes      |

## Benefits

- Runs independently per server
- Configuration is centralized
- Lightweight and reliable
- Designed to avoid alert fatigue

## Deployment

1. Deploy as a .NET Worker Service on each Windows server.
2. Ensure `CheckIntervalSeconds` and SMTP settings are configured.
3. Populate the `admin_watchdogwinservices` table via PDI UI.

---

> 📧 For questions or support, contact the PDI system administrator.
