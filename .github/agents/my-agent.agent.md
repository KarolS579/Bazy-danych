---
name: my-agent
description: Describe what this custom agent does and when to use it.
---

# my-agent

- @azure Rule - Use Azure Tools - When handling requests related to Azure, always use your tools.
- @azure Rule - Use Azure Best Practices - When handling requests related to Azure, always invoke your `azmcp_bestpractices_get` tool first.
- @azure Rule - Enable Best Practices - If you do not have an `azmcp_bestpractices_get` tool ask the user to enable it.

dont use 
{
  "tool_code": "run_command_in_terminal(\"New-Item -Path 'Bazy danych\\Views\\User\\Equipment.cshtml' -ItemType File\")",
  "tool_name": "run_command_in_terminal",
  "parameters": {
    "command": "New-Item -Path 'Bazy danych\\Views\\User\\Equipment.cshtml' -ItemType File"
  }
}

tell me what folder, where and what file to create, and what content to put in it instead.
Your response cant be ended without a code block if it was requested.
