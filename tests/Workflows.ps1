# Invoked by Smoke.ps1; all SQL is restricted to the synthetic LocalDB database.
function Query($sql) {
    $output = & sqlcmd -S '(localdb)\MSSQLLocalDB' -E -C -d AttendanceManagement_Demo -Q "SET NOCOUNT ON; $sql" -h -1 -W -b
    if ($LASTEXITCODE) { throw 'Demo database query failed.' }
    return ($output -join '').Trim()
}
function PostJson($session, $path, $body) {
    $page=GetPage $session '/Administrator/GenIncharge/ShiftScheduling'
    $token=[regex]::Match($page.Content,'name="__RequestVerificationToken"[^>]*value="([^"]+)"').Groups[1].Value
    Invoke-WebRequest "$BaseUrl$path" -WebSession $session -Method Post -ContentType 'application/json' -Headers @{RequestVerificationToken=$token} -Body ($body|ConvertTo-Json -Depth 8) -SkipHttpErrorCheck
}
$company=Query "SELECT Id FROM Companies WHERE Domain='demo.hospital'"
if (!$company) { throw 'Expected demo company is missing.' }
$emp=[int](Query "SELECT Id FROM Employees WHERE EmployeeNo='DEMO-001'")
$other=[int](Query "SELECT Id FROM Employees WHERE EmployeeNo='DEMO-003'")
$shift=[int](Query "SELECT Id FROM Shifts WHERE Name='Demo Night 20-04'")
$group=[int](Query "SELECT Id FROM Groups WHERE Code='DEMO-ER'")
$run='QA-'+[guid]::NewGuid().ToString('N').Substring(0,8)
$date=(Get-Date).AddDays(40).ToString('yyyy-MM-dd')
# Remove only prior synthetic workflow requests, never real records.
Query "DELETE FROM LeaveApplications WHERE Reason LIKE 'QA workflow %' AND EmployeeId=$emp" | Out-Null
$body=@{SelectedLeaveType='Annual';FromDate=$date;ToDate=$date;Reason="QA workflow $run"}
$r=PostForm $employee '/Administrator/GenEmployee/ApplyForLeave' $body '/Administrator/GenEmployee/ApplyForLeave'
$id=Query "SELECT Id FROM LeaveApplications WHERE Reason='QA workflow $run'"
Check 'Leave submission persists' ($id -match '^\d+$')
if ($id -match '^\d+$') {
    $r=PostForm $employee '/Administrator/GenEmployee/ApplyForLeave' @{SelectedLeaveType='Annual';FromDate=$date;ToDate=$date;Reason='QA duplicate'} '/Administrator/GenEmployee/ApplyForLeave'
    Check 'Overlapping leave rejected' ($r.Content -match 'overlap an existing')
    $r=PostForm $supervisor '/Administrator/GenIncharge/ApproveLeave' @{id=$id} '/Administrator/GenIncharge/TeamLeaveRequests'
    Check 'Approval updates persisted status' ((Query "SELECT Status FROM LeaveApplications WHERE Id=$id") -eq 'Approved')
    $r=PostForm $supervisor '/Administrator/GenIncharge/RejectLeave' @{id=$id} '/Administrator/GenIncharge/TeamLeaveRequests'
    Check 'Approved leave cannot be rejected again' ($r.StatusCode -eq 409)
    $r=PostForm $admin '/Administrator/LeaveCancellation/Cancel' @{id=$id;cancellationReason='QA cancellation'} '/Administrator/LeaveCancellation'
    Check 'Cancellation persists' ((Query "SELECT Status FROM LeaveApplications WHERE Id=$id") -eq 'Cancelled')
    $r=PostForm $supervisor '/Administrator/GenIncharge/ApproveLeave' @{id=$id} '/Administrator/GenIncharge/TeamLeaveRequests'
    Check 'Cancelled leave cannot be approved again' ($r.StatusCode -eq 409)
}
$date2=(Get-Date).AddDays(41).ToString('yyyy-MM-dd')
$r=PostForm $employee '/Administrator/GenEmployee/ApplyForLeave' @{SelectedLeaveType='Sick';FromDate=$date2;ToDate=$date2;Reason="QA workflow reject $run"} '/Administrator/GenEmployee/ApplyForLeave'
$rejectId=Query "SELECT Id FROM LeaveApplications WHERE Reason='QA workflow reject $run'"
if ($rejectId -match '^\d+$') {
    $r=PostForm $supervisor '/Administrator/GenIncharge/RejectLeave' @{id=$rejectId} '/Administrator/GenIncharge/TeamLeaveRequests'
    Check 'Rejection updates persisted status' ((Query "SELECT Status FROM LeaveApplications WHERE Id=$rejectId") -eq 'Rejected')
} else { Check 'Rejection request created' $false }
$r=GetPage $supervisor "/Administrator/GenIncharge/GetShiftSchedule?employeeId=$other"
Check 'Supervisor cannot read another team schedule' ($r.StatusCode -eq 404)
$r=GetPage $supervisor "/Administrator/GenIncharge/GetShiftSchedule?employeeId=$emp"
$schedule=$r.Content|ConvertFrom-Json
$entries=@($schedule.entries|ForEach-Object { @{id=$_.id;shiftId=$_.shiftId;effectiveFrom=$_.effectiveFrom;isActive=$_.isActive} })
$future=(Get-Date).AddDays(8).ToString('yyyy-MM-dd')
$entries=@($entries | Where-Object { $_.effectiveFrom -ne $future })
$entries+=@{shiftId=$shift;effectiveFrom=$future;isActive=$true}
$r=PostJson $supervisor '/Administrator/GenIncharge/SaveShiftSchedule' @{employeeId=$emp;entries=$entries}
Check 'Future schedule saved' ($r.StatusCode -eq 200)
$r=PostJson $supervisor '/Administrator/GenIncharge/SaveShiftSchedule' @{employeeId=$emp;entries=@()}
Check 'Past schedule cannot be deleted' ($r.StatusCode -eq 400)
$r=PostJson $supervisor '/Administrator/GenIncharge/SaveShiftSchedule' @{employeeId=$other;entries=$entries}
Check 'Another team schedule cannot be written' ($r.StatusCode -eq 400)
$oldShift=Query "SELECT ShiftId FROM Employees WHERE Id=$emp"
$r=PostForm $admin '/Administrator/GroupShiftAssign/Assign' @{CompanyId=$company;GroupId=$group;AssignedShiftId=$shift;AssignedShiftDate=(Get-Date).AddDays(9).ToString('yyyy-MM-dd');'SelectedEmployeeIds[0]'=$emp} '/Administrator/GroupShiftAssign'
Check 'Future group assignment preserves current shift' ((Query "SELECT ShiftId FROM Employees WHERE Id=$emp") -eq $oldShift)
Check 'Future group assignment creates schedule' ([int](Query "SELECT COUNT(*) FROM EmployeeShiftSchedules WHERE EmployeeId=$emp AND ShiftId=$shift AND EffectiveFrom=CONVERT(date,DATEADD(day,9,GETDATE()))") -eq 1)
$r=PostForm $admin '/Administrator/GroupShiftAssign/Assign' @{CompanyId=$company;GroupId=$group;AssignedShiftId=$shift;AssignedShiftDate=$future;'SelectedEmployeeIds[0]'=$other} '/Administrator/GroupShiftAssign'
Check 'Group assignment rejects nonmember' ($r.StatusCode -eq 400)
$today=Get-Date
$calendar=@{employeeId=$emp;month=$today.Month;year=$today.Year}
$calendar['status_'+$today.ToString('yyyy-MM-dd')]='Strike'
$r=PostForm $supervisor '/Administrator/GenIncharge/SaveCalendar' $calendar "/Administrator/GenIncharge/UpdateCalendar?employeeId=$emp"
Check 'Calendar override persists' ((Query "SELECT DayStatus FROM EmployeeCalendarDays WHERE EmployeeId=$emp AND Date=CONVERT(date,GETDATE())") -eq 'Strike')
# Basic create/edit/delete round trips for independent master records.
foreach($case in @(@{Controller='Location';Table='Locations'},@{Controller='LeavePolicy';Table='LeavePolicies'},@{Controller='Group';Table='Groups'})) {
    $name="$run-$($case.Controller)"
    $data=@{Name=$name;CompanyId=$company;IsActive='true';Status='Active';Code=$name}
    $r=PostForm $admin "/Administrator/$($case.Controller)/Create" $data "/Administrator/$($case.Controller)/Create"
    $record=Query "SELECT Id FROM $($case.Table) WHERE Name='$name'"
    Check "$($case.Controller) create persists" ($record -match '^\d+$')
    if($record -match '^\d+$') {
        $data.Id=$record; $data.Name="$name-edited"
        $r=PostForm $admin "/Administrator/$($case.Controller)/Edit/$record" $data "/Administrator/$($case.Controller)/Edit/$record"
        Check "$($case.Controller) edit persists" ((Query "SELECT Name FROM $($case.Table) WHERE Id=$record") -eq "$name-edited")
        $r=PostForm $admin "/Administrator/$($case.Controller)/Delete/$record" @{} "/Administrator/$($case.Controller)/Index"
        Check "$($case.Controller) delete persists" ((Query "SELECT COUNT(*) FROM $($case.Table) WHERE Id=$record") -eq '0')
    }
}
$r=PostForm $admin "/Administrator/Employee/Delete/$emp" @{} '/Administrator/Employee'
Check 'Referenced employee deletion handled safely' ($r.StatusCode -eq 409 -and (Query "SELECT COUNT(*) FROM Employees WHERE Id=$emp") -eq '1')

$r=GetPage $supervisor "/Administrator/GenIncharge/ViewCalendar?employeeId=$other"
Check 'Supervisor calendar blocks another team employee' ($r.StatusCode -eq 403)
$r=GetPage $employee '/Administrator/GenEmployee/ViewCalendar'
Check 'Saved calendar override visible to employee' ($r.Content -match 'Strike')

foreach($path in '/Administrator/Process','/Administrator/Report','/Administrator/List') {
    $r=GetPage $admin $path; Check "Implemented module $path" ($r.StatusCode -eq 200)
    $r=GetPage $employee $path; Check "Employee blocked from $path" ($r.StatusCode -eq 403)
}
# Import the same fixture twice to confirm duplicate prevention.
$csvPath=Join-Path $PSScriptRoot 'demo-attendance.csv'
@"
EmployeeNo,PunchDateTime,PunchType,DeviceId
DEMO-001,2026-01-05 08:00:00,In,QA-DEVICE
DEMO-001,2026-01-05 16:00:00,Out,QA-DEVICE
"@ | Set-Content $csvPath
for($i=0;$i -lt 2;$i++) {
    $page=GetPage $admin '/Administrator/Process'
    $token=[regex]::Match($page.Content,'name="__RequestVerificationToken"[^>]*value="([^"]+)"').Groups[1].Value
        $handler=[System.Net.Http.HttpClientHandler]::new()
    $handler.CookieContainer=$admin.Cookies
    $client=[System.Net.Http.HttpClient]::new($handler)
    $multipart=[System.Net.Http.MultipartFormDataContent]::new()
    $multipart.Add([System.Net.Http.StringContent]::new($token),'__RequestVerificationToken')
    $multipart.Add([System.Net.Http.ByteArrayContent]::new([System.IO.File]::ReadAllBytes($csvPath)),'file','attendance.csv')
    try {
        $response=$client.PostAsync("$BaseUrl/Administrator/Process/Import",$multipart).GetAwaiter().GetResult()
        $r=@{StatusCode=[int]$response.StatusCode;Content=$response.Content.ReadAsStringAsync().GetAwaiter().GetResult()}
    } finally { $multipart.Dispose(); $client.Dispose() }
    Check "CSV import attempt $i" ($r.StatusCode -eq 200 -and $r.Content -match 'Imported')
}
Check 'CSV reimport does not duplicate punches' ((Query "SELECT COUNT(*) FROM Attendances WHERE EmployeeId=$emp AND DeviceId='QA-DEVICE'") -eq '2')
$category=Query 'SELECT TOP 1 Id FROM Categories ORDER BY Id'
$policy=Query 'SELECT TOP 1 Id FROM LeavePolicies ORDER BY Id'
foreach($case in @(
    @{Controller='Employee';Table='Employees';Fields=@{Name="$run-Employee";CompanyId=$company;EmployeeNo=$run;EmployeeType='Permanent';IsActive='true'}},
    @{Controller='Leave';Table='Leaves';Fields=@{Name="$run-Leave";CompanyId=$company;CategoryId=$category;LeavePolicyId=$policy;IsActive='true';'LeaveDetails[0].LeaveType'='Annual';'LeaveDetails[0].NoOfDays'=10}},
    @{Controller='Shift';Table='Shifts';Fields=@{Name="$run-Shift";CompanyId=$company;CompanyName='Demo';IsActive='true'}}
)) {
    $data=$case.Fields
    if($case.Controller -eq 'Shift') {
        for($d=0;$d -lt 7;$d++) { $data["ShiftDays[$d].DayOfWeek"]=$d; $data["ShiftDays[$d].DayName"]="Day$d"; $data["ShiftDays[$d].IsWorking"]='true'; $data["ShiftDays[$d].StartTimeHours"]=8; $data["ShiftDays[$d].StartTimeMinutes"]=0; $data["ShiftDays[$d].DurationHours"]=8; $data["ShiftDays[$d].DurationMinutes"]=0 }
    }
    $r=PostForm $admin "/Administrator/$($case.Controller)/Create" $data "/Administrator/$($case.Controller)/Create"
    $record=Query "SELECT Id FROM $($case.Table) WHERE Name='$($data.Name)'"
    Check "$($case.Controller) create persists" ($record -match '^\d+$')
    if($record -match '^\d+$') {
        $data.Id=$record; $data.Name+='-edited'
        $r=PostForm $admin "/Administrator/$($case.Controller)/Edit/$record" $data "/Administrator/$($case.Controller)/Edit/$record"
        Check "$($case.Controller) edit persists" ((Query "SELECT Name FROM $($case.Table) WHERE Id=$record") -eq $data.Name)
        $r=PostForm $admin "/Administrator/$($case.Controller)/Delete/$record" @{} "/Administrator/$($case.Controller)/Index"
        Check "$($case.Controller) delete persists" ((Query "SELECT COUNT(*) FROM $($case.Table) WHERE Id=$record") -eq '0')
    }
}


foreach($lookup in @(@{Name='Category';Table='Categories'},@{Name='Designation';Table='Designations'},@{Name='Department';Table='Departments'})) {
    $name="$run-$($lookup.Name)"
    $r=PostForm $admin "/Administrator/Dashboard/Add$($lookup.Name)" @{name=$name} '/Administrator/Dashboard/Lookups'
    $record=Query "SELECT Id FROM $($lookup.Table) WHERE Name='$name'"
    Check "$($lookup.Name) create persists" ($record -match '^\d+$')
    if($record -match '^\d+$') {
        $r=PostForm $admin "/Administrator/Dashboard/Update$($lookup.Name)" @{id=$record;name="$name-edited"} '/Administrator/Dashboard/Lookups'
        Check "$($lookup.Name) edit persists" ((Query "SELECT Name FROM $($lookup.Table) WHERE Id=$record") -eq "$name-edited")
        $r=PostForm $admin "/Administrator/Dashboard/Delete$($lookup.Name)" @{id=$record} '/Administrator/Dashboard/Lookups'
        Check "$($lookup.Name) delete persists" ((Query "SELECT COUNT(*) FROM $($lookup.Table) WHERE Id=$record") -eq '0')
    }
}
$roleName="$run-role"
$roleData=@{Name=$roleName;DisplayName=$roleName;Description='QA';IsActive='true';'RoleModules[0].ModuleId'=2;'RoleModules[0].ModuleName'='Administrator';'RoleModules[0].ModuleDisplayName'='Administrator';'RoleModules[0].CanView'='true'}
$r=PostForm $admin '/SuperUser/Role/Create' $roleData '/SuperUser/Role/Create'
$roleId=Query "SELECT Id FROM Roles WHERE Name='$roleName'"
Check 'Role create persists' ($roleId -match '^\d+$')
if($roleId -match '^\d+$') {
    $r=PostForm $admin '/SuperUser/AssignRole/Create' @{RoleId=$roleId;CompanyId=$company;AssignmentType='Group';GroupId=$group;IsActive='true'} '/SuperUser/AssignRole/Create'
    $assignment=Query "SELECT Id FROM RoleAssignments WHERE RoleId=$roleId"
    Check 'Group role assignment persists' ($assignment -match '^\d+$')
    $r=GetPage $employee '/Administrator/Employee'
    Check 'Group role grants management view' ($r.StatusCode -eq 200)
    $r=PostForm $employee '/Administrator/Employee/Create' @{Name='Denied';EmployeeNo='DENIED';CompanyId=$company;EmployeeType='Permanent'} '/Administrator/Employee'
    Check 'Read-only group role cannot create' ($r.StatusCode -eq 403)
    if($assignment -match '^\d+$') {
        $r=PostForm $admin "/SuperUser/AssignRole/Delete/$assignment" @{} '/SuperUser/AssignRole'
        Check 'Role assignment delete persists' ((Query "SELECT COUNT(*) FROM RoleAssignments WHERE Id=$assignment") -eq '0')
        $r=GetPage $employee '/Administrator/Employee'; Check 'Permission revoked immediately' ($r.StatusCode -eq 403)
    }
    $departmentId=Query "SELECT DepartmentId FROM Employees WHERE Id=$other"
    $individual=@{RoleId=$roleId;CompanyId=$company;AssignmentType='Individual';DepartmentId=$departmentId;EmployeeId=$other;IsActive='true'}
    $r=PostForm $admin '/SuperUser/AssignRole/Create' $individual '/SuperUser/AssignRole/Create'
    $individualId=Query "SELECT Id FROM RoleAssignments WHERE RoleId=$roleId AND EmployeeId=$other"
    Check 'Individual role assignment persists' ($individualId -match '^\d+$')
    if($individualId -match '^\d+$') {
        $individualUser=Login 'DEMO-003'
        $r=GetPage $individualUser '/Administrator/Employee'
        Check 'Individual permission allows view' ($r.StatusCode -eq 200)
        $individual.Id=$individualId;$individual.IsActive='false'
        $r=PostForm $admin "/SuperUser/AssignRole/Edit/$individualId" $individual "/SuperUser/AssignRole/Edit/$individualId"
        Check 'Role assignment edit deactivates access' ((Query "SELECT CAST(IsActive AS int) FROM RoleAssignments WHERE Id=$individualId") -eq '0')
        $r=GetPage $individualUser '/Administrator/Employee'
        Check 'Inactive individual assignment denied immediately' ($r.StatusCode -eq 403)
        $r=PostForm $admin "/SuperUser/AssignRole/Delete/$individualId" @{} '/SuperUser/AssignRole'
        Check 'Individual role assignment delete persists' ((Query "SELECT COUNT(*) FROM RoleAssignments WHERE Id=$individualId") -eq '0')
    }
    $roleData.Id=$roleId;$roleData.DisplayName='QA edited'
    $r=PostForm $admin "/SuperUser/Role/Edit/$roleId" $roleData "/SuperUser/Role/Edit/$roleId"
    Check 'Role edit persists' ((Query "SELECT DisplayName FROM Roles WHERE Id=$roleId") -eq 'QA edited')
    $r=PostForm $admin "/SuperUser/Role/Delete/$roleId" @{} '/SuperUser/Role'
    Check 'Role delete persists' ((Query "SELECT COUNT(*) FROM Roles WHERE Id=$roleId") -eq '0')
}
$moduleData=@{Name="$run-module";DisplayName='QA Module';Description='QA';Area='Administrator';Controller='Report';Action='Index';Icon='bi-clock';IsActive='true'}
$r=PostForm $admin '/SuperUser/Module/Create' $moduleData '/SuperUser/Module/Create'
$moduleId=Query "SELECT Id FROM Modules WHERE Name='$run-module'"
Check 'Module create persists' ($moduleId -match '^\d+$')
if($moduleId -match '^\d+$') {
    $moduleData.Id=$moduleId;$moduleData.DisplayName='QA edited'
    $r=PostForm $admin "/SuperUser/Module/Edit/$moduleId" $moduleData "/SuperUser/Module/Edit/$moduleId"
    Check 'Module edit persists' ((Query "SELECT DisplayName FROM Modules WHERE Id=$moduleId") -eq 'QA edited')
    $r=PostForm $admin "/SuperUser/Module/Delete/$moduleId" @{} '/SuperUser/Module'
    Check 'Module delete persists' ((Query "SELECT COUNT(*) FROM Modules WHERE Id=$moduleId") -eq '0')
}
$dateMark=(Get-Date).AddDays(70).ToString('yyyy-MM-dd')
$r=PostForm $admin '/Administrator/MarkDay/Create' @{CompanyId=$company;GroupId=$group;FromDate=$dateMark;ToDate=$dateMark;IsOff='true'} '/Administrator/MarkDay/Create'
$markId=Query "SELECT TOP 1 Id FROM MarkDays WHERE FromDate='$dateMark' ORDER BY Id DESC"
Check 'Mark day create persists' ($markId -match '^\d+$')
if($markId -match '^\d+$') {
    $r=PostForm $admin "/Administrator/MarkDay/Edit/$markId" @{Id=$markId;CompanyId=$company;GroupId=$group;FromDate=$dateMark;ToDate=$dateMark;IsOn='true'} "/Administrator/MarkDay/Edit/$markId"
    Check 'Mark day edit persists' ((Query "SELECT CAST(IsOn AS int) FROM MarkDays WHERE Id=$markId") -eq '1')
    $r=PostForm $admin "/Administrator/MarkDay/Delete/$markId" @{} '/Administrator/MarkDay'
    Check 'Mark day delete persists' ((Query "SELECT COUNT(*) FROM MarkDays WHERE Id=$markId") -eq '0')
}
$r=PostForm $admin '/Administrator/AssignGroup/Create' @{CompanyId=$company;GroupId=$group;EmployeeType=$run;IsActive='true'} '/Administrator/AssignGroup/Create'
$assignId=Query "SELECT Id FROM AssignGroups WHERE EmployeeType='$run'"
Check 'Assign group create persists' ($assignId -match '^\d+$')
if($assignId -match '^\d+$') {
    $r=PostForm $admin "/Administrator/AssignGroup/Edit/$assignId" @{Id=$assignId;CompanyId=$company;GroupId=$group;EmployeeType="$run-edit";IsActive='true'} "/Administrator/AssignGroup/Edit/$assignId"
    Check 'Assign group edit persists' ((Query "SELECT EmployeeType FROM AssignGroups WHERE Id=$assignId") -eq "$run-edit")
    $r=PostForm $admin "/Administrator/AssignGroup/Delete/$assignId" @{} '/Administrator/AssignGroup'
    Check 'Assign group delete persists' ((Query "SELECT COUNT(*) FROM AssignGroups WHERE Id=$assignId") -eq '0')
}
$country=Query 'SELECT TOP 1 CountryId FROM Cities ORDER BY Id'
$city=Query 'SELECT TOP 1 Id FROM Cities ORDER BY Id'
$companyData=@{Name="$run-company";CountryId=$country;CityId=$city;Domain="$run.invalid";AdminUserId='qa';AdminPassword=$Password;IsActive='true'}
$r=PostForm $admin '/Administrator/Company/Create' $companyData '/Administrator/Company/Create'
$newCompany=Query "SELECT Id FROM Companies WHERE Name='$run-company'"
Check 'Company create persists' ($newCompany -match '^[0-9a-fA-F-]{36}$')
if($newCompany -match '^[0-9a-fA-F-]{36}$') {
    $companyData.Id=$newCompany;$companyData.Name+='-edited'
    $r=PostForm $admin "/Administrator/Company/Edit/$newCompany" $companyData "/Administrator/Company/Edit/$newCompany"
    Check 'Company edit persists' ((Query "SELECT Name FROM Companies WHERE Id='$newCompany'") -eq $companyData.Name)
    $r=PostForm $admin "/Administrator/Company/Delete/$newCompany" @{} '/Administrator/Company'
    Check 'Company delete persists' ((Query "SELECT COUNT(*) FROM Companies WHERE Id='$newCompany'") -eq '0')
}

$passwordUser=Login 'DEMO-003'
$changedPassword=$Password+'-Change'
$r=PostForm $passwordUser '/Administrator/GenEmployee/ChangePassword' @{CurrentPassword='incorrect';NewPassword=$changedPassword;ConfirmPassword=$changedPassword} '/Administrator/GenEmployee/ChangePassword'
Check 'Wrong current password rejected' ($r.Content -match 'Current password is incorrect')
$r=PostForm $passwordUser '/Administrator/GenEmployee/ChangePassword' @{CurrentPassword=$Password;NewPassword=$changedPassword;ConfirmPassword=$changedPassword} '/Administrator/GenEmployee/ChangePassword'
Check 'Password change confirmed' ($r.Content -match 'Password changed successfully')
try {
    $oldLogin=[Microsoft.PowerShell.Commands.WebRequestSession]::new()
    $r=PostForm $oldLogin '/Auth/EmployeeLogin' @{'EmployeeLogin.EmployeeNo'='DEMO-003';'EmployeeLogin.Password'=$Password} '/Auth/Login'
    Check 'Old password no longer authenticates' ($r.Content -match 'Invalid Employee No')
    $newLogin=[Microsoft.PowerShell.Commands.WebRequestSession]::new()
    $r=PostForm $newLogin '/Auth/EmployeeLogin' @{'EmployeeLogin.EmployeeNo'='DEMO-003';'EmployeeLogin.Password'=$changedPassword} '/Auth/Login'
    Check 'New password authenticates' ($r.StatusCode -eq 200 -and $r.Content -notmatch 'Invalid Employee No' -and $r.Content -match 'View Calendar')
} finally {
    $r=PostForm $passwordUser '/Administrator/GenEmployee/ChangePassword' @{CurrentPassword=$changedPassword;NewPassword=$Password;ConfirmPassword=$Password} '/Administrator/GenEmployee/ChangePassword'
    Check 'Test account password restored' ($r.Content -match 'Password changed successfully')
}
