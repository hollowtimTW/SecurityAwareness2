# Plan: SA2 UI Completion + EmailDeliveryLog Bug Fix

## Goal
Complete UI gaps so admin can:
1. Pick which departments are targeted when creating a campaign
2. View the email dispatch log (currently broken — no log written)
3. Use <select> dropdowns everywhere (no more plain <input> for foreign keys)
4. Resend a failed email
5. Browse per-campaign delivery log

## Phases

### Phase 1: Bug fixes (foundation)
- [ ] EmailMessage: add AssignmentId + MailboxId fields
- [ ] DispatchAsync: pass assignmentId + mailboxId into EmailMessage
- [ ] CampaignService.CreateAssignmentsAsync: filter by TargetDepartmentIds
- [ ] EmailDispatcherService: persist EmailDeliveryLog + update DispatchedAt

### Phase 2: Repository + Service for EmailDeliveryLog
- [ ] IEmailDeliveryLogRepository (QueryAsync, GetByIdAsync, ResendAsync, AddAsync)
- [ ] EmailDeliveryLogRepository implementation
- [ ] IEmailDeliveryService interface for resend

### Phase 3: Data — Campaign.TargetDepartments
Option A: JSON column (DepartmentIds int[])
Option B: Join table CampaignTargetDepartment
Pick A — simpler, no migration schema change beyond adding one column.

- [ ] Add Campaign.TargetDepartmentIds nvarchar(max) JSON column
- [ ] EF migration
- [ ] CampaignEditViewModel: SelectList for departments

### Phase 4: ViewModels
- [ ] CampaignEditViewModel: SelectedDepartmentIds (int[]), AllDepartments (SelectList)
- [ ] EmployeeEditViewModel: DepartmentOptions (SelectList)
- [ ] DepartmentEditViewModel: ManagerOptions (SelectList nullable)
- [ ] ScheduleEditViewModel: ScheduleTypes (SelectList with Chinese)
- [ ] MailboxEditViewModel: Providers (SelectList)

### Phase 5: Controllers — load ViewBags
- [ ] CampaignController.Create GET: load departments
- [ ] CampaignController.Create POST: parse SelectedDepartmentIds → save
- [ ] CampaignController.Edit GET: load departments + current selection
- [ ] EmployeeController.Create/Edit GET: load departments
- [ ] DepartmentController.Create/Edit GET: load employees (potential managers)
- [ ] ScheduleController.Create/Edit GET: load schedule types
- [ ] SenderMailboxController.Create/Edit GET: load providers

### Phase 6: Views — replace <input> with <select>
- [ ] Campaign/Create.cshtml + Edit.cshtml: department multi-select
- [ ] Employee/Create.cshtml + Edit.cshtml: department select
- [ ] Department/Create.cshtml + Edit.cshtml: manager select (nullable)
- [ ] Schedule/Create.cshtml + Edit.cshtml: schedule type select
- [ ] SenderMailbox/Create.cshtml + Edit.cshtml: provider select

### Phase 7: EmailDeliveryLog UI
- [ ] EmailDeliveryLogController Index (with filters: campaignId, status, mailboxId)
- [ ] EmailDeliveryLogController Details (DeliveryId)
- [ ] EmailDeliveryLogController Resend (DeliveryId)
- [ ] Views/EmailDeliveryLog/Index.cshtml + Details.cshtml
- [ ] Navbar link

### Phase 8: Tests
- [ ] Update tests for EmailMessage signature change
- [ ] Update tests for DispatchAsync flow

### Phase 9: Smoke test
- [ ] dotnet build clean
- [ ] dotnet test green
- [ ] Manual: login → create campaign with departments → start → dispatch → check EmailDeliveryLog → click tracking → see status update
