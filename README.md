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
- **Get order** returns complete order metadata, documents, finance details, the latest QC evaluation, and delivery history.
- **Create order** creates one order per target language from either an existing project or an uploaded source file.
- **Request human review** submits an eligible completed or delivered order for professional review.
- **Re-run order** reprocesses an eligible AI order, optionally including its connected dependency chain.

### Files

- **Upload source file** uploads a source asset and returns the ID of the newly created Ollang project. The optional folder input lets you create it inside an existing folder.
- **Upload VTT file** attaches a `.vtt` subtitle file to an existing project.
- **Upload reference document** uploads a glossary, guideline, or character list to either a project or a folder. Select the destination and provide the corresponding project or folder ID.
- **Download order deliverables** downloads order documents using optional type, name, and language filters, and can include the VTT file.

### Quality control

- **Run QC evaluation** starts an asynchronous QC evaluation and returns its evaluation ID and processing state. An optional public HTTPS callback URL can receive the completed result.

### Revisions

- **Create revision** requests a correction at a specified media timestamp.
- **Search order revisions** returns the revision history for an order.

### Subtitles

- **Request subtitle embedding** starts burning subtitles into an eligible completed video order.

Language inputs accept BCP-47 codes such as `en`, `uk`, `fr-CA`, or `zh-Hant`.

Actions that create or modify orders require sufficient Ollang credits and an order in the appropriate state. The order-status event is not included because Ollang currently exposes neither a global webhook subscription nor an updated-since order feed.

## Feedback

Do you want to use this app or do you have feedback on our implementation? Reach out to us using the [established channels](https://www.blackbird.io/) or create an issue.

<!-- end docs -->
