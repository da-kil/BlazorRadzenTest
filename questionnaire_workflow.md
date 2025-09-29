# Questionnaire Workflow Logic for Performance Review

## ✅ Template Structure

We have **questionnaire templates** structured as follows:

- **Sections**: Each section contains one **question**.
- **Questions**: Each question contains multiple **items** (i.e., answerable elements).
- **Completion Role**: Each section is assigned a completion role:  
  - `Employee`  
  - `Manager`  
  - `Both`

## 🧩 Assignment & Completion Flow

1. **Template Assignment**: Templates are assigned to employees.
2. **Employee Access**: After assignment, the employee can complete the questionnaire, but only:
   - Sections marked as `Employee` or `Both`.
   - Sections marked as `Manager` or `Both` are **visible only to the manager**.
3. **Manager Access**: The manager completes their sections (`Manager` or `Both`) independently.

## 🔄 Review & Confirmation Flow

4. **Mutual Completion Check**: Once both employee and manager have completed and confirmed their respective sections:
   - The manager can initiate a **combined view** for the performance review.
   - This view includes **all sections** (Employee, Manager, Both).
   - During this review, **answers can be edited** collaboratively.
5. **Final Confirmation**:
   - After the review, the **employee confirms** that everything is correct.
   - Then, the **manager finalizes** the questionnaire.

## 🔒 Locking Behavior

6. **Locking**:
   - Once finalized, the questionnaire becomes **read-only**.
   - No further edits are allowed.

---

## 🧠 Expert Perspective: Senior Architect & UI Designer Insights

To ensure a robust and user-friendly implementation of the questionnaire workflow, we propose the following architectural and design considerations:

### 🔐 Role-Based Access Control (RBAC)

- Implement strict RBAC at the section level:
  - Employees can only view and edit sections marked `Employee` or `Both`.
  - Managers can only view and edit sections marked `Manager` or `Both`.
- Use backend validation to enforce access restrictions, even if frontend logic is bypassed.

### 🧭 Workflow State Machine

- Model the questionnaire lifecycle as a finite state machine:
  - `Assigned → In Progress → Confirmed (Employee) → Confirmed (Manager) → In Review → Finalized`
- Each transition should be explicitly triggered and logged for auditability.

### 🧑‍💼 UI/UX Design Principles

- **Clarity**: Clearly label who is responsible for each section.
- **Progress Indicators**: Show completion status per section and overall.
- **Guided Flow**: Use step-by-step navigation to guide users through their responsibilities.
- **Review Mode**: Provide a side-by-side comparison view during the performance review.
- **Edit with Context**: Allow inline editing with change tracking during the review phase.

### 🤝 Collaboration Experience

- Enable real-time collaboration or locking mechanisms during the review phase to prevent conflicts.
- Provide comment threads or notes per section to facilitate discussion.

### ✅ Finalization and Read-Only Mode

- Once finalized, render the questionnaire in a clean, printable, read-only format.
- Include timestamps and digital signatures (if applicable) for compliance.

By combining thoughtful architecture with intuitive UI design, we ensure a seamless and secure experience for both employees and managers throughout the performance review process.
