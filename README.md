# What is MCP
MCP idea came from the original Tron movie with the Master Control Program as a watcher of all things.

## The Goal
The goal of MCP is to have a code centric solution with mechanisms to collect various types of information reported back to a central repository in which is viewed in a dashboard.

## Who is this for?  
It's initial intent was for I.T. staff to have a single place to go for errors, warnings, alerts, etc. across their environment. And with ongoing data collections recognizing patterns or reliability issues from a high level perspective of their environment.

## How does it work?  
The Agents (made in any language that can create and save a json file) perform some task like look through log files, perform sql query, check a network connection, etc. and saves a MCPPackage file containing it's findings.  

The server app is monitoring for new MCPPackage files and imports them into a data store as they show up.  

The user can then use the dashboard app to view the results.

Please refer to the Wiki for more information.
