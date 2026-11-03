# Blackbird.io Ollang [Beta]

Blackbird is the new automation backbone for the language technology industry. Blackbird provides enterprise-scale automation and orchestration with a simple no-code/low-code platform. Blackbird enables ambitious organizations to identify, vet and automate as many processes as possible. Not just localization workflows, but any business and IT process. This repository represents an application that is deployable on Blackbird and usable inside the workflow editor.

## Introduction

<!-- begin docs -->

Ollang is a localization platform for translating documents, subtitles, and audiovisual content. This app connects Blackbird workflows to the Ollang integration API.

## Connecting

Create a developer API key in the Ollang dashboard and use it as the **API token** when adding the connection in Blackbird.

## Actions

### Orders

- **Search orders** returns all orders matching optional text, type, name, date, and sorting filters. Pagination is handled automatically.

### Files

- **Upload source file** uploads a source asset and returns the ID of the newly created Ollang project. The optional folder input lets you create it inside an existing folder.
- **Upload VTT file** attaches a `.vtt` subtitle file to an existing project.
- **Upload reference document** uploads a glossary, guideline, or character list to either a project or a folder. Select the destination and provide the corresponding project or folder ID.

Language inputs accept BCP-47 codes such as `en`, `uk`, `fr-CA`, or `zh-Hant`.

Order creation, order-dependent operations, deliverable downloads, quality control, revisions, subtitle embedding, and order status events are not included yet because they could not be verified end to end in the sandbox without AI credits and eligible completed orders.

## Feedback

Do you want to use this app or do you have feedback on our implementation? Reach out to us using the [established channels](https://www.blackbird.io/) or create an issue.

<!-- end docs -->
