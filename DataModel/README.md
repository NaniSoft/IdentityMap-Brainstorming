# IdentityMap

> **A content-centric identity and access governance model for the enterprise.**
> Every piece of data your organisation owns has a known sensitivity, a known owner,
> and a complete, traversable map of every identity that can reach it — across every
> system it flows through.

---

## Table of Contents

1. [The Problems with Current IAM Systems](#the-problems-with-current-iam-systems)
2. [Why RBAC and ABAC Are Not Enough](#why-rbac-and-abac-are-not-enough)
3. [The IdentityMap Philosophy](#the-identitymap-philosophy)
4. [Core Principles](#core-principles)
5. [How IdentityMap Solves Each Problem](#how-identitymap-solves-each-problem)
6. [Key Concepts](#key-concepts)
7. [The Data Model at a Glance](#the-data-model-at-a-glance)
8. [A Worked Example](#a-worked-example)

---

## The Problems with Current IAM Systems

Identity and access management has been practised for decades. Mature tooling exists for
provisioning, single sign-on, multi-factor authentication, and access reviews. And yet
most organisations cannot confidently answer the following questions:

- **Who can read our salary data — across all the systems it passes through?**
- **If Alice moves from Finance to Engineering, what access does she actually retain?**
- **Which API endpoints can serve customer PII to the outside world?**
- **Does our access model reflect our GDPR and PCI-DSS data scope?**
- **What is the blast radius if our web application's service account is compromised?**

The fact that these questions are hard — often requiring weeks of manual audit — is not
a tooling problem. It is a **modelling problem.** Current IAM systems model the wrong
thing. They model *entitlements.* They should model *content.*

The following problems stem from this root cause.

---

### Problem 1 — Entitlements are modelled without content

Every RBAC and ABAC system can tell you that a user *has access* to a resource. None of
them can tell you *what data that access exposes.* When a security team reviews a role
assignment like `DB_READER_PROD_01`, they see a string. They do not see that this role
grants SELECT on `dbo.Customers` (which contains PII), `dbo.Payments` (which contains
card data), and `dbo.Salaries` (which contains compensation). The entitlement is
recorded; the content it unlocks is invisible.

This means every access review, every risk assessment, and every compliance audit starts
with a translation exercise — manually mapping roles and permissions to the data they
touch. This translation is always incomplete, always out of date, and always wrong at
the edges.

---

### Problem 2 — Data sensitivity is classified where the data lives but enforced nowhere else

Organisations invest heavily in data classification. They tag database columns as PII,
label files as Confidential, and apply sensitivity labels in their DLP tools. These
classifications live in the system that hosts the data — the database, the file server,
the data catalogue.

The access control system knows nothing about them.

When an API endpoint is built that queries the classified table, nobody updates the
endpoint's access policy to reflect the upstream sensitivity. When that endpoint is
accessed by a frontend application, the application's access controls have no
information about what the underlying data classification implies. Sensitivity is
declared at the data layer and evaporates the moment data moves to the next layer.

The result is a systematic mismatch: sensitive data is protected at rest but the access
paths that serve it are governed by policies written without any awareness of the data's
classification.

---

### Problem 3 — Content moves through systems; access control does not follow

A salary record does not live only in `hr.Salaries`. It is queried by a stored procedure,
served by a REST endpoint, consumed by a frontend dashboard, cached in a CDN edge node,
exported to a CSV for the finance team, and logged (partially) in an audit database.
Each hop is a new access surface, governed by a different system with a different security
model.

Current IAM tools model each of these surfaces independently. The fact that they are all
serving — or storing — derivatives of the same original sensitive record is invisible to
every access control engine in the chain.

This creates **ghost access paths**: a user who cannot access `hr.Salaries` directly
may be able to reach its contents through the dashboard endpoint, the cached CSV export,
or the audit log. Nobody designed this. Nobody knows it exists.

---

### Problem 4 — Identity is fragmented across silos

Alice is not one identity. She is:

- `alice@corp.com` in Active Directory
- `alice.smith` in the local Linux `/etc/passwd` on three production servers
- `ALICE` in a legacy Oracle database
- a member of `AD\Finance-Users` which maps to a SQL Server Windows login
- the owner of `alice-reporting-key` in the secrets vault
- the subject of an SSH certificate issued last Tuesday by the internal CA

Every IAM tool tracks one or two of these identities. None of them present a unified view
of "all the ways Alice can authenticate and all the resources each authentication path
can reach." When Alice leaves the organisation, the off-boarding process enumerates the
systems it knows about and misses the ones it doesn't.

---

### Problem 5 — Ownership is personal, not organisational

The person who built a system becomes its owner in the IAM tool. When they leave, the
resource is orphaned. When they move to a different team, the access decisions they make
as owner are no longer appropriate to their new role. When a team grows and access
decisions need to be shared, there is no model for collective ownership.

Most organisations work around this by assigning fictional "service owner" accounts or by
making access approvals fall through to IT helpdesk by default. Neither is governance.

---

### Problem 6 — Access reviews are blind to the data they are reviewing

Periodic access certification campaigns ask reviewers to approve or revoke access. The
reviewer sees: the user's name, the role or group name, and a yes/no button. They do not
see what data the role exposes. They do not see the sensitivity classification of that
data. They do not see whether the user's current job function still warrants that access.

A reviewer certifying "Alice has DB_READER_PROD_01" with no context about what that role
exposes cannot make a meaningful governance decision. They click "certify" to clear the
queue. This is compliance theatre, not governance.

---

### Problem 7 — Compliance is a bolt-on exercise

GDPR, PCI-DSS, HIPAA, and SOX all require organisations to know what data is in scope,
where it lives, and who can access it. This information exists in three separate places:
the data catalogue (what data, what classification), the IAM tool (who has what access),
and the GRC platform (which systems are in scope for which frameworks). None of these
systems talk to each other in real time.

Compliance audits are therefore snapshot exercises. A query run today will produce a
different answer than one run in three months. The answer is never fully trusted. External
auditors request evidence; internal teams spend weeks assembling it manually.

---

### Problem 8 — Machine identities have no governance

Service accounts, API keys, certificates, OAuth clients, and SSH keys collectively
outnumber human accounts in most enterprises by a factor of three to one. They have
broader access than most humans (they run continuously, without MFA, with often
over-privileged permissions accumulated over years). They are almost never rotated on
schedule. They are rarely reviewed. When a service account is compromised, the blast
radius is unknown because nobody has modelled what it can access.

IGA tools were built for human identity lifecycle management. Machine identities are
managed — if at all — through a different team, a different tool, and a different
process. The two are never correlated.

---

### Problem 9 — Effective access is unanswerable

"What can Alice actually read?" sounds like a simple question. Answering it requires:

1. Enumerating every group Alice belongs to (transitively — groups of groups)
2. Enumerating every role assigned to every one of those groups
3. Mapping each role to the permissions it grants in each system
4. Understanding which permissions in which systems are equivalent
5. Mapping permissions back to the resources they protect
6. Understanding which resources serve the same underlying data

No IAM tool answers this question end-to-end. Most answer step 1 and stop. The question
is filed as "requires manual investigation."

---

### Problem 10 — Privileged access is ungoverned at the capability level

Most organisations implement Privileged Access Management (PAM) to control who gets
admin shells, who can run `sudo`, and who can use break-glass accounts. PAM tools
record sessions and enforce just-in-time access. But they govern *the path*, not *what
the path exposes.*

A DBA with break-glass access to a production SQL Server can query any database on that
server. The PAM tool knows the DBA took a session. It does not know the DBA ran
`SELECT * FROM hr.Salaries` during it. The governance record says "privileged access was
taken and recorded" but says nothing about which content was reached.

---

### Problem 11 — The attribute-level sensitivity gap

Even when a table is classified as "Confidential," individual columns within it carry
different sensitivity levels. The `customer_name` column is Internal. The `ssn_encrypted`
column is TopSecret. The `email` column is Confidential. Current IAM systems model access
at the table level. A role that grants SELECT on `dbo.Customers` grants access to all
columns equally. Column-level permissions exist in some databases but they are not
reflected in any IGA or access review tool.

The result is that fine-grained sensitivity classifications in the data dictionary are
meaningless from an access governance perspective.

---

## Why RBAC and ABAC Are Not Enough

Role-Based Access Control (RBAC) assigns permissions to roles and roles to users. It is
simple, well-understood, and scales to most use cases. Its failure mode is **role
explosion** — as access requirements grow more granular, the number of roles grows
combinatorially. Large enterprises routinely accumulate thousands of roles, most of which
nobody understands.

More fundamentally: **RBAC models who can do what. It does not model what the "what" is.**
A role named `REPORTS_READ_PROD` is a string. The content it exposes is external knowledge.

Attribute-Based Access Control (ABAC) improves on RBAC by making access decisions based
on attributes of the user, the resource, and the environment. It is more expressive and
can encode context-sensitive policies. Its failure mode is **policy complexity** — ABAC
policies become opaque, difficult to audit, and nearly impossible to reason about for
non-technical reviewers.

More fundamentally: **ABAC models how access decisions are made. It does not model the
data those decisions are protecting or the paths through which that data flows.**

Both systems answer "can this identity perform this action on this resource?" They cannot
answer "what sensitive content does this grant ultimately expose?" and "what other systems
can a subject reach the same content through?" These are the questions that matter for
enterprise governance.

---

## The IdentityMap Philosophy

IdentityMap is built on a single reorientation:

> **Content is the centre of gravity. Everything else orbits it.**

Traditional IAM starts with the identity: who is this user, what roles do they have,
what can they do? IdentityMap starts with the content: what data exists, how sensitive
is it, who owns it, through which systems does it flow, and which identities can reach it?

This is not merely a philosophical difference. It changes what you can model, what
questions you can answer, and what governance is possible.

When content is central:
- Sensitivity classifications live where they belong — on the content — and propagate upward automatically through every system the content touches.
- Access reviews show reviewers the actual data exposure, not an abstract role name.
- Compliance audits become queries: "which identities can reach GDPR-scoped content?"
- When an identity is compromised, the blast radius is immediately known.
- When content moves from a database to an API to a cache to a file export, the access graph follows it.

The second pillar is that **every entity in the IT landscape is a Resource.** Not just
users and groups, but tables, endpoints, certificates, firewall rules, GPOs, Kubernetes
namespaces, sudo rules, and SSH keys. When every entity is modelled, the access graph is
complete. Shadow access paths become visible. Orphaned resources are discoverable.
Effective access becomes answerable.

---

## Core Principles

### Principle 1 — Every IT entity is a Resource

Every object in the enterprise technology landscape — infrastructure, applications, data,
identity, and network — is modelled as a `Resource`. A server is a Resource. A database
table is a Resource. An AD group is a Resource. An X.509 certificate is a Resource. A
sudo rule on a Linux host is a Resource. A Kubernetes namespace is a Resource.

This universality is what makes a complete access graph possible. Governance gaps exist
wherever there are entities that are not modelled. IdentityMap makes the absence of a
model an explicit design decision rather than an accident.

**What this solves:** The fragmented-identity problem (P4), the machine-identity-governance
gap (P8), and the inability to answer "what can this identity reach?" (P9).

---

### Principle 2 — Enterprise data is Content; the model exists to govern it

Resources that hold or serve enterprise data are `Content`. Content is the primary subject
of governance. Every other concept in the model — ownership, capabilities, grants,
relationships — exists to answer questions about content: who can reach it, through what
path, under what conditions, and with what level of exposure.

Content carries a `SensitivityClassification`. Content has owners. Content has a `ContentNature`
(static, like a file; or dynamic, like an API response assembled at runtime). Content has
a `ContentAccessModel` that declares whether the resource *is* content (a file, a database
row) or *surfaces* content (an API endpoint, a stored procedure).

**What this solves:** The entitlements-without-content problem (P1), the compliance
bolt-on problem (P7), and the fact that access reviews are meaningless without content
context (P6).

---

### Principle 3 — Resource attributes are themselves Content

The properties of a Resource are not metadata — they are content. The `ssn_encrypted`
column definition on a `dbo.Customers` table is content with TopSecret sensitivity. The
`base_salary` field on an `hr.Employees` record is content with Restricted sensitivity.

When attributes carry their own sensitivity classification, two things become possible:
first, a resource can have mixed-sensitivity attributes (a table that is Confidential
overall but contains a TopSecret column); second, sensitivity propagation can operate at
the field level, not just the resource level.

**What this solves:** The attribute-level sensitivity gap (P11).

---

### Principle 4 — Resources are either pure Content or Content Holders

Every resource is explicitly classified as one of two things:

- **`ResourceIsContent`** — the resource *is* the data: a file, a database row, a queue message, an S3 object. Governing it means governing who can read or write the bytes.
- **`AccessSurface`** — the resource *serves or exposes* content: an API endpoint, a stored procedure, a database view, a report. It is a window into content that lives elsewhere.

This distinction is critical because access controls on an `AccessSurface` govern the
path to content, while the actual sensitivity and ownership of the content lives on the
underlying `ResourceIsContent` resources. Governing only the surface without understanding
the content it exposes — the status quo in most organisations — is incomplete governance.

**What this solves:** The ghost-access-paths problem (P3 partial) and the disconnect
between surface-level access policies and the sensitivity of underlying data.

---

### Principle 5 — Every Resource has an Owner; ownership bubbles up the hierarchy

Every resource has at least one owner. Owners are accountable for access decisions: they
approve capability grants, certify access during reviews, and are notified of anomalies.

Ownership is **organisational, not personal.** An owner is attached to a `BusinessApp`
container (representing a team or product) or a `Group`. When a person moves teams
(Mover), the resource's ownership is updated at the container level — all resources owned
by the container inherit the change. When a person leaves (Leaver), the container absorbs
the transition; no resources become orphaned.

If no explicit owner is defined on a resource, ownership is **resolved by bubbling up the
resource's parent chain** — a table inherits from its database, a database from its server,
a server from its infrastructure owner. Orphaned resources are not possible in a correctly
modelled graph.

**What this solves:** The personal-ownership fragility problem (P5) and the orphaned-resource problem.

---

### Principle 6 — All access mechanisms are modelled as Capabilities

Capabilities are what identities can *do* to or *through* a resource. Read, Write, Delete,
Execute, Administer, Delegate, Provision, Audit — these are all Capabilities. But so are
system-level access mechanisms: an AD group is a capability container; a SQL Server server
role is a capability; a sudo rule is a capability; a firewall rule is a capability; a
Kubernetes ClusterRole is a capability.

All existing IT access mechanisms are modelled as Capabilities on their host Resource,
not as separate, siloed concepts. This means the same governance model — capability grants,
approval workflows, expiry, certification — applies uniformly to AD permissions, database
roles, Linux sudo entries, API gateway policies, and K8s RBAC rules.

Identities that can *receive* capabilities — accounts, groups, service accounts,
certificates — are `Actionable Resources`. Resources that *expose* capabilities — tables,
endpoints, file shares — are not actionable; granting "Read" to a table rather than to
an account is a model error.

**What this solves:** The siloed-access-mechanisms problem, the machine-identity-governance
gap (P8), and the inability to express "this AD group confers this capability on this content."

---

### Principle 7 — Capabilities have a defined hierarchy

Capabilities are not flat. The ability to write data implies the ability to read it. The
ability to delete implies the ability to read. The ability to administer implies the
ability to write. These implications are not hard-coded logic — they are persisted in the
data model as `CapabilityImplication` records, queryable and overridable.

This means: when a grant review asks "should Alice have Write on `dbo.Customers`?", the
reviewer and the system both understand that this also confers Read. When a grant for
Delete is requested, the system verifies that the prerequisite Write capability already
exists or will be granted simultaneously.

Privilege escalation paths are visible in the model. A user who can Administer a group
can grant themselves Read on everything the group can access. This chain is traversable.

**What this solves:** The flat-RBAC expressiveness problem and makes privilege escalation
paths visible before they are exploited.

---

### Principle 8 — Content flows through systems; the model tracks the entire flow

Content does not live in one place. A customer record flows from `dbo.Customers` through a
stored procedure through a REST endpoint through an API gateway through a CDN cache to a
browser. Each hop is a `ContentBinding`: a declared edge in the data flow graph that says
"this consumer resource exposes content from this source resource, accessed by this accessor
identity."

The content flow graph means:
- The `AccessGraphResolver` can traverse from any point in the enterprise to the original sensitive content it is derived from.
- Sensitivity is known at every hop, not just at the source.
- Every accessor identity at every hop is enumerable.
- A change to the sensitivity of source content automatically propagates to every surface that binds it.

**What this solves:** The ghost-access-paths problem (P3), the sensitivity-evaporation problem (P2 partial), and the inability to answer "who can reach this sensitive record?" (P9).

---

### Principle 9 — Sensitivity is declared on Content and propagates upward; never downward

Sensitivity is a property of data, not of access paths. It lives on `ResourceIsContent`
resources and on attribute definitions. It propagates **upward** through the content flow
graph: if a table has TopSecret rows, the database that hosts it has an effective
sensitivity of at least TopSecret. The API endpoint that binds to that table has an
effective sensitivity of at least TopSecret. The application that hosts the endpoint
inherits the same floor.

This propagation is automatic, continuous, and stored as `EffectiveSensitivity` on every
resource that can be reached from sensitive content. Propagation never flows downward:
an endpoint classified as Confidential does not make its bound data sources Confidential.
The source is authoritative.

The implication is significant: a security team can query `EffectiveSensitivity >= Restricted`
and receive a complete list of all surfaces across all systems — endpoints, applications,
file shares, queue topics, cached exports — that can expose restricted content. No manual
cross-referencing required.

**What this solves:** The sensitivity-evaporation problem (P2), the compliance-bolt-on
problem (P7), and the attribute-level sensitivity gap (P11).

---

### Principle 10 — Identity is a graph, not a list

An identity is not an account. An identity is the complete graph of resources that
represent a principal's presence across the enterprise: their AD account, their Linux
local users, their service accounts, their certificates, their SSH keys, their OAuth
clients, the groups they belong to, the roles they hold. All of these are Resources,
connected by `UsesIdentity`, `BelongsTo`, and `DependsOn` relationships.

Querying "who is Alice?" means traversing this graph. Querying "what can Alice reach?"
means following the graph from every leaf identity resource outward through groups,
grants, and content bindings to every piece of content that any component of Alice's
graph can access.

Human and machine identities are modelled with the same entity type using the same
relationships. There is no privileged identity class that escapes governance.

**What this solves:** The fragmented-identity problem (P4), the machine-identity-governance
gap (P8), and the unanswerable effective-access question (P9).

---

### Principle 11 — Effective access is always answerable

The `AccessGraphResolver` can traverse from any Resource in the model and produce, within
milliseconds, the complete set of identities that can reach it — directly or transitively
through groups, grants, service accounts, or content bindings. It can also traverse in
reverse: starting from an identity and producing every resource that identity can reach,
including the content those resources expose.

This is not an approximation or a periodic batch job. It is a live query over the stored
data model. The answer is always current because the model is always current.

**What this solves:** The unanswerable-effective-access problem (P9) and makes blast-radius
analysis instant rather than requiring a week-long investigation.

---

### Principle 12 — Compliance is native to the content model

Regulatory frameworks (GDPR, PCI-DSS, HIPAA, SOX) define scopes of sensitive data. In
IdentityMap, compliance framework scope is expressed as `ResourcePolicy` records attached
directly to content resources. A `dbo.Payments` table has a `ResourcePolicy` with
`Framework=PCIDSS, SubClassification=CDE`. A `dbo.Customers` table has a `ResourcePolicy`
with `Framework=GDPR, SubClassification=PersonalData`.

Because content bindings track the full data flow, the compliance scope of any piece of
content is automatically propagated to every surface that can expose it. Answering
"which identities can access our GDPR data scope across all systems?" is a graph
traversal, not a three-week audit.

**What this solves:** The compliance-bolt-on problem (P7) and makes continuous compliance
monitoring architecturally possible for the first time.

---

### Principle 13 — Ownership and certification are designed for Joiner-Mover-Leaver

Ownership via `BusinessApp` containers is designed so that the JML (Joiner-Mover-Leaver)
lifecycle is a first-class operation. When a person joins: add them to the relevant
`BusinessApp` containers and they inherit the appropriate access. When a person moves:
update their container memberships and trigger an `IdentityCertification` campaign against
their existing grants. When a person leaves: deactivate their memberships; grants enter
`SuspendedDueToLeaver` status and are automatically revoked after a configurable window.

Access reviews are content-aware: the certifier sees not just "this role" but "this role
exposes these content resources at these sensitivity levels." A reviewer certifying access
to `DB_READER_PROD_01` sees that it exposes `dbo.Customers (Restricted)` and
`dbo.Payments (TopSecret)`. The decision is informed.

**What this solves:** The personal-ownership fragility problem (P5), the blind-access-review problem (P6), and the identity-fragmentation problem during off-boarding (P4).

---

### Principle 14 — Every access grant traces to actual content

No capability grant exists in isolation. Every grant exists on a `ResourceCapability`
defined on a specific `Resource`. Every `Resource` that is an `AccessSurface` must declare
its content sources via `ContentBinding`. Every `ResourceIsContent` resource carries a
`SensitivityClassification`.

This means the grant chain is always: **Identity → Grant → Capability → Resource → ContentBinding → Content → Sensitivity.** Every step is queryable. There is no grant that cannot be traced to the content it ultimately exposes. Grants without content context are a model anomaly, detected by the `ContentAnomalyDetector`.

**What this solves:** The entitlements-without-content problem (P1) at its root.

---

### Principle 15 — The access graph is anomaly-aware

A correctly modelled access graph makes certain classes of security misconfiguration
automatically visible:

- An `AccessSurface` that declares a Write capability but has no ContentBinding with `AccessType=Write` — the capability is structurally unachievable.
- A `ContentBinding` where the accessor identity does not have a sufficient grant on the content source — the runtime access will fail or is being achieved through an ungoverned path.
- A dynamic `AccessSurface` with no ContentBindings — its content exposure is untraced.
- A write-target binding paired with a read-only capability declaration — an intent inconsistency.
- A user with an active grant whose membership in the granting group expired — a stale entitlement.

These are detected continuously by the `ContentAnomalyDetector`, not discovered during
periodic audits. The model is self-describing enough to know when it is internally inconsistent.

**What this solves:** Makes governance proactive rather than reactive, and closes the gap between what the model says and what the system actually does.

---

## How IdentityMap Solves Each Problem

| Problem | Solving Principle(s) | Mechanism |
|---|---|---|
| Entitlements recorded without content context | P1, P2, P14 | Every Resource is a first-class entity. Every grant traces through Capability → Resource → ContentBinding → Content |
| Sensitivity not reflected at entitlement level | P2, P9 | SensitivityClassification on Content. EffectiveSensitivity propagated upward through every ContentBinding |
| Content moves; access control doesn't follow | P8, P9, P14 | ContentBinding models every data flow hop. AccessGraphResolver traverses the complete chain |
| Identity fragmented across silos | P1, P10 | Every identity representation — AD account, Linux user, certificate, API key — is a Resource in the same graph, connected by UsesIdentity relationships |
| Ownership is personal, breaks on attrition | P5, P13 | Ownership attaches to BusinessApp containers. OwnershipResolver walks the parent chain if no explicit owner exists |
| Access reviews blind to data exposure | P6, P13 | IdentityCertification shows reviewer the complete content exposure of every grant being certified |
| Compliance is a separate exercise | P12 | ResourcePolicy attaches compliance framework scope to content. Scope propagates through ContentBindings |
| Machine identities have no governance | P1, P6, P10 | Certificates, API keys, OAuth clients, service accounts are Resources. They receive Capability Grants. The same governance applies |
| Effective access is unanswerable | P10, P11 | AccessGraphResolver.FindTouchpoints traverses from any resource to every identity that can reach it, or reverse |
| Privileged access not linked to content | P6, P8 | Privileged scope Capabilities are modelled on the systems they grant access to. ContentBindings trace what content that privileged access reaches |
| Attribute-level sensitivity gap | P3, P9 | AttributeSensitivity on ResourceAttributeDefinition. SensitivityBubbleUpHelper propagates the max over all attribute definitions |
| Flat capability model, no hierarchy | P7 | CapabilityImplication persists the implication graph (Write→Read, Delete→Read) in the data model |

---

## Key Concepts

### Resource

The universal entity. Every IT object — from a cloud region to a database row to an SSH
key — is a `Resource` with a type, sensitivity, status, owner, and set of attributes.
Resources are connected to each other via `ResourceRelationship` (structural topology)
and `ContentBinding` (data flow). They expose `ResourceCapability` records and receive
`CapabilityGrant` records.

### ContentBinding

The edge in the data flow graph. A ContentBinding says: "Resource A (consumer) exposes
content from Resource B (source), accessed by Resource C (accessor identity)." It is the
primitive that makes content flow visible. Without ContentBindings, an API endpoint is
an island. With them, it is a node in a fully traversable content graph.

### ResourceCapability

A declared action that can be performed on or through a resource. Read, Write, Execute,
Administer, Privileged — each is a Capability. Capabilities exist on every resource type:
a table has a Read capability, a firewall rule has an Allow capability, a sudo entry has
an Execute capability. They are all governed the same way.

### CapabilityGrant

The assignment of a Capability to an Actionable Resource (an identity). Grants have status
(Pending, Active, Revoked), approval workflows, expiry dates, and justifications. They are
the formal record of why an identity can do something. They are subject to certification.

### OwnershipResolver

The runtime resolver that answers "who owns this resource?" for any resource in the graph.
It starts with direct ownership records. If none exist, it walks the parent chain via
HostedIn and BelongsTo relationships until it finds an owner or reaches the root of the
graph. Orphaned resources are detected explicitly rather than silently inherited by nobody.

### SensitivityBubbleUpHelper

The runtime helper that propagates sensitivity upward through the content graph. It
computes `EffectiveSensitivity` for a resource as the maximum of: its own declared
sensitivity, the sensitivity of its attribute definitions, the sensitivity of its
ContentBinding sources, and the sensitivity of its hosted children. The result is stored
on the resource so that any query can filter by effective sensitivity without a graph
traversal.

### AccessGraphResolver

The graph traversal engine. Given any resource, it produces every other resource that is
connected to it through the access graph: grants, group memberships, content bindings,
identity relationships, and attribute references. Traversed forward (from content to
identities), it produces a complete blast-radius report. Traversed in reverse (from an
identity to content), it produces a complete effective-access report.

### ContentAnomalyDetector

The continuous consistency checker. It inspects the data model for structural
mismatches — capabilities that cannot be satisfied by any existing content binding,
accessors without sufficient grants for the bindings they fulfil, dynamic content sources
without declared bindings — and produces actionable anomaly records. It turns the model's
internal consistency into a live governance signal.

---

## The Data Model at a Glance

```
Resource                         ← universal entity for every IT object
├── ResourceRelationship         ← structural topology (HostedIn, BelongsTo, UsesIdentity, ...)
├── ResourceCapability           ← what can be done to/through this resource
│   ├── CapabilityApprovalRequirement  ← who must approve grants for this capability
│   └── CapabilityGrant              ← assignment of this capability to an identity
│       └── GrantApprovalVote        ← approval workflow record
├── ContentBinding               ← data flow edge: source → consumer via accessor
├── ResourceOwnership            ← who owns this resource (Human/Group/BusinessApp)
├── ResourceAttributeValue       ← typed attribute value for this resource instance
│   └── AttributeValueOwnership  ← who governs this specific attribute value
├── ResourcePolicy               ← compliance framework labelling (GDPR, PCI-DSS, ...)
└── IdentityCertification        ← JML access review record

ResourceTypeSchema               ← attribute schema per resource type
└── ResourceAttributeDefinition  ← definition of one attribute (type, sensitivity, ...)
    ├── ResourceAttributeEnumOption  ← valid enum values
    └── AttributeDefinitionOwnership ← who governs this attribute class

CapabilityImplication            ← formal hierarchy: Write implies Read, etc.
BusinessAppMembership            ← member of a group/app with a typed role
```

---

## A Worked Example

**Scenario:** A `TopSecret` salary record in `hr.Salaries` (hosted on `SQLSRV-PROD-02`)
is served by `GET /api/employees/{id}/salary` (on `HRPortal` web application). The
endpoint is accessible by members of `AD\Executives`. The question is: can Carol, who is
in `AD\Executives`, read this salary record — and through what path?

**In IdentityMap:**

1. `hr.Salaries` is a `Table` resource with `Sensitivity=TopSecret, ContentAccessModel=ResourceIsContent`.
2. `GET /api/employees/{id}/salary` is a `ServiceEndpoint` resource with `ContentAccessModel=AccessSurface`.
3. A `ContentBinding` connects `hr.Salaries` → `GET /api/employees/{id}/salary` with `AccessType=Read` and `AccessorResourceId=svc-hrapp` (the service account).
4. The endpoint's `EffectiveSensitivity` is computed as `TopSecret` (propagated from the binding).
5. `AD\Executives` has a `CapabilityGrant` for `Execute` on the endpoint (allowing members to call it).
6. `Carol` is a member of `AD\Executives`.
7. `Carol` has a `CapabilityGrant` for `Read` on `hr.Salaries` (granted directly, inherited from her group, or denied).

`AccessGraphResolver.FindTouchpoints(hr.Salaries)` returns `Carol` as a human accessor
at depth 3: `hr.Salaries → ContentBinding:Consumer(GET /salary) → HasExecuteGrant:Executives → MemberOf:Carol`.

The `PrintSensitivityReport` for `hr.Salaries` shows every identity that can reach it,
the full path, and confirms that `EffectiveSensitivity=TopSecret` is visible at the endpoint
level — so any user reviewing the endpoint's access policy can see the content it exposes.

---

## Summary

IdentityMap is not an access control system. It is an **identity and content governance
model** — a complete, traversable map of who your enterprise is, what data it holds, how
that data flows through its systems, and who can reach every piece of it at every point
in that flow.

It exists because current IAM systems have a fundamental blind spot: they model the paths
to content but never the content itself. IdentityMap closes that blind spot by making
content the root object of the model, making sensitivity a propagating property of that
content, and making every access path — across every system, every protocol, and every
identity type — a first-class, queryable, governable citizen of the same graph.

The result is an enterprise that can answer, at any moment and without manual
investigation:

- Who can read our most sensitive data — through every system it flows through?
- What is the blast radius of this compromised service account?
- Which resources are out of compliance with our GDPR scope?
- What happens to all this access when this person leaves tomorrow?

These are the questions that matter. IdentityMap is built to answer them.
